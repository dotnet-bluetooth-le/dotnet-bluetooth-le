using BLE.Client.WinConsole;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;

Console.WriteLine("Hello, BLE World!");
var ct = new ConsoleTracer();
Plugin.BLE.Abstractions.Trace.TraceImplementation = ct.Trace;
var demo = new BleDemo(ct.Trace);
await demo.DoTheScanning(ScanMode.LowPower);
await demo.ConnectTest("Shure");
ct.Dispose();


