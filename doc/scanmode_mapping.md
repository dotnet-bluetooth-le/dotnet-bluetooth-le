### Mapping of Scan Modes

<table>
  <tr>
    <th>Value</th>
    <th>iOS/macOS</th>
    <th>Android</th>
    <th>UWP</th>
  </tr>
  <tr>
    <td>Passive</td>
    <td rowspan="4">System default.</td>
    <td>
	   SCAN_MODE_OPPORTUNISTIC (Android >= M)<br>
	   SCAN_MODE_LOW_POWER (Android < M)
	</td>
    <td>Passive</td>
  </tr>
  <tr>
    <td>LowPower</td>
    <td>SCAN_MODE_LOW_POWER</td>
    <td rowspan="3">Active</td>
  </tr>
  <tr>
    <td>Balanced</td>
    <td>SCAN_MODE_BALANCED</td>
  </tr>
  <tr>
    <td>LowLatency</td>
    <td>SCAN_MODE_LOW_LATENCY</td>
  </tr>
</table>

- iOS: Not supported. [Passive Mode will be used automatically in background mode](https://lists.apple.com/archives/bluetooth-dev/2012/May/msg00041.html) (caution: very old information!))
- Android: [ScanSettings](https://developer.android.com/reference/android/bluetooth/le/ScanSettings.html). Falls back to system default for all scan modes, if Android < Lollipop.
- UWP: [BluetoothLEScanningMode](https://docs.microsoft.com/en-us/uwp/api/windows.devices.bluetooth.advertisement.bluetoothlescanningmode)
