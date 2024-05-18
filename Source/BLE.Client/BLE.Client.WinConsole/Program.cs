using BLE.Client.WinConsole;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Capture;

Console.WriteLine("Hello, BLE World!");
Console.WriteLine($"Environment.OSVersion.Version.Build: {Environment.OSVersion.Version.Build}");
using (var ct = new ConsoleTracer())
{

    Plugin.BLE.Abstractions.Trace.TraceImplementation = ct.GetPrefixedTrace("Plugin.BLE");
    var ppemos = new PluginDemos(ct.GetPrefixedTrace("      DEMO"));
    var wdemos = new WindowsDemos(ct.GetPrefixedTrace("      DEMO"));
    var demoDict = new Dictionary<ConsoleKey, Demo>
    {

        {ConsoleKey.B, new Demo("Turn Bluetooth ON", ppemos.TurnBluetoothOn) },
        {ConsoleKey.N, new Demo("Turn Bluetooth OFF", ppemos.TurnBluetoothOff) },
        {ConsoleKey.D1, new Demo("Discover and set the BleAddress", ppemos.DiscoverAndSelect) },
        {ConsoleKey.D2, new Demo("Set the BleAddress", BleAddressSelector.NewBleAddress) },
        {ConsoleKey.D3, new Demo("Connect -> Disconnect", ppemos.Connect_Disconnect) },
        {ConsoleKey.D4, new Demo("Pair -> Connect -> Disconnect", ppemos.Pair_Connect_Disconnect) },
        {ConsoleKey.D5, new Demo("Connect -> Change Parameters -> Disconnect", ppemos.Connect_Change_Parameters_Disconnect) },
        {ConsoleKey.D6, new Demo("Run GetSystemConnectedOrPairedDevices", ppemos.RunGetSystemConnectedOrPairedDevices) },
        {ConsoleKey.D7, new Demo("Loop: Connect -> Read services -> Disconnect", ppemos.Connect_Read_Services_Disconnect_Loop) },
        {ConsoleKey.D8, new Demo("Loop: Connect -> Read services -> Dispose", ppemos.Connect_Read_Services_Dispose_Loop) },
        {ConsoleKey.D9, new Demo("Connect -> Loop: ConnectionLost -> Connect", ppemos.Connect_ConnectionLost_Reconnect) },
        {ConsoleKey.Q, new Demo("Adapter.BondAsync", ppemos.BondAsync) },
        {ConsoleKey.W, new Demo("Adapter.BondedDevices", ppemos.GetBondedDevices) },
        {ConsoleKey.E, new Demo("Device.BondState", ppemos.ShowBondState) },
        {ConsoleKey.A, new Demo("Pure Windows: Connect -> Disconnect", wdemos.Connect_Disconnect) },
        {ConsoleKey.S, new Demo("Pure Windows: Unpair all BLE devices", wdemos.UnPairAllBleDevices) },
    };

    while (true)
    {

        Console.WriteLine();
        if (BleAddressSelector.DoesBleAddressExists())
        {
            Console.WriteLine($"Using BLE Address: {BleAddressSelector.GetBleAddress()}");
        }
        else
        {
            Console.WriteLine("No Ble address has been set - use key '1' or '2' to set the BLE address)");
        }
        Console.WriteLine("List of tests to run for key:");
        Console.WriteLine();
        Console.WriteLine(ConsoleKey.Escape + ": Quit!");

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
            Console.WriteLine($"Running: {chosendemo.Description}");
            Console.WriteLine("-------------------------------------------------------");
            if (chosendemo is null)
            {
                throw new Exception("No such demo!");
            }
            await chosendemo.Method();
        }
        else
        {
            Console.WriteLine($"{key}  -> No such test. Remember {ConsoleKey.Escape} -> Quit!");
        }
        await Task.Delay(200);
        Console.WriteLine("-------------------------------------------------------");
    }
}


