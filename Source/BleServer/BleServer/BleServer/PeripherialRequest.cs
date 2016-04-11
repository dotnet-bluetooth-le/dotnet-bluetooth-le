namespace BleServer
{
    public class PeripherialRequest
    {
        public PeripherialRequest(int requestId, int offset, PeripherialRequestType type, byte[] value = null)
        {
            RequestId = requestId;
            Offset = offset;
            Type = type;
            Value = value;
        }

        public byte[] Value { get; private set; }
        public int RequestId { get; private set; }
        public int Offset { get; private set; }
        public PeripherialRequestType Type { get; private set; }
    }

    public enum PeripherialRequestType
    {
        Read,
        Write
    }
}