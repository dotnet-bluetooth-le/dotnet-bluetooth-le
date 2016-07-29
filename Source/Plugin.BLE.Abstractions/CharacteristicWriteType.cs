using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.BLE.Abstractions
{

    public enum CharacteristicWriteType
    {
        WithResponse = 0,
        WithoutResponse = 1
    }


    /* IOS:
    //
    // Summary:
    //     Enumerates the possible types of writes to a characteristic's value.
    [Native]
    public enum CBCharacteristicWriteType : long
    {
        WithResponse = 0,
        WithoutResponse = 1
    }
    */

    /* ANDROID:
    [Flags]
    public enum GattWriteType
    {
        //
        // Summary:
        //     ///
        //     Wrtite characteristic without requiring a response by the remote device ///
        //     ///
        NoResponse = 1,
        //
        // Summary:
        //     ///
        //     Write characteristic, requesting acknoledgement by the remote device ///
        //     ///
        Default = 2,
        //
        // Summary:
        //     ///
        //     Write characteristic including authentication signature ///
        //     ///
        Signed = 4
    }
    */

}
