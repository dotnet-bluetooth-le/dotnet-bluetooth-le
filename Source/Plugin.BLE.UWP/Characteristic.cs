using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Security.Cryptography;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;

namespace Plugin.BLE.UWP
{
    public class Characteristic : CharacteristicBase
    {
        private readonly GattCharacteristic _nativeCharacteristic;

        /// <summary>
        /// Value of the characteristic to be stored locally after
        /// update notification or read
        /// </summary>
        private byte[] _value;
        public override Guid Id => _nativeCharacteristic.Uuid;
        public override string Uuid => _nativeCharacteristic.Uuid.ToString();
        public override CharacteristicPropertyType Properties => (CharacteristicPropertyType)(int)_nativeCharacteristic.CharacteristicProperties;

        public override event EventHandler<CharacteristicUpdatedEventArgs> ValueUpdated;
        public override byte[] Value => _value ?? new byte[0]; // return empty array if value is equal to null

        public override string Name => string.IsNullOrEmpty(_nativeCharacteristic.UserDescription)
            ? base.Name
            : _nativeCharacteristic.UserDescription;

        public Characteristic(GattCharacteristic nativeCharacteristic, IService service) : base(service)
        {
            _nativeCharacteristic = nativeCharacteristic;
        }

        protected override async Task<IReadOnlyList<IDescriptor>> GetDescriptorsNativeAsync()
        {
            var nativeDescriptors = (await _nativeCharacteristic.GetDescriptorsAsync()).Descriptors;

            //convert to generic descriptors
            return nativeDescriptors.Select(nativeDescriptor => new Descriptor(nativeDescriptor, this)).Cast<IDescriptor>().ToList();
        }

        protected override async Task<byte[]> ReadNativeAsync()
        {
            var readResult = await _nativeCharacteristic.ReadValueAsync();
            switch (readResult.Status)
            {
                case GattCommunicationStatus.Success:
                    return _value = readResult.Value.ToArray();
                case GattCommunicationStatus.Unreachable:
                case GattCommunicationStatus.ProtocolError:
                case GattCommunicationStatus.AccessDenied:
                    throw new CharacteristicReadException($"Error while reading characteristic. Status: {readResult.Status}");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override async Task StartUpdatesNativeAsync()
        {
            _nativeCharacteristic.ValueChanged -= OnCharacteristicValueChanged;
            _nativeCharacteristic.ValueChanged += OnCharacteristicValueChanged;

            var result = await _nativeCharacteristic.WriteClientCharacteristicConfigurationDescriptorWithResultAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);

            //ToDo throw
            switch (result.Status)
            {
                //output trace message with status of update
                case GattCommunicationStatus.Success:
                    Trace.Message("Start Updates Successful");
                    break;
                case GattCommunicationStatus.AccessDenied:
                    Trace.Message("Incorrect permissions to start updates");
                    break;
                case GattCommunicationStatus.ProtocolError when result.ProtocolError != null:
                    Trace.Message("Start updates returned with error: {0}", parseError(result.ProtocolError));
                    break;
                case GattCommunicationStatus.ProtocolError:
                    Trace.Message("Start updates returned with unknown error");
                    break;
                case GattCommunicationStatus.Unreachable:
                    Trace.Message("Characteristic properties are unreachable");
                    break;
            }
        }

        protected override async Task StopUpdatesNativeAsync()
        {
            _nativeCharacteristic.ValueChanged -= OnCharacteristicValueChanged;

            var result = await _nativeCharacteristic.WriteClientCharacteristicConfigurationDescriptorWithResultAsync(GattClientCharacteristicConfigurationDescriptorValue.None);

            //ToDo throw
            switch (result.Status)
            {
                case GattCommunicationStatus.Success:
                    Trace.Message("Stop Updates Successful");
                    break;
                case GattCommunicationStatus.AccessDenied:
                    Trace.Message("Incorrect permissions to stop updates");
                    break;
                case GattCommunicationStatus.ProtocolError when result.ProtocolError != null:
                    Trace.Message("Stop updates returned with error: {0}", parseError(result.ProtocolError));
                    break;
                case GattCommunicationStatus.ProtocolError:
                    Trace.Message("Stop updates returned with unknown error");
                    break;
                case GattCommunicationStatus.Unreachable:
                    Trace.Message("Characteristic properties are unreachable");
                    break;
            }
        }

        protected override async Task<bool> WriteNativeAsync(byte[] data, CharacteristicWriteType writeType)
        {
            //print errors if error and write with response
            if (writeType == CharacteristicWriteType.WithResponse)
            {
                var result = await _nativeCharacteristic.WriteValueWithResultAsync(CryptographicBuffer.CreateFromByteArray(data));

                //Todo throw
                switch (result.Status)
                {
                    case GattCommunicationStatus.Success:
                        Trace.Message("Write successful");
                        return true;
                    case GattCommunicationStatus.AccessDenied:
                        Trace.Message("Incorrect permissions to stop updates");
                        break;
                    case GattCommunicationStatus.ProtocolError when result.ProtocolError != null:
                        Trace.Message("Write Characteristic returned with error: {0}", parseError(result.ProtocolError));
                        break;
                    case GattCommunicationStatus.ProtocolError:
                        Trace.Message("Write Characteristic returned with unknown error");
                        break;
                    case GattCommunicationStatus.Unreachable:
                        Trace.Message("Characteristic write is unreachable");
                        break;
                }

                return false;
            }

            var status = await _nativeCharacteristic.WriteValueAsync(CryptographicBuffer.CreateFromByteArray(data), GattWriteOption.WriteWithoutResponse);

            // ToDo switch and throw
            if (status == GattCommunicationStatus.Success)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Handler for when the characteristic value is changed. Updates the
        /// stored value
        /// </summary>
        private void OnCharacteristicValueChanged(object sender, GattValueChangedEventArgs e)
        {
            _value = e.CharacteristicValue.ToArray();  //add value to array
            ValueUpdated?.Invoke(this, new CharacteristicUpdatedEventArgs(this));
        }

        /// <summary>
        /// Used to parse errors returned by UWP methods in byte form
        /// </summary>
        /// <param name="err">The byte describing the type of error</param>
        /// <returns>Returns a string with the name of an error byte</returns>
        private string parseError(byte? err)
        {
            if (err == GattProtocolError.AttributeNotFound)
            {
                return "Attribute Not Found";
            }
            if (err == GattProtocolError.AttributeNotLong)
            {
                return "Attribute Not Long";
            }
            if (err == GattProtocolError.InsufficientAuthentication)
            {
                return "Insufficient Authentication";
            }
            if (err == GattProtocolError.InsufficientAuthorization)
            {
                return "Insufficient Authorization";
            }
            if (err == GattProtocolError.InsufficientEncryption)
            {
                return "Insufficient Encryption";
            }
            if (err == GattProtocolError.InsufficientEncryptionKeySize)
            {
                return "Insufficient Encryption Key Size";
            }
            if (err == GattProtocolError.InsufficientResources)
            {
                return "Insufficient Resource";
            }
            if (err == GattProtocolError.InvalidAttributeValueLength)
            {
                return "Invalid Attribute Value Length";
            }
            if (err == GattProtocolError.InvalidHandle)
            {
                return "Invalid Handle";
            }
            if (err == GattProtocolError.InvalidOffset)
            {
                return "Invalid Offset";
            }
            if (err == GattProtocolError.InvalidPdu)
            {
                return "Invalid PDU";
            }
            if (err == GattProtocolError.PrepareQueueFull)
            {
                return "Prepare Queue Full";
            }
            if (err == GattProtocolError.ReadNotPermitted)
            {
                return "Read Not Permitted";
            }
            if (err == GattProtocolError.RequestNotSupported)
            {
                return "Request Not Supported";
            }
            if (err == GattProtocolError.UnlikelyError)
            {
                return "Unlikely Error";
            }
            if (err == GattProtocolError.UnsupportedGroupType)
            {
                return "Unsupported Group Type";
            }
            if (err == GattProtocolError.WriteNotPermitted)
            {
                return "Write Not Permitted";
            }
            return null;
        }
    }
}
