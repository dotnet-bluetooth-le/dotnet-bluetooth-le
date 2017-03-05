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
    }

    public class GattCallback : BluetoothGattCallback, IGattCallback
    {
        private readonly Adapter _adapter;
        private readonly Device _device;
        public event EventHandler<ServicesDiscoveredCallbackEventArgs> ServicesDiscovered;
        public event EventHandler<CharacteristicReadCallbackEventArgs> CharacteristicValueUpdated;
        public event EventHandler<CharacteristicWriteCallbackEventArgs> CharacteristicValueWritten;
        public event EventHandler<RssiReadCallbackEventArgs> RemoteRssiRead;
        public event EventHandler<DescriptorCallbackEventArgs> DescriptorValueWritten;
        public event EventHandler<DescriptorCallbackEventArgs> DescriptorValueRead;

        public GattCallback(Adapter adapter, Device device)
        {
            _adapter = adapter;
            _device = device;
        }

        public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
        {
            base.OnConnectionStateChange(gatt, status, newState);

            if (!gatt.Device.Address.Equals(_device.BluetoothDevice.Address))
            {
                Trace.Message($"Gatt callback for device {_device.BluetoothDevice.Address} was called for device with address {gatt.Device.Address}. This shoud not happen. Please log an issue.");
                return;
            }

            //just for me
            Trace.Message($"References of parnet device and gatt callback device equal? {ReferenceEquals(_device.BluetoothDevice, gatt.Device).ToString().ToLower()}");
            //Trace.Message($"References of parnet device gatt and callback gatt equal? {ReferenceEquals(_device, gatt.Device).ToString().ToLower()}");

            Trace.Message($"OnConnectionStateChange: GattStatus: {status}");

            switch (newState)
            {
                // disconnected
                case ProfileState.Disconnected:

                    //if (_adapter.DeviceOperationRegistry.TryGetValue(gatt.Device.Address, out device))
                    if (_device.IsOperationRequested)
                    {
                        Trace.Message("Disconnected by user");

                        //Found so we can remove it
                        //_adapter.DeviceOperationRegistry.Remove(gatt.Device.Address);
                        _device.IsOperationRequested = false;
                        _adapter.ConnectedDeviceRegistry.Remove(gatt.Device.Address);
                        gatt.Close();


                        if (status != GattStatus.Success)
                        {
                            // The above error event handles the case where the error happened during a Connect call, which will close out any waiting asyncs.
                            // Android > 5.0 uses this switch branch when an error occurs during connect
                            Trace.Message($"Error while connecting '{_device.Name}'. Not raising disconnect event.");
                            _adapter.HandleConnectionFail(_device, $"GattCallback error: {status}");

                        }
                        else
                        {
                            //we already hadled device error so no need th raise disconnect event(happens when device not in range)
                            _adapter.HandleDisconnectedDevice(true, _device);
                        }
                        break;
                    }

                    //connection must have been lost, bacause our device was not found in the registry but was still connected
                    //if (_adapter.ConnectedDeviceRegistry.TryGetValue(gatt.Device.Address, out device))
                    // {

                    //connection must have been lost, because the callback was not triggered by calling disconnect
                    Trace.Message($"Disconnected '{_device.Name}' by lost connection");

                    _adapter.ConnectedDeviceRegistry.Remove(gatt.Device.Address);
                    gatt.Close();

                    _adapter.HandleDisconnectedDevice(false, _device);
                    //break;
                    //}

                    //gatt.Close(); // Close GATT regardless, else we can accumulate zombie gatts.
                    //Trace.Message("Disconnect. Device not found in registry. Not raising disconnect/lost event.");

                    break;
                // connecting
                case ProfileState.Connecting:
                    Trace.Message("Connecting");
                    break;
                // connected
                case ProfileState.Connected:
                    Trace.Message("Connected");

                    //Try to find the device in the registry so that the same instance is updated
                    //if (_adapter.DeviceOperationRegistry.TryGetValue(gatt.Device.Address, out device))
                    if (_device.IsOperationRequested)
                    {
                        _device.Update(gatt.Device, gatt);//ToDO check if this is required

                        _device.IsOperationRequested = false;
                        //Found so we can remove it
                        //_adapter.DeviceOperationRegistry.Remove(gatt.Device.Address);
                    }
                    else
                    {
                        //ToDo explore this
                        //only for on auto-reconnect (device is not in operation registry)
                        _device.Update(gatt.Device, gatt);
                        //device = new Device(_adapter, gatt.Device, gatt, this, 0);
                    }

                    if (status != GattStatus.Success)
                    {
                        // The above error event handles the case where the error happened during a Connect call, which will close out any waiting asyncs.
                        // Android <= 4.4 uses this switch branch when an error occurs during connect
                        Trace.Message($"Error while connecting '{_device.Name}'. GattStatus: {status}. ");
                        _adapter.HandleConnectionFail(_device, $"GattCallback error: {status}");

                        gatt.Close();
                    }
                    else
                    {
                        _adapter.ConnectedDeviceRegistry[gatt.Device.Address] = _device;
                        _adapter.HandleConnectedDevice(_device);
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

        public override void OnReadRemoteRssi(BluetoothGatt gatt, int rssi, GattStatus status)
        {
            base.OnReadRemoteRssi(gatt, rssi, status);

            Trace.Message("OnReadRemoteRssi: device {0} status {1} value {2}", gatt.Device.Name, status, rssi);

            //IDevice device;
            //if (!_adapter.ConnectedDeviceRegistry.TryGetValue(gatt.Device.Address, out device))
            if(!gatt.Device.Address.Equals(_device.BluetoothDevice.Address))
            {
                //device = new Device(_adapter, gatt.Device, gatt, rssi);
                Trace.Message("Rssi updated for another device in this callback instance. This should not happen.");
            }

            RemoteRssiRead?.Invoke(this, new RssiReadCallbackEventArgs(GetExceptionFromGattStatus(status), rssi));
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