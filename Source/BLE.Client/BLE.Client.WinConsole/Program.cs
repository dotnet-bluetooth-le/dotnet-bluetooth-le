using BLE.Client.WinConsole;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System;
using Windows.Media.Capture;

Console.WriteLine("Hello, BLE World!");
using (var ct = new ConsoleTracer())
{
    const string bleaddress = "8C4B14C8602A";
    Plugin.BLE.Abstractions.Trace.TraceImplementation = ct.GetPrefixedTrace("Plugin.BLE");
    var ppemos = new PluginDemos(ct.GetPrefixedTrace("      DEMO"));
    var wdemos = new WindowsDemos(ct.GetPrefixedTrace("      DEMO"));
    var demoDict = new Dictionary<ConsoleKey, Demo>
    {
        {ConsoleKey.D8, 
            new Demo("Plugin: Test Connect -> Disconnect", ppemos.Test_Connect_Disconnect) },
        {ConsoleKey.D9, 
            new Demo("Windows: Test Connect -> Disconnect", wdemos.Test_Connect_Disconnect) },        
    };
    Console.WriteLine("Using BLE Address: " + bleaddress);
    Console.WriteLine();
    Console.WriteLine("List of tests to run for key:");
    Console.WriteLine(ConsoleKey.Escape + " -> Quit!");
    foreach (var demo in demoDict)
    {
        Console.WriteLine(demo.Key + ": " +  demo.Value.Description);
    }
    while (true)
    {
        var key = Console.ReadKey();   
        if (key.Key == ConsoleKey.Escape)
        {
            break;
        }
        if (demoDict.TryGetValue(key.Key, out Demo? chosendemo))
        {
            Console.WriteLine(key.Key + " -> Running: " + chosendemo.Description);
            Console.WriteLine("---------------------------------------------");
            if (chosendemo is null)
            {
                throw new Exception("No such demo!");
            }
            await chosendemo.Method(bleaddress);
        } else
        {
            Console.WriteLine(key.Key + " -> No such test. Remember " + ConsoleKey.Escape + " -> Quit!");
        }
    }
}


