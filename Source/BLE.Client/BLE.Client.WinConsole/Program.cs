using BLE.Client.WinConsole;
using Plugin.BLE;

Console.WriteLine("Hello, BLE World!");
var ct = new ConsoleTracer();
Plugin.BLE.Abstractions.Trace.TraceImplementation = ct.Trace;
var demo = new BleDemo(ct.Trace);
await demo.DoTheScanning();
await demo.ConnectTest("egoo");



