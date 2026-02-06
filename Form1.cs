using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Timers;
using System.Xml.Linq;
using MathNet.Numerics.IntegralTransforms;
using ScottPlot;
using ScottPlot.Hatches;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Networking;
using Windows.Networking.NetworkOperators;
using Windows.Storage.Streams;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Cryptography;

//For single write queue refactor
using System.Threading.Channels;
using System.Threading;

namespace controller
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private static BluetoothLEAdvertisementWatcher watcher;
        private static ulong address = 0;
        private static BluetoothLEDevice BLEDevice;
        private GattDeviceService? nusService;
        private GattCharacteristic? txCharacteristic = null;
        private GattCharacteristic? rxCharacteristic = null;
        private const byte START_MANUAL_THERAPY = 0xA5;
        private const byte STOP_MANUAL_THERAPY = 0xA6;
        private const byte GET_SENSOR = 0xA9;
        private const byte GET_HPARS = 0xAA;
        private const byte GET_SPARS = 0xAC;
        private const byte SET_SPARS = 0xAD;
        private const byte AUTH = 0xF0;
        private const byte GET_BLEPARS = 0xF1;
        private const byte SET_BLEPARS = 0xF2;
        private const byte MEASURE_IMPEDANCE = 0xA7;
        private static int PacketLen = 100;
        private static int PacketCnt = 100;
        private double[] EMGdata = new double[PacketLen * PacketCnt]; //10,000
        private float sampleRate = 6400; // Hz
        private int DecValue = 38;
        private int FFTsize = 256; //10000/DecValue = 263
        private int inMaxValue = 3; //mV
        private int outMaxValue = 4; //mV
        private int DECPeriod = (int)(1000 * 38 / 2000); // in msec, 1000 * DecValue / sampleRate        
        private string currentDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\"));

        //Location of private key for Admin mode
        private string private_key_path = @"secrets\admin_priv_d_hex.txt";

        private static byte[] bAdv = new byte[24];
        private static byte[] bResponse;
        public static int PMode = 0;
        private static string[] stimdata = { "n", "n", "n", "n", "n", "n", "n", "n" };
        private bool bConnected;
        private bool bReceived;    //Turns true every time there is a new message received from BLE
        private SerialPort port;
        private List<byte> usbRxBuffer = new List<byte>();

        //Tag for whether to use Bluetooth or USB
        private static bool useBluetooth = true;

        //Flag forGen1 or gen2
        private static int BoardGen = 2;
        private string FWVersion;

        // CSV logging support
        private StreamWriter csvWriter = null;
        private bool isSaving = false;
        private ushort[] packetRaw = new ushort[100];  // per-packet
        private double[] bufferRaw = new double[10000]; // reconstructed full buffer
        private readonly object csvLock = new object();

        // ######## DATA QUEUE ########
        private readonly ConcurrentQueue<byte[]> dataQueue = new();
        private readonly AutoResetEvent dataEvent = new(false);

        // ######## BLE WRITE QUEUE #######
        private readonly ConcurrentQueue<byte[]> writeQueue = new();
        private readonly AutoResetEvent writeEvent = new(false);
        private CancellationTokenSource bleWorkerCts;
        private Task processingTask;
        private Task writingTask;

        //EMG / ECG / ENG etc.
        private enum SignalMode
        {
            EMG1 = 3,
            EMG2 = 4,
            ECGH = 1,
            ECGR = 2
        }
        private SignalMode CurrentSignalMode = SignalMode.ECGR;

        //Impedance
        private double currentZ = 0.0;

        //Nerve block
        private bool useVNBlock = false;

        System.Timers.Timer BLEtimer = new System.Timers.Timer();

        //For single write queue refactor
        private readonly Channel<byte[]> _txChannel =
        Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
        private CancellationTokenSource? _txCts;
        private Task? _txLoopTask;
        private bool txNotifyEnabled = false;

        private void StartTxLoop()//Start the write loop
        {
            _txCts?.Cancel();
            _txCts = new CancellationTokenSource();
            _txLoopTask = Task.Run(() => TxLoopAsync(_txCts.Token));
        }
        
        private async Task StopTxLoopAsync() //Stop the write loop
        {
            if (_txCts == null) return;
            _txCts.Cancel();
            try { if (_txLoopTask != null) await _txLoopTask; }
            catch { /* swallow */ }
            finally { _txCts = null; _txLoopTask = null; }
        }


        private void bView_Click(object sender, EventArgs e)
        {
            if (bView.Text == "Start Viewing")
            {
                bView.Text = "Stop Viewing";
                bSave.Enabled = false;
                groupSignalMode.Enabled = false;
                byte EMGch = (byte)CurrentSignalMode;
                formsPlot1.Plot.Axes.SetLimitsY(-80, 80);

                byte[] bSampleRate = BitConverter.GetBytes(sampleRate);

                byte[] data = [GET_SENSOR, 5, EMGch, bSampleRate[0], bSampleRate[1], bSampleRate[2], bSampleRate[3]];
                //byte[] data = [GET_SENSOR, 1, EMGch];
                Sending(data);
                labelTX.Text = "Getting Data";
                Debug.WriteLine("TX Getting Data");
            }
            else
            {
                bView.Text = "Start Viewing";
                bSave.Enabled = true;
                groupSignalMode.Enabled = true;
                byte[] data = [GET_SENSOR, 0x01, 0x00];
                Sending(data);
                labelTX.Text = "Stopped Data";
            }
        }
        private void bSave_Click(object sender, EventArgs e)
        {
            if (!isSaving)
            {
                // --- START SAVING ---
                bSave.Text = "Stop Saving";
                bView.Enabled = false;
                groupSignalMode.Enabled = false;
                labelTX.Text = "Saving Data";
                isSaving = true;

                string dateStr = DateTime.Now.ToString("yyyy-MM-dd");
                string timeStr = DateTime.Now.ToString("HH:mm:ss");

                // ----- Build filename from the textbox -----
                string fname = txtFname.Text.Trim();
                if (string.IsNullOrWhiteSpace(fname))
                {
                    fname = "Data_" + dateStr + timeStr;
                }
                if (!fname.EndsWith(".csv"))
                    fname += ".csv";

                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string fullPath = Path.Combine(desktop, fname);

                csvWriter = new StreamWriter(fullPath, append: false);

                // ----- Write file header -----
                double effectiveFs = sampleRate / (double)DecValue;   // DECdata sampling rate
                double df = effectiveFs / FFTsize; // frequency bin spacing
                double fmax = effectiveFs / 2.0;

                csvWriter.WriteLine($"Date,{dateStr}");
                csvWriter.WriteLine($"Start Time,{timeStr}");
                csvWriter.WriteLine($"Signal Type,{CurrentSignalMode.ToString()}");
                csvWriter.WriteLine($"Sample Rate (raw),{sampleRate}");
                csvWriter.WriteLine($"Decimation Factor,{DecValue}");
                csvWriter.WriteLine($"Effective Fs (after DEC),{effectiveFs}");
                csvWriter.WriteLine($"FFT size,{FFTsize}");
                csvWriter.WriteLine($"Frequency bin spacing (Hz),{df}");
                csvWriter.WriteLine($"FFT Max Frequency (Hz),{fmax}");
                csvWriter.WriteLine("----------------------------------------------------");
                csvWriter.Flush();

                // ---- Send START command to device ----
                byte EMGch = (byte)CurrentSignalMode;   //= rbEMG1.Checked ? (byte)4 : (byte)4;

                byte[] bSampleRate = BitConverter.GetBytes(sampleRate);
                byte[] data = { GET_SENSOR, 5, EMGch, bSampleRate[0], bSampleRate[1], bSampleRate[2], bSampleRate[3] };
                Sending(data);
            }
            else
            {
                // --- STOP SAVING ---
                bSave.Text = "Start Saving";
                bView.Enabled = true;
                groupSignalMode.Enabled = true;
                labelTX.Text = "Stopped Data";
                isSaving = false;

                // Stop device streaming
                byte[] data = { GET_SENSOR, 0x01, 0x00 };
                Sending(data);

                // Close CSV file
                csvWriter?.Flush();
                csvWriter?.Close();
                csvWriter = null;
            }
        }
        private void BLEtimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Debug.WriteLine("BLE Timer Elapsed, bReceived=" + bReceived + ", bConnected=" + bConnected);
            if (bReceived)
            {
                //Debug.WriteLine("Resetting bReceived");
                bReceived = false;
            }
            else if (bConnected)
            {
                //Debug.WriteLine("Sending Connect+ ask");
                byte[] data = [GET_BLEPARS, 0x00];
                Sending(data);
                bConnected = false;
            }
            else
            {
                Debug.WriteLine("BLE Timer Issue, disconnecting");
                DoDisconnect();
            }
        }
        private void bDisconnect_Click(object sender, EventArgs e)
        {
            try
            { Debug.WriteLine("Disconnecting..."); DoDisconnect(); }
            catch (Exception ex)
            { Debug.WriteLine($"Disconnect Error: {ex.Message}"); Application.Exit(); }
        }
        private void bGet_Click(object sender, EventArgs e)
        {
            byte[] data = [GET_SPARS, 0x04, 0x53, 0x50, 0x30, 0x32]; // Get PA
            Debug.WriteLine("TX Get Params");
            labelTX.Text = "Get Params";
            labelRX.Text = "";
            txtC1.Text = "";
            txtC2.Text = "";
            txtPA.Text = "";
            txtPW.Text = "";
            txtPF.Text = "";
            txtPR.Text = "";
            txtTN.Text = "";
            txtTF.Text = "";
            PMode = 9;
            Sending(data);
        }
        private void bSendC1_Click(object sender, EventArgs e)
        {
            byte st = (byte)Int16.Parse(txtC1.Text);
            byte[] data = [SET_SPARS, 0x06, 0x53, 0x50, 0x30, 0x39, st, 0x00];
            labelTX.Text = "Set Params";
            labelRX.Text = "";
            Sending(data);
        }
        private void bSendC2_Click(object sender, EventArgs e)
        {
            byte st = (byte)Int16.Parse(txtC2.Text);
            byte[] data = [SET_SPARS, 0x06, 0x53, 0x50, 0x31, 0x30, st, 0x00];
            labelTX.Text = "Set Params";
            labelRX.Text = "";
            Sending(data);
        }
        private void bSendPA_Click(object sender, EventArgs e)
        {
            byte st = (byte)(Convert.ToDecimal(txtPA.Text) * 10);
            byte[] data = [SET_SPARS, 0x06, 0x53, 0x50, 0x30, 0x32, st, 0x00];
            labelTX.Text = "Set Params";
            labelRX.Text = "";
            Sending(data);
        }
        private void bSendPW_Click(object sender, EventArgs e)
        {
            byte st = (byte)(Int16.Parse(txtPW.Text) / 50);
            byte[] data = [SET_SPARS, 0x06, 0x53, 0x50, 0x30, 0x33, st, 0x00];
            labelTX.Text = "Set Params";
            labelRX.Text = "";
            Sending(data);
        }
        private void bSendPF_Click(object sender, EventArgs e)
        {
            byte st1 = (byte)(Int16.Parse(txtPF.Text) % 256);
            byte st2 = (byte)(Int16.Parse(txtPF.Text) / 256);
            byte[] data = [SET_SPARS, 0x06, 0x53, 0x50, 0x30, 0x34, st1, st2];
            labelTX.Text = "Set Params";
            labelRX.Text = "";
            Sending(data);
        }
        private void bSendPR_Click(object sender, EventArgs e)
        {
            PMode = 1; //+ RampUp = st
            byte st = (byte)Int16.Parse(txtPR.Text);
            byte[] data = [SET_SPARS, 0x06, 0x53, 0x50, 0x30, 0x35, st, 0x00];
            labelTX.Text = "Set Params";
            labelRX.Text = "";
            Sending(data);
        }
        private void bSendTN_Click(object sender, EventArgs e)
        {
            byte st = (byte)(Int16.Parse(txtTN.Text) / 10);
            byte[] data = [SET_SPARS, 0x06, 0x53, 0x50, 0x30, 0x37, st, 0x00];
            labelTX.Text = "Set Params";
            labelRX.Text = "";
            Sending(data);
        }
        private void bSendTF_Click(object sender, EventArgs e)
        {
            byte st = (byte)(Int16.Parse(txtTF.Text) / 10);
            byte[] data = [SET_SPARS, 0x06, 0x53, 0x50, 0x30, 0x38, st, 0x00];
            labelTX.Text = "Set Params";
            labelRX.Text = "";
            Sending(data);
        }
        private void bGetC1_Click(object sender, EventArgs e)
        {
            byte[] data = [GET_SPARS, 0x04, 0x53, 0x50, 0x30, 0x39]; // Get C1
            Debug.WriteLine("TX Get Params");
            labelTX.Text = "Get Params";
            labelRX.Text = "";
            txtC1.Text = "";
            PMode = 0;
            Sending(data);
        }
        private void bGetC2_Click(object sender, EventArgs e)
        {
            byte[] data = [GET_SPARS, 0x04, 0x53, 0x50, 0x31, 0x30]; // Get C2
            Debug.WriteLine("TX Get Params");
            labelTX.Text = "Get Params";
            labelRX.Text = "";
            txtC2.Text = "";
            PMode = 0;
            Sending(data);
        }
        private void bGetPA_Click(object sender, EventArgs e)
        {
            byte[] data = [GET_SPARS, 0x04, 0x53, 0x50, 0x30, 0x32]; // Get PA
            Debug.WriteLine("TX Get Params");
            labelTX.Text = "Get Params";
            labelRX.Text = "";
            txtPA.Text = "";
            PMode = 0;
            Sending(data);
        }
        private void bGetPW_Click(object sender, EventArgs e)
        {
            byte[] data = [GET_SPARS, 0x04, 0x53, 0x50, 0x30, 0x33]; // Get PW
            Debug.WriteLine("TX Get Params");
            labelTX.Text = "Get Params";
            labelRX.Text = "";
            txtPW.Text = "";
            PMode = 0;
            Sending(data);
        }
        private void bGetPF_Click(object sender, EventArgs e)
        {
            byte[] data = [GET_SPARS, 0x04, 0x53, 0x50, 0x30, 0x34]; // Get PF
            Debug.WriteLine("TX Get Params");
            labelTX.Text = "Get Params";
            labelRX.Text = "";
            txtPF.Text = "";
            PMode = 0;
            Sending(data);
        }
        private void bGetPR_Click(object sender, EventArgs e)
        {
            byte[] data = [GET_SPARS, 0x04, 0x53, 0x50, 0x30, 0x35]; // Get PR
            Debug.WriteLine("TX Get Params");
            labelTX.Text = "Get Params";
            labelRX.Text = "";
            txtPR.Text = "";
            PMode = 0;
            Sending(data);
        }
        private void bGetTN_Click(object sender, EventArgs e)
        {
            byte[] data = [GET_SPARS, 0x04, 0x53, 0x50, 0x30, 0x37]; // Get TN
            Debug.WriteLine("TX Get Params");
            labelTX.Text = "Get Params";
            labelRX.Text = "";
            txtTN.Text = "";
            PMode = 0;
            Sending(data);
        }
        private void bGetTF_Click(object sender, EventArgs e)
        {
            byte[] data = [GET_SPARS, 0x04, 0x53, 0x50, 0x30, 0x38]; // Get TF
            Debug.WriteLine("TX Get Params");
            labelTX.Text = "Get Params";
            labelRX.Text = "";
            txtTF.Text = "";
            PMode = 0;
            Sending(data);
        }
        private void bStimOn_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("TX Stim On");
            labelTX.Text = "Stim On";
            labelRX.Text = "";
            byte[] data = [START_MANUAL_THERAPY, 0x00];
            if (useVNBlock)
            {
                data = [START_MANUAL_THERAPY, 0x01, 0x01];
            }
            Sending(data);
        }
        private void bStimOff_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("TX Stim Off");
            labelTX.Text = "Stim Off";
            labelRX.Text = "";
            byte[] data = [STOP_MANUAL_THERAPY, 0x00];
            Sending(data);
        }
        private void Sending(byte[] data)
        {
            byte[] packet = data.ToArray();
            int dataLength = packet.Length;
            const ushort polynomial = 0x1021; // CRC-16/CCITT-FALSE polynomial
            ushort crc = 0xFFFF; // Initial value
            foreach (byte b in packet)
            {
                crc ^= (ushort)(b << 8); // Shift the current byte to the high bits and XOR with CRC
                for (int i = 0; i < 8; i++) // // Perform 8 bit-shifting operations per byte
                {
                    if ((crc & 0x8000) != 0) // If the highest bit is 1
                    { crc = (ushort)((crc << 1) ^ polynomial); }
                    else
                    { crc <<= 1; }
                }
            }
            byte[] calCRC16 = [(byte)(crc & 0xFF), (byte)((crc >> 8) & 0xFF),];
            Array.Resize(ref packet, dataLength + 2);
            Array.Copy(calCRC16, 0, packet, dataLength, 2);

            if (useBluetooth)
            {
                try
                {
                    if (BLEDevice != null)
                    {
                        //Debug.WriteLine("Sending CRC byte via BLE: " + data[0]);
                        QueueSendToRemote(packet);
                    }
                    else
                    { Debug.WriteLine("BLEDevice is null"); Application.Exit(); }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"BLE Error: {ex.Message}");
                }
            }
            else
            {
                try
                {
                    Debug.WriteLine("Sending data + CRC byte via USB");
                    SendUSB(packet);
                }
                catch (Exception ex) { Debug.WriteLine("USB exception: " + ex); }
            }
        }
        public async Task ConnectAsync()
        {
            try
            {
                BLEDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(address);
                if (!BLEDevice.DeviceInformation.Pairing.IsPaired)
                {
                    var customPairing = BLEDevice.DeviceInformation.Pairing.Custom;
                    //var pairing = BLEDevice.DeviceInformation.Pairing.Custom;
                    customPairing.PairingRequested += (sender, args) =>
                    {
                        Debug.WriteLine($"Pairing requested: {args.PairingKind}");
                        if (args.PairingKind == DevicePairingKinds.ProvidePin)
                        { args.Accept("000000"); }
                        else
                        { args.Accept(); }
                    };
                    var result = await customPairing.PairAsync(DevicePairingKinds.ProvidePin);
                    if (result.Status == DevicePairingResultStatus.Paired)
                    {
                        Debug.WriteLine("Paired. Waiting for ATT/GATT stabilization...");
                        await Task.Delay(500);  // 300–800 ms works best
                        //TODO see if I can remove this to speed up handshake
                    }
                    if (result.Status != DevicePairingResultStatus.Paired)
                    {
                        Debug.WriteLine($"Pairing failed: {result.Status}");
                        DoDisconnect();
                        return;
                    }
                }
                if (BLEDevice.ConnectionStatus != BluetoothConnectionStatus.Connected)
                {
                    if (BLEDevice.DeviceInformation.Pairing.IsPaired)
                    {
                        DeviceUnpairingResult dupr = await BLEDevice.DeviceInformation.Pairing.UnpairAsync();
                        while (dupr.Status != DeviceUnpairingResultStatus.Unpaired)
                        {
                            Application.DoEvents();
                        }
                        var customPairing = BLEDevice.DeviceInformation.Pairing.Custom;
                        customPairing.PairingRequested += (sender, args) =>
                        {
                            if (args.PairingKind == DevicePairingKinds.ProvidePin)
                            {
                                args.Accept("000000");
                            }
                            else
                            {
                                args.Accept();
                            }
                        };
                        var result = await customPairing.PairAsync(DevicePairingKinds.ProvidePin);
                        if (result.Status != DevicePairingResultStatus.Paired)
                        {
                            Debug.WriteLine("Pairing 2 failed");
                            DoDisconnect();
                            return;
                        }
                        if (BLEDevice.ConnectionStatus != BluetoothConnectionStatus.Connected)
                        {
                            Debug.WriteLine("Failed to Connect");
                            DoDisconnect();
                            return;
                        }
                    }
                }
                var gattServicesResult = await BLEDevice.GetGattServicesAsync();
                nusService = null;
                if (gattServicesResult.Status == GattCommunicationStatus.Success)
                {
                    Debug.WriteLine($"Found {gattServicesResult.Services.Count} GATT services.");
                    foreach (var service in gattServicesResult.Services)
                    {
                        if (service.Uuid == Guid.Parse("6E400001-B5A3-F393-E0A9-E50E24DCCA9E")) // UUID
                        {
                            nusService = service;
                            Debug.WriteLine($"Found target service: {service.Uuid}");

                            GattCharacteristicsResult characteristicsResult;
                            characteristicsResult = await nusService.GetCharacteristicsAsync();

                            for (int i = 0; i < 5; i++)
                            {
                                if (characteristicsResult.Status == GattCommunicationStatus.Success)
                                {
                                    Debug.WriteLine("Gatt communication success: " + characteristicsResult.Status);
                                    break;
                                }
                                else
                                {
                                    Debug.WriteLine($"Characteristic attempt {i + 1}/5: {characteristicsResult.Status}");
                                    await Task.Delay(200);
                                    characteristicsResult = await nusService.GetCharacteristicsAsync();
                                }
                            }

                            txCharacteristic = null;
                            rxCharacteristic = null;
                            if (characteristicsResult.Status != GattCommunicationStatus.Success)
                            {
                                Debug.WriteLine("Failed to retrieve characteristics.");
                                Debug.WriteLine($"Characteristic discovery failed. Status: {characteristicsResult.Status}, Protocol: {characteristicsResult.ProtocolError}");
                                DoDisconnect();
                                return;
                            }
                            foreach (var characteristic in characteristicsResult.Characteristics)
                            {
                                if (characteristic.Uuid == Guid.Parse("6E400003-B5A3-F393-E0A9-E50E24DCCA9E")) // TX UUID
                                {
                                    Debug.WriteLine("TX Characteristic found.");
                                    var status = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                                        GattClientCharacteristicConfigurationDescriptorValue.None);
                                    Debug.WriteLine($"TXNone:{status}");
                                    if (status == GattCommunicationStatus.Success)
                                    {
                                        status = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                                            GattClientCharacteristicConfigurationDescriptorValue.Notify);
                                        Debug.WriteLine($"TXNotify:{status}");
                                        if (status == GattCommunicationStatus.Success)
                                        {
                                            txCharacteristic = characteristic;
                                            txCharacteristic.ValueChanged -= SendFromRemote;
                                            txCharacteristic.ValueChanged += SendFromRemote;
                                            txNotifyEnabled = true;
                                            Debug.WriteLine("TX added");
                                        }
                                        else
                                        { Debug.WriteLine($"Failed to enable TX: {status}"); DoDisconnect(); }
                                    }
                                    else
                                    { Debug.WriteLine($"Failed to disable TX: {status}"); DoDisconnect(); }
                                }
                                else if (characteristic.Uuid == Guid.Parse("6E400002-B5A3-F393-E0A9-E50E24DCCA9E")) // RX UUID
                                {
                                    rxCharacteristic = characteristic;
                                    Debug.WriteLine("RX Characteristic found.");
                                }
                            }
                            if (rxCharacteristic == null || txCharacteristic == null || !txNotifyEnabled)
                            {
                                Debug.WriteLine("BLE setup incomplete (rx/tx/notify missing). Disconnecting.");
                                DoDisconnect();
                                return;
                            }

                            StartTxLoop();
                            Debug.WriteLine("TX loop started.");
                        }
                    }
                    if (nusService == null || txCharacteristic == null || rxCharacteristic == null)
                    {
                        Debug.WriteLine("Connection failed");
                        DoDisconnect();
                    }
                    else
                    {
                        Debug.WriteLine("TX Admin Mode");
                        labelTX.Text = "Admin Mode";
                        labelRX.Text = "";
                        Debug.WriteLine("Private Key File: " + currentDir + private_key_path);
                        byte[] authPayload = BuildAuthData(currentDir + private_key_path);
                        Sending(authPayload);

                        //Sending(AuthData); //dataIni1
                    }
                }
                else
                {
                    Debug.WriteLine("Failed to get GATT services");
                    DoDisconnect();
                }

                StartWorkers();
                Debug.WriteLine("Started workers");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
            }
        }
        private void Watcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            BluetoothLEAdvertisement adv = args.Advertisement;
            if (adv.ManufacturerData.Count >= 1)
            {
                foreach (BluetoothLEManufacturerData md in adv.ManufacturerData)
                {
                    if (md.CompanyId == 0xFFFF || md.CompanyId == 0xF0F0)
                    {
                        address = args.BluetoothAddress;
                        DataReader reader = DataReader.FromBuffer(md.Data);
                        reader.ReadBytes(bAdv);
                        if (md.CompanyId == 0xFFFF) ///DVT
                        { bAdv[23] = 1; }
                        if (md.CompanyId == 0xF0F0) ///SRS 
                        { bAdv[23] = 2; }
                        watcher.Stop();
                        break;
                    }
                }
            }
        }
        private void SendFromRemote(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var reader = DataReader.FromBuffer(args.CharacteristicValue);
            var packet = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(packet);

            if (packet.Length == 0) return;

            //Array.Resize(ref bResponse, (int)reader.UnconsumedBufferLength);
            //reader.ReadBytes(bResponse);

            //For debugging, can show full data packet. But, writing this much slows things down and could cause crashing
            //Debug.WriteLine("RAW: " + BitConverter.ToString(bResponse));

            bReceived = true;
            //Debug.WriteLine("Received BLE packet, bReceived=" + bReceived);

            //Attempt to add response to queue
            dataQueue.Enqueue(packet);
            dataEvent.Set();
            //Debug.WriteLine("Received Data");
        }
        private void StartWorkers()
        {
            bleWorkerCts = new CancellationTokenSource();
            var token = bleWorkerCts.Token;

            processingTask = Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    dataEvent.WaitOne();

                    while (dataQueue.TryDequeue(out var packet))
                    {
                        // Handle the packet in a separate function
                        ProcessIncomingPacket(packet);
                    }
                }
            }, token);

            writingTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    writeEvent.WaitOne();

                    while (writeQueue.TryDequeue(out var data))
                    {
                        if (useBluetooth)
                        {
                            if (rxCharacteristic != null)
                            {
                                try
                                {
                                    await rxCharacteristic.WriteValueAsync(data.AsBuffer());
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine("Write error: " + ex);
                                }
                            }
                        }
                        else
                        {
                            continue;
                            /*
                            try
                            {
                                SendUSB(data);
                            }
                            catch (Exception ex) { Debug.WriteLine("Worker USB Issue: " + ex); }
                        */
                        }
                    }
                }
            }, token);
        }
        private void ProcessIncomingPacket(byte[] bResponse)
        {
            switch (bResponse[0])
            {
                case START_MANUAL_THERAPY:
                    bStimOn.BeginInvoke((Action)(() => bStimOn.Enabled = false));
                    bStimOff.BeginInvoke((Action)(() => bStimOff.Enabled = true));
                    Debug.WriteLine("RX Stim On");
                    labelRX.BeginInvoke((Action)(() => labelRX.Text = "Stim On"));
                    break;
                case STOP_MANUAL_THERAPY:
                    bStimOn.BeginInvoke((Action)(() => bStimOn.Enabled = true));
                    bStimOff.BeginInvoke((Action)(() => bStimOff.Enabled = false));
                    Debug.WriteLine("RX Stim Off");
                    labelRX.BeginInvoke((Action)(() => labelRX.Text = "Stim Off"));
                    break;
                case GET_SENSOR:
                    if (bResponse[1] == 0xC9) // 201 bytes
                    {
                        int CurPacket = bResponse[3];
                        for (int i = 0; i < 100; i++)
                        {
                            //EMGdata[CurPacket * PacketLen + i] = Convert.ToDouble((ushort)((bResponse[i * 2 + 5] << 8) + bResponse[i * 2 + 4])); //V 
                            ushort raw = (ushort)((bResponse[i * 2 + 5] << 8) + bResponse[i * 2 + 4]);
                            EMGdata[CurPacket * PacketLen + i] = Convert.ToDouble(raw);

                            // ----------- CSV LOGGING HERE -----------
                            packetRaw[i] = raw;
                            bufferRaw[CurPacket * 100 + i] = raw;
                        }
                        if (CurPacket == PacketCnt - 1)
                        {
                            /*
                            if (CurrentSignalMode == SignalMode.ECGR)
                            {
                                ProcessECGR(EMGdata);
                            } else
                            {
                                ProcessPlotSave(EMGdata);
                            }
                            */

                            ProcessPlotSave(EMGdata);
                        }
                    }
                    else
                    {
                        //Debug.WriteLine($"RX {Convert.ToHexString(bResponse)}");
                        labelRX.BeginInvoke((Action)(() => labelRX.Text = "Stopped Data"));
                    }
                    break;
                case GET_HPARS: //Get hardware parameters
                    //TODO: different settings and buttons for Gen1 and Gen2
                    string HPar = "" + (char)(bResponse[5]) + (char)(bResponse[6]); //Different switch cases for different parameters. Right now, FW version will continue the handshake

                    //Debug.WriteLine("HPars_ID: " + HPar);
                    switch (HPar)
                    {
                        case "05": //HP05: Get FW Version
                            FWVersion = System.Text.Encoding.ASCII.GetString(bResponse, 7, 6);
                            PMode = 2; //Set MaxA = 5mA
                            labelTX.BeginInvoke((Action)(() => labelTX.Text = "Set MaxA"));
                            Debug.WriteLine("TX Set MaxA");
                            byte[] dataIni4;

                            if (FWVersion == "251226" || BoardGen == 2) //Gen2
                            {
                                dataIni4 = [SET_SPARS, 6, 0x53, 0x50, 0x31, 0x33, 50, 0x00];
                                //groupSineWave.Enabled = true;
                                labelRX.BeginInvoke((Action)(() => labelRX.Text = "FW2-" + FWVersion));
                                this.BeginInvoke((Action)(() => this.Text = this.Text + " FW2-" + FWVersion));
                                Debug.WriteLine("RX " + labelRX.Text);
                            }
                            else //Gen1
                            {
                                dataIni4 = [SET_SPARS, 6, 0x53, 0x50, 0x31, 0x31, 50, 0x00];
                                labelRX.BeginInvoke((Action)(() => labelRX.Text = "FW1-" + FWVersion));
                                this.BeginInvoke((Action)(() => this.Text = this.Text + " FW2-" + FWVersion));
                                Debug.WriteLine("RX " + labelRX.Text);
                            }
                            Sending(dataIni4);

                            /*
                            labelRX.BeginInvoke((Action)(() => labelRX.Text = "FW-" + System.Text.Encoding.ASCII.GetString(bResponse, 7, 6)));
                            this.BeginInvoke((Action)(() => this.Text = this.Text + " " + labelRX.Text));
                            Debug.WriteLine("RX " + labelRX.Text);
                            Debug.WriteLine("TX Connected");
                            labelTX.BeginInvoke((Action)(() => labelTX.Text = "Connected"));
                            byte[] dataIni5 = [GET_BLEPARS, 0x00];
                            Sending(dataIni5);
                            */
                            break;
                        default:
                            break;
                    }
                    break;
                case GET_SPARS: //Get Stimulation Parameters
                    if (bResponse[5] == 0x30)
                    {
                        switch (bResponse[6])
                        {
                            case 0x32: // Get PA
                                txtPA.BeginInvoke((Action)(() => txtPA.Text = (Math.Round((((int)bResponse[8] * 256 + (int)bResponse[7]) * 0.1), 1)).ToString()));
                                stimdata[0] = txtPA.Text;
                                if (PMode == 9)
                                {
                                    byte[] data3 = [GET_SPARS, 0x04, 0x53, 0x50, 0x30, 0x33];
                                    Sending(data3);
                                }
                                else
                                {
                                    File.WriteAllLines("stim.txt", stimdata);
                                    Debug.WriteLine("RX Get Params");
                                    labelRX.BeginInvoke((Action)(() => labelRX.Text = "Get Params"));
                                }
                                break;
                            case 0x33: // Get PW
                                txtPW.BeginInvoke((Action)(() => txtPW.Text = (((int)bResponse[8] * 256 + (int)bResponse[7]) * 50).ToString()));
                                stimdata[1] = txtPW.Text;
                                if (PMode == 9)
                                {
                                    byte[] data4 = [GET_SPARS, 0x04, 0x53, 0x50, 0x30, 0x34];
                                    Sending(data4);
                                }
                                else
                                {
                                    File.WriteAllLines("stim.txt", stimdata);
                                    Debug.WriteLine("RX Get Params");
                                    labelRX.BeginInvoke((Action)(() => labelRX.Text = "Get Params"));
                                }
                                break;
                            case 0x34: // Get PF
                                txtPF.BeginInvoke((Action)(() => txtPF.Text = ((int)bResponse[8] * 256 + (int)bResponse[7]).ToString()));
                                stimdata[2] = txtPF.Text;
                                if (PMode == 9)
                                {
                                    byte[] data5 = [GET_SPARS, 0x04, 0x53, 0x50, 0x30, 0x35];
                                    Sending(data5);
                                }
                                else
                                {
                                    File.WriteAllLines("stim.txt", stimdata);
                                    Debug.WriteLine("RX Get Params");
                                    labelRX.BeginInvoke((Action)(() => labelRX.Text = "Get Params"));
                                }
                                break;
                            case 0x35: // Get PR 
                                txtPR.BeginInvoke((Action)(() => txtPR.Text = ((int)bResponse[8] * 256 + (int)bResponse[7]).ToString()));
                                stimdata[3] = txtPR.Text;
                                if (PMode == 9)
                                {
                                    byte[] data7 = [GET_SPARS, 0x04, 0x53, 0x50, 0x30, 0x37];
                                    Sending(data7);
                                }
                                else
                                {
                                    File.WriteAllLines("stim.txt", stimdata);
                                    Debug.WriteLine("RX Get Params");
                                    labelRX.BeginInvoke((Action)(() => labelRX.Text = "Get Params"));
                                }

                                break;
                            case 0x37: // Get TN
                                txtTN.BeginInvoke((Action)(() => txtTN.Text = (((int)bResponse[8] * 256 + (int)bResponse[7]) * 10).ToString()));
                                stimdata[4] = txtTN.Text;
                                if (PMode == 9)
                                {
                                    byte[] data8 = [GET_SPARS, 0x04, 0x53, 0x50, 0x30, 0x38];
                                    Sending(data8);
                                }
                                else
                                {
                                    File.WriteAllLines("stim.txt", stimdata);
                                    Debug.WriteLine("RX Get Params");
                                    labelRX.BeginInvoke((Action)(() => labelRX.Text = "Get Params"));
                                }
                                break;
                            case 0x38: // Get TF
                                txtTF.BeginInvoke((Action)(() => txtTF.Text = (((int)bResponse[8] * 256 + (int)bResponse[7]) * 10).ToString()));
                                stimdata[5] = txtTF.Text;
                                if (PMode == 9)
                                {
                                    byte[] data9 = [GET_SPARS, 0x04, 0x53, 0x50, 0x30, 0x39];
                                    Sending(data9);
                                }
                                else
                                {
                                    File.WriteAllLines("stim.txt", stimdata);
                                    Debug.WriteLine("RX Get Params");
                                    labelRX.BeginInvoke((Action)(() => labelRX.Text = "Get Params"));
                                }
                                break;
                            case 0x39: // Get C1
                                txtC1.BeginInvoke((Action)(() => txtC1.Text = ((int)bResponse[8] * 256 + (int)bResponse[7]).ToString()));
                                stimdata[6] = txtC1.Text;
                                if (PMode == 9)
                                {
                                    byte[] data10 = [GET_SPARS, 0x04, 0x53, 0x50, 0x31, 0x30];
                                    Sending(data10);
                                }
                                else
                                {
                                    File.WriteAllLines("stim.txt", stimdata);
                                    Debug.WriteLine("RX Get Params");
                                    labelRX.BeginInvoke((Action)(() => labelRX.Text = "Get Params"));
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    else if (bResponse[5] == 0x31)
                    {
                        switch (bResponse[6])
                        {
                            case 0x30: // Get C2 - stim
                                txtC2.BeginInvoke((Action)(() => txtC2.Text = ((int)bResponse[8] * 256 + (int)bResponse[7]).ToString()));
                                stimdata[7] = txtC2.Text;
                                File.WriteAllLines("stim.txt", stimdata);
                                Debug.WriteLine("RX Get Params");
                                labelRX.BeginInvoke((Action)(() => labelRX.Text = "Get Params"));
                                break;
                            case 0x35: // Get VNB Amplitude
                                Debug.WriteLine("VNB Amp: " + (((int)bResponse[8] * 256 + (int)bResponse[7] * 0.1).ToString()));

                                txtVAmp.BeginInvoke((Action)(() => txtVAmp.Text = (Math.Round((((int)bResponse[8] * 256 + (int)bResponse[7]) * 0.1), 1)).ToString()));
                                /*
                                stimdata[0] = txtPA.Text;
                                if (PMode == 9)
                                {
                                    byte[] data3 = [GET_SPARS, 0x04, 0x53, 0x50, 0x30, 0x33];
                                    Sending(data3);
                                }
                                else
                                {
                                    File.WriteAllLines("stim.txt", stimdata);
                                    Debug.WriteLine("RX Get Params");
                                    labelRX.BeginInvoke((Action)(() => labelRX.Text = "Get Params"));
                                }
                                */
                                break;
                            case 0x37: // Get VNB frequency
                                Debug.WriteLine("VNB Frequency: " + (((int)bResponse[8] * 256 + (int)bResponse[7]) * 0.1).ToString());
                                txtVfreq.BeginInvoke((Action)(() => txtVfreq.Text = (Math.Round((((int)bResponse[8] * 256 + (int)bResponse[7]) * 0.1), 1)).ToString()));
                              
                                break;
                            case 0x38: // Get VNB on time
                                Debug.WriteLine("VNB On Time: " + (((int)bResponse[8] * 256 + (int)bResponse[7])).ToString());
                                txtVon.BeginInvoke((Action)(() => txtVon.Text = (Math.Round((((int)bResponse[8] * 256.0 + (int)bResponse[7])), 1)).ToString()));
                               
                                break;
                            case 0x39: // Get VNB off time

                                Debug.WriteLine("VNB Off Time: " + (((int)bResponse[8] * 256 + (int)bResponse[7])).ToString());
                                txtVoff.BeginInvoke((Action)(() => txtVoff.Text = (Math.Round((((int)bResponse[8] * 256.0 + (int)bResponse[7])), 1)).ToString()));
                                 break;
                            case 0x31: // Get VNB cathode
                                txtV1.BeginInvoke((Action)(() => txtV1.Text = ((int)bResponse[8] * 256 + (int)bResponse[7]).ToString()));
                                
                                break;
                            case 0x32: // Get VNB anode
                                txtV2.BeginInvoke((Action)(() => txtV2.Text = ((int)bResponse[8] * 256 + (int)bResponse[7]).ToString()));
                               
                                break;
                            case 0x33:
                                Debug.WriteLine("Stim Max Safe Amp: " + (((int)bResponse[8] * 256 + (int)bResponse[7] * 0.1).ToString()));
                                break;
                            case 0x34:
                                Debug.WriteLine("Min Safe Impedance: " + ((ushort)(bResponse[4] << 8) + (double)bResponse[3]).ToString());
                                break;
                            case 0x36:
                                Debug.WriteLine("VNB Max Safe Amp: " + (((int)bResponse[8] * 256 + (int)bResponse[7] * 2.0).ToString()));
                                txtMaxVNB.BeginInvoke((Action)(() => txtMaxVNB.Text = (Math.Round((((int)bResponse[8] * 256 + (int)bResponse[7]) * 2.0), 1)).ToString()));
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case SET_SPARS: //Set params 
                    switch (PMode)
                    {
                        case 0:
                            Debug.WriteLine("RX Set Params");
                            labelRX.BeginInvoke((Action)(() => labelRX.Text = "Set Params"));
                            break;
                        case 1: // set RampDown
                            PMode = 0;
                            byte st = (byte)Int16.Parse(txtPR.Text);
                            byte[] data12 = [SET_SPARS, 0x06, 0x53, 0x50, 0x30, 0x36, st, 0x00];
                            Sending(data12);
                            break;
                        case 2: // set MaxA
                            PMode = 0;
                            Debug.WriteLine("RX Set MaxA");
                            labelRX.BeginInvoke((Action)(() => labelRX.Text = "Set MaxA"));
                            bDisconnect.BeginInvoke((Action)(() => bDisconnect.Enabled = true));
                            bGet.BeginInvoke((Action)(() => bGet.Enabled = true));
                            bSendC1.BeginInvoke((Action)(() => bSendC1.Enabled = true));
                            bSendC2.BeginInvoke((Action)(() => bSendC2.Enabled = true));
                            bSendPA.BeginInvoke((Action)(() => bSendPA.Enabled = true));
                            bSendPW.BeginInvoke((Action)(() => bSendPW.Enabled = true));
                            bSendPF.BeginInvoke((Action)(() => bSendPF.Enabled = true));
                            bSendPR.BeginInvoke((Action)(() => bSendPR.Enabled = true));
                            bSendTN.BeginInvoke((Action)(() => bSendTN.Enabled = true));
                            bSendTF.BeginInvoke((Action)(() => bSendTF.Enabled = true));
                            bStimOn.BeginInvoke((Action)(() => bStimOn.Enabled = true));
                            bStimOff.BeginInvoke((Action)(() => bStimOff.Enabled = false));
                            bGetC1.BeginInvoke((Action)(() => bGetC1.Enabled = true));
                            bGetC2.BeginInvoke((Action)(() => bGetC2.Enabled = true));
                            bGetPA.BeginInvoke((Action)(() => bGetPA.Enabled = true));
                            bGetPW.BeginInvoke((Action)(() => bGetPW.Enabled = true));
                            bGetPF.BeginInvoke((Action)(() => bGetPF.Enabled = true));
                            bGetPR.BeginInvoke((Action)(() => bGetPR.Enabled = true));
                            bGetTN.BeginInvoke((Action)(() => bGetTN.Enabled = true));
                            bGetTF.BeginInvoke((Action)(() => bGetTF.Enabled = true));
                            bView.BeginInvoke((Action)(() => bView.Enabled = true));
                            bSave.BeginInvoke((Action)(() => bSave.Enabled = true));
                            Debug.WriteLine("TX BLE Pars");
                            labelTX.BeginInvoke((Action)(() => labelTX.Text = "BLE Pars"));
                            //byte[] dataIni4 = [GET_HPARS, 0x04, 0x48, 0x50, 0x30, 0x35]; // Get FW Version
                            byte[] dataIni5 = [GET_BLEPARS, 0x00];
                            Sending(dataIni5);
                            break;
                        case 3: //Measuring impedance
                            PMode = 0;
                            Debug.WriteLine("RX Set Max Amp for Impedance");
                            labelRX.BeginInvoke((Action)(() => labelRX.Text = "Set MaxA"));
                            
                            byte[] dataZ = [MEASURE_IMPEDANCE, 0x00]; // Get Impedance
                            Debug.WriteLine("TX Get Impedance");
                            labelTX.BeginInvoke((Action)(() => labelTX.Text = "Get Impedance"));
                            Sending(dataZ);
                            break;
                        default:
                            break;
                    }
                    break;
                case AUTH: //Admin mode
                    if (bResponse[3] == 0xFF)
                    {
                        Debug.WriteLine("RX Admin Mode");
                        Debug.WriteLine("TX No whitelist");
                        labelRX.BeginInvoke((Action)(() => labelRX.Text = "Admin Mode"));
                        labelTX.BeginInvoke((Action)(() => labelTX.Text = "No whitelist"));
                        byte[] dataIni2 = [SET_BLEPARS, 0x07, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x00]; // Disable whitelist
                        Sending(dataIni2);
                    }
                    else
                    { Debug.WriteLine("RX NO Admin Mode"); }
                    break;
                case GET_BLEPARS:

                    bConnected = true;
                    //Debug.WriteLine("Got BLEPARS, bConnected=" + bConnected);
                    if (BLEtimer.Enabled)
                    { labelRX.BeginInvoke((Action)(() => labelRX.Text = "Connected+")); }
                    else
                    {
                        labelRX.BeginInvoke((Action)(() => labelRX.Text = "Connected")); Debug.WriteLine("RX Connected");
                        BLEtimer.Interval = 30000; // in milliseconds
                        BLEtimer.Elapsed += BLEtimer_Elapsed;
                        BLEtimer.AutoReset = true; // retrigger 
                        BLEtimer.Enabled = true;
                    }
                    break;
                case SET_BLEPARS: //Disable whitelist
                    Debug.WriteLine("TX FW Version");
                    labelTX.BeginInvoke((Action)(() => labelTX.Text = "FW Version"));
                    byte[] dataIni3 = [GET_HPARS, 0x04, 0x48, 0x50, 0x30, 0x35]; //Get FW Version
                    /*
                    Debug.WriteLine("RX No whitelist");
                    labelRX.BeginInvoke((Action)(() => labelRX.Text = "No whitelist"));
                    PMode = 2; // set MaxA
                    labelTX.BeginInvoke((Action)(() => labelTX.Text = "Set MaxA"));
                    Debug.WriteLine("TX Set MaxA");
                    byte[] dataIni3 = [SET_SPARS, 0x06, 0x53, 0x50, 0x31, 0x31, 50, 0x00]; //Set MaxA = 5mA
                    */
                    Sending(dataIni3);
                    break;
                case MEASURE_IMPEDANCE:
                    //Process impedance value
                    ushort zraw = (ushort)((bResponse[4] << 8) + bResponse[3]);
                    currentZ = Convert.ToDouble(zraw);

                    //Write impedance to UI
                    Debug.WriteLine("Impedance received: " + currentZ);
                    labelRX.BeginInvoke((Action)(() => labelRX.Text = "Impedance Received"));
                    labelZ.BeginInvoke((Action)(() => labelZ.Text = "" + currentZ));

                    //Set impedance button to not be grey anymore
                    getImp.BeginInvoke((Action)(() => getImp.Enabled = true));

                    //Send command to set max amplitude back to 5mA
                    byte[] data = [SET_SPARS, 0x06, 0x53, 0x50, 0x31, 0x33, 50, 0x00];
                    Debug.WriteLine("TX Set Max Amp");
                    Sending(data);

                    break;
                default:
                    break;
            }
        }
        public async void SendToRemote(byte[] data)
        {
            if (rxCharacteristic != null)
            {
                try
                {
                    var writer = new DataWriter();
                    writer.WriteBytes(data);
                    var status = await rxCharacteristic.WriteValueAsync(writer.DetachBuffer(), GattWriteOption.WriteWithoutResponse);
                    if (status != GattCommunicationStatus.Success)
                    {
                        Debug.WriteLine($"TX failed: {status}");
                        // Retry once
                        await Task.Delay(100);
                        status = await rxCharacteristic.WriteValueAsync(writer.DetachBuffer(), GattWriteOption.WriteWithoutResponse);
                        Debug.WriteLine($"TX retry status: {status}");

                        if (status != GattCommunicationStatus.Success)
                            DoDisconnect();   // or attempt reconnect?
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"SendToRemote Error {ex.Message}"); Application.Exit();
                }
            }
        }
        public void QueueSendToRemote(byte[] data)
        {
            if (data == null || data.Length == 0) return;
            _txChannel.Writer.TryWrite(data);  // returns false only if completed
        }
        private async Task TxLoopAsync(CancellationToken ct)
        {
            try
            {
                while (await _txChannel.Reader.WaitToReadAsync(ct))
                {
                    while (_txChannel.Reader.TryRead(out var payload))
                    {
                        if (ct.IsCancellationRequested) return;

                        // If disconnected/invalidated, you can optionally drop or buffer
                        var ch = rxCharacteristic;
                        if (ch == null)
                            continue;

                        var status = await WriteOnceWithRetryAsync(ch, payload, ct);

                        if (status != GattCommunicationStatus.Success)
                        {
                            Debug.WriteLine($"TX permanently failed: {status}");

                            // For "Unreachable", treat as likely disconnect
                            if (status == GattCommunicationStatus.Unreachable)
                            {
                                // IMPORTANT: don't call Application.Exit()
                                // Prefer: trigger your disconnect/reconnect workflow
                                DoDisconnect();
                                return;
                            }

                            // For other statuses, either keep going or disconnect based on your preference
                            // DoDisconnect();
                        }
                    }
                }
            }
            catch (OperationCanceledException) { Debug.WriteLine("Shutting down"); }
        }
        private static async Task<GattCommunicationStatus> WriteOnceWithRetryAsync(
            GattCharacteristic ch,
            byte[] payload,
            CancellationToken ct)
        {
            // 1st attempt
            var status = await WriteAsync(ch, payload, ct);
            if (status == GattCommunicationStatus.Success)
                return status;

            Debug.WriteLine($"TX failed: {status}");

            // Retry once (or twice) with backoff
            await Task.Delay(100, ct);
            status = await WriteAsync(ch, payload, ct);
            Debug.WriteLine($"TX retry status: {status}");

            return status;
        }

        private static async Task<GattCommunicationStatus> WriteAsync(
            GattCharacteristic ch,
            byte[] payload,
            CancellationToken ct)
        {
            // DataWriter per call (cheap enough, avoids state issues)
            var writer = new DataWriter();
            writer.WriteBytes(payload);
            var buffer = writer.DetachBuffer();

            // ct can't be passed directly to WriteValueAsync, but we can at least honor it before/after
            if (ct.IsCancellationRequested) return GattCommunicationStatus.Unreachable;

            return await ch.WriteValueAsync(buffer, GattWriteOption.WriteWithoutResponse);
        }


        public async void DoDisconnect()
        {
            Debug.WriteLine("Disconnecting.");

            BLEtimer?.Stop();

            await StopTxLoopAsync();    //Stops write queue (I think)

            if (txCharacteristic != null)
            {
                txCharacteristic.ValueChanged -= SendFromRemote;
            }

            //Unpair BLE device
            if (BLEDevice != null)
            {
                try
                {
                    if (BLEDevice.DeviceInformation.Pairing.IsPaired)
                    {
                        DeviceUnpairingResult dupr = await BLEDevice.DeviceInformation.Pairing.UnpairAsync();
                        while (dupr.Status != DeviceUnpairingResultStatus.Unpaired)
                        { Application.DoEvents(); }
                        Debug.WriteLine("Unpaired BLE");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error during unpairing: {ex.Message}");
                }
                bleWorkerCts?.Cancel();
                nusService?.Dispose();
                nusService = null;
                txCharacteristic = null;
                rxCharacteristic = null;
                BLEDevice?.Dispose();
                BLEDevice = null;
            }

            //Close COM port
            if (port != null && port.IsOpen)
            {
                port.Close();
                port.Dispose();
                Debug.WriteLine("Closed COM ports");
            }

            Application.Exit();

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Directory.SetCurrentDirectory(currentDir);
            formsPlot1.Plot.Axes.SetLimitsX(0, (int)((10000 / DecValue - 9) * DECPeriod)); //msec
            //formsPlot1.Plot.Axes.SetLimitsX(0, FFTsize); //Hz
            formsPlot1.Plot.Axes.SetLimitsY(-40, 40);

            if (useBluetooth)
            {
                //BLE connection
                try
                {
                    watcher = new BluetoothLEAdvertisementWatcher
                    { ScanningMode = BluetoothLEScanningMode.Active };
                    watcher.Received += Watcher_Received;
                    watcher.Start();
                    while (watcher.Status != BluetoothLEAdvertisementWatcherStatus.Stopped)
                    { Application.DoEvents(); }
                    watcher.Received -= Watcher_Received;
                    _ = ConnectAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error: {ex.Message}"); Application.Exit();
                }
            }
            else
            {
                //Serial port for USB connection
                string[] ports = SerialPort.GetPortNames();
                Debug.WriteLine("Ports: " + ports);
                if (ports.Length > 0 && ports[0] != "")
                {
                    port = new SerialPort(ports[0], 115200, Parity.None, 8, StopBits.One);
                    port.DataReceived += OnUSBRX;
                    port.Open();
                    Debug.WriteLine("Serial port open: " + port.ToString());
                    Debug.WriteLine("TX Admin Mode");
                    labelTX.Text = "Admin Mode";
                    labelRX.Text = "";
                    byte[] authPayload = BuildAuthData(currentDir + private_key_path);
                    Sending(authPayload);
                    //Sending(AuthData); //dataIni1

                    StartWorkers();
                    Debug.WriteLine("Started workers");
                }
                else { Debug.WriteLine("No serial ports found!"); DoDisconnect(); }
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Debug.WriteLine("Closing Form");
            DoDisconnect();
        }

        private void txtFname_TextChanged(object sender, EventArgs e)
        {

        }
        private void SendUSB(byte[] data)
        {
            //Do we need to discard buffer? Question for Victor
            port.DiscardInBuffer();
            port.DiscardOutBuffer();

            port.Write(data, 0, data.Length);
            Debug.WriteLine("USB TX: " + BitConverter.ToString(data));
        }

        private void OnUSBRX(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                //New USB code with buffer
                int count = port.BytesToRead;
                byte[] incoming = new byte[count];
                port.Read(incoming, 0, count);

                Debug.WriteLine("USB RX: " + BitConverter.ToString(incoming));

                lock (usbRxBuffer)
                {
                    usbRxBuffer.AddRange(incoming);
                    //Debug.WriteLine("USB Buffer: " + System.String.Join("-", usbRxBuffer));

                    while (true)
                    {
                        Debug.WriteLine("USB Buffer: " + System.String.Join("-", usbRxBuffer));

                        //Check if buffer is too short -> return and wait for more data
                        if (usbRxBuffer.Count < 4)
                        {
                            Debug.WriteLine("Buffer too short: " + usbRxBuffer.Count + " bits.");
                            return;
                        }

                        byte opcode = usbRxBuffer[0];
                        int payloadLen = usbRxBuffer[1];
                        int expectedPacketLen = 1 + 2 + payloadLen + 2;

                        //check if first byte is zero -> remove first byte and try again
                        if (usbRxBuffer[0] == 0x00)
                        {
                            Debug.WriteLine("First bit null.");
                            usbRxBuffer.RemoveAt(0);
                            continue;
                        }

                        //Check if first byte is not an opcode -> remove first byte and try again
                        if (!ValidOpcode(opcode))
                        {
                            Debug.WriteLine("Not a valid opcode.");
                            usbRxBuffer.RemoveAt(0);
                            continue;
                        }

                        //Check if buffer too large -> remove first byte and try again
                        if (usbRxBuffer.Count > 201)
                        {
                            Debug.WriteLine("Buffer too large.");
                            usbRxBuffer.RemoveAt(0);
                            continue;
                        }

                        //Check if packet length matches -> return and wait for more data
                        if (usbRxBuffer.Count < expectedPacketLen)
                        {
                            Debug.WriteLine("Received data too short.Expected: " + expectedPacketLen + " Counted: " + usbRxBuffer.Count);
                            return;
                        }

                        byte[] packet = usbRxBuffer.GetRange(0, expectedPacketLen).ToArray();

                        /*
                        //Check CRC is valid -> return and wait for more data (or flush and wait for another command? Maybe do this by removing first bit and cycling through?)
                        if (!ValidateCRC(packet))
                        {
                            Debug.WriteLine("USB RX CRC ERROR");
                            usbRxBuffer.RemoveAt(0);
                            continue;
                        }
                        */

                        usbRxBuffer.RemoveRange(0, expectedPacketLen);
                        dataQueue.Enqueue(packet);
                        dataEvent.Set();

                        Debug.WriteLine("USB RX sent to dataQueue: " + BitConverter.ToString(packet));
                    }
                }
            }
            catch (Exception ex)
            { Console.WriteLine($"Error in async data reception: {ex.Message}"); }
        }

        //Does not work with extra 0x00 in received packets
        //TODO need to fix
        private bool ValidateCRC(byte[] packet)
        {
            if (packet == null || packet.Length < 4)
            {
                Debug.WriteLine("CRC Check - Packet too short");
                return false; // too small to contain opcode,len,crc
            }

            int payloadLen = packet[1];
            int expectedLength = 1 + 1 + payloadLen + 2;

            if (packet.Length != expectedLength)
            {
                Debug.WriteLine("CRC Check - Packet Wrong Length");
                return false; // malformed or truncated
            }

            // Compute CRC over all bytes except last two
            const ushort polynomial = 0x1021;
            ushort crc = 0xFFFF;

            for (int i = 0; i < packet.Length - 2; i++)
            {
                crc ^= (ushort)(packet[i] << 8);

                for (int bit = 0; bit < 8; bit++)
                {
                    if ((crc & 0x8000) != 0)
                        crc = (ushort)((crc << 1) ^ polynomial);
                    else
                        crc <<= 1;
                }
            }

            // Extract CRC from packet (little-endian)
            ushort crcPacket = (ushort)(packet[packet.Length - 2] |
                                       (packet[packet.Length - 1] << 8));

            return crc == crcPacket;
        }

        private bool ValidOpcode(byte op)
        {
            return op == AUTH ||
                op == GET_HPARS ||
                op == SET_SPARS ||
                op == START_MANUAL_THERAPY ||
                op == STOP_MANUAL_THERAPY ||
                op == GET_SENSOR ||
                op == GET_SPARS ||
                op == SET_BLEPARS;
        }

        private void rbSignalMode_CheckedChange(object sender, EventArgs e)
        {
            if (!(sender is RadioButton rb) || !rb.Checked) return;

            if (rb == rbEMG1)
            {
                CurrentSignalMode = SignalMode.ECGH;

                //Change constants for analyzing signal
                sampleRate = 256; // Hz
                DecValue = 1;
                DECPeriod = (int)(1000 * DecValue / sampleRate); // msec
            }
            else if (rb == rbEMG2)
            {
                CurrentSignalMode = SignalMode.ECGR;

                //Change constants for analyzing signal
                sampleRate = 256; // Hz
                DecValue = 38;
                FFTsize = 256; //10000/DecValue = 263
                inMaxValue = 3; //mV
                outMaxValue = 4; //mV
                DECPeriod = (int)(1000 * DecValue / sampleRate); // msec
            }
            else if (rb == rbEMG3)
            {
                CurrentSignalMode = SignalMode.EMG1;

                //Change constants for analyzing signal
                sampleRate = 6400; // Hz
                DecValue = 10;
                DECPeriod = (int)(1000 * DecValue / sampleRate);
            }
            else if (rb == rbEMG4)
            {
                CurrentSignalMode = SignalMode.EMG2;

                //Change constants for analyzing signal
                sampleRate = 6400; // Hz
                DecValue = 10;
                DECPeriod = (int)(1000 * DecValue / sampleRate);
            }

            Debug.WriteLine($"Signal mode changed to {CurrentSignalMode}");
        }

        private void ProcessPlotSave(double[] raw)
        {
            if (raw == null || raw.Length < 10)
            {
                Debug.WriteLine("Signal packet too short!");
                return;
            }

            // DecValue is only for making plotting faster in this function's implementation (only plot every N values)
            // plotDec = 1 is to plot every value
            int plotDec = Math.Max(1, DecValue);
            double fsPlot = sampleRate / plotDec;
            double dtPlot = 1.0 / fsPlot;

            // Copy + remove DC (simple mean subtract)
            double mean = raw.Average();
            double[] x = raw.Select(v => v - mean).ToArray();

            // Filter cutoffs based on signal type
            double hpHz = 1.0;
            double desiredLpHz = 100.0;

            switch (CurrentSignalMode)
            {
                case SignalMode.ECGH:
                    hpHz = 0.5;
                    desiredLpHz = 40.0;
                    break;
                case SignalMode.ECGR:
                    hpHz = 0.01;
                    desiredLpHz = 20;
                    break;
                case SignalMode.EMG1:   //For now, assuming this is skeletal muscle EMG / spike bursts from smooth muscles
                    hpHz = 20.0;
                    desiredLpHz = 500.0;
                    break;
                case SignalMode.EMG2:   //For now, assuming this is eCAPs for testing purposes
                    hpHz = 300.0;
                    desiredLpHz = 3000.0;
                    break;
                default:
                    break;
            }

            //If LPF max isn't achievable based on sampling frequency, adjust
            double maxSafeLpHz = 0.45 * (fsPlot / 2.0) * 2.0;
            double lpHz = Math.Min(desiredLpHz, maxSafeLpHz);
            //Debug.WriteLine("LPF: " + lpHz);

            //TODO these are commented out for hardware testing, can add back in or change filtering parameters for actual use
            //x = ApplyHighPassBiquad(x, sampleRate, hpHz);
            //x = ApplyLowPassBiquad(x, sampleRate, lpHz);

            // 60Hz notch filter
            //x = ApplyNotchBiquad(x, sampleRate, 60.0, q: 30.0);

            // Downsample plotting based on decimation factor
            double[] yPlot = (plotDec == 1) ? x : DownsampleByPicking(x, plotDec);
            formsPlot1.BeginInvoke((Action)(() =>
            {
                formsPlot1.Plot.Clear();
                formsPlot1.Plot.Add.Signal(yPlot, dtPlot);
                // Optional: axes labels
                formsPlot1.Plot.XLabel("Time (s)");
                formsPlot1.Plot.YLabel("ECG (a.u.)");
                formsPlot1.Plot.Axes.AutoScale();
                formsPlot1.Refresh();
            }));

            //Save data to CSV
            if (isSaving && csvWriter != null)
            {
                long ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                StringBuilder sb = new StringBuilder(400000); // preallocate for speed

                //Raw data
                sb.Append(ts);
                for (int i = 0; i < bufferRaw.Length; i++)
                    sb.Append($",{bufferRaw[i]}");
                sb.AppendLine();

                //Filtered data
                sb.Append(ts);
                int decLen = x.Length;
                for (int i = 0; i < decLen; i++)
                    sb.Append($"," + x[i].ToString("F6"));
                sb.AppendLine();

                lock (csvLock)
                {
                    csvWriter.Write(sb.ToString());
                    csvWriter.Flush();
                }
            }
        }

        private void ProcessECGR(double[] EMGdataP)
        {
            //Data processing
            double[] DECdata = new double[10000 / DecValue]; //10,000
            var subset = EMGdataP.Take(DecValue).ToArray();
            double avg = subset.Average();

            for (int i = 0; i < DecValue; i++)
            { if (subset[i] > avg + inMaxValue || subset[i] < avg - inMaxValue) { subset[i] = avg; } }

            DECdata[0] = subset.Average(); //0
            subset = EMGdataP.Skip(10000 - DecValue).Take(DecValue).ToArray();
            avg = subset.Average();

            for (int i = 0; i < DecValue; i++)
            { if (subset[i] > avg + inMaxValue || subset[i] < avg - inMaxValue) { subset[i] = avg; } }

            DECdata[10000 / DecValue - 1] = subset.Average(); //last

            for (int st = 1; st < 10000 / DecValue - 1; st++)
            {
                subset = EMGdataP.Skip(DecValue * (st - 1)).Take(DecValue * 2).ToArray();
                avg = subset.Average();
                for (int i = 0; i < DecValue * 2; i++)
                { if (subset[i] > avg + inMaxValue || subset[i] < avg - inMaxValue) { subset[i] = avg; } }
                DECdata[st] = subset.Average(); //all others
            }

            for (int st = 4; st < 10000 / DecValue - 4; st++)
            {
                avg = (DECdata[st - 4] + DECdata[st - 3] + DECdata[st - 2] + DECdata[st - 1] + DECdata[st + 1] + DECdata[st + 2] + DECdata[st + 3] + DECdata[st + 4]) * 0.125;
                if (DECdata[st] > avg + outMaxValue || DECdata[st] < avg - outMaxValue)
                { DECdata[st] = avg; } // remove spikes
            }
            DECdata[0] = DECdata[4]; DECdata[1] = DECdata[4]; DECdata[2] = DECdata[4]; DECdata[3] = DECdata[4];
            DECdata[10000 / DecValue - 1] = DECdata[10000 / DecValue - 5];
            DECdata[10000 / DecValue - 2] = DECdata[10000 / DecValue - 5];
            DECdata[10000 / DecValue - 3] = DECdata[10000 / DecValue - 5];
            DECdata[10000 / DecValue - 4] = DECdata[10000 / DecValue - 5];
            avg = DECdata.Average();
            //double square = 0;

            for (int i = 0; i < 10000 / DecValue - 4; i++)
            { DECdata[i] = DECdata[i + 4] - avg; }  //square += Math.Pow(DECdata[i], 2);
            double tail = DECdata[10000 / DecValue - 5];
            for (int i = 10000 / DecValue - 4; i < 10000 / DecValue; i++) { DECdata[i] = tail; }
            formsPlot1.Plot.Clear();
            formsPlot1.BeginInvoke((Action)(() => formsPlot1.Plot.Add.Signal(DECdata, DECPeriod))); //subset, 1
                                                                                                    //formsPlot1.BeginInvoke((Action)(() => formsPlot1.Plot.Axes.AutoScaleY()));
            formsPlot1.BeginInvoke((Action)(() => { formsPlot1.Plot.Axes.AutoScale(); formsPlot1.Refresh(); }));
            subset = DECdata.Take(FFTsize + 2).ToArray();
            Fourier.ForwardReal(subset, FFTsize);

            for (int i = 0; i < FFTsize + 2; i++)
            { subset[i] = Math.Abs(subset[i]); }
            Debug.WriteLine("RX Getting Data");
            labelRX.BeginInvoke((Action)(() => labelRX.Text = "Getting Data"));
            int maxIndex = subset.ToList().IndexOf(subset.Max()); // 0.1 * 0.01 * FFTsize * DECPeriod *

            if (maxIndex < 100)
            { labelV.BeginInvoke((Action)(() => labelV.Text = (maxIndex * 0.1).ToString("F1"))); }
            //labelV.BeginInvoke((Action)(() => labelV.Text = ((int)Math.Sqrt(square*DecValue/10000)).ToString())));

            //Save data to CSV
            if (isSaving && csvWriter != null)
            {
                long ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                StringBuilder sb = new StringBuilder(400000); // preallocate for speed

                //Raw data
                sb.Append(ts);
                for (int i = 0; i < bufferRaw.Length; i++)
                    sb.Append($",{bufferRaw[i]}");
                sb.AppendLine();

                //Processed data
                sb.Append(ts);
                int decLen = DECdata.Length;
                for (int i = 0; i < decLen; i++)
                    sb.Append($"," + DECdata[i].ToString("F6"));
                sb.AppendLine();

                //FFTs
                int fftLen = FFTsize + 2;

                sb.Append(ts);
                for (int i = 0; i < fftLen; i++)
                    sb.Append($"," + subset[i].ToString("F6"));
                sb.AppendLine();

                lock (csvLock)
                {
                    csvWriter.Write(sb.ToString());
                    csvWriter.Flush();
                }
            }
        }

        //Helper functions
        private double[] DownsampleByPicking(double[] x, int dec)
        {
            int n = (x.Length + dec - 1) / dec;
            double[] y = new double[n];
            int j = 0;
            for (int i = 0; i < x.Length; i += dec)
                y[j++] = x[i];
            return y;
        }

        private double[] ApplyLowPassBiquad(double[] x, double fs, double fc, double q = 0.7071)
        {
            double w0 = 2 * Math.PI * fc / fs;
            double cosw0 = Math.Cos(w0);
            double sinw0 = Math.Sin(w0);
            double alpha = sinw0 / (2 * q);

            double b0 = (1 - cosw0) / 2;
            double b1 = 1 - cosw0;
            double b2 = (1 - cosw0) / 2;
            double a0 = 1 + alpha;
            double a1 = -2 * cosw0;
            double a2 = 1 - alpha;

            return ApplyBiquad(x, b0 / a0, b1 / a0, b2 / a0, a1 / a0, a2 / a0);
        }

        private double[] ApplyHighPassBiquad(double[] x, double fs, double fc, double q = 0.7071)
        {
            double w0 = 2 * Math.PI * fc / fs;
            double cosw0 = Math.Cos(w0);
            double sinw0 = Math.Sin(w0);
            double alpha = sinw0 / (2 * q);

            double b0 = (1 + cosw0) / 2;
            double b1 = -(1 + cosw0);
            double b2 = (1 + cosw0) / 2;
            double a0 = 1 + alpha;
            double a1 = -2 * cosw0;
            double a2 = 1 - alpha;

            return ApplyBiquad(x, b0 / a0, b1 / a0, b2 / a0, a1 / a0, a2 / a0);
        }

        private double[] ApplyNotchBiquad(double[] x, double fs, double f0, double q = 30.0)
        {
            double w0 = 2 * Math.PI * f0 / fs;
            double cosw0 = Math.Cos(w0);
            double sinw0 = Math.Sin(w0);
            double alpha = sinw0 / (2 * q);

            double b0 = 1;
            double b1 = -2 * cosw0;
            double b2 = 1;
            double a0 = 1 + alpha;
            double a1 = -2 * cosw0;
            double a2 = 1 - alpha;

            return ApplyBiquad(x, b0 / a0, b1 / a0, b2 / a0, a1 / a0, a2 / a0);
        }

        private double[] ApplyBiquad(double[] x, double b0, double b1, double b2, double a1, double a2)
        {
            double[] y = new double[x.Length];
            double x1 = 0, x2 = 0, y1 = 0, y2 = 0;

            for (int n = 0; n < x.Length; n++)
            {
                double x0 = x[n];
                double y0 = b0 * x0 + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2;

                y[n] = y0;

                x2 = x1; x1 = x0;
                y2 = y1; y1 = y0;
            }
            return y;
        }

        private void getImp_Click(object sender, EventArgs e)
        {
            //From Victor: need to set max safe amplitude to 0.5mA, then get impedance, then change back to 5mA

            //Set button to be greyed out
            getImp.Enabled = false;

            //Set "measuring impedance" flag to True
            PMode = 3;

            //Send command to change max safe amplitude to 0.5mA
            byte st = (byte)(Convert.ToDecimal(0.5 * 10));  //Max safe amp of 0.5, multiply by 10 when sending byte
            byte[] data = [SET_SPARS, 0x06, 0x53, 0x50, 0x31, 0x33, st, 0x00]; 
            Debug.WriteLine("TX Set Max Amp");
            Sending(data);

            //In received area, need to receive it and if impedance flag is true then get impedance:
            //Once received impedance, then send command to set back to 5mA and change flag to false
        }

        private void bGetVamp_Click(object sender, EventArgs e)
        {
            byte[] data = [GET_SPARS, 0x04, 0x53, 0x50, 0x31, 0x35]; 
            Debug.WriteLine("TX Get Params");
            labelTX.Text = "Get Params";
            labelRX.Text = "";
            txtVAmp.Text = "";
            PMode = 0;
            Sending(data);
        }

        private void bGetVfreq_Click(object sender, EventArgs e)
        {
            byte[] data = [GET_SPARS, 0x04, 0x53, 0x50, 0x31, 0x37]; 
            Debug.WriteLine("TX Get Params");
            labelTX.Text = "Get Params";
            labelRX.Text = "";
            txtVfreq.Text = "";
            PMode = 0;
            Sending(data);
        }

        private void bGetVon_Click(object sender, EventArgs e)
        {
            byte[] data = [GET_SPARS, 0x04, 0x53, 0x50, 0x31, 0x38]; 
            Debug.WriteLine("TX Get Params");
            labelTX.Text = "Get Params";
            labelRX.Text = "";
            txtVon.Text = "";
            PMode = 0;
            Sending(data);
        }

        private void bGetVoff_Click(object sender, EventArgs e)
        {
            byte[] data = [GET_SPARS, 0x04, 0x53, 0x50, 0x31, 0x39];
            Debug.WriteLine("TX Get Params");
            labelTX.Text = "Get Params";
            labelRX.Text = "";
            txtVoff.Text = "";
            PMode = 0;
            Sending(data);
        }

        private void bGetV1_Click(object sender, EventArgs e)
        {
            byte[] data = [GET_SPARS, 0x04, 0x53, 0x50, 0x31, 0x31]; 
            Debug.WriteLine("TX Get Params");
            labelTX.Text = "Get Params";
            labelRX.Text = "";
            txtV1.Text = "";
            PMode = 0;
            Sending(data);
        }

        private void bGetV2_Click(object sender, EventArgs e)
        {
            byte[] data = [GET_SPARS, 0x04, 0x53, 0x50, 0x31, 0x32]; 
            Debug.WriteLine("TX Get Params");
            labelTX.Text = "Get Params";
            labelRX.Text = "";
            txtV2.Text = "";
            PMode = 0;
            Sending(data);
        }

        private void bSendV1_Click(object sender, EventArgs e)  //Nerve block cathod
        {
            byte st = (byte)Int16.Parse(txtV1.Text);
            byte[] data = [SET_SPARS, 0x06, 0x53, 0x50, 0x31, 0x31, st, 0x00];
            labelTX.Text = "Set Params";
            labelRX.Text = "";
            Sending(data);
        }

        private void bSendV2_Click(object sender, EventArgs e)
        {
            byte st = (byte)Int16.Parse(txtV2.Text);
            byte[] data = [SET_SPARS, 0x06, 0x53, 0x50, 0x31, 0x32, st, 0x00];
            labelTX.Text = "Set Params";
            labelRX.Text = "";
            Sending(data);
        }

        private void bSendVamp_Click(object sender, EventArgs e)
        {
            byte st = (byte)(Convert.ToDecimal(txtVAmp.Text) * 10);
            byte[] data = [SET_SPARS, 0x06, 0x53, 0x50, 0x31, 0x35, st, 0x00];
            labelTX.Text = "Set Params";
            labelRX.Text = "";
            Sending(data);
        }

        private void bSendVfreq_Click(object sender, EventArgs e)
        {
            int sineFreq = Int16.Parse(txtVfreq.Text) * 10;
            byte st1 = (byte)(sineFreq % 256);
            byte st2 = (byte)(sineFreq / 256);
            //byte[] data = [SET_SPARS, 0x06, 0x53, 0x54, 0x33, 0x31, st1, st2];
            byte[] data = [SET_SPARS, 0x06, 0x53, 0x50, 0x31, 0x37, st1, st2];
            labelTX.Text = "Set Params";
            labelRX.Text = "";
            Sending(data);
        }

        private void bSendVon_Click(object sender, EventArgs e)
        {
            byte st = (byte)(Int16.Parse(txtVon.Text));
            byte[] data = [SET_SPARS, 0x06, 0x53, 0x50, 0x31, 0x38, st, 0x00];
            labelTX.Text = "Set Params";
            labelRX.Text = "";
            Sending(data);
        }

        private void bSendVoff_Click(object sender, EventArgs e)
        {
            byte st = (byte)(Int16.Parse(txtVoff.Text));
            byte[] data = [SET_SPARS, 0x06, 0x53, 0x50, 0x31, 0x39, st, 0x00];
            labelTX.Text = "Set Params";
            labelRX.Text = "";
            Sending(data);
        }

        private void bStimMax_Click(object sender, EventArgs e)
        {
            byte[] data = [GET_SPARS, 0x04, 0x53, 0x50, 0x31, 0x33];
            Debug.WriteLine("TX Get Params");
            labelTX.Text = "Get Params";
            labelRX.Text = "";
            Sending(data);
        }

        private void bZmin_Click(object sender, EventArgs e)
        {
            byte[] data = [GET_SPARS, 0x04, 0x53, 0x50, 0x31, 0x34];
            Debug.WriteLine("TX Get Params");
            labelTX.Text = "Get Params";
            labelRX.Text = "";
            Sending(data);
        }

        private void bBlockMax_Click(object sender, EventArgs e)
        {
            byte[] data = [GET_SPARS, 0x04, 0x53, 0x50, 0x31, 0x36];
            Debug.WriteLine("TX Get Params");
            labelTX.Text = "Get Params";
            labelRX.Text = "";
            txtMaxVNB.Text = "";
            Sending(data);
        }

        private void bSetMaxBlock_Click(object sender, EventArgs e)
        {
            byte st = (byte)(Convert.ToDecimal(txtMaxVNB.Text));
            byte[] data = [SET_SPARS, 0x06, 0x53, 0x50, 0x31, 0x36, st, 0x00];
            labelTX.Text = "Set Params";
            labelRX.Text = "";
            Sending(data);
        }

        private void ckVNB_CheckedChanged(object sender, EventArgs e)
        {
            if (ckVNB.Checked)
            {
                useVNBlock = true;
                groupSineWave.Enabled = true;
            } else
            {
                useVNBlock = false;
                groupSineWave.Enabled= false;
            }
        }

        private byte[] BuildAuthData(string privateKeyPath)
        {
            // 1) Message plus random number
            Random rnd = new Random();
            string msg = "OpenAVNS ADMIN AUTH v1" + rnd.Next().ToString();
            byte[] msgBytes = System.Text.Encoding.UTF8.GetBytes(msg);

            // 2) Hash it
            byte[] hash = CryptoAuth.Sha256(msgBytes);

            // 3) Load private key scalar d (32 bytes)
            byte[] priv = CryptoAuth.LoadPrivateKey32FromHexFile(privateKeyPath); // your file loader

            // 4) Create signature (r,s)
            (byte[] r, byte[] s) = CryptoAuth.SignHashP256_RawRs(hash, priv);

            // 5) Payload = r||s||hash
            return BuildAuthCode(r, s, hash);
        }

        private static byte[] BuildAuthCode(byte[] r, byte[] s, byte[] hash)
        {
            if (r.Length != 32 || s.Length != 32 || hash.Length != 32)
                throw new ArgumentException("r, s, hash must all be 32 bytes.");

            byte[] opCode = [AUTH];
            byte[] pLength = [0x6A];
            byte[] linkedFW = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00];
            byte[] linkedBLE = [0x00, 0x00, 0x00, 0x00];

            byte[] payload = new byte[108];
            System.Buffer.BlockCopy(opCode, 0, payload, 0, 1);   //Op code: 0xF0
            System.Buffer.BlockCopy(pLength, 0, payload, 1, 1);   //Payload length (106)
            System.Buffer.BlockCopy(r, 0, payload, 2, 32);  //r parameter
            System.Buffer.BlockCopy(s, 0, payload, 34, 32); //s parameter
            System.Buffer.BlockCopy(hash, 0, payload, 66, 32);  //hash
            System.Buffer.BlockCopy(linkedFW, 0, payload, 98, 6);  //6 bytes linked FW version (all zeroes for now)
            System.Buffer.BlockCopy(linkedBLE, 0, payload, 104, 4);  //4 bytes linked BLE ID (all zeroes for now)

            return payload;
        }
    }

    //A class for hashing and signing a private key
    public static class CryptoAuth
    {
        // Step 2: Hash an arbitrary byte[] message with SHA-256.
        // Returns 32 bytes.
        public static byte[] Sha256(byte[] message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            // SHA256.HashData is available in .NET 5+. If you're on older .NET, use SHA256.Create().
            return SHA256.HashData(message);
        }

        // Step 3: Sign a 32-byte SHA-256 hash with ECDSA P-256 using a raw 32-byte private key.
        // Returns (r,s) as two 32-byte big-endian arrays.
        public static (byte[] r, byte[] s) SignHashP256_RawRs(byte[] hash32, byte[] privateKey32)
        {
            if (hash32 == null) throw new ArgumentNullException(nameof(hash32));
            if (privateKey32 == null) throw new ArgumentNullException(nameof(privateKey32));
            if (hash32.Length != 32) throw new ArgumentException("Expected 32-byte hash (SHA-256).", nameof(hash32));
            if (privateKey32.Length != 32) throw new ArgumentException("Expected 32-byte P-256 private key.", nameof(privateKey32));

            using var ecdsa = CreateEcdsaP256FromPrivateKey(privateKey32);

            // IMPORTANT: SignHash returns DER-encoded ECDSA signature by default.
            byte[] derSig = ecdsa.SignHash(hash32, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);

            return (derSig[0..32], derSig[32..64]);
        }

        // Convenience: loads a private key from a text file containing 64 hex chars (32 bytes).
        // Allows comments (#...) and whitespace.
        public static byte[] LoadPrivateKey32FromHexFile(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException($"Private key file not found: {path}");

            string text = File.ReadAllText(path);

            // Remove comments and whitespace
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                            .Select(l => l.Trim())
                            .Where(l => l.Length > 0 && !l.StartsWith("#"));
            string hex = string.Concat(lines);

            // Keep only hex characters
            hex = new string(hex.Where(Uri.IsHexDigit).ToArray());

            if (hex.Length != 64)
                throw new FormatException($"Expected 64 hex chars (32 bytes) for P-256 private key. Got {hex.Length}.");

            return HexToBytes(hex);
        }

        // Internal helper functions
        private static ECDsa CreateEcdsaP256FromPrivateKey(byte[] privateKey32)
        {
            // For P-256, the "D" parameter is the 32-byte private key.
            // This creates a keypair. (Public key derived internally.)
            var ecParams = new ECParameters
            {
                Curve = ECCurve.NamedCurves.nistP256,
                D = privateKey32
            };

            return ECDsa.Create(ecParams);
        }

        private static byte[] HexToBytes(string hex)
        {
            if (hex.Length % 2 != 0)
                throw new FormatException("Hex string must have even length.");

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }
    }

}
