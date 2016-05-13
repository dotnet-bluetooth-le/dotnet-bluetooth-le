# <img src="icon_small.png" width="71" height="71"/> Bluetooth LE plugin for Xamarin [![Build Status](https://www.bitrise.io/app/3fe54d0a5f43c2bf.svg?token=i9LUY4rIecZWd_3j7hwXgw)](https://www.bitrise.io/app/3fe54d0a5f43c2bf)


Xamarin and MvvMCross plugin for accessing the bluetooth functionality. The plugin is based on the BLE implementation of [Monkey Robotics](https://github.com/xamarin/Monkey.Robotics).

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

## Sample app

We provide a sample Xamarin.Forms app, that is a basic BLE scanner. With this app, it's possible to 

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
var adapter = CrossBle.Current.Adapter;
```

**MvvmCross**

Let MvvmCross inject the `IAdapter` service in your shared code and start using BLE.

```csharp
var adapter = Mvx.Resolve<IAdapter>();
```
or
```csharp
MyViewModel(IAdapter adapter)
{
    this.adapter = adapter;
}
```

#### Scan for devices
```csharp
adapter.DeviceDiscovered += (s,a) => deviceList.Add(a.Device);
await adapter.StartScanningForDevicesAsync();
```

#### Connect to device
```csharp
var connectedDevice = await _adapter.ConnectAsync(device);
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




