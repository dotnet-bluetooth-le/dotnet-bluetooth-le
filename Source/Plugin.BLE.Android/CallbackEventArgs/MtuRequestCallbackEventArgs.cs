﻿using System;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Android.CallbackEventArgs 
{ 
    public class MtuRequestCallbackEventArgs : EventArgs 
    { 
        public IDevice Device { get; }
        public Exception Error { get; } 
        public int Mtu { get; } 

        public MtuRequestCallbackEventArgs(IDevice device, Exception error, int mtu) 
        { 
            Device = device;
            Error = error; 
            Mtu = mtu; 
        } 
    }
}