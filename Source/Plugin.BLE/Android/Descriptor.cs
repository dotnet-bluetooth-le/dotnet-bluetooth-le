using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Bluetooth;
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

        public override byte[] Value => NativeDescriptor.GetValue();

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
            if (!NativeDescriptor.SetValue(data))
                throw new Exception("GATT: SET descriptor value failed");

            if (!_gatt.WriteDescriptor(NativeDescriptor))
                throw new Exception("GATT: WRITE descriptor value failed");
        }

        protected override async Task<byte[]> ReadNativeAsync(CancellationToken cancellationToken)
        {
            return await TaskBuilder.FromEvent<byte[], EventHandler<DescriptorCallbackEventArgs>, EventHandler>(
               execute: ReadInternal,
               getCompleteHandler: (complete, reject) => ((sender, args) =>
                  {
                      if (args.Descriptor.Uuid == NativeDescriptor.Uuid)
                      {
                          complete(args.Descriptor.GetValue());
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