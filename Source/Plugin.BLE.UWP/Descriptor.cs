using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography;

namespace Plugin.BLE.UWP
{
    public class Descriptor : DescriptorBase
    {
        private readonly GattDescriptor _nativeDescriptor;
        /// <summary>
        /// The locally stored value of a descriptor updated after a
        /// notification or a read
        /// </summary>
        private byte[] _value;
        public override Guid Id => _nativeDescriptor.Uuid;
        public override byte[] Value => _value ?? new byte[0];

        public Descriptor(GattDescriptor nativeDescriptor, ICharacteristic characteristic) : base(characteristic)
        {
            _nativeDescriptor = nativeDescriptor;
        }

        protected override async Task<byte[]> ReadNativeAsync()
        {
            var readResult = await _nativeDescriptor.ReadValueAsync();
            
            //ToDo throw
            switch (readResult.Status)
            {
                case GattCommunicationStatus.Success:
                    Trace.Message("Descriptor Read Successfully");
                    break;
                case GattCommunicationStatus.Unreachable:
                case GattCommunicationStatus.ProtocolError:
                case GattCommunicationStatus.AccessDenied:
                default:
                    Trace.Message("Descriptor Read Failed");
                    break;
            }

            return _value = readResult.Value.ToArray();
        }

        protected override async Task WriteNativeAsync(byte[] data)
        {
            //method contains no option for writing with response, so always write
            //without response
            var writeResult = await _nativeDescriptor.WriteValueAsync(CryptographicBuffer.CreateFromByteArray(data));

            // ToDO throw error
            switch (writeResult)
            {
                case GattCommunicationStatus.Success:
                    Trace.Message("Descriptor Write Successfully");
                    break;
                case GattCommunicationStatus.Unreachable:
                case GattCommunicationStatus.ProtocolError:
                case GattCommunicationStatus.AccessDenied:
                default:
                    Trace.Message("Descriptor Write Failed");
                    break;
            }
        }
    }
}
