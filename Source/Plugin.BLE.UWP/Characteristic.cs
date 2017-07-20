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
        public override byte[] Value
        {
            get
            {
                //return empty array if value is equal to null
                if (_value == null)
                {
                    return new byte[0];
                }
                return _value;
            }
        }


        public Characteristic(GattCharacteristic nativeCharacteristic, IService service) : base(service)
        {
            _nativeCharacteristic = nativeCharacteristic;
        }       

        protected async override Task<IList<IDescriptor>> GetDescriptorsNativeAsync()
        {
            var nativeDescriptors = (await _nativeCharacteristic.GetDescriptorsAsync()).Descriptors;
            var descriptorList = new List<IDescriptor>();
            //convert to generic descriptors
            foreach (var nativeDescriptor in nativeDescriptors)
            {
                var descriptor = new Descriptor(nativeDescriptor, this);
                descriptorList.Add(descriptor);
            }
            return descriptorList;
        }

        protected async override Task<byte[]> ReadNativeAsync()
        {
            var readResult = (await _nativeCharacteristic.ReadValueAsync()).Value.ToArray();
            _value = readResult;
            return readResult;
        }

        protected async override Task StartUpdatesNativeAsync()
        {
            _nativeCharacteristic.ValueChanged += OnCharacteristicValueChanged;
            var result = await _nativeCharacteristic.WriteClientCharacteristicConfigurationDescriptorWithResultAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
            //output trace message with status of update
            if (result.Status == GattCommunicationStatus.Success)
            {
                Trace.Message("Start Updates Successful");
            }
            else if (result.Status == GattCommunicationStatus.AccessDenied)
            {
                Trace.Message("Incorrect permissions to start updates");
            }
            else if (result.Status == GattCommunicationStatus.ProtocolError && result.ProtocolError != null)
            {
                Trace.Message("Start updates returned with error: {0}", parseError(result.ProtocolError));
            }
            else if (result.Status == GattCommunicationStatus.ProtocolError)
            {
                Trace.Message("Start updates returned with unknown error");
            }
            else if (result.Status == GattCommunicationStatus.Unreachable)
            {
                Trace.Message("Characteristic properties are unreachable");
            }
        }

        protected async override Task StopUpdatesNativeAsync()
        {
            _nativeCharacteristic.ValueChanged -= OnCharacteristicValueChanged;
            var result  = await _nativeCharacteristic.WriteClientCharacteristicConfigurationDescriptorWithResultAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
            if (result.Status == GattCommunicationStatus.Success)
            {
                Trace.Message("Stop Updates Successful");
            }
            else if (result.Status == GattCommunicationStatus.AccessDenied)
            {
                Trace.Message("Incorrect permissions to stop updates");
            }
            else if (result.Status == GattCommunicationStatus.ProtocolError && result.ProtocolError != null)
            {
                Trace.Message("Stop updates returned with error: {0}", parseError(result.ProtocolError));
            }
            else if (result.Status == GattCommunicationStatus.ProtocolError)
            {
                Trace.Message("Stop updates returned with unknown error");
            }
            else if (result.Status == GattCommunicationStatus.Unreachable)
            {
                Trace.Message("Characteristic properties are unreachable");
            }
        }

        protected async override Task<bool> WriteNativeAsync(byte[] data, CharacteristicWriteType writeType)
        {
            //print errors if error and write with response
            if(writeType == CharacteristicWriteType.WithResponse)
            {
                var result = await _nativeCharacteristic.WriteValueWithResultAsync(CryptographicBuffer.CreateFromByteArray(data));
                if (result.Status == GattCommunicationStatus.Success) {
                    Trace.Message("Write successful");
                    return true;
                }
                else if (result.Status == GattCommunicationStatus.AccessDenied)
                {
                    Trace.Message("Incorrect permissions to stop updates");
                }
                else if (result.Status == GattCommunicationStatus.ProtocolError && result.ProtocolError != null)
                {
                    Trace.Message("Write Characteristic returned with error: {0}", parseError(result.ProtocolError));
                }
                else if (result.Status == GattCommunicationStatus.ProtocolError)
                {
                    Trace.Message("Write Characteristic returned with unknown error");
                }
                else if (result.Status == GattCommunicationStatus.Unreachable)
                {
                    Trace.Message("Characteristic write is unreachable");
                }
                return false;
            }
            var status = await _nativeCharacteristic.WriteValueAsync(CryptographicBuffer.CreateFromByteArray(data), GattWriteOption.WriteWithoutResponse);
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
