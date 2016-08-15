namespace Plugin.BLE.Abstractions
{
    /// <summary>
    /// Specifies how a value should be written.
    /// </summary>
    public enum CharacteristicWriteType
    {
        /// <summary>
        /// Value should be written with response.
        /// </summary>
        WithResponse,

        /// <summary>
        /// Value should be written without response.
        /// </summary>
        WithoutResponse
    }
}
