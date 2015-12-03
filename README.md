![alt tag](https://raw.github.com/xabre/MvvmCross-BluetoothLE/master/icon_small.png)
# MvvmCross---Bluetooth-LE   
MvvmCross plugin for BLE - iOS and Android   
Based on Xamarin - [Monkey Robotics](https://github.com/xamarin/Monkey.Robotics)

This plugin/library adds some additional features to the Monkey.Robotics API and also fixes some issues.  

Targeted for iOS and Android.
    
## Installation
   
Install-Package MvvmCross.Plugin.BLE   

## Usage basics   
   
Let MvvmCross inject the IAdapter service in your shared code and start using BLE :)

```csharp
_adapter = Mvx.Resolve<IAdapter>();
```
or
```csharp
MyViewModel(IAdapter adapter)
{
	_adapter = adapter;
}
```

Scan for devices:
```csharp
_adapter.DeviceDiscovered += (s,a) => _deviceList.Add(a.Device);
_adapter.StartScanningForDevices();
```

Connect to device:
```csharp
_adapter.ConnectToDevice(device);
```
or
```csharp
_connectedDevice = await _adapter.ConnectAsync(_deviceList[selectedDeviceIndex]);
```

Get services:
```csharp
_connectedDevice.DiscoverServices();
_connectedDevice.ServicesDiscovered += (o, args) => { };
```
or
```csharp
var service = await _connectedDevice.GetServiceAsync(Guid.Parse("ffe0ecd2-3d16-4f8d-90de-e89e7fc396a5"));
```

Get characteristics:
```csharp
service.DiscoverCharacteristics();
service.CharacteristicsDiscovered += (o, args) => { };
```
or
```csharp
var characteristic = await service.GetCharacteristicAsync(Guid.Parse("d8de624e-140f-4a22-8594-e2216b84a5f2"));
```

Read charactersitic:
```csharp
var bytes = await characteristic.ReadAsync();
```

Write characteristic:
```csharp
characteristic.Write(bytes);
```
or with acknowledgment:
```csharp
await characteristic.WriteAsync(bytes);
```

Characteristic notifications:
```csharp
characteristic.ValueUpdated += (o, args) =>
{
 	var bytes = args.Characteristic.Value;
};

characteristic.StartUpdates();
```

## Usefull Links

[Monkey Robotics](https://github.com/xamarin/Monkey.Robotics)

## Licence

[Apache 2.0](https://github.com/xabre/MvvmCross-BluetoothLE/blob/master/LICENSE)




