using System;
using Android.Bluetooth;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Extensions;
using Plugin.BLE.Android.CallbackEventArgs;
using Plugin.BLE.Abstractions.Exceptions;

namespace Plugin.BLE.Android
{
    public interface IGattCallback
    {
        event EventHandler<ServicesDiscoveredCallbackEventArgs> ServicesDiscovered;
        event EventHandler<CharacteristicReadCallbackEventArgs> CharacteristicValueUpdated;
        event EventHandler<CharacteristicWriteCallbackEventArgs> CharacteristicValueWritten;
        event EventHandler<DescriptorCallbackEventArgs> DescriptorValueWritten;
        event EventHandler<DescriptorCallbackEventArgs> DescriptorValueRead;
        event EventHandler<RssiReadCallbackEventArgs> RemoteRssiRead;
        event EventHandler<MtuRequestCallbackEventArgs> MtuRequested;
    }

    public class GattCallback : BluetoothGattCallback, IGattCallback
    {
        private readonly Adapter _adapter;
        public event EventHandler<ServicesDiscoveredCallbackEventArgs> ServicesDiscovered;
        public event EventHandler<CharacteristicReadCallbackEventArgs> CharacteristicValueUpdated;
        public event EventHandler<CharacteristicWriteCallbackEventArgs> CharacteristicValueWritten;
        public event EventHandler<RssiReadCallbackEventArgs> RemoteRssiRead;
        public event EventHandler<DescriptorCallbackEventArgs> DescriptorValueWritten;
        public event EventHandler<DescriptorCallbackEventArgs> DescriptorValueRead;
        public event EventHandler<MtuRequestCallbackEventArgs> MtuRequested;

        public GattCallback(Adapter adapter)
        {
            _adapter = adapter;
        }

        public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
        {
            base.OnConnectionStateChange(gatt, status, newState);
            IDevice device;


            Trace.Message($"OnConnectionStateChange: GattStatus: {status}");

            switch (newState)
            {
                // disconnected
                case ProfileState.Disconnected:

                    if (_adapter.DeviceOperationRegistry.TryGetValue(gatt.Device.Address, out device))
                    {
                        Trace.Message("Disconnected by user");

                        //Found so we can remove it
                        _adapter.DeviceOperationRegistry.Remove(gatt.Device.Address);
                        _adapter.ConnectedDeviceRegistry.Remove(gatt.Device.Address);
                        gatt.Close();


                        if (status != GattStatus.Success)
                        {
                            // The above error event handles the case where the error happened during a Connect call, which will close out any waiting asyncs.
                            // Android > 5.0 uses this switch branch when an error occurs during connect
                            Trace.Message($"Error while connecting '{device.Name}'. Not raising disconnect event.");
                            _adapter.HandleConnectionFail(device, $"GattCallback error: {status}");

                        }
                        else
                        {
                            //we already hadled device error so no need th raise disconnect event(happens when device not in range)
                            _adapter.HandleDisconnectedDevice(true, device);
                        }
                        break;
                    }

                    //connection must have been lost, bacause our device was not found in the registry but was still connected
                    if (_adapter.ConnectedDeviceRegistry.TryGetValue(gatt.Device.Address, out device))
                    {
                        Trace.Message($"Disconnected '{device.Name}' by lost connection");

                        _adapter.ConnectedDeviceRegistry.Remove(gatt.Device.Address);
                        gatt.Close();

                        _adapter.HandleDisconnectedDevice(false, device);
                        break;
                    }

                    gatt.Close(); // Close GATT regardless, else we can accumulate zombie gatts.
                    Trace.Message("Disconnect. Device not found in registry. Not raising disconnect/lost event.");

                    break;
                // connecting
                case ProfileState.Connecting:
                    Trace.Message("Connecting");
                    break;
                // connected
                case ProfileState.Connected:
                    Trace.Message("Connected");

                    //Try to find the device in the registry so that the same instance is updated
                    if (_adapter.DeviceOperationRegistry.TryGetValue(gatt.Device.Address, out device))
                    {
                        ((Device)device).Update(gatt.Device, gatt, this);

                        //Found so we can remove it
                        _adapter.DeviceOperationRegistry.Remove(gatt.Device.Address);
                    }
                    else
                    {
                        //only for on auto-reconnect (device is not in operation registry)
                        device = new Device(_adapter, gatt.Device, gatt, this, 0);
                    }

                    if (status != GattStatus.Success)
                    {
                        // The aboe error event handles the case where the error happened during a Connect call, which will close out any waiting asyncs.
                        // Android <= 4.4 uses this switch branch when an error occurs during connect
                        Trace.Message($"Error while connecting '{device.Name}'. GattStatus: {status}. ");
                        _adapter.HandleConnectionFail(device, $"GattCallback error: {status}");

                        gatt.Close();
                    }
                    else
                    {
                        _adapter.ConnectedDeviceRegistry[gatt.Device.Address] = device;
                        _adapter.HandleConnectedDevice(device);
                    }

                    break;
                // disconnecting
                case ProfileState.Disconnecting:
                    Trace.Message("Disconnecting");
                    break;
            }
        }

        public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status)
        {
            base.OnServicesDiscovered(gatt, status);

            Trace.Message("OnServicesDiscovered: {0}", status.ToString());

            ServicesDiscovered?.Invoke(this, new ServicesDiscoveredCallbackEventArgs());
        }

        public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, GattStatus status)
        {
            base.OnCharacteristicRead(gatt, characteristic, status);

            Trace.Message("OnCharacteristicRead: value {0}; status {1}", characteristic.GetValue().ToHexString(), status);

            CharacteristicValueUpdated?.Invoke(this, new CharacteristicReadCallbackEventArgs(characteristic));
        }

        public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
        {
            base.OnCharacteristicChanged(gatt, characteristic);

            CharacteristicValueUpdated?.Invoke(this, new CharacteristicReadCallbackEventArgs(characteristic));
        }

        public override void OnCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, GattStatus status)
        {
            base.OnCharacteristicWrite(gatt, characteristic, status);

            Trace.Message("OnCharacteristicWrite: value {0} status {1}", characteristic.GetValue().ToHexString(), status);

            CharacteristicValueWritten?.Invoke(this, new CharacteristicWriteCallbackEventArgs(characteristic, GetExceptionFromGattStatus(status)));
        }

        public override void OnReliableWriteCompleted(BluetoothGatt gatt, GattStatus status)
        {
            base.OnReliableWriteCompleted(gatt, status);

            Trace.Message("OnReliableWriteCompleted: {0}", status);
        }

        public override void OnMtuChanged(BluetoothGatt gatt, int mtu, GattStatus status)
        {
            base.OnMtuChanged(gatt, mtu, status);

            Trace.Message("OnMtuChanged to value: {0}", mtu);

            IDevice device;
            if (!_adapter.ConnectedDeviceRegistry.TryGetValue(gatt.Device.Address, out device))
            {
                Trace.Message("Device for MTU changed is not in connected list. This should not happen.");
            }

            MtuRequested?.Invoke(this, new MtuRequestCallbackEventArgs(device, GetExceptionFromGattStatus(status), mtu));
        }

        public override void OnReadRemoteRssi(BluetoothGatt gatt, int rssi, GattStatus status)
        {
            base.OnReadRemoteRssi(gatt, rssi, status);

            Trace.Message("OnReadRemoteRssi: device {0} status {1} value {2}", gatt.Device.Name, status, rssi);

            IDevice device;
            if (!_adapter.ConnectedDeviceRegistry.TryGetValue(gatt.Device.Address, out device))
            {
                device = new Device(_adapter, gatt.Device, gatt, this, rssi);
                Trace.Message("Rssi updated for device not in connected list. This should not happen.");
            }

            RemoteRssiRead?.Invoke(this, new RssiReadCallbackEventArgs(device, GetExceptionFromGattStatus(status), rssi));
        }

        public override void OnDescriptorWrite(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, GattStatus status)
        {
            base.OnDescriptorWrite(gatt, descriptor, status);

            Trace.Message("OnDescriptorWrite: {0}", descriptor.GetValue()?.ToHexString());

            DescriptorValueWritten?.Invoke(this, new DescriptorCallbackEventArgs(descriptor, GetExceptionFromGattStatus(status)));
        }

        public override void OnDescriptorRead(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, GattStatus status)
        {
            base.OnDescriptorRead(gatt, descriptor, status);

            Trace.Message("OnDescriptorRead: {0}", descriptor.GetValue()?.ToHexString());

            DescriptorValueRead?.Invoke(this, new DescriptorCallbackEventArgs(descriptor, GetExceptionFromGattStatus(status)));
        }

        private Exception GetExceptionFromGattStatus(GattStatus status)
        {
            Exception exception = null;
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
                    exception = new Exception(status.ToString());
                    break;
                case GattStatus.Success:
                    break;
            }

            return exception;
        }
    }
}