using System;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions
{
    public abstract class BleImplementationBase : IBluetoothLE
    {
        private readonly Lazy<IAdapter> _adapter;
        private BluetoothState _state;

        public event EventHandler<BluetoothStateChangedArgs> StateChanged;

        public BluetoothState State
        {
            get { return _state; }
            protected set
            {
                if (_state == value)
                    return;

                _state = value;
                StateChanged?.Invoke(this, new BluetoothStateChangedArgs(_state));
            }
        }

        public IAdapter Adapter => _adapter.Value;

        protected BleImplementationBase()
        {
            _adapter = new Lazy<IAdapter>(CreateNativeAdapter, System.Threading.LazyThreadSafetyMode.PublicationOnly);
        }

        protected abstract IAdapter CreateNativeAdapter();
    }
}