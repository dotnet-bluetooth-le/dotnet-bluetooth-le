namespace Plugin.BLE.Abstractions
{
    /// <summary>
    /// Bond/pairing state of a device (currently only used on Android),
    /// see https://developer.android.com/reference/android/bluetooth/BluetoothDevice#getBondState()
    /// </summary>
    public enum DeviceBondState
    {
        /// <summary>
        /// Indicates that the remote device is not bonded (paired),
        /// see https://developer.android.com/reference/android/bluetooth/BluetoothDevice#BOND_NONE
        /// </summary>
        NotBonded,
        /// <summary>
        /// Indicates bonding (pairing) is in progress with the remote device,
        /// see https://developer.android.com/reference/android/bluetooth/BluetoothDevice#BOND_BONDING
        /// </summary>
        Bonding,
        /// <summary>
        /// Indicates the remote device is bonded (paired),
        /// see https://developer.android.com/reference/android/bluetooth/BluetoothDevice#BOND_BONDED
        /// </summary>
        Bonded
    }
}