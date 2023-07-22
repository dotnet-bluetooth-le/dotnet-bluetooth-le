namespace Plugin.BLE.Abstractions
{
    /// <summary>
    /// This enum is currently being used on Android only.
    /// It will be mapped to Android's gattConnectionPriorities,
    /// see https://developer.android.com/reference/android/bluetooth/BluetoothGatt.html#requestConnectionPriority(int)
    /// </summary>
    public enum ConnectionInterval
    {
        /// <summary>
        /// Normal (default) connection interval.
        /// This is mapped to CONNECTION_PRIORITY_BALANCED,
        /// see https://developer.android.com/reference/android/bluetooth/BluetoothGatt#CONNECTION_PRIORITY_BALANCED
        /// </summary>
        Normal = 0,
        /// <summary>
        /// High-priority connection interval (low latency connection).
        /// This is mapped to CONNECTION_PRIORITY_HIGH,
        /// see https://developer.android.com/reference/android/bluetooth/BluetoothGatt#CONNECTION_PRIORITY_HIGH
        /// </summary>
        High = 1,
        /// <summary>
        /// Low-priority connection interval (low power, reduced data rate).
        /// This is mapped to CONNECTION_PRIORITY_LOW_POWER,
        /// see https://developer.android.com/reference/android/bluetooth/BluetoothGatt#CONNECTION_PRIORITY_LOW_POWER
        /// </summary>
        Low = 2
    }
}
