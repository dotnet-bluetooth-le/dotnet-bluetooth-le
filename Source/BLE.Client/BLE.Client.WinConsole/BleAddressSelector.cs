using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLE.Client.WinConsole
{
    public static class BleAddressSelector
    {
        static string bleaddressTxtPath = Path.Combine(Path.GetTempPath(), "bleaddress.txt");
        static string? bleaddress = null;

        public static bool DoesBleAddressExists()
        {
            if (File.Exists(bleaddressTxtPath))
            {
                bleaddress = File.ReadAllText(bleaddressTxtPath);
                return true;
            }
            return false;
        }

        public static string GetBleAddress()
        {
            if (bleaddress is null)
            {
                if (File.Exists(bleaddressTxtPath))
                {
                    bleaddress = File.ReadAllText(bleaddressTxtPath);
                }
                else
                {
                    NewBleAddress();
                }
            }
            if (bleaddress is null)
            {
                throw new Exception("BleAddressSelector says bleaddress is null");
            }
            return bleaddress;
        }

        public static void SetBleAddress(string? bleaddressIn)
        {
            if (bleaddressIn is null || bleaddressIn.Length != 12)
            {
                Console.WriteLine("Wrong BLE Address entered");
                throw new Exception("Wrong BLE Address entered");
            }
            bleaddress = bleaddressIn.ToUpperInvariant();
            File.WriteAllText(bleaddressTxtPath, bleaddress);
        }

        public static Task NewBleAddress()
        {
            Console.Write("Enter BLE Address (12 hex chars): ");
            SetBleAddress(Console.ReadLine());
            return Task.CompletedTask;
        }
    }
}
