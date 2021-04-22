# Changelog

## 2.1 MacOS  

### 2.1.2
- Correctly close a gatt when a connection attempt is cancelled (fixes #484)
- Use NSBluetoothAlwaysUsageDescription in iOS and macOS samples (fixes #455, #498)
- Android: enable BT5 advertising extensions (fixes #495)
- Fixed cake build and updated build instructions (fixes #492)
- Added locking to DeviceBase.KnownServices (fixes #406)
- Updated Mac project to output the same filename as the other platforms (fixes #430, #491)
- Added iOS/Mac support for 32-bit and 16-bit Service UUIDs (fixes #445)

### 2.1.1 Service Release for 2.1.0
- [iOS] #373, #377 Fixed trace ouptut that caused NRE.

### 2.1.0 Stable Release MacOS
- Use IReadOnlyLists for Services/Charactersitics/Descriptors and concurrent collections for DiscoveredDevices/ConnectedDevices
Should prevent crashes like: #320

#### 2.1.0-pre.1
- #54 macOS support

## 2.0 .NETStanard

#### 2.0.1
- Fix #367

#### 2.0.0
- .NETStandard 2.0
- Merge PR #365 (NRE), #358, #359, #341, #314, #332, #331, #329, #307
- Update to package references/ update sample apps/ update libraries

#### 2.0.0-pre1
- .NETStandard 1.0 support
- Merge PR #298, #289, #290, #263

## 1.3.0
- Stable release including all the 1.3.0-beta.x previous releases.
- Merge pull request #229 and #224 which fixed #227 set descriptor for android characteristic stop notify.

#### 1.3.0-beta.2
- Merge pull request #200. Possibility to change ConnectionPriority/ ConnectionInterval for Android

#### 1.3.0-beta.2
- #198 Android. Clear cached services, characteristics etc on signal loss

#### 1.3.0-beta.1
- Merge PR #195 Request MTU

#### 1.3.0-alpha.1
- GATT callback refactoring, one GattCallback instance per device instead of a global one

## 1.2.3
- #183: Android fixed UpdateRssiAsync

## 1.2.2
- #136: Added support for scan modes

## 1.2.1
- Merge PR #157 iOS add support for NSString descriptor values
- Merge PR #148 Added a boolean (forceBleTransport) to force the use of 'transport' parameter to BLE in connectGatt() method in Android
- Breaking changes: ConnectDeviceAsync optional parameters are now encapsulated in a ConnectParameter class

## 1.2.0
- #150 iOS: No disconnect when the connect CancelationToken is cancelled after a successful connect.
#### 1.2.0-beta4
- #122 Android: Introduced a extra connectivity state to distinguish system connected device from app connected devices. System connected devices can't be used by the app because we have no gatt instance so we should allow to connect them via the adapter even though the ProfileState is -Connected-
#### 1.2.0-beta3
- #121 #126 hardened characteristic discovery error handling for iOS
#### 1.2.0-beta2
- #118 fixed crash on read in empty value on iOS
#### 1.2.0-beta1
- #90: API change, added parent reference for IDescriptor to ICharacteristic to IService to IDevice
- #109, #111 merge PR: iOS parse TxPower, ServiceData

## 1.1.0

#### 1.1.0-beta5
- #97 Fixe iOS GetSystemConnectedDevices implementation. FYI method is now called GetSystemConnectedOrPairedDevices
- #98 and #96 Merged GetSystemConnectedDevices and GetSystemPairedDevice into single method. iOS has no equivalent method for this so it makes more sense like this.
- #94 iOS: Quickfix, change to GetDescriptorsAsync in order to wait for callback

#### 1.1.0-beta4
- #94 Android: Quickfix for descriptor read async, callback not invoked

#### 1.1.0-beta3
- #82 Enable setting PeripheralScanningOptions for ScanForPeripherals on iOS
- #93 Fixed iOS crash when ble is off and ConnectingToKnownDeviceAsync. Wait for state & proper use of cancellation token.
- #94 Implementation of descriptor Write/Read for iOS and Android.
- #95 Async for start/stop notifications so that the descriptor write callback is invoked

#### 1.1.0-beta2
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
