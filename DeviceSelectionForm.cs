using System.Diagnostics;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;

namespace controller
{
    public class DeviceSelectionForm : Form
    {
        private BluetoothLEAdvertisementWatcher? _watcher;
        private readonly Dictionary<ulong, DateTime> _lastSeen = new();
        private readonly Dictionary<ulong, (string displayName, byte[] advData, byte[] payload)> _devices = new();

        private readonly ListBox _listBox;
        private readonly TextBox _txtAdvertisement;
        private readonly Button _connectButton;
        private readonly Button _quitButton;
        private readonly Label _statusLabel;
        private readonly System.Windows.Forms.Timer _staleTimer;

        private class DeviceItem
        {
            public ulong Address { get; }
            private readonly string _display;
            public DeviceItem(ulong addr, string display) { Address = addr; _display = display; }
            public override string ToString() => _display;
        }

        public DeviceSelectionForm()
        {
            Text = "Select CARSS Device";
            Width = 520;
            Height = 520;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            _listBox = new ListBox { Left = 12, Top = 12, Width = 488, Height = 140 };
            _txtAdvertisement = new TextBox
            {
                Left = 12,
                Top = 188,
                Width = 488,
                Height = 220,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9F),
            };
            _statusLabel = new Label { Left = 12, Top = 160, Width = 488, Height = 20, Text = "Scanning..." };
            _connectButton = new Button { Left = 12, Top = 420, Width = 235, Height = 32, Text = "Connect", Enabled = false };
            _quitButton = new Button { Left = 265, Top = 420, Width = 235, Height = 32, Text = "Quit" };

            _listBox.SelectedIndexChanged += ListBox_SelectedIndexChanged;

            _connectButton.Click += ConnectButton_Click;
            _quitButton.Click += (s, e) => Application.Exit();

            Controls.AddRange(new Control[] { _listBox, _statusLabel, _txtAdvertisement, _connectButton, _quitButton });

            _staleTimer = new System.Windows.Forms.Timer { Interval = 2000 };
            _staleTimer.Tick += StaleTimer_Tick;

            FormClosing += (s, e) => Application.Exit();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            StartScanning();
        }

        public void StartScanning()
        {
            if (_watcher != null)
            {
                _watcher.Received -= Watcher_Received;
                try { _watcher.Stop(); } catch { }
                _watcher = null;
            }

            _devices.Clear();
            _lastSeen.Clear();
            _listBox.Items.Clear();
            _txtAdvertisement.Clear();
            _connectButton.Enabled = false;
            _statusLabel.Text = "Scanning...";

            _watcher = new BluetoothLEAdvertisementWatcher { ScanningMode = BluetoothLEScanningMode.Active };
            _watcher.Received += Watcher_Received;
            _watcher.Start();
            _staleTimer.Start();
        }

        private void StopScanning()
        {
            _staleTimer.Stop();
            if (_watcher != null)
            {
                _watcher.Received -= Watcher_Received;
                try { _watcher.Stop(); } catch { }
                _watcher = null;
            }
        }

        private void Watcher_Received(BluetoothLEAdvertisementWatcher sender,
                                       BluetoothLEAdvertisementReceivedEventArgs args)
        {
            if (!IsHandleCreated) return;

            var adv = args.Advertisement;
            if (adv.ManufacturerData.Count < 1) return;

            foreach (var md in adv.ManufacturerData)
            {
                if (md.CompanyId != 0xFFFF && md.CompanyId != 0xF0F0) continue;

                ulong addr = args.BluetoothAddress;
                string devType = md.CompanyId == 0xFFFF ? "DVT" : "SRS";

                var payload = new byte[23];
                int n = Math.Min((int)md.Data.Length, payload.Length);
                if (n > 0)
                {
                    var reader = DataReader.FromBuffer(md.Data);
                    var tmp = new byte[n];
                    reader.ReadBytes(tmp);
                    Array.Copy(tmp, payload, n);
                }

                var advBytes = new byte[24];
                Array.Copy(payload, advBytes, Math.Min(payload.Length, 23));
                advBytes[23] = md.CompanyId == 0xFFFF ? (byte)1 : (byte)2;

                string mac = $"{(addr >> 40) & 0xFF:X2}:{(addr >> 32) & 0xFF:X2}:" +
                             $"{(addr >> 24) & 0xFF:X2}:{(addr >> 16) & 0xFF:X2}:" +
                             $"{(addr >> 8) & 0xFF:X2}:{addr & 0xFF:X2}";
                string display = $"{mac} ({devType})";

                this.BeginInvoke((Action)(() =>
                {
                    _lastSeen[addr] = DateTime.UtcNow;
                    _devices[addr] = (display, advBytes, payload);
                    RebuildListBox();
                    if (_listBox.SelectedItem is DeviceItem selected && selected.Address == addr)
                    {
                        ShowAdvertisement(addr, devType);
                    }
                }));

                break;
            }
        }

        private void StaleTimer_Tick(object? sender, EventArgs e)
        {
            var cutoff = DateTime.UtcNow - TimeSpan.FromSeconds(10);
            var stale = _lastSeen.Where(kv => kv.Value < cutoff)
                                 .Select(kv => kv.Key)
                                 .ToList();
            if (stale.Count == 0) return;

            foreach (var addr in stale)
            {
                _lastSeen.Remove(addr);
                _devices.Remove(addr);
            }
            RebuildListBox();
        }

        private void RebuildListBox()
        {
            ulong? selectedAddr = null;
            if (_listBox.SelectedIndex >= 0 && _listBox.SelectedItem is DeviceItem sel)
                selectedAddr = sel.Address;

            _listBox.BeginUpdate();
            _listBox.Items.Clear();
            foreach (var kv in _devices)
            {
                var item = new DeviceItem(kv.Key, kv.Value.displayName);
                _listBox.Items.Add(item);
                if (selectedAddr.HasValue && kv.Key == selectedAddr.Value)
                    _listBox.SelectedItem = item;
            }
            _listBox.EndUpdate();

            _connectButton.Enabled = _listBox.SelectedIndex >= 0;
            if (_listBox.SelectedIndex < 0)
            {
                _txtAdvertisement.Clear();
            }
        }

        private void ListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            _connectButton.Enabled = _listBox.SelectedIndex >= 0;
            if (_listBox.SelectedItem is not DeviceItem item)
            {
                _txtAdvertisement.Clear();
                return;
            }

            var devType = item.ToString().Contains("(SRS)", StringComparison.Ordinal) ? "SRS" : "DVT";
            ShowAdvertisement(item.Address, devType);
        }

        private void ShowAdvertisement(ulong address, string deviceType)
        {
            if (!_devices.TryGetValue(address, out var entry))
            {
                _txtAdvertisement.Clear();
                return;
            }

            _txtAdvertisement.Text = BleAdvertisementFormatter.Format(entry.payload, deviceType);
        }

        private void ConnectButton_Click(object? sender, EventArgs e)
        {
            if (_listBox.SelectedItem is not DeviceItem item) return;
            if (!_devices.TryGetValue(item.Address, out var entry)) return;

            StopScanning();

            var mainForm = new Form1(item.Address, entry.advData, this);
            this.Hide();
            mainForm.Show();
        }
    }
}
