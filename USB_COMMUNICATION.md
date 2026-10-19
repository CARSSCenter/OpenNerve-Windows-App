# USB-C Communication Guide

This guide explains how the Windows app (ONrecorder) and the Gen2 IPG firmware talk over USB-C,
what already works, and what still needs to change to (1) stream data over USB and
(2) find and connect to an IPG over USB without using BLE at all.

## 1. How the USB-C link works

```
PC  --USB-C-->  Test fixture: CY7C65215 USB-to-UART bridge
                   ├── COM port A --> MCU USART1 ("DEBUG_UART", 115200 8N1)   <-- the one we use
                   └── COM port B --> nRF52810 BLE_UART (not used)
```

- The bridge shows up in Windows as **two COM ports**. The app finds the right one by
  sending a `GET_RTC` (0xB0) command to every port and keeping the one that answers
  (`ProbeUsbPorts()` in `Form1.cs`).
- On the IPG, bytes received on USART1 go into **the same command parser that BLE uses**
  (`bsp_sp_cmd_handler()` in `App/Bsp/Src/bsp_serialport.c`). So the packet format is identical:
  - Request:  `[opcode, length, payload..., CRC16-lo, CRC16-hi]`
  - Response: `[opcode, length, status, payload..., CRC16-lo, CRC16-hi]`
- The IPG only listens on USB while it is **awake** (advertising, connected, charging/WPT).
  When asleep, the UART is turned off.

## 2. What works today (Stage 1 — done)

With BLE connected, ticking **Developer Options → USB?** sends all commands over USB:
get/set parameters, start/stop stim, impedance, logs, shutdown. Key app details:

- Commands are sent **one at a time**: the app waits for the reply (up to 1.5 s) before sending
  the next one. This is needed because the IPG reuses one receive buffer, so a second
  command sent too early overwrites the first.
- In BLE-connected mode the IPG only checks USB about **once per second**, so each USB
  command can take up to 1 s. (Optional fix: see firmware step F4 below.)
- Streaming data (sensor/EMG) still arrives over **BLE**, even in USB mode. Section 3 fixes that.

## 3. Streaming data over USB (Stage 2)

### Why it doesn't work yet
1. **Stream packets are hard-wired to BLE.** The firmware sends them with
   `app_func_command_resp_send()` → `bsp_sp_cmd_send()`, which only fills the SPI buffer
   for the BLE chip.
2. **115200 baud is too slow.** One sensor packet is 206 bytes. At 6400 Hz that is
   64 packets/s ≈ 13.2 KB/s, but 115200 baud carries only ~11.5 KB/s.
3. **The IPG can freeze.** If the UART is still sending when the next send starts,
   `HAL_UART_Transmit_IT` returns BUSY, and `HAL_ERROR_CHECK` calls `Error_Handler()`,
   which loops forever with interrupts off.
4. **The app's USB receive code drops big packets.** `OnUSBRX` throws away anything when the
   buffer is over 201 bytes (stream packets are 206), and prints so much debug text that it
   can't keep up.

### Firmware changes (`Gen2 PCBA/FW-MCU-H2/FW-NIH-MCU-H2`)

| # | File | Change |
|---|------|--------|
| F1 | `Core/Src/usart.c` + `FW-NIH-MCU-H2.ioc` | Set USART1 baud rate to **921600**. The MCU clock (160 MHz) and the CY7C65215 both support it. Change the `.ioc` too, or CubeMX will overwrite it. |
| F2 | `App/Bsp/Src/bsp_serialport.c` | Add `static bool route_to_uart`. In `bsp_sp_cmd_handler()`, set it to `true` when a command came from the UART and `false` when one came from SPI/BLE. Whichever link started the stream receives it. |
| F3 | `App/Bsp/Src/bsp_serialport.c` | In `bsp_sp_cmd_send()`: if `route_to_uart`, send the packet over the UART instead of filling `active_spi_tx`. Use a helper that first waits (max ~10 ms) for `huart1.gState == HAL_UART_STATE_READY`, then calls `HAL_UART_Transmit(..., 10)`. **Do not** wrap it in `HAL_ERROR_CHECK`: a dropped packet is OK, a frozen IPG is not. Use the same "wait until ready" step before the existing `HAL_UART_Transmit_IT` in `bsp_sp_cmd_handler()`. |
| F4 | `App/Src/app_mode_ble_connection.c` | (Optional, recommended) In the `while` loop of `app_mode_ble_conn_handler()`, add `else if (sp_uart.rx.len > 0U) { bsp_sp_cmd_handler(); }` so USB commands are answered right away instead of within 1 s. |

### App changes (`Form1.cs`)

| # | Change |
|---|--------|
| A1 | Change `UsbBaudRate` to **921600** (one constant; the probe and port opening both use it). |
| A2 | In `OnUSBRX`: remove the `count > 201` check. Drop a byte only if `payloadLen > 239` (firmware max). |
| A3 | In `OnUSBRX`: check each frame's CRC with `Crc16Ccitt(frame, 3 + len)` before accepting it. On a mismatch, drop one byte and keep searching. This lets the parser recover if it gets out of step. (The old `ValidateCRC` uses the wrong length; don't reuse it.) |
| A4 | Remove the `Debug.WriteLine` calls that print the whole buffer on every chunk. Keep a single line per non-sensor packet. |

`ProcessIncomingPacket` needs **no changes**, because the packets are the same as over BLE.

### How to test
1. Flash the firmware. **Test USB** should still say "USB OK".
2. Tick USB?, choose ENG1, tick Save, click Start Measuring. The plot should update and the CSV should fill.
3. Check the packet counter (first payload byte, counts 0–99) for gaps.
4. Repeat at 6400 Hz and with the accelerometer.
5. Untick USB? and stream over BLE. It should still work, because the next BLE command switches `route_to_uart` back.

## 4. Discovery and first connection over USB (Stage 3)

Goal: the IPG shows up in the device list (`DeviceSelectionForm`) when it is plugged in over USB,
and clicking Connect does the handshake (AUTH) over USB. BLE is not needed at all.

### Problems to solve
1. **Before AUTH the IPG only answers AUTH.** Every other command gets status 0xF3, so we
   can't read battery, ID, etc. to show in the list.
2. **A USB-only session gets kicked out after ~1 s.** After AUTH the IPG goes into "connected"
   mode. But nothing is connected over BLE, so advertising keeps stopping every ~1 s. The
   connected-mode loop treats that as "BLE dropped", goes back to advertising mode, and
   forgets the login.
3. **The IPG must be awake.** It wakes only on a magnet or a WPT coil, and only listens on USB
   while awake.

### Firmware changes

| # | File | Change |
|---|------|--------|
| F5 | `App/Functions/Inc/app_func_command.h`, `App/Src/app_mode_ble_active.c` | Add `#define OP_READ_ADV_DATA 0xB2U`. In `app_mode_ble_act_cmd_parser()`, add a case that is allowed **without AUTH**. It fills a 24-byte buffer with `app_mode_ble_act_adv_msd_update()`, adds the BLE ID (`HPID_IPG_BLE_ID`) and `HW_VERSION` (same layout as the advertisement), and replies with those 24 bytes plus the 2-byte company ID (`BPID_BLE_COMPANY_ID`). It is read-only, so it is safe without login. |
| F6 | `App/Src/app_mode_ble_active.c` | In the `OP_AUTH` case, remember where AUTH came from: `usb_session = true` if it arrived over the UART. (Pass the source in from F2's `route_to_uart`, or add a getter in `bsp_serialport.c`.) |
| F7 | `App/Src/app_mode_ble_connection.c` | In `app_mode_ble_conn_handler()`, skip the `BLE_STATE_ADV_STOP → STATE_ACT_MODE_BLE_ACT` change when `usb_session` is true. Because advertising isn't restarted, a phone can't connect over BLE in the meantime. The session still ends normally through the idle timer (`idle_connection_ms_timer`), shutdown or a disconnect request. Clear `usb_session` whenever you leave connected mode. |
| F8 | (Optional) `App/Src/app_mode_ble_active.c` + EXTI setup | Keep the IPG awake while USB power is present. Skip the advertising timeout while `VCHG_PGOOD` is high (the firmware already reads this pin for "USB bench power"). Optionally add a `VCHG_PGOOD` rising-edge wake from sleep into `STATE_ACT_MODE_BLE_ACT`. **Check on the schematic that fixture VBUS really drives VCHG_PGOOD.** Without F8, wake the IPG with the magnet first, as for BLE. |

### App changes

| # | File | Change |
|---|------|--------|
| A5 | `DeviceSelectionForm.cs` | Add a **"Scan USB"** button, or a background check every few seconds. It runs `ProbeUsbPorts()` (move it to a small shared static class, e.g. `UsbLink.cs`) and then sends `OP_READ_ADV_DATA` (0xB2) on the port that answered. |
| A6 | `DeviceSelectionForm.cs` | Build the same `advBytes[24]` / `payload[23]` that `Watcher_Received` builds from a BLE advert, using the 0xB2 reply. Add it to `_devices` under a fake key (e.g. `0xFFFF_0000_0000 + COM number`) and display it as `"COM5 (USB, SRS)"`. `BleAdvertisementFormatter.Format()` then works unchanged. |
| A7 | `DeviceSelectionForm.cs` → `Form1` | On Connect for a USB entry, open Form1 in USB mode. Add a Form1 constructor parameter, e.g. `string? usbPort`. If it is set, set `useBluetooth = false` and skip `ConnectAsync()`. |
| A8 | `Form1.cs` `Form1_Load` | The USB branch already exists (open port → send AUTH → `StartWorkers()`). Change it to open the port passed in (no re-probe). The normal init chain (AUTH → SET_BLEPARS → GET_HPARS → … → GET_BLEPARS → `SetIpgRtc()`) then runs over USB unchanged. |
| A9 | `Form1.cs` | Treat "no BLE device" as normal in USB mode. `Sending()` already skips the BLE path when `useBluetooth == false`. Check that `DoDisconnect()` and `Form1_FormClosing` handle `BLEDevice == null` (they do, so auto log download on close is skipped). Close the port on disconnect (already done). |

### How to test
1. Wake the IPG (magnet, or plug in USB if F8 is done) with the fixture connected.
2. Click **Scan USB**. The list should show "COMx (USB, SRS)" with the same battery/voltage info as the BLE entry.
3. Click Connect. Debug should show AUTH → SET_BLEPARS → GET_HPARS → GET_BLEPARS ("Connected") over USB.
4. Leave it for over 1 minute. It should stay connected (proves F7). Get/set params and stream (needs Stage 2).
5. While connected over USB, try connecting from another PC/phone over BLE. It should not be able to (no advertising).
6. Click Disconnect. The IPG should go back to advertising and the device list should come back.

## 5. Suggested order
1. **Stage 2 firmware F1–F4 + app A1–A4:** USB streaming while BLE is connected.
2. **Stage 3 F5 + A5–A6:** USB discovery only. It is read-only and easy to test.
3. **Stage 3 F6–F7 + A7–A9:** USB-only connection.
4. **F8:** USB wake / stay-awake, if needed.

Flash and test after each step. Each one can be tested on its own.
