using System;
using System.Threading.Tasks;
using Android.Bluetooth;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BLE.Android.CallbackEventArgs;
using Plugin.BLE.Extensions;
using Plugin.BLE.Abstractions.Utils;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Android
{
    public class WriteTransaction: IWriteTransaction
    {
        private readonly BluetoothGattCharacteristic nativeCharacteristic;
        private readonly BluetoothGatt gatt;
        private readonly IGattCallback gattCallback;

        internal WriteTransaction(BluetoothGattCharacteristic nativeCharacteristic, BluetoothGatt gatt, IGattCallback gattCallback)
        {
            this.nativeCharacteristic = nativeCharacteristic;
            this.gatt = gatt;
            this.gattCallback = gattCallback;
        }

        public Guid Id => Guid.Parse(nativeCharacteristic.Uuid.ToString());
        public string Uuid => nativeCharacteristic.Uuid.ToString();

        public bool Begin()
        {
            return gatt.BeginReliableWrite();
        }

        private void InternalWrite(byte[] data)
        {
            if (!nativeCharacteristic.SetValue(data))
            {
                throw new CharacteristicReadException("Gatt characteristic set value FAILED.");
            }

            Trace.Message("Write {0}", Id);

            if (!gatt.WriteCharacteristic(nativeCharacteristic))
            {
                throw new CharacteristicReadException("Gatt write characteristic FAILED.");
            }
        }

        public async Task<bool> Write(byte[] data)
        {
            return await WriteNativeAsync(data, CharacteristicWriteType.WithResponse);
        }

        protected async Task<bool> WriteNativeAsync(byte[] data, CharacteristicWriteType writeType)
        {
            nativeCharacteristic.WriteType = writeType.ToNative();

            return await TaskBuilder.FromEvent<bool, EventHandler<CharacteristicWriteCallbackEventArgs>, EventHandler>(
                execute: () => InternalWrite(data),
                getCompleteHandler: (complete, reject) => ((sender, args) =>
                {
                    if (args.Characteristic.Uuid == nativeCharacteristic.Uuid)
                    {
                        complete(args.Exception == null);
                    }
                }),
               subscribeComplete: handler => gattCallback.CharacteristicValueWritten += handler,
               unsubscribeComplete: handler => gattCallback.CharacteristicValueWritten -= handler,
               getRejectHandler: reject => ((sender, args) =>
               {
                   reject(new Exception($"Device disconnected while writing characteristic with {Id}."));
               }),
               subscribeReject: handler => gattCallback.ConnectionInterrupted += handler,
               unsubscribeReject: handler => gattCallback.ConnectionInterrupted -= handler);
        }

        public async Task<bool> Commit()
        {
            return await TaskBuilder.FromEvent<bool, EventHandler<ReliableWriteCallbackEventArgs>, EventHandler>(
                execute: () => gatt.ExecuteReliableWrite(),
                getCompleteHandler: (complete, reject) => ((sender, args) =>
                {
                    complete(args.Exception == null);
                }),
               subscribeComplete: handler => gattCallback.ReliableWriteResult += handler,
               unsubscribeComplete: handler => gattCallback.ReliableWriteResult -= handler,
               getRejectHandler: reject => ((sender, args) =>
               {
                   reject(new Exception($"Device disconnected while writing characteristic with {Id}."));
               }),
               subscribeReject: handler => gattCallback.ConnectionInterrupted += handler,
               unsubscribeReject: handler => gattCallback.ConnectionInterrupted -= handler);
        }


        public void RollBack()
        {
            gatt.AbortReliableWrite();
        }
    }

}