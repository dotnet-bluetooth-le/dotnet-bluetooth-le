using System.Threading.Tasks;

namespace Plugin.BLE.Abstractions.Contracts
{
    public interface IWriteTransaction
    {
        bool Begin();
        Task<bool> Commit();

        void RollBack();

        Task<bool> Write(byte[] data);
    }
}

