using System;
using Android.Bluetooth;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Android.CallbackEventArgs;

namespace Plugin.BLE.Android
{
    public interface IGattCallback
    {
        event EventHandler<ServicesDiscoveredCallbackEventArgs> ServicesDiscovered;
        event EventHandler<CharacteristicReadCallbackEventArgs> CharacteristicValueUpdated;
        event EventHandler<CharacteristicWriteCallbackEventArgs> CharacteristicValueWritten;
        event EventHandler<RssiReadCallbackEventArgs> RemoteRssiRead;
    }

    public class GattCallback : BluetoothGattCallback, IGattCallback
    {
        private readonly Adapter _adapter;
        public event EventHandler<ServicesDiscoveredCallbackEventArgs> ServicesDiscovered;
        public event EventHandler<CharacteristicReadCallbackEventArgs> CharacteristicValueUpdated;
        public event EventHandler<CharacteristicWriteCallbackEventArgs> CharacteristicValueWritten;
        public event EventHandler<RssiReadCallbackEventArgs> RemoteRssiRead;

        public GattCallback(Adapter adapter)
        {
            _adapter = adapter;
        }

        public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
        {
            base.OnConnectionStateChange(gatt, status, newState);
            IDevice device;

            if (status != GattStatus.Success)
            {
                Trace.Message($"OnConnectionStateChange: GattCallback error: {status}");
                device = new Device(gatt.Device, gatt, this, 0);
                _adapter.HandleConnectionFail(device, $"GattCallback error: {status}");
                // We don't return. Allowing to fall-through to the SWITCH, which will assume a disconnect, close GATT and clean up.
                // The above error event handles the case where the error happened during a Connect call, which will close out any waiting asyncs.
            }
            else
            {
                Trace.Message("GattCallback state: {0}", newState.ToString());
            }

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

                        _adapter.HandleDisconnectedDevice(true, device);
                        break;
                    }

                    //connection must have been lost, bacause our device was not found in the registry but was still connected
                    if (_adapter.ConnectedDeviceRegistry.TryGetValue(gatt.Device.Address, out device))
                    {
                        Trace.Message("Disconnected by lost connection");

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
                        device = new Device(gatt.Device, gatt, this, 0);
                    }

                    _adapter.ConnectedDeviceRegistry[gatt.Device.Address] = device;
                    _adapter.HandleConnectedDevice(device);

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

        public override void OnDescriptorRead(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, GattStatus status)
        {
            base.OnDescriptorRead(gatt, descriptor, status);

            Trace.Message("OnDescriptorRead: {0}", descriptor.ToString());

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

            var isSuccessful = false;
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
                    break;
                case GattStatus.Success:
                    isSuccessful = true;
                    break;
            }

            CharacteristicValueWritten?.Invoke(this, new CharacteristicWriteCallbackEventArgs(characteristic, isSuccessful));
        }

        public override void OnReliableWriteCompleted(BluetoothGatt gatt, GattStatus status)
        {
            base.OnReliableWriteCompleted(gatt, status);

            Trace.Message("OnReliableWriteCompleted: {0}", status);
        }

        public override void OnReadRemoteRssi(BluetoothGatt gatt, int rssi, GattStatus status)
        {
            base.OnReadRemoteRssi(gatt, rssi, status);

            Trace.Message("OnReadRemoteRssi: device {0} status {1} value {2}", gatt.Device.Name, status, rssi);

            IDevice device;
            if (!_adapter.ConnectedDeviceRegistry.TryGetValue(gatt.Device.Address, out device))
            {
                device = new Device(gatt.Device, gatt, this, rssi);
                Trace.Message("Rssi updated for device not in connected list. This should not happen.");
            }

            Exception error = null;
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
                    error = new Exception(status.ToString());
                    break;
                case GattStatus.Success:
                    break;
            }

            var args = new RssiReadCallbackEventArgs(device, error, rssi);

            RemoteRssiRead?.Invoke(this, args);
        }
    }


}

