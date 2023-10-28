using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;

namespace Plugin.BLE.Extensions;

public static class TextHelpers
{    
    public static string ToDetailedString(this BluetoothLEAdvertisementReceivedEventArgs btAdv, string name = "na")
    {
        string hexadr = btAdv.BluetoothAddress.ToHexBleAddress();
        StringBuilder sb = new StringBuilder();
        sb.Append(hexadr)
            .Append(", ").Append(name)
            .Append(", ").Append(btAdv.BluetoothAddressType)
            .Append(", ").Append(btAdv.RawSignalStrengthInDBm)
            .Append(", ").Append(btAdv.AdvertisementType);

        if (btAdv.IsConnectable)
        {
            sb.Append(", Connectable");
        }
        if (btAdv.IsScannable)
        {
            sb.Append(", Scannable");
        }
        if (btAdv.IsScanResponse)
        {
            sb.Append(", ScanResponse");
        }
        if (btAdv.IsDirected)
        {
            sb.Append(", Directed");
        }
        return sb.ToString();
    }    

    /// <summary>
    /// Get a string of the BLE address: 48 bit = 6 bytes = 12 Hex chars
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string ToHexBleAddress(this Guid id)
    {
        return id.ToString("N").Substring(20).ToUpperInvariant();
        //return id.ToString()[^12..].ToUpperInvariant(); //Not for netstandard2.0
    }

    /// <summary>
    /// Get a string of the BLE address: 48 bit = 6 bytes = 12 Hex chars
    /// </summary>
    /// <param name="bluetoothAddress"></param>
    /// <returns></returns>
    public static string ToHexBleAddress(this ulong bluetoothAddress)
    {
        return bluetoothAddress.ToString("X12");
    }

    /// <summary>
    /// Method to parse the bluetooth address as a hex string to a UUID
    /// </summary>
    /// <param name="bluetoothAddress">BluetoothLEDevice native device address</param>
    /// <returns>a GUID that is padded left with 0 and the last 6 bytes are the bluetooth address</returns>
    public static Guid ParseDeviceId(this ulong bluetoothAddress)
    {
        var macWithoutColons = bluetoothAddress.ToString("x");
        macWithoutColons = macWithoutColons.PadLeft(12, '0'); //ensure valid length
        var deviceGuid = new byte[16];
        Array.Clear(deviceGuid, 0, 16);
        var macBytes = Enumerable.Range(0, macWithoutColons.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(macWithoutColons.Substring(x, 2), 16))
            .ToArray();
        macBytes.CopyTo(deviceGuid, 10);
        return new Guid(deviceGuid);
    }

    /// <summary>
    /// Convert 12 chars hex string = 6 bytes = 48 bits to Guid used in this plugin
    /// </summary>
    /// <param name="macWithoutColons"></param>
    /// <returns></returns>
    public static Guid ToBleDeviceGuid(this string macWithoutColons)
    {
        macWithoutColons = macWithoutColons.PadLeft(12, '0'); //ensure valid length
        var deviceGuid = new byte[16];
        Array.Clear(deviceGuid, 0, 16);
        var macBytes = Enumerable.Range(0, macWithoutColons.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(macWithoutColons.Substring(x, 2), 16))
            .ToArray();
        macBytes.CopyTo(deviceGuid, 10);
        return new Guid(deviceGuid);
    }

    public static Guid ToBleDeviceGuidFromId(this string idWithColons)
    {
        //example: Bluetooth#Bluetoothe4:aa:ea:cd:28:00-70:bf:92:06:e1:9e
        var nocolons = idWithColons.Replace(":", "");
        return ToBleDeviceGuid(nocolons.Substring(nocolons.Length-12, 12));
    }


    public static ulong ToBleAddress(this Guid deviceGuid)
    {
        //convert GUID to string and take last 12 characters as MAC address
        var guidString = deviceGuid.ToString("N").Substring(20);
        var bluetoothAddress = Convert.ToUInt64(guidString, 16);
        return bluetoothAddress;
    }
}
