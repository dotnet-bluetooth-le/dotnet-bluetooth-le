using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
#if WINDOWS_UWP
using Microsoft.Toolkit.Uwp.Connectivity;
#else
using CommunityToolkit.WinUI.Connectivity;
#endif
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Exceptions;

namespace Plugin.BLE.Extensions
{
    public static class GattResultExtensions
    {
        public static void ThrowIfError(this GattWriteResult result, [CallerMemberName]string tag = null)
            => result.Status.ThrowIfError(tag, result.ProtocolError);

        public static void ThrowIfError(this GattCharacteristicsResult result, [CallerMemberName]string tag = null)
            => result.Status.ThrowIfError(tag, result.ProtocolError);

        public static void ThrowIfError(this GattDescriptorsResult result, [CallerMemberName]string tag = null)
            => result.Status.ThrowIfError(tag, result.ProtocolError);

        public static void ThrowIfError(this GattDeviceServicesResult result, [CallerMemberName]string tag = null)
            => result.Status.ThrowIfError(tag, result.ProtocolError);


        public static byte[] GetValueOrThrowIfError(this GattReadResult result, [CallerMemberName]string tag = null)
        {
            var errorMessage = result.Status.GetErrorMessage(tag, result.ProtocolError);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                throw new CharacteristicReadException(errorMessage);
            }

            return result.Value?.ToArray() ?? new byte[0];
        }

        public static void ThrowIfError(this GattCommunicationStatus status, [CallerMemberName]string tag = null, byte? protocolError = null)
        {
            var errorMessage = status.GetErrorMessage(tag, protocolError);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                throw new Exception(errorMessage);
            }
        }

        private static string GetErrorMessage(this GattCommunicationStatus status, string tag, byte? protocolError)
        {
            switch (status)
            {
                //output trace message with status of update
                case GattCommunicationStatus.Success:
                    Trace.Message($"[{tag}] success.");
                    return null;
                case GattCommunicationStatus.ProtocolError when protocolError != null:
                    return $"[{tag}] failed with status: {status} and protocol error {protocolError.GetErrorString()}";
                case GattCommunicationStatus.AccessDenied:
                case GattCommunicationStatus.ProtocolError:
                case GattCommunicationStatus.Unreachable:
                    return $"[{tag}] failed with status: {status}";
            }

            return null;
        }
    }
}