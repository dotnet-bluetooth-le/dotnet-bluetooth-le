using System;
using System.Collections.Generic;

namespace MvvmCross.Plugins.BLE.Bluetooth.LE
{
    public abstract class DeviceBase : IDevice
    {
        public virtual event EventHandler ServicesDiscovered = delegate { };

        public virtual Guid ID
        {
            get { throw new NotImplementedException(); }
        }

        public virtual string Name
        {
            get { throw new NotImplementedException(); }
        }

        public virtual int Rssi
        {
            get { throw new NotImplementedException(); }
        }

        public virtual DeviceState State
        {
            get { throw new NotImplementedException(); }
        }

        public virtual object NativeDevice
        {
            get { throw new NotImplementedException(); }
        }

        public virtual byte[] AdvertisementData
        {
            get { throw new NotImplementedException(); }
        }

        public virtual IList<AdvertisementRecord> AdvertisementRecords
        {
            get { throw new NotImplementedException(); }
        }

        public virtual IList<IService> Services
        {
            get { throw new NotImplementedException(); }
        }

        public virtual void DiscoverServices()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return Name;
        }

        #region IEquatable implementation

        public override bool Equals(object other)
        {
            if (other == null)
            {
                return false;
            }

            if (other.GetType() != GetType())
            {
                return false;
            }

            var otherDeviceBase = (DeviceBase) other;
            return ID == otherDeviceBase.ID;
        }

        #endregion
    }
}