# MvvmCross---Bluetooth-LE   
MvvmCross plugin for BLE - iOS and Android   
Based on Xamarin - [Monkey Robotics](https://github.com/xamarin/Monkey.Robotics)

This plugin/library adds some additional features to the Monkey.Robotics API and also fixes some issues.  

Targeted for iOS and Android.
    
## Installation
   
Install-Package MvvmCross.Plugin.BLE   

## Usage basics   
   
Let MvvmCross inject the IAdapter service in your shared code and start using BLE :)

```
_adapter = Mvx.Resolve<IAdapter>();
```
or
```
MyViewModel(IAdapter adapter)
{
	_adapter = adapter;
}
```

Scan for devices:
```
 _adapter.DeviceDiscovered += (s,a) => _deviceList.Add(a.Device);
 _adapter.StartScanningForDevices();
```

Connect to device:
```
 _adapter.ConnectToDevice(device);
```
or
```
  _connectedDevice = await _adapter.ConnectAsync(_deviceList[selectedDeviceIndex]);
```

Get services:
```
	_connectedDevice.DiscoverServices();
	_connectedDevice.ServicesDiscovered += (o, args) => { };
```
or
```
 	var service = await _connectedDevice.GetServiceAsync(Guid.Parse("ffe0ecd2-3d16-4f8d-90de-e89e7fc396a5"));
```

Get characteristics:
```
	service.DiscoverCharacteristics();
	service.CharacteristicsDiscovered += (o, args) => { };
```
or
```
	var characteristic = await service.GetCharacteristicAsync(Guid.Parse("d8de624e-140f-4a22-8594-e2216b84a5f2"));
```

Read charactersitic:
```
	var bytes = await characteristic.ReadAsync();
```

Write characteristic:
```
	characteristic.Write(bytes);
```
or with acknowledgment:
```
	await characteristic.WriteAsync(bytes);
```

Characteristic notifications:
```
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




