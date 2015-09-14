using System;
using Android.Bluetooth;
using MvvmCross.Plugins.BLE.Bluetooth.LE;

namespace MvvmCross.Plugins.BLE.Droid.Bluetooth.LE
{
    public class GattCallback : BluetoothGattCallback
    {

        public event EventHandler<DeviceConnectionEventArgs> DeviceConnected = delegate { };
        public event EventHandler<DeviceConnectionEventArgs> DeviceDisconnected = delegate { };
        public event EventHandler<ServicesDiscoveredEventArgs> ServicesDiscovered = delegate { };
        public event EventHandler<CharacteristicReadEventArgs> CharacteristicValueUpdated = delegate { };
        public event EventHandler<CharacteristicWriteEventArgs> CharacteristicValueWritten = delegate { };

        protected Adapter _adapter;

        public GattCallback(Adapter adapter)
        {
            this._adapter = adapter;
        }

        public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
        {
            Console.WriteLine("OnConnectionStateChange: ");
            base.OnConnectionStateChange(gatt, status, newState);

            //TODO: need to pull the cached RSSI in here, or read it (requires the callback)

            switch (newState)
            {
                // disconnected
                case ProfileState.Disconnected:
                    Console.WriteLine("disconnected");
                    this.DeviceDisconnected(this, new DeviceConnectionEventArgs() { Device = new Device(gatt.Device, null, null, 0) });
                    break;
                // connecting
                case ProfileState.Connecting:
                    Console.WriteLine("Connecting");
                    break;
                // connected
                case ProfileState.Connected:
                    Console.WriteLine("Connected");
                    this.DeviceConnected(this, new DeviceConnectionEventArgs() { Device = new Device(gatt.Device, gatt, this, 0) });
                    break;
                // disconnecting
                case ProfileState.Disconnecting:
                    Console.WriteLine("Disconnecting");
                    break;
            }
        }

        public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status)
        {
            base.OnServicesDiscovered(gatt, status);

            Console.WriteLine("OnServicesDiscovered: " + status.ToString());

            this.ServicesDiscovered(this, new ServicesDiscoveredEventArgs());
        }

        public override void OnDescriptorRead(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, GattStatus status)
        {
            base.OnDescriptorRead(gatt, descriptor, status);

            Console.WriteLine("OnDescriptorRead: " + descriptor.ToString());

        }

        public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, GattStatus status)
        {
            base.OnCharacteristicRead(gatt, characteristic, status);

            Console.WriteLine("OnCharacteristicRead: {0}, {1}", characteristic.GetStringValue(0), status);

            this.CharacteristicValueUpdated(this, new CharacteristicReadEventArgs
            {
                // memory leak ... used null params
                // dummy device with null gatt/gattcalback
                Characteristic = new Characteristic(characteristic, null, null)
            }
            );
        }

        public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
        {
            base.OnCharacteristicChanged(gatt, characteristic);

            Console.WriteLine("OnCharacteristicChanged: " + characteristic.GetStringValue(0));

            this.CharacteristicValueUpdated(this, new CharacteristicReadEventArgs()
            {
                // use null to avoid huge memory leaks due to characterisitc events
                Characteristic = new Characteristic(characteristic, null, null)
            }
            );
        }

        public override void OnCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, GattStatus status)
        {
            base.OnCharacteristicWrite(gatt, characteristic, status);

            Console.WriteLine("OnCharacteristicWrite: {0}", status);

            var args = new CharacteristicWriteEventArgs() { Characteristic = new Characteristic(characteristic, null, null) };
            switch (status)
            {
                case GattStatus.Failure:
                case GattStatus.InsufficientAuthentication:
                case GattStatus.InsufficientEncryption:
                case GattStatus.InvalidAttributeLength:
                case GattStatus.InvalidOffset:
                case GattStatus.ReadNotPermitted:
                case GattStatus.RequestNotSupported:
                case GattStatus.WriteNotPermitted:
                    args.IsSuccessfull = false;
                    break;
                case GattStatus.Success:
                    args.IsSuccessfull = true;
                    break;
            }
            this.CharacteristicValueWritten(this, args);
        }

        public override void OnReliableWriteCompleted(BluetoothGatt gatt, GattStatus status)
        {
            base.OnReliableWriteCompleted(gatt, status);

            Console.WriteLine("OnReliableWriteCompleted: {0}", status);

        }
    }
}

