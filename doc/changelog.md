# Changelog

## 1.1.0

#### 1.1.0-beta2 (current)
- #78 BluetoothStateChangedArgs contain the old state and the new state
- #81 iOS: Removed code smell which caused crash. Temporarily removed UpdateName subscription.
- Android <=4.4: fixed gatt callback to correctly detect gatt error when connecting to known device and not in range
- #86: GetSystemConnectedDevices, beta implementation, in order to use the device in the app call ConnectAsync
- #73: fixed crash when calling connecttoknwondevice without cancellation token

#### 1.1.0-beta1
- improvements on xml documentation
- #62 Characteristic write type can be specified by the user
- fixed #69, ConnectAsync throws NullReferenceException if device is null

## 1.0.0
With this release we deliver a streamlined async API, additional functionality, xamarin vanilla plugin, sample app, better documentation.

#### 1.0.0-beta5
- added indicate support for notifications
 
#### 1.0.0-beta4
- fixed #47, clear cached services on disconnect
- IDevice is IDisposable now and disconnects the device on disposal

#### 1.0.0-beta3
- fixed vanilla plugin error

#### 1.0.0-beta
- refactored/stabilized API
- sample app
- xamarin vanilla plugin in addition to MvvmCross plugin
