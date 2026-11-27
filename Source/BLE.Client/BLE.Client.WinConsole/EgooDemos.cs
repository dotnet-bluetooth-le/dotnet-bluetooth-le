using Plugin.BLE.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BLE.Client.WinConsole
{
    /// <summary>
    /// Demonstrations for communicating with an Egoo instrument using the Windows BLE API directly
    /// </summary>
    public class EgooDemos
    {
        private static readonly Guid CommonService = new("000000DD-0000-1000-8000-00805F9B34FB");
        private static readonly Guid InfoCharacteristic = new("0000DD01-0000-1000-8000-00805F9B34FB");
        private static readonly Guid StateCharacteristic = new("0000DD02-0000-1000-8000-00805F9B34FB");
        private static readonly Guid AssayService = new("000000EE-0000-1000-8000-00805F9B34FB");
        private static readonly Guid DrawerCharacteristic = new("0000EE01-0000-1000-8000-00805F9B34FB");
        private static readonly Guid CapsuleCharacteristic = new("0000EE02-0000-1000-8000-00805F9B34FB");

        private readonly Action<string, object[]>? writer;
        private readonly ManualResetEvent disconnectedSignal = new ManualResetEvent(false);
        private readonly ManualResetEvent connectedSignal = new ManualResetEvent(false);

        public EgooDemos(Action<string, object[]>? writer = null)
        {
            this.writer = writer;
        }

        private void Write(string format, params object[] args)
        {
            writer?.Invoke(format, args);
        }

        public async Task Egoo_DEMO_Information()
        {
            string bleaddress = BleAddressSelector.GetBleAddress();
            ulong bleaddressUl = bleaddress.ToBleDeviceGuid().ToBleAddress();
            BluetoothLEDevice dev = await BluetoothLEDevice.FromBluetoothAddressAsync(bleaddressUl);
            dev.RequestPreferredConnectionParameters(BluetoothLEPreferredConnectionParameters.ThroughputOptimized);
            dev.ConnectionStatusChanged += Dev_ConnectionStatusChanged;
            var devId = BluetoothDeviceId.FromId(dev.DeviceId);
            Write("Connecting...");
            var stopwatch = Stopwatch.StartNew();
            GattSession gattSession = await GattSession.FromDeviceIdAsync(devId);
            gattSession.MaintainConnection = true;
            if (!connectedSignal.WaitOne(10000))
            {
                Write("Not Connected!!!");
                return;
            }
            Write("Connected in {0} ms", stopwatch.ElapsedMilliseconds);

            Write("Instrument info: {0}", await ReadJson(dev, CommonService, InfoCharacteristic));
            Write("Drawer state: {0}", await ReadJson(dev, AssayService, DrawerCharacteristic));
            Write("Capsule state: {0}", await ReadJson(dev, AssayService, CapsuleCharacteristic));

            stopwatch = Stopwatch.StartNew();
            gattSession.MaintainConnection = false;
            gattSession.Dispose();
            dev.Dispose();
            if (!disconnectedSignal.WaitOne(10000))
            {
                Write("Not Disconnected!!!");
                return;
            }
            Write("Disconnected in {0} ms", stopwatch.ElapsedMilliseconds);
        }

        private static readonly (Guid Service, Guid Characteristic, string Name)[] NotifyCharacteristics =
        {
            (new("000000CC-0000-1000-8000-00805F9B34FB"), new("0000CC01-0000-1000-8000-00805F9B34FB"), "OtaInitialize"),
            (new("000000CC-0000-1000-8000-00805F9B34FB"), new("0000CC02-0000-1000-8000-00805F9B34FB"), "OtaWrite"),
            (new("000000CC-0000-1000-8000-00805F9B34FB"), new("0000CC03-0000-1000-8000-00805F9B34FB"), "OtaActivate"),
            (AssayService, DrawerCharacteristic, "Drawer"),
            (AssayService, CapsuleCharacteristic, "Capsule"),
            (AssayService, new("0000EE03-0000-1000-8000-00805F9B34FB"), "CapsuleDecode"),
            (AssayService, new("0000EE04-0000-1000-8000-00805F9B34FB"), "StartAnalysis"),
            (AssayService, new("0000EE05-0000-1000-8000-00805F9B34FB"), "Progress"),
            (AssayService, new("0000EE06-0000-1000-8000-00805F9B34FB"), "Result"),
            (AssayService, new("0000EE07-0000-1000-8000-00805F9B34FB"), "Error"),
            (CommonService, new("0000DD02-0000-1000-8000-00805F9B34FB"), "State"),
            (AssayService, new("0000EE0B-0000-1000-8000-00805F9B34FB"), "CustomAssayStart"),
            (AssayService, new("0000EE0C-0000-1000-8000-00805F9B34FB"), "PdCalibration"),
            (AssayService, new("0000EE0D-0000-1000-8000-00805F9B34FB"), "OpticalSelfCheck"),
            (AssayService, new("0000EE0E-0000-1000-8000-00805F9B34FB"), "AssayStop"),
            (CommonService, new("0000DD03-0000-1000-8000-00805F9B34FB"), "CallService"),
            (AssayService, new("0000EE12-0000-1000-8000-00805F9B34FB"), "CalibrationUpdatesStore"),
        };

        /// <summary>
        /// Mimics EgooInstrument NotifyHandler.SubscribeToAllChanges and times every step
        /// </summary>
        public async Task Egoo_ALL_Services()
        {
            var overall = Stopwatch.StartNew();
            var connection = await Connect();
            if (connection is null)
                return;
            var (dev, gattSession) = connection.Value;
            long connectMs = overall.ElapsedMilliseconds;

            var total = Stopwatch.StartNew();
            int index = 0;
            var services = new Dictionary<Guid, GattDeviceService>();
            var subscribed = new List<GattCharacteristic>();
            foreach (var (serviceId, characteristicId, name) in NotifyCharacteristics)
            {
                var sw = Stopwatch.StartNew();
                Write("[{0}/{1}] Subscribing to {2}...", ++index, NotifyCharacteristics.Length, name);
                try
                {
                    if (!services.TryGetValue(serviceId, out var service))
                    {
                        var sr = await dev.GetGattServicesForUuidAsync(serviceId, BluetoothCacheMode.Cached);
                        if (sr.Status != GattCommunicationStatus.Success || sr.Services.Count == 0)
                        {
                            Write("{0}: service not found ({1})", name, sr.Status);
                            continue;
                        }
                        service = sr.Services[0];
                        services[serviceId] = service;
                    }
                    long tService = sw.ElapsedMilliseconds;
                    var cr = await service.GetCharacteristicsForUuidAsync(characteristicId, BluetoothCacheMode.Cached);
                    if (cr.Status != GattCommunicationStatus.Success || cr.Characteristics.Count == 0)
                    {
                        Write("{0}: characteristic not found ({1})", name, cr.Status);
                        continue;
                    }
                    var characteristic = cr.Characteristics[0];
                    long tChar = sw.ElapsedMilliseconds;
                    var cccd = characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate)
                        ? GattClientCharacteristicConfigurationDescriptorValue.Indicate
                        : GattClientCharacteristicConfigurationDescriptorValue.Notify;
                    var wr = await characteristic.WriteClientCharacteristicConfigurationDescriptorWithResultAsync(cccd);
                    characteristic.ValueChanged += Characteristic_ValueChanged;
                    if (wr.Status == GattCommunicationStatus.Success)
                        subscribed.Add(characteristic);
                    string err = wr.ProtocolError is byte pe ? $" 0x{pe:X2} {ProtocolErrorName(pe)}" : "";
                    Write("{0}: {1}{2} (service {3} ms, characteristic {4} ms, subscribe {5} ms)",
                        name, wr.Status, err, tService, tChar - tService, sw.ElapsedMilliseconds - tChar);
                }
                catch (Exception ex)
                {
                    Write("{0}: exception after {1} ms: {2}", name, sw.ElapsedMilliseconds, ex.Message);
                }
            }
            Write("Subscribed to {0}/{1} characteristics in {2} ms", subscribed.Count, NotifyCharacteristics.Length, total.ElapsedMilliseconds);
            Write("TOTAL: connect {0} ms + subscribe {1} ms = {2} ms until ready", connectMs, total.ElapsedMilliseconds, overall.ElapsedMilliseconds);

            var readSw = Stopwatch.StartNew();
            Write("Instrument info: {0} ({1} ms)", await ReadJson(dev, CommonService, InfoCharacteristic), readSw.ElapsedMilliseconds);
            readSw.Restart();
            Write("Drawer status: {0} ({1} ms)", await ReadJson(dev, AssayService, DrawerCharacteristic), readSw.ElapsedMilliseconds);
            readSw.Restart();
            Write("Capsule status: {0} ({1} ms)", await ReadJson(dev, AssayService, CapsuleCharacteristic), readSw.ElapsedMilliseconds);
            readSw.Restart();
            Write("State: {0} ({1} ms)", await ReadJson(dev, CommonService, StateCharacteristic), readSw.ElapsedMilliseconds);

            foreach (var characteristic in subscribed)
            {
                characteristic.ValueChanged -= Characteristic_ValueChanged;
            }
            foreach (var service in services.Values)
            {
                service.Dispose();
            }

            Write("Disconnecting...");
            var stopwatch = Stopwatch.StartNew();
            gattSession.MaintainConnection = false;
            gattSession.Dispose();
            dev.Dispose();
            if (!disconnectedSignal.WaitOne(10000))
            {
                Write("Not Disconnected!!!");
                return;
            }
            Write("Disconnected in {0} ms", stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Faster variant: force encryption first, discover each service once, then write all CCCDs concurrently
        /// </summary>
        public async Task Egoo_ALL_Services_Fast()
        {
            var overall = Stopwatch.StartNew();
            var connection = await Connect();
            if (connection is null)
                return;
            var (dev, gattSession) = connection.Value;
            long connectMs = overall.ElapsedMilliseconds;

            var sw = Stopwatch.StartNew();
            Write("Step 1: read State to force encryption... {0}", await ReadJson(dev, CommonService, StateCharacteristic));
            Write("Step 1 done in {0} ms", sw.ElapsedMilliseconds);

            sw.Restart();
            var servicesResult = await dev.GetGattServicesAsync(BluetoothCacheMode.Cached);
            Write("Step 2: GetGattServicesAsync -> {0}, {1} services in {2} ms", servicesResult.Status, servicesResult.Services.Count, sw.ElapsedMilliseconds);

            sw.Restart();
            var wanted = NotifyCharacteristics.Select(n => n.Characteristic).ToHashSet();
            var targets = new List<(GattCharacteristic Characteristic, string Name)>();
            foreach (var service in servicesResult.Services)
            {
                if (!NotifyCharacteristics.Any(n => n.Service == service.Uuid))
                    continue;
                var cr = await service.GetCharacteristicsAsync(BluetoothCacheMode.Cached);
                foreach (var c in cr.Characteristics.Where(c => wanted.Contains(c.Uuid)))
                    targets.Add((c, NotifyCharacteristics.First(n => n.Characteristic == c.Uuid).Name));
            }
            Write("Step 3: found {0}/{1} characteristics in {2} ms", targets.Count, NotifyCharacteristics.Length, sw.ElapsedMilliseconds);

            sw.Restart();
            var tasks = targets.Select(async t =>
            {
                var cccd = t.Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate)
                    ? GattClientCharacteristicConfigurationDescriptorValue.Indicate
                    : GattClientCharacteristicConfigurationDescriptorValue.Notify;
                t.Characteristic.ValueChanged += Characteristic_ValueChanged;
                try
                {
                    var wr = await t.Characteristic.WriteClientCharacteristicConfigurationDescriptorWithResultAsync(cccd);
                    return (t.Name, Status: wr.Status.ToString(), Ms: sw.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    return (t.Name, Status: "Exception: " + ex.Message, Ms: sw.ElapsedMilliseconds);
                }
            }).ToList();
            var results = await Task.WhenAll(tasks);
            foreach (var r in results)
                Write("  {0}: {1} (done at {2} ms)", r.Name, r.Status, r.Ms);
            Write("Step 4: {0}/{1} subscribed concurrently in {2} ms",
                results.Count(r => r.Status == "Success"), targets.Count, sw.ElapsedMilliseconds);
            Write("TOTAL FAST: connect {0} ms, ready after {1} ms", connectMs, overall.ElapsedMilliseconds);

            foreach (var t in targets)
                t.Characteristic.ValueChanged -= Characteristic_ValueChanged;
            foreach (var s in servicesResult.Services)
                s.Dispose();

            Write("Disconnecting...");
            sw.Restart();
            gattSession.MaintainConnection = false;
            gattSession.Dispose();
            dev.Dispose();
            if (disconnectedSignal.WaitOne(10000))
                Write("Disconnected in {0} ms", sw.ElapsedMilliseconds);
            else
                Write("Not Disconnected!!!");
        }

        /// <summary>
        /// Experiment: kick off encryption early (State read) in parallel with discovery,
        /// read CCCDs and only write the ones not already set.
        /// </summary>
        public async Task Egoo_ALL_Services_Smart()
        {
            var overall = Stopwatch.StartNew();
            var connection = await Connect();
            if (connection is null)
                return;
            var (dev, gattSession) = connection.Value;
            long connectMs = overall.ElapsedMilliseconds;
            var pairing = dev.DeviceInformation.Pairing;
            Write("Pairing: IsPaired={0}, ProtectionLevel={1}", pairing.IsPaired, pairing.ProtectionLevel);

            var sw = Stopwatch.StartNew();
            var servicesChanged = new TaskCompletionSource<long>(TaskCreationOptions.RunContinuationsAsynchronously);
            void Ev(string what) => Write("  EVENT +{0} ms: {1}", overall.ElapsedMilliseconds, what);
            gattSession.SessionStatusChanged += (s, e) => Ev($"SessionStatus={e.Status} Error={e.Error}");
            gattSession.MaxPduSizeChanged += (s, e) => Ev($"MaxPduSize={s.MaxPduSize}");
            dev.GattServicesChanged += (s, e) => { Ev("GattServicesChanged"); servicesChanged.TrySetResult(overall.ElapsedMilliseconds); };
            dev.ConnectionParametersChanged += (s, e) =>
            {
                var p = s.GetConnectionParameters();
                Ev($"ConnectionParameters Interval={p.ConnectionInterval} Latency={p.ConnectionLatency} Timeout={p.LinkTimeout}");
            };
            dev.ConnectionPhyChanged += (s, e) =>
            {
                var phy = s.GetConnectionPhy();
                Ev($"Phy Tx={phy.TransmitInfo.IsUncoded1MPhy}/{phy.TransmitInfo.IsUncoded2MPhy} Rx={phy.ReceiveInfo.IsUncoded1MPhy}/{phy.ReceiveInfo.IsUncoded2MPhy}");
            };
            dev.ConnectionStatusChanged += (s, e) => Ev($"ConnectionStatus={s.ConnectionStatus}");
            dev.DeviceInformation.Pairing.ToString();

            var servicesResult = await dev.GetGattServicesAsync(BluetoothCacheMode.Cached);
            Write("Step 1: GetGattServicesAsync -> {0}, {1} services in {2} ms", servicesResult.Status, servicesResult.Services.Count, sw.ElapsedMilliseconds);
            var waitChanged = await Task.WhenAny(servicesChanged.Task, Task.Delay(5000));
            Write("Step 1b: GattServicesChanged {0} at +{1} ms", waitChanged == servicesChanged.Task ? "received" : "NOT received (5 s timeout)", overall.ElapsedMilliseconds);
            foreach (var s in servicesResult.Services)
                s.Dispose();
            sw.Restart();
            servicesResult = await dev.GetGattServicesAsync(BluetoothCacheMode.Cached);
            Write("Step 1c: re-discover -> {0}, {1} services in {2} ms", servicesResult.Status, servicesResult.Services.Count, sw.ElapsedMilliseconds);

            var wanted = NotifyCharacteristics.Select(n => n.Characteristic).Append(StateCharacteristic).ToHashSet();
            var targets = new List<(GattCharacteristic Characteristic, string Name)>();
            GattCharacteristic stateChar = null;
            foreach (var service in servicesResult.Services)
            {
                if (!NotifyCharacteristics.Any(n => n.Service == service.Uuid))
                    continue;
                var cr = await service.GetCharacteristicsAsync(BluetoothCacheMode.Cached);
                foreach (var c in cr.Characteristics.Where(c => wanted.Contains(c.Uuid)))
                {
                    if (c.Uuid == StateCharacteristic)
                        stateChar = c;
                    if (NotifyCharacteristics.Any(n => n.Characteristic == c.Uuid))
                        targets.Add((c, NotifyCharacteristics.First(n => n.Characteristic == c.Uuid).Name));
                }
            }
            Write("Step 2: found {0}/{1} characteristics at {2} ms", targets.Count, NotifyCharacteristics.Length, sw.ElapsedMilliseconds);

            var encryptTask = Task.Run(async () =>
            {
                var t = Stopwatch.StartNew();
                Ev("Early State read started");
                try
                {
                    var rr = await stateChar.ReadValueAsync(BluetoothCacheMode.Uncached);
                    var json = rr.Status == GattCommunicationStatus.Success
                        ? System.Text.Encoding.UTF8.GetString(System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeBufferExtensions.ToArray(rr.Value))
                        : $"Read failed ({rr.Status}, {rr.ProtocolError})";
                    Ev("Early State read finished");
                    return (json, t.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    return ("exception: " + ex.Message, t.ElapsedMilliseconds);
                }
            });
            var pollCts = new CancellationTokenSource();
            var pollTask = Task.Run(async () =>
            {
                string last = null;
                while (!pollCts.IsCancellationRequested)
                {
                    var p = dev.GetConnectionParameters();
                    var now = $"Status={dev.ConnectionStatus} Session={gattSession.SessionStatus} Interval={p.ConnectionInterval} Pdu={gattSession.MaxPduSize} SecureConn={dev.WasSecureConnectionUsedForPairing}";
                    if (now != last)
                    {
                        Ev("POLL " + now);
                        last = now;
                    }
                    await Task.Delay(50);
                }
            });

            var toWrite = new List<(GattCharacteristic Characteristic, string Name, GattClientCharacteristicConfigurationDescriptorValue Value)>();
            foreach (var t in targets)
            {
                var wantedValue = t.Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate)
                    ? GattClientCharacteristicConfigurationDescriptorValue.Indicate
                    : GattClientCharacteristicConfigurationDescriptorValue.Notify;
                t.Characteristic.ValueChanged += Characteristic_ValueChanged;
                var rs = Stopwatch.StartNew();
                try
                {
                    var r = await t.Characteristic.ReadClientCharacteristicConfigurationDescriptorAsync();
                    Write("  CCCD {0}: {1} value={2} ({3} ms)", t.Name, r.Status, r.ClientCharacteristicConfigurationDescriptor, rs.ElapsedMilliseconds);
                    if (r.Status != GattCommunicationStatus.Success || r.ClientCharacteristicConfigurationDescriptor != wantedValue)
                        toWrite.Add((t.Characteristic, t.Name, wantedValue));
                }
                catch (Exception ex)
                {
                    Write("  CCCD {0}: exception {1} ({2} ms)", t.Name, ex.Message, rs.ElapsedMilliseconds);
                    toWrite.Add((t.Characteristic, t.Name, wantedValue));
                }
            }
            Write("Step 3: CCCD read done at {0} ms, {1} already set, {2} need write", sw.ElapsedMilliseconds, targets.Count - toWrite.Count, toWrite.Count);

            foreach (var w in toWrite)
            {
                var ws = Stopwatch.StartNew();
                try
                {
                    var wr = await w.Characteristic.WriteClientCharacteristicConfigurationDescriptorWithResultAsync(w.Value);
                    Write("  Write {0}: {1} ({2} ms)", w.Name, wr.Status, ws.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    Write("  Write {0}: exception {1} ({2} ms)", w.Name, ex.Message, ws.ElapsedMilliseconds);
                }
            }
            Write("Step 4: writes done at {0} ms", sw.ElapsedMilliseconds);

            var (stateJson, encryptMs) = await encryptTask;
            Write("Early State read: {0} ({1} ms)", stateJson, encryptMs);
            Write("TOTAL SMART: connect {0} ms, ready after {1} ms", connectMs, overall.ElapsedMilliseconds);
            await Task.Delay(500);
            pollCts.Cancel();
            await pollTask;

            foreach (var t in targets)
                t.Characteristic.ValueChanged -= Characteristic_ValueChanged;
            foreach (var s in servicesResult.Services)
                s.Dispose();

            Write("Disconnecting...");
            sw.Restart();
            gattSession.MaintainConnection = false;
            gattSession.Dispose();
            dev.Dispose();
            if (disconnectedSignal.WaitOne(10000))
                Write("Disconnected in {0} ms", sw.ElapsedMilliseconds);
            else
                Write("Not Disconnected!!!");
        }

        private async Task<(BluetoothLEDevice Device, GattSession Session)?> Connect()
        {
            connectedSignal.Reset();
            disconnectedSignal.Reset();
            var sw = Stopwatch.StartNew();
            string bleaddress = BleAddressSelector.GetBleAddress();
            ulong bleaddressUl = bleaddress.ToBleDeviceGuid().ToBleAddress();
            Write("Getting BluetoothLEDevice for {0}...", bleaddress);
            BluetoothLEDevice? dev = await BluetoothLEDevice.FromBluetoothAddressAsync(bleaddressUl);
            if (dev is null)
            {
                Write("Device {0} not found after {1} ms", bleaddress, sw.ElapsedMilliseconds);
                return null;
            }
            Write("Got device '{0}' in {1} ms (status: {2})", dev.Name, sw.ElapsedMilliseconds, dev.ConnectionStatus);
            dev.RequestPreferredConnectionParameters(BluetoothLEPreferredConnectionParameters.ThroughputOptimized);
            dev.ConnectionStatusChanged += Dev_ConnectionStatusChanged;
            var devId = BluetoothDeviceId.FromId(dev.DeviceId);

            Write("Creating GattSession...");
            GattSession gattSession = await GattSession.FromDeviceIdAsync(devId);
            gattSession.MaintainConnection = true;
            Write("GattSession created at {0} ms, waiting for connection...", sw.ElapsedMilliseconds);

            if (dev.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                connectedSignal.Set();
            }
            const int timeoutMs = 10000;
            for (int waited = 0; !connectedSignal.WaitOne(1000); waited += 1000)
            {
                if (waited + 1000 >= timeoutMs)
                {
                    Write("Not Connected after {0} ms!!! (status: {1}, session: {2})",
                        sw.ElapsedMilliseconds, dev.ConnectionStatus, gattSession.SessionStatus);
                    gattSession.Dispose();
                    dev.Dispose();
                    return null;
                }
                Write("  still waiting... {0} ms (session: {1})", sw.ElapsedMilliseconds, gattSession.SessionStatus);
            }
            var conpar = dev.GetConnectionParameters();
            Write("Connected in {0} ms (Interval = {1}, Latency = {2}, Timeout = {3}, MaxPdu = {4})",
                sw.ElapsedMilliseconds, conpar.ConnectionInterval, conpar.ConnectionLatency, conpar.LinkTimeout, gattSession.MaxPduSize);
            return (dev, gattSession);
        }

        private static string ProtocolErrorName(byte code)
        {
            if (code == GattProtocolError.InsufficientAuthentication) return "InsufficientAuthentication";
            if (code == GattProtocolError.InsufficientEncryption) return "InsufficientEncryption";
            if (code == GattProtocolError.InsufficientAuthorization) return "InsufficientAuthorization";
            if (code == GattProtocolError.WriteNotPermitted) return "WriteNotPermitted";
            if (code == GattProtocolError.RequestNotSupported) return "RequestNotSupported";
            if (code == GattProtocolError.InvalidHandle) return "InvalidHandle";
            if (code == GattProtocolError.UnlikelyError) return "UnlikelyError";
            return "Unknown";
        }

        private void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            using var reader = Windows.Storage.Streams.DataReader.FromBuffer(args.CharacteristicValue);
            Write("ValueChanged {0}: {1}", sender.Uuid, reader.ReadString(args.CharacteristicValue.Length));
        }

        private static async Task<string> ReadJson(BluetoothLEDevice dev, Guid serviceId, Guid characteristicId)
        {
            Exception? last = null;
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    return await ReadJsonOnce(dev, serviceId, characteristicId);
                }
                catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
                {
                    last = ex;
                    await Task.Delay(1000);
                }
            }
            return $"Read cancelled after retries ({last?.Message})";
        }

        private static async Task<string> ReadJsonOnce(BluetoothLEDevice dev, Guid serviceId, Guid characteristicId)
        {
            var services = await dev.GetGattServicesForUuidAsync(serviceId, BluetoothCacheMode.Cached);
            if (services.Status != GattCommunicationStatus.Success || services.Services.Count == 0)
                return $"Service not found ({services.Status})";
            var characteristics = await services.Services[0].GetCharacteristicsForUuidAsync(characteristicId, BluetoothCacheMode.Cached);
            if (characteristics.Status != GattCommunicationStatus.Success || characteristics.Characteristics.Count == 0)
                return $"Characteristic not found ({characteristics.Status})";
            var read = await characteristics.Characteristics[0].ReadValueAsync(BluetoothCacheMode.Uncached);
            if (read.Status != GattCommunicationStatus.Success)
                return $"Read failed ({read.Status})";
            using var reader = Windows.Storage.Streams.DataReader.FromBuffer(read.Value);
            return reader.ReadString(read.Value.Length);
        }

        private void Dev_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            Write("ConnectionStatusChanged:{0}", sender.ConnectionStatus);
            switch (sender.ConnectionStatus)
            {
                case BluetoothConnectionStatus.Disconnected:
                    disconnectedSignal.Set();
                    break;
                case BluetoothConnectionStatus.Connected:
                    connectedSignal.Set();
                    break;
            }
        }
    }
}
