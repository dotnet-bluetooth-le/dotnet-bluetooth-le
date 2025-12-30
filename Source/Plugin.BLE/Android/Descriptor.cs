using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Bluetooth;
using AndroidOS = Android.OS;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Utils;
using Plugin.BLE.Android.CallbackEventArgs;

namespace Plugin.BLE.Android
{
    public class Descriptor : DescriptorBase<BluetoothGattDescriptor>
    {
        private readonly BluetoothGatt _gatt;
        private readonly IGattCallback _gattCallback;

        public override Guid Id => Guid.ParseExact(NativeDescriptor.Uuid.ToString(), "d");

        public override byte[] Value
        {
            get
            {
#if NET6_0_OR_GREATER
#pragma warning disable CA1422 // Validate platform compatibility
#endif
                return NativeDescriptor.GetValue();
#if NET6_0_OR_GREATER
#pragma warning restore CA1422
#endif
            }
        }

        public Descriptor(BluetoothGattDescriptor nativeDescriptor, BluetoothGatt gatt, IGattCallback gattCallback, ICharacteristic characteristic) : base(characteristic, nativeDescriptor)
        {
            _gattCallback = gattCallback;
            _gatt = gatt;
        }

        protected override Task WriteNativeAsync(byte[] data, CancellationToken cancellationToken)
        {
            return TaskBuilder.FromEvent<bool, EventHandler<DescriptorCallbackEventArgs>, EventHandler>(
               execute: () => InternalWrite(data),
               getCompleteHandler: (complete, reject) => ((sender, args) =>
               {
                   if (args.Descriptor.Uuid != NativeDescriptor.Uuid)
                       return;

                   if (args.Exception != null)
                       reject(args.Exception);
                   else
                       complete(true);
               }),
               subscribeComplete: handler => _gattCallback.DescriptorValueWritten += handler,
               unsubscribeComplete: handler => _gattCallback.DescriptorValueWritten -= handler,
               getRejectHandler: reject => ((sender, args) =>
               {
                   reject(new Exception($"Device '{Characteristic.Service.Device.Id}' disconnected while writing descriptor with {Id}."));
               }),
               subscribeReject: handler => _gattCallback.ConnectionInterrupted += handler,
               unsubscribeReject: handler => _gattCallback.ConnectionInterrupted -= handler,
			   token: cancellationToken);
        }

        private void InternalWrite(byte[] data)
        {
#if NET6_0_OR_GREATER
            if (OperatingSystem.IsAndroidVersionAtLeast(33))
#else
            if (AndroidOS.Build.VERSION.SdkInt >= AndroidOS.BuildVersionCodes.Tiramisu)
#endif
            {
                // Use new API for Android 33+
                var result = _gatt.WriteDescriptor(NativeDescriptor, data);
                if (result != 0)
                    throw new Exception("GATT: WRITE descriptor value failed");
            }
            else
            {
                // Use legacy API for Android < 33
                if (!NativeDescriptor.SetValue(data))
                    throw new Exception("GATT: SET descriptor value failed");

                if (!_gatt.WriteDescriptor(NativeDescriptor))
                    throw new Exception("GATT: WRITE descriptor value failed");
            }
        }

        protected override async Task<byte[]> ReadNativeAsync(CancellationToken cancellationToken)
        {
            return await TaskBuilder.FromEvent<byte[], EventHandler<DescriptorCallbackEventArgs>, EventHandler>(
               execute: ReadInternal,
               getCompleteHandler: (complete, reject) => ((sender, args) =>
                  {
                      if (args.Descriptor.Uuid == NativeDescriptor.Uuid)
                      {
#if NET6_0_OR_GREATER
#pragma warning disable CA1422 // Validate platform compatibility
#endif
                          complete(args.Descriptor.GetValue());
#if NET6_0_OR_GREATER
#pragma warning restore CA1422
#endif
                      }
                  }),
               subscribeComplete: handler => _gattCallback.DescriptorValueRead += handler,
               unsubscribeComplete: handler => _gattCallback.DescriptorValueRead -= handler,
               getRejectHandler: reject => ((sender, args) =>
               {
                   reject(new Exception($"Device '{Characteristic.Service.Device.Id}' disconnected while reading descriptor with {Id}."));
               }),
               subscribeReject: handler => _gattCallback.ConnectionInterrupted += handler,
               unsubscribeReject: handler => _gattCallback.ConnectionInterrupted -= handler,
			   token: cancellationToken);
        }

        private void ReadInternal()
        {
            if (!_gatt.ReadDescriptor(NativeDescriptor))
                throw new Exception("GATT: read characteristic FALSE");
        }
    }
}