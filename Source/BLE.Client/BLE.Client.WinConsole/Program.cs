using BLE.Client.WinConsole;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System;

Console.WriteLine("Hello, BLE World!");
using (var ct = new ConsoleTracer())
{
    Plugin.BLE.Abstractions.Trace.TraceImplementation = ct.GetPrefixedTrace("Plugin.BLE");
    var demo = new BleDemo(ct.GetPrefixedTrace("      DEMO"));
    await demo.ShowNumberOfServices("40CBC0DD37E2");
}


