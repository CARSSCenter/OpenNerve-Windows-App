# Developer Guide — ONrecorder (OpenNerve Windows App)

*Last reviewed against commit `14562e6`.*

This guide is for someone modifying the app itself — where things live, how
data flows, and what to watch out for. If you just want to run the app and
use it with an OpenNerve board, see [Getting-Started.md](Getting-Started.md)
instead.

Every structural claim below cites a file and approximate line number so it
stays checkable against the code. Line numbers drift as the code changes —
if a citation looks off, trust the code and consider updating this doc.

## 1. Build, run, and prerequisites

- Target: `net8.0-windows10.0.26100.0`, **x86 only**
  (`<Platforms>x86</Platforms>` in `ONrecorder.csproj`), output type `WinExe`,
  `StartupObject` = `controller.ONrecorder`.
- Open `ONrecorder.sln` in Visual Studio 2022, select the x86 configuration,
  let NuGet restore packages, build/run.
- **Third-party packages** (`ONrecorder.csproj`, restored automatically by
  NuGet — not something you install separately): `ScottPlot` 5.1.57 and
  `ScottPlot.WinForms` 5.1.57 (plotting), `MathNet.Numerics` 5.0.0 (FFT),
  `System.IO.Ports` 10.0.0 (USB serial transport), `Microsoft.WindowsAppSDK`
  1.8.251106002.
- **Admin key.** Admin-authenticated features need a private key at
  `secrets\admin_priv_d_hex.txt`, relative to the resolved working directory
  (see the gotcha below) — 64 hex characters, a 32-byte P-256 private scalar.
  `LoadPrivateKey32FromHexFile` (`Form1.cs:2716`) throws a `FormatException`
  if the length is wrong. Contact carss@usc.edu for the shared development
  key, or generate your own — see the IPG repo's
  [encryption-instructions.md](https://github.com/CARSSCenter/OpenNerve-Implantable-Pulse-Generator/blob/main/Docs/encryption-instructions.md).

**Gotcha:** `currentDir` (`Form1.cs:77`) is computed as
`Path.GetDirectoryName(Application.ExecutablePath)` plus `..\..\..\..\` —
i.e. it assumes the running exe sits at
`bin/<Configuration>/<TargetFramework>/<RuntimeIdentifier>/` inside the
build tree, four levels under the repo root. That's true when you run from
Visual Studio, but not necessarily true for a copied or published exe — the
`secrets\` folder (and anything else resolved via `currentDir`) will be
looked for in the wrong place if the exe has moved.

## 2. Startup and window flow

`ONrecorder.Main()` (`ONrecorder.cs`) calls `Logger.Initialize()`, registers
`AppDomain.UnhandledException` and `Application.ThreadException` handlers
that append to a crash log, then runs `DeviceSelectionForm` — **not**
`Form1` — as the startup window.

`DeviceSelectionForm` runs a `BluetoothLEAdvertisementWatcher` in active
scanning mode, filters advertisements to manufacturer `CompanyId` `0xFFFF`
("DVT") or `0xF0F0` ("SRS"), and ages an entry out of the list after 10
seconds with no advertisement seen. This is also why the app can look like
it's not doing anything if no board is present and broadcasting: the device
list stays empty and Connect stays disabled.

Pressing Connect constructs `Form1(address, advData, selectionForm)`, hides
the selection form, and shows `Form1`.

## 3. File map

| File | Owns |
|---|---|
| `ONrecorder.cs` | Entry point, global exception handlers, crash log |
| `DeviceSelectionForm.cs` | BLE scan/select UI, advertisement parsing hookup |
| `Form1.cs` | Everything else — see the map below |
| `Form1.Designer.cs` | WinForms-designer-generated layout. Don't hand-edit; edit via the designer or `Form1.cs` code |
| `Form1.resx` | Designer resources for `Form1` |
| `BleAdvertisementFormatter.cs` | Decodes the 23-byte manufacturer advertisement payload into readable text |
| `StimInputValidation.cs` | Shared text-box parsing/validation helpers for stimulation parameter fields |
| `Logger.cs` | File-backed `Trace` listener |

## 4. A map of `Form1.cs`

`Form1.cs` is a single 2,766-line partial class holding BLE/USB transport,
the wire protocol codec, DSP, plotting, CSV export, log download, and
roughly 60 UI event handlers. There's no internal `#region` structure to
navigate by, so here's one by approximate line range:

| Concern | Approx. lines |
|---|---|
| Fields, opcode constants, tuning parameters | 36–190 |
| TX loop start/stop (`StartTxLoop`/`StopTxLoopAsync`) | 193–210 |
| UI event handlers — get/set stim parameters, stim on/off, logs | 216–610 |
| `Sending()` — CRC framing + transport dispatch | 610–668 |
| `ConnectAsync()` — GATT service/characteristic discovery, auth handshake | 668–850 |
| Worker tasks (`StartWorkers`) | 872–928 |
| `ProcessIncomingPacket` — inbound opcode dispatch | 928–1530 |
| BLE transmit (`QueueSendToRemote`, `TxLoopAsync`, `WriteOnceWithRetryAsync`) | 1534–1640 |
| `DoDisconnect`, form lifecycle (`Form1_Load`, `Form1_FormClosing`) | 1642–1755 |
| USB serial transport (`SendUSB`, `OnUSBRX`) | 1756–1855 |
| `ValidateCRC`, `ValidOpcode`, RTC sync | 1854–1930 |
| IPG log download state machine | 2007–2060 |
| Signal mode, DSP chain, rolling display buffer | 2060–2270 |
| Impedance / nerve-block handlers | 2274–2500 |
| ECDSA auth + hex/crypto helpers | 2519–2760 |

## 5. Threading and data flow

**Receive**, for both transports, funnels into the same queue:
BLE notifications (`SendFromRemote`) and USB RX (`OnUSBRX`, around
`Form1.cs:1841`) both enqueue onto `dataQueue` (a `ConcurrentQueue`) and
signal `dataEvent` (an `AutoResetEvent`). A background `processingTask`
(started in `StartWorkers`, `Form1.cs:872`) drains `dataQueue` and calls
`ProcessIncomingPacket` for each item.

**Transmit over BLE** goes `Sending()` → `QueueSendToRemote()` → a
`System.Threading.Channels` channel (`_txChannel`, single-reader) →
`TxLoopAsync` → `WriteOnceWithRetryAsync` →
`rxCharacteristic.WriteValueAsync`.

**Transmit over USB** goes `Sending()` → `SendUSB()` directly — synchronous,
no queue.

**Not on the current send path**, despite being present in the file:
`writeQueue` / `writeEvent` and the `writingTask` half of `StartWorkers()`
— nothing enqueues onto `writeQueue` and `writeEvent.Set()` is never called,
so that loop never wakes. `SendToRemote()` (`Form1.cs:1534`) is likewise
never called anywhere; `QueueSendToRemote()` is the method actually used.
Worth knowing before you go looking for where a change to either of those
would take effect — currently, nowhere.

UI updates issued from worker threads go through `BeginInvoke`; follow that
pattern for any new background work that touches controls.

## 6. Transports

`useBluetooth` (`Form1.cs:92`, static `bool`) selects BLE vs. USB serial for
both send and receive. BLE talks to the Nordic UART Service; the TX/RX
characteristic UUIDs are matched by GUID in `ConnectAsync`. `BoardGen`
(`Form1.cs:95`) selects Gen1 vs. Gen2 behavior — see the README for how to
change it; not repeated here.

## 7. Talking to the IPG — framing and opcodes

This section describes what this client implements, not a normative
protocol specification — there isn't a published one yet (see the note at
the end of this section).

Every message is framed as `[opcode][length][payload...][CRC16 lo][CRC16 hi]`
— see `ValidateCRC` (`Form1.cs:1854`), which checks
`expectedLength == 1 + 1 + payloadLen + 2`. The CRC is CRC-16/CCITT-FALSE
(polynomial `0x1021`, initial value `0xFFFF`), computed and appended
little-endian by `Sending()` (`Form1.cs:610`).

Opcode constants are declared around `Form1.cs:54–68` (e.g.
`START_MANUAL_THERAPY = 0xA5`, `AUTH = 0xF0`, `SET_SPARS = 0xAD`); the
`ValidOpcode` whitelist (`Form1.cs:1895`) is the closest thing to an
enumeration of what this client expects to send or receive. Start there
rather than in this document if you need the full opcode list — it changes
independently of this guide.

**Open item:** the IPG firmware repo doesn't currently publish a normative
protocol document; these constants (and their firmware-side counterparts)
are the only written record. If you're extending the protocol, coordinate
with the firmware team so both sides agree on new opcodes.

## 8. Authentication

Admin authentication uses ECDSA over the P-256 curve with SHA-256, and sends
the signature as raw `r‖s` (32 + 32 bytes), not DER — `SignHashP256_RawRs`
(`Form1.cs:2699`) slices the DER-encoded signature down to that format.
`BuildAuthData`/`BuildAuthCode` (`Form1.cs:2519`) assemble the auth payload,
sent with opcode `AUTH = 0xF0`. See the IPG repo's
[encryption-instructions.md](https://github.com/CARSSCenter/OpenNerve-Implantable-Pulse-Generator/blob/main/Docs/encryption-instructions.md)
for key generation and how the firmware side verifies this.

## 9. Signal processing and display

`SignalMode` (`Form1.cs:156`) values are the wire values sent with
`GET_SENSOR`, not just internal labels: `ECGH=1`, `ECGR=2`, `EMG1=3`,
`EMG2=4`, `XL=5`. Filter/gain/sampling-rate figures for each AFE are already
documented in [Getting-Started.md](Getting-Started.md) — not repeated here.

Processing chain per incoming packet: ring buffer → `LinearizeRingBuffer` →
persistent per-channel biquad filters (`ApplyHighPassBiquad`,
`ApplyLowPassBiquad`, `ApplyNotchBiquad`; state held in `BiquadState`, reset
by `ResetSignalState`) → `DownsampleByPicking` → FFT via
`MathNet.Numerics.IntegralTransforms` → plotted with ScottPlot.

Worth knowing before touching the filter chain: a DC estimate is tracked
with a slow exponential moving average (`DcAlpha = 0.001`) and used to prime
the high-pass filter at startup, specifically to avoid a large transient
when a new signal starts. Resetting or bypassing that priming will
reintroduce the transient.

## 10. Advertisement payload decoding

`BleAdvertisementFormatter.Format` decodes the 23-byte BLE manufacturer
advertisement payload used by `DeviceSelectionForm`:

- Byte 0: DVDD, ×0.1 V
- Bytes 1–2: battery A/B, ×0.1 V
- Bytes 3–4: impedance A/B, ×0.01 V
- Bytes 5–7: thermistor readings, converted to °C against a 104AP-2 NTC
  curve (a 49.9 kΩ divider, interpolated over a 5-point lookup table)
- Bytes 8+: status bits (coil present, charging state, fault flags) —
  several are active-low, hence the `invert` parameter on `StatusBit`
- Byte 22: hardware version

## 11. Files the app writes

Three separate, independently-controlled outputs — easy to be surprised by
if you're not expecting all three:

- `ONrecorderLog_<timestamp>.txt` — written beside the executable by
  `Logger.cs`'s `Trace` listener. Disable by setting
  `Logger.EnableLogging = false`.
- `ONrecorderDebugLog.txt` — a separate crash/exception log, written to the
  **Desktop** by `ONrecorder.cs`'s unhandled-exception handlers.
- Recorded signal data — a CSV written to the Desktop, filename chosen by
  the user via the save controls.

## 12. Common modifications

- **Add a new command opcode:** add the constant near `Form1.cs:54–68`,
  add it to `ValidOpcode` (`Form1.cs:1895`), send it via `Sending()`, and
  handle the response (if any) in `ProcessIncomingPacket`
  (`Form1.cs:928`) — coordinate the opcode value with firmware first (see
  §7).
- **Add a new signal mode:** extend the `SignalMode` enum with its wire
  value, wire up a radio button through `rbSignalMode_CheckedChange`
  (`Form1.cs:2060`), and add filter parameters to `ApplySignalConfig`.
- **Change the display window:** `PlotLen` (`Form1.cs:139`) controls
  seconds of data shown in the rolling display.
- **Switch the default transport:** flip `useBluetooth` (`Form1.cs:92`).
- **Turn off file logging:** `Logger.EnableLogging = false` in `Logger.cs`.

## 13. Contributing

Standard PR process against this repo. All code here is released under
CC-BY-4.0 (see `LICENSE.md`) — if you use it, give credit. For the site-wide
contribution process, see the "How to contribute" page under the Community
section of the OpenNerve site.
