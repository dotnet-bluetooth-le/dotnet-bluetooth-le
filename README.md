# <img src="icon_small.png" width="71" height="71"/> Bluetooth LE plugin for Xamarin [![Build Status](https://www.bitrise.io/app/3fe54d0a5f43c2bf.svg?token=i9LUY4rIecZWd_3j7hwXgw)](https://www.bitrise.io/app/3fe54d0a5f43c2bf)


Xamarin and MvvMCross plugin for accessing the bluetooth functionality. The plugin is based on the BLE implementation of [Monkey Robotics](https://github.com/xamarin/Monkey.Robotics). 

**Important Note:** With the term *"vanilla"* we mean the non MvvmCross/pure Xamarin version. You **can** use it without MvvmCross, if you download the vanilla package.

## Support & Limitations

| Platform  | Version | Limitations |
| ------------- | ----------- | ----------- |
| Xamarin.Android | 4.3 |  |
| Xamarin.iOS     | 7.0 |  |

## Installation

**Vanilla**

```
Install-Package Plugin.BLE
// or
Install-Package MvvmCross.Plugin.BLE -Pre
```
[![NuGet](https://img.shields.io/nuget/v/Plugin.BLE.svg?label=NuGet)](https://www.nuget.org/packages/Plugin.BLE) [![NuGet Beta](https://img.shields.io/nuget/vpre/Plugin.BLE.svg?label=NuGet Beta)](https://www.nuget.org/packages/Plugin.BLE)

**MvvmCross**

```
Install-Package MvvmCross.Plugin.BLE
// or
Install-Package MvvmCross.Plugin.BLE -Pre
```

[![NuGet MvvMCross](https://img.shields.io/nuget/v/MvvmCross.Plugin.BLE.svg?label=NuGet MvvMCross)](https://www.nuget.org/packages/MvvmCross.Plugin.BLE) [![NuGet MvvMCross Beta](https://img.shields.io/nuget/vpre/MvvmCross.Plugin.BLE.svg?label=NuGet MvvMCross Beta)](https://www.nuget.org/packages/MvvmCross.Plugin.BLE)

**Android**

Add these permissions to AndroidManifest.xml. For Marshmallow, please follow [Requesting Runtime Permissions in Android Marshmallow](https://blog.xamarin.com/requesting-runtime-permissions-in-android-marshmallow/) 

```xml
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.BLUETOOTH" />
<uses-permission android:name="android.permission.BLUETOOTH_ADMIN" />
```

## Sample app

We provide a sample Xamarin.Forms app, that is a basic bluetooth LE scanner. With this app, it's possible to 

- check the ble status
- discover devices
- connect/disconnect
- discover the services
- discover the characteristics
- see characteristic details
- read/write and register for notifications of a characteristic

Have a look at the code and use it as starting point to learn about the plugin and play around with it.

## Usage  

**Vanilla**

```csharp
var ble = CrossBluetoothLE.Current;
var adapter = CrossBluetoothLE.Current.Adapter;
```

**MvvmCross**

The MvvmCross plugin registers `IBluetoothLE` and  `IAdapter` as lazy initialized singletons. You can resolve/inject them as any other MvvmCross service. You don't have to resolve/inject both. It depends on your use case.

```csharp
var ble = Mvx.Resolve<IBluetoothLE>();
var adapter = Mvx.Resolve<IAdapter>();
```
or
```csharp
MyViewModel(IBluetoothLE ble, IAdapter adapter)
{
    this.ble = ble;
    this.adapter = adapter;
}
```

### IBluetothLE
#### Get the bluetooth status
```csharp
var state = ble.State;
```
You can also listen for State changes. So you can react if the user turns on/off bluetooth on you smartphone.
```csharp
ble.StateChanged += (s, e) => 
{
    Debug.WriteLine($"The bluetooth state changed to {e.NewState}");
};
```


### IAdapter
#### Scan for devices
```csharp
adapter.DeviceDiscovered += (s,a) => deviceList.Add(a.Device);
await adapter.StartScanningForDevicesAsync();
```

#### Connect to device
`ConnectToDeviceAync` returns a Task that finishes if the device has been connected successful. Otherwise a `DeviceConnectionException` gets thrown.

```csharp
try 
{
    await _adapter.ConnectToDeviceAync(device);
}
catch(DeviceConnectionException e)
{
    // ... could not connect to device
}
```

#### Get services
```csharp
var services = await connectedDevice.GetServicesAsync();
```
or get a specific service:
```csharp
var service = await connectedDevice.GetServiceAsync(Guid.Parse("ffe0ecd2-3d16-4f8d-90de-e89e7fc396a5"));
```

#### Get characteristics
```csharp
var characteristics = await service.GetCharacteristicsAsync();
```
or get a specific characteristic:
```csharp
var characteristic = await service.GetCharacteristicAsync(Guid.Parse("d8de624e-140f-4a22-8594-e2216b84a5f2"));
```

#### Read characteristic
```csharp
var bytes = await characteristic.ReadAsync();
```

#### Write characteristic
```csharp
await characteristic.WriteAsync(bytes);
```

#### Characteristic notifications
```csharp
characteristic.ValueUpdated += (o, args) =>
{
    var bytes = args.Characteristic.Value;
};

characteristic.StartUpdates();
```

## Useful Links

- [MvvmCross](https://github.com/MvvmCross)
- [Monkey Robotics](https://github.com/xamarin/Monkey.Robotics)

## Licence

[Apache 2.0](https://github.com/xabre/MvvmCross-BluetoothLE/blob/master/LICENSE)




