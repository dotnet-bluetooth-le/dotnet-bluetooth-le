using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Android
{
    public class Device : DeviceBase
    {
        private BluetoothDevice _nativeDevice;
        /// <summary>
        /// we have to keep a reference to this because Android's api is weird and requires
        /// the GattServer in order to do nearly anything, including enumerating services
        /// 
        /// TODO: consider wrapping the Gatt and Callback into a single object and passing that around instead.
        /// </summary>
        private BluetoothGatt _gatt;
        /// <summary>
        /// we also track this because of gogole's weird API. the gatt callback is where
        /// we'll get notified when services are enumerated
        /// </summary>
        private IGattCallback _gattCallback;

        public Device(BluetoothDevice nativeDevice, BluetoothGatt gatt, IGattCallback gattCallback, int rssi, byte[] advertisementData = null)
        {
            Update(nativeDevice, gatt, gattCallback);
            _rssi = rssi;

            AdvertisementData = advertisementData ?? new byte[0];
        }

        public void Update(BluetoothDevice nativeDevice, BluetoothGatt gatt,
            IGattCallback gattCallback)
        {
            _nativeDevice = nativeDevice;
            _gatt = gatt;
            _gattCallback = gattCallback;
        }

        public void OnServicesDiscovered(object sender, ServicesDiscoveredEventArgs args)
        {
            if (_gatt != null)
            {
                _services = _gatt.Services.Select(service => new Service(service, _gatt, _gattCallback)).ToList<IService>();
            }

            _gattCallback.ServicesDiscovered -= OnServicesDiscovered;

            RaiseServicesDiscovered(args);
        }

        public override Guid ID
        {
            get
            {
                //TODO: verify - fix from Evolve player
                var deviceGuid = new byte[16];
                var macWithoutColons = _nativeDevice.Address.Replace(":", "");
                var macBytes = Enumerable.Range(0, macWithoutColons.Length)
                    .Where(x => x % 2 == 0)
                    .Select(x => Convert.ToByte(macWithoutColons.Substring(x, 2), 16))
                    .ToArray();
                macBytes.CopyTo(deviceGuid, 10);
                return new Guid(deviceGuid);
            }
        }

        public override string Name => _nativeDevice.Name;

        public override int Rssi => _rssi;
        private int _rssi;

        public override object NativeDevice => _nativeDevice;

        public override byte[] AdvertisementData { get; }

        public override IList<AdvertisementRecord> AdvertisementRecords => ParseScanRecord(AdvertisementData);

        // TODO: investigate the validity of this. Android API seems to indicate that the
        // bond state is available, rather than the connected state, which are two different 
        // things. you can be bonded but not connected.
        public override DeviceState State => GetState();

        //TODO: strongly type IService here
        public override IList<IService> Services => _services;
        private IList<IService> _services = new List<IService>();

        #region public methods

        public override void DiscoverServices()
        {
            if (_gattCallback == null || _gatt == null)
            {
                return;
            }

            Trace.Message("...Discover services");

            _gattCallback.ServicesDiscovered += OnServicesDiscovered;
            _gatt.DiscoverServices();
        }

        // First step
        public void Disconnect()
        {
            if (_gatt != null)
            {
                //clear cached services
                _services.Clear();

                _gatt.Disconnect();
            }
            else
            {
                Console.WriteLine("Can't disconnect {0}. Gatt is null.", Name);
            }
        }

        //Second step
        public void CloseGatt()
        {
            if (_gatt != null)
            {
                _gatt.Close();
                _gatt = null;
            }
            else
            {
                Console.WriteLine("Can't close gatt {0}. Gatt is null.", Name);
            }

        }

        #endregion

        #region internal methods

        protected DeviceState GetState()
        {
            var manager = (BluetoothManager)Application.Context.GetSystemService(Context.BluetoothService);
            var state = manager.GetConnectionState(_nativeDevice, ProfileType.Gatt);

            switch (state)
            {
                case ProfileState.Connected:
                    return DeviceState.Connected;

                case ProfileState.Connecting:
                    return DeviceState.Connecting;

                case ProfileState.Disconnected:
                case ProfileState.Disconnecting:
                default:
                    return DeviceState.Disconnected;
            }
        }
        #endregion

        public static List<AdvertisementRecord> ParseScanRecord(byte[] scanRecord)
        {
            var records = new List<AdvertisementRecord>();

            if (scanRecord == null)
                return records;

            int index = 0;
            while (index < scanRecord.Length)
            {
                byte length = scanRecord[index++];
                //Done once we run out of records 
                // 1 byte for type and length-1 bytes for data
                if (length == 0) break;

                int type = scanRecord[index];
                //Done if our record isn't a valid type
                if (type == 0) break;

                if (!Enum.IsDefined(typeof(AdvertisementRecordType), type))
                {
                    Trace.Message("Advertisment record type not defined: {0}", type);
                    break;
                }

                //data length is length -1 because type takes the first byte
                byte[] data = new byte[length - 1];
                Array.Copy(scanRecord, index + 1, data, 0, length - 1);

                // don't forget that data is little endian so reverse
                // Supplement to Bluetooth Core Specification 1
                // NOTE: all relevant devices are already little endian, so this is not necessary for any type except UUIDs
                //var record = new AdvertisementRecord((AdvertisementRecordType)type, data.Reverse().ToArray());

                switch ((AdvertisementRecordType)type)
                {
                    case AdvertisementRecordType.ServiceDataUuid32Bit:
                    case AdvertisementRecordType.SsUuids128Bit:
                    case AdvertisementRecordType.SsUuids16Bit:
                    case AdvertisementRecordType.SsUuids32Bit:
                    case AdvertisementRecordType.UuidCom32Bit:
                    case AdvertisementRecordType.UuidsComplete128Bit:
                    case AdvertisementRecordType.UuidsComplete16Bit:
                    case AdvertisementRecordType.UuidsIncomple16Bit:
                    case AdvertisementRecordType.UuidsIncomplete128Bit:
                        Array.Reverse(data);
                        break;
                }
                var record = new AdvertisementRecord((AdvertisementRecordType)type, data);

                Trace.Message(record.ToString());

                records.Add(record);

                //Advance
                index += length;
            }

            return records;
        }

        #region RSSI

        public override void ReadRssi()
        {
            _gattCallback.RemoteRssiRead += OnRemoteRssiRead;
            _gatt.ReadRemoteRssi();
        }

        private void OnRemoteRssiRead(object sender, RssiReadEventArgs e)
        {
            _gattCallback.RemoteRssiRead -= OnRemoteRssiRead;
            if (e.Error == null)
            {
                _rssi = e.Rssi;
            }

            RaiseRssiRead(e);
        }
        #endregion
    }
}

