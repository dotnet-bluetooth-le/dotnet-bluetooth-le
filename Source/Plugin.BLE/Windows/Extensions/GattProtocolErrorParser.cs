// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


// This file has been copied from CommunityToolkit.WinUI.Connectivity
// https://github.com/CommunityToolkit/WindowsCommunityToolkit/blob/main/Microsoft.Toolkit.Uwp.Connectivity/BluetoothLEHelper/GattProtocolErrorParser.cs
// Date: 2023-10-28 with these changes applied:
// 1. These comments are added
// 2. namespace Microsoft.Toolkit.Uwp.Connectivity -> Plugin.BLE.Extensions
// 3. public static class GattProtocolErrorParser -> internal static class GattProtocolErrorParser

using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Plugin.BLE.Extensions
{
    /// <summary>
    /// Helper function when working with <see cref="GattProtocolError" />
    /// </summary>
    internal static class GattProtocolErrorParser
    {
        /// <summary>
        /// Helper to convert an Gatt error value into a string
        /// </summary>
        /// <param name="errorValue"> the byte error value.</param>
        /// <returns>String representation of the error</returns>
        public static string GetErrorString(this byte? errorValue)
        {
            var errorString = "Protocol Error";

            if (errorValue.HasValue == false)
            {
                return errorString;
            }

            if (errorValue == GattProtocolError.AttributeNotFound)
            {
                return "Attribute Not Found";
            }

            if (errorValue == GattProtocolError.AttributeNotLong)
            {
                return "Attribute Not Long";
            }

            if (errorValue == GattProtocolError.InsufficientAuthentication)
            {
                return "Insufficient Authentication";
            }

            if (errorValue == GattProtocolError.InsufficientAuthorization)
            {
                return "Insufficient Authorization";
            }

            if (errorValue == GattProtocolError.InsufficientEncryption)
            {
                return "Insufficient Encryption";
            }

            if (errorValue == GattProtocolError.InsufficientEncryptionKeySize)
            {
                return "Insufficient Encryption Key Size";
            }

            if (errorValue == GattProtocolError.InsufficientResources)
            {
                return "Insufficient Resources";
            }

            if (errorValue == GattProtocolError.InvalidAttributeValueLength)
            {
                return "Invalid Attribute Value Length";
            }

            if (errorValue == GattProtocolError.InvalidHandle)
            {
                return "Invalid Handle";
            }

            if (errorValue == GattProtocolError.InvalidOffset)
            {
                return "Invalid Offset";
            }

            if (errorValue == GattProtocolError.InvalidPdu)
            {
                return "Invalid Pdu";
            }

            if (errorValue == GattProtocolError.PrepareQueueFull)
            {
                return "Prepare Queue Full";
            }

            if (errorValue == GattProtocolError.ReadNotPermitted)
            {
                return "Read Not Permitted";
            }

            if (errorValue == GattProtocolError.RequestNotSupported)
            {
                return "Request Not Supported";
            }

            if (errorValue == GattProtocolError.UnlikelyError)
            {
                return "UnlikelyError";
            }

            if (errorValue == GattProtocolError.UnsupportedGroupType)
            {
                return "Unsupported Group Type";
            }

            if (errorValue == GattProtocolError.WriteNotPermitted)
            {
                return "Write Not Permitted";
            }

            return errorString;
        }
    }
}