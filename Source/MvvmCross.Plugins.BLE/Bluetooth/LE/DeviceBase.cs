using System;
using System.Collections.Generic;

namespace MvvmCross.Plugins.BLE.Bluetooth.LE
{
    public abstract class DeviceBase : IDevice, IEquatable<IDevice>
    {
        public virtual event EventHandler ServicesDiscovered = delegate { };

        public virtual Guid ID
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual int Rssi
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual DeviceState State
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual object NativeDevice
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual byte[] AdvertisementData
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual IList<IService> Services
        {
            get { throw new NotImplementedException(); }
        }

        public override string ToString()
        {
            return this.Name;
        }

        public virtual void DiscoverServices()
        {
            throw new NotImplementedException();
        }

        #region IEquatable implementation
        public bool Equals(IDevice other)
        {
            return this.ID == other.ID;
        }
        #endregion
    }
}

