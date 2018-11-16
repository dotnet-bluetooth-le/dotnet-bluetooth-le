### iOS state restoration (basic support)

If your app needs to connect to a known device whenever it becomes available, state restoration will allow it to keep the connection request active even after the app is terminated by the OS due to a low memory scenario. To enable state restoration, run the following within `AppDelegate.FinishedLaunching` before any other usage of the plugin:

```BleImplementation.UseRestorationIdentifier("YourAppRestorationId");```

When the device becomes available, the OS will automatically re-launch your application in background. Once the app is launched, it should re-connect to the device.

#### Considerations:

* State restoration is available for background connections, so the app will require the `bluetooth-central` background mode enabled.

* Since the app is re-launched into background, device connection should not rely on any UI interaction. Ideally, the device connection process should be fired straight from the `AppDelegate.FinishedLaunching`.

* Background processing rules still apply after re-launching (i.e. ~10 seconds time window).

#### Limitations:

* State restoration provides a list of devices (peripherals) connected when re-launching the app. The current implementation ignores this list, so you will need to re-connect to all the expected devices.

#### Other documentation:

* [Core Bluetooth Programming Guide - State preservation and restoration](https://developer.apple.com/library/archive/documentation/NetworkingInternetWeb/Conceptual/CoreBluetooth_concepts/CoreBluetoothBackgroundProcessingForIOSApps/PerformingTasksWhileYourAppIsInTheBackground.html#//apple_ref/doc/uid/TP40013257-CH7-SW10)