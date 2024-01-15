using BLE.Client.WinConsole;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using Windows.Media.Capture;

Console.WriteLine("Hello, BLE World!");
using (var ct = new ConsoleTracer())
{
    const string bleaddress = "8C4B14C9C68A";
    Plugin.BLE.Abstractions.Trace.TraceImplementation = ct.GetPrefixedTrace("Plugin.BLE");
    var ppemos = new PluginDemos(ct.GetPrefixedTrace("      DEMO"));
    var wdemos = new WindowsDemos(ct.GetPrefixedTrace("      DEMO"));
    var demoDict = new Dictionary<ConsoleKey, Demo>
    {
        {ConsoleKey.D1, new Demo("Plugin:  Connect -> Disconnect", ppemos.Connect_Disconnect) },
        {ConsoleKey.D2, new Demo("Plugin:  Pair -> Connect -> Disconnect", ppemos.Pair_Connect_Disconnect) },
        {ConsoleKey.D8, new Demo("Windows: Connect -> Disconnect", wdemos.Connect_Disconnect) },
        {ConsoleKey.D9, new Demo("Windows: Unpair all BLE devices", wdemos.UnPairAllBleDevices) },
    };

    Console.WriteLine("Using BLE Address: " + bleaddress);
    Console.WriteLine();
    Console.WriteLine("List of tests to run for key:");
    Console.WriteLine(ConsoleKey.Escape + " -> Quit!");
    while (true)
    {
        foreach (var demo in demoDict)
        {
            Console.WriteLine(demo.Key + ": " + demo.Value.Description);
        }

        var key = Console.ReadKey();
        if (key.Key == ConsoleKey.Escape)
        {
            break;
        }
        if (demoDict.TryGetValue(key.Key, out Demo? chosendemo))
        {
            Console.WriteLine();
            Console.WriteLine("Running: " + chosendemo.Description);
            if (chosendemo is null)
            {
                throw new Exception("No such demo!");
            }
            await chosendemo.Method(bleaddress);
        }
        else
        {
            Console.WriteLine(key.Key + " -> No such test. Remember " + ConsoleKey.Escape + " -> Quit!");
        }
        Console.WriteLine("---------------------------------------------");
    }
}


