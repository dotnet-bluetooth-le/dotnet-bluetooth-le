using System;
using Android.Bluetooth;
using Cirrious.CrossCore;
using MvvmCross.Plugins.BLE.Bluetooth.LE;

namespace MvvmCross.Plugins.BLE.Droid.Bluetooth.LE
{
    public interface IGattCallback
    {
        event EventHandler<ServicesDiscoveredEventArgs> ServicesDiscovered;
        event EventHandler<CharacteristicReadEventArgs> CharacteristicValueUpdated;
        event EventHandler<CharacteristicWriteEventArgs> CharacteristicValueWritten;
    }

    public partial class Adapter : BluetoothGattCallback, IGattCallback
    {
        public event EventHandler<ServicesDiscoveredEventArgs> ServicesDiscovered = delegate { };
        public event EventHandler<CharacteristicReadEventArgs> CharacteristicValueUpdated = delegate { };
        public event EventHandler<CharacteristicWriteEventArgs> CharacteristicValueWritten = delegate { };

        public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
        {
            Mvx.Trace("OnConnectionStateChange: ");
            base.OnConnectionStateChange(gatt, status, newState);

            IDevice device = null;
            switch (newState)
            {
                // disconnected
                case ProfileState.Disconnected:
                    Mvx.Trace("Disconnected");

                    if (DeviceOperationRegistry.TryGetValue(gatt.Device.Address, out device))
                    {
                        //Found so we can remove it
                        DeviceOperationRegistry.Remove(gatt.Device.Address);

                        RemoveDeviceFromList(device);
                        ((Device)device).CloseGatt();

                        DeviceDisconnected(this, new DeviceConnectionEventArgs { Device = device });
                        break;
                    }

                    //connection must have been lost, bacause our device was not found in the registry but was still connected
                    if (ConnectedDeviceRegistry.TryGetValue(gatt.Device.Address, out device))
                    {
                        RemoveDeviceFromList(device);
                        ((Device)device).CloseGatt();

                        DeviceConnectionLost(this, new DeviceConnectionEventArgs() { Device = device });
                    }


                    Mvx.Trace("Device not found in registry. Not raising diconnect/lost event.");

                    break;
                // connecting
                case ProfileState.Connecting:
                    Mvx.Trace("Connecting");
                    break;
                // connected
                case ProfileState.Connected:
                    Mvx.Trace("Connected");

                    //Try to find the device in the registry so that the same instanece is updated
                    if (DeviceOperationRegistry.TryGetValue(gatt.Device.Address, out device))
                    {
                        ((Device)device).Update(gatt.Device, gatt, this);

                        //Found so we can remove it
                        DeviceOperationRegistry.Remove(gatt.Device.Address);
                    }
                    else
                    {
                        //should not be the case
                        device = new Device(gatt.Device, gatt, this, 0);
                    }

                    this.ConnectedDeviceRegistry.Add(device.ID.ToString(), device);
                    this.DeviceConnected(this, new DeviceConnectionEventArgs() { Device = device });

                    break;
                // disconnecting
                case ProfileState.Disconnecting:
                    Mvx.Trace("Disconnecting");
                    break;
            }
        }

        public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status)
        {
            base.OnServicesDiscovered(gatt, status);

            Mvx.Trace("OnServicesDiscovered: " + status.ToString());

            this.ServicesDiscovered(this, new ServicesDiscoveredEventArgs());
        }

        public override void OnDescriptorRead(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, GattStatus status)
        {
            base.OnDescriptorRead(gatt, descriptor, status);

            Mvx.Trace("OnDescriptorRead: " + descriptor.ToString());

        }

        public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, GattStatus status)
        {
            base.OnCharacteristicRead(gatt, characteristic, status);

            Mvx.Trace("OnCharacteristicRead: {0}, {1}", characteristic.GetStringValue(0), status);

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

            //Console.WriteLine("OnCharacteristicChanged: " + characteristic.GetStringValue(0));

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

            Mvx.Trace("OnCharacteristicWrite: {0}", status);

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

            Mvx.Trace("OnReliableWriteCompleted: {0}", status);
        }
    }


}

