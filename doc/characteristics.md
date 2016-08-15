### Comparsion of property values

<table>
  <tr>
    <td>Value</td>
    <td>Specification</td>
    <td>Plugin</td>
    <td>iOS</td>
    <td>Android</td>
    <td>UWP</td>
  </tr>
  <tr>
    <td>0</td>
    <td></td>
    <td>None</td>
    <td colspan="2"></td>
    <td>None</td>
  </tr>
  <tr>
	<td>1</td>
    <td colspan="5" align="center">Broadcast</td>
  </tr>
  <tr>
	<td>2</td>
    <td colspan="5" align="center">Read</td>
  </tr>
  <tr>
	<td>4</td>
    <td colspan="5" align="center">WriteWithoutResponse</td>
  </tr>
  <tr>
	<td>8</td>
    <td colspan="5" align="center">Write</td>
  </tr>
  <tr>
	<td>16</td>
    <td colspan="5" align="center">Notify</td>
  </tr>
  <tr>
	<td>32</td>
    <td colspan="5" align="center">Indicate</td>
  </tr>
  <tr>
	<td>64</td>
    <td colspan="5" align="center">AuthenticatedSignedWrites</td>
  </tr>
  <tr>
	<td>128</td>
    <td colspan="5" align="center">ExtendedProperties</td>
  </tr>
  <tr>
	<td>256</td>
    <td></td>
    <td></td>
    <td></td>
    <td>NotifyEncryptionRequired</td>
    <td>ReliableWrites</td>
  </tr>
  <tr>
	<td>512</td>
    <td></td>
    <td></td>
    <td></td>
    <td>IndicateEncryptionRequired</td>
    <td>WritableAuxiliaries</td>
  </tr>
</table>

Specification: [Core 4.2 Vol.3 3.3.1.1](https://www.bluetooth.org/DocMan/handlers/DownloadDoc.ashx?doc_id=286439)
UWP: [GattCharacteristicProperties](https://msdn.microsoft.com/en-in/library/windows/apps/windows.devices.bluetooth.genericattributeprofile.gattcharacteristicproperties)
Android: [GattProperty](https://developer.xamarin.com/api/type/Android.Bluetooth.GattProperty/)
iOS: [CBCharacteristicProperties](https://developer.apple.com/library/ios/documentation/CoreBluetooth/Reference/CBCharacteristic_Class/#//apple_ref/c/tdef/CBCharacteristicProperties)

From 1 to 128 all platforms are using the values from the specification.
iOS and UWP are using the values 256, and 512. On UWP they are mapped to extended properties (Core 4.2 §3.3.3.1). iOS is using it for non standard (4.2) values.  
