﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreBluetooth;
using Foundation;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BLE.Abstractions.Extensions;
using Plugin.BLE.Abstractions.Utils;
using Plugin.BLE.Extensions;

namespace Plugin.BLE.iOS
{
    public class Characteristic : CharacteristicBase
    {
        private readonly CBCharacteristic _nativeCharacteristic;
        private readonly CBPeripheral _parentDevice;
        public override event EventHandler<CharacteristicUpdatedEventArgs> ValueUpdated;

        public override Guid Id => _nativeCharacteristic.UUID.GuidFromUuid();
        public override string Uuid => _nativeCharacteristic.UUID.ToString();
        public override byte[] Value => _nativeCharacteristic.Value?.ToArray();
        public override CharacteristicPropertyType Properties => (CharacteristicPropertyType)(int)_nativeCharacteristic.Properties;

        public Characteristic(CBCharacteristic nativeCharacteristic, CBPeripheral parentDevice)
        {
            _nativeCharacteristic = nativeCharacteristic;
            _parentDevice = parentDevice;
        }

        protected override IList<IDescriptor> GetDescriptorsNative()
        {
            return _nativeCharacteristic.Descriptors.Select(item => new Descriptor(item)).Cast<IDescriptor>().ToList();
        }

        protected override Task<byte[]> ReadNativeAsync()
        {
            return TaskBuilder.FromEvent<byte[], EventHandler<CBCharacteristicEventArgs>>(
                    execute: () => _parentDevice.ReadValue(_nativeCharacteristic),
                    getCompleteHandler: complete => (sender, args) =>
                    {
                        if (args.Characteristic.UUID != _nativeCharacteristic.UUID)
                            return;

                        if (args.Error != null)
                            throw new CharacteristicReadException($"Read async error: {args.Error.Description}");

                        Trace.Message($"Read characterteristic value {Value.ToHexString()}");

                        complete(Value);
                    },
                    subscribeComplete: handler => _parentDevice.UpdatedCharacterteristicValue += handler,
                    unsubscribeComplete: handler => _parentDevice.UpdatedCharacterteristicValue -= handler);
        }

        protected override Task<bool> WriteNativeAsync(byte[] data)
        {
            Task<bool> task;

            CBCharacteristicWriteType nativeWriteType = WriteType.ToNative();
            if (nativeWriteType == CBCharacteristicWriteType.WithResponse)
            {
                task = TaskBuilder.FromEvent<bool, EventHandler<CBCharacteristicEventArgs>>(
                    execute: () => { },
                    getCompleteHandler: complete => (sender, args) =>
                    {
                        if (args.Characteristic.UUID != _nativeCharacteristic.UUID)
                            return;

                        complete(args.Error == null);
                    },
                    subscribeComplete: handler => _parentDevice.WroteCharacteristicValue += handler,
                    unsubscribeComplete: handler => _parentDevice.WroteCharacteristicValue -= handler);
            }
            else
            {
                task = Task.FromResult(true);
            }

            var nsdata = NSData.FromArray(data);
            _parentDevice.WriteValue(nsdata, _nativeCharacteristic, nativeWriteType);

            return task;
        }

        protected override void StartUpdatesNative()
        {
            _parentDevice.UpdatedCharacterteristicValue += UpdatedNotify;
            _parentDevice.SetNotifyValue(true, _nativeCharacteristic);
            Trace.Message("Characteristic.StartUpdatesNative: successful");
        }

        protected override void StopUpdatesNative()
        {
            _parentDevice.SetNotifyValue(false, _nativeCharacteristic);
            _parentDevice.UpdatedCharacterteristicValue -= UpdatedNotify;
            Trace.Message("Characteristic.StopUpdatesNative: successful");
        }

        private void UpdatedNotify(object sender, CBCharacteristicEventArgs e)
        {
            if (e.Characteristic.UUID == _nativeCharacteristic.UUID)
            {
                ValueUpdated?.Invoke(this, new CharacteristicUpdatedEventArgs(this));
            }
        }
    }
}