using System;
using System.Linq;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Tests.Mocks;
using Xunit;

namespace Plugin.BLE.Tests
{ 
    public class CharacteristicBaseTests
    {
        [Theory(DisplayName = "Setting WriteType to not supported type throws InvalidOperationException")]
        [InlineData(CharacteristicWriteType.WithResponse, CharacteristicPropertyType.WriteWithoutResponse)]
        [InlineData(CharacteristicWriteType.WithResponse, CharacteristicPropertyType.Read)]
        [InlineData(CharacteristicWriteType.WithoutResponse, CharacteristicPropertyType.Write)]
        [InlineData(CharacteristicWriteType.WithoutResponse, CharacteristicPropertyType.Read)]
        public void WriteType_set_throws_InvalidOperationException(CharacteristicWriteType writeType, CharacteristicPropertyType currentProperties)
        {
            var characteristic = new CharacteristicMock { MockPropterties = currentProperties };
            Assert.Throws<InvalidOperationException>(() =>
            {
                characteristic.WriteType = writeType;
            });
        }

        [Theory(DisplayName = "Setting WriteType to supported type")]
        [InlineData(CharacteristicWriteType.WithResponse, CharacteristicPropertyType.Write)]
        [InlineData(CharacteristicWriteType.WithResponse, CharacteristicPropertyType.Write | CharacteristicPropertyType.WriteWithoutResponse)]
        [InlineData(CharacteristicWriteType.WithoutResponse, CharacteristicPropertyType.WriteWithoutResponse)]
        [InlineData(CharacteristicWriteType.WithoutResponse, CharacteristicPropertyType.Write | CharacteristicPropertyType.WriteWithoutResponse)]
        public void WriteType_set(CharacteristicWriteType writeType, CharacteristicPropertyType currentProperties)
        {
            var characteristic = new CharacteristicMock
            {
                MockPropterties = currentProperties,
                WriteType = writeType
            };

            Assert.Equal(writeType, characteristic.WriteType);
        }

        [Fact(DisplayName = "WriteType initial value should be default.")]
        public void WriteType_initial_value()
        {
            var characteristic = new CharacteristicMock();

            Assert.Equal(CharacteristicWriteType.Default, characteristic.WriteType);
        }

        [Theory(DisplayName = "WriteAsync should derive write type from properties if set to default")]
        [InlineData(CharacteristicWriteType.WithResponse, CharacteristicWriteType.Default, CharacteristicPropertyType.Write)]
        [InlineData(CharacteristicWriteType.WithResponse, CharacteristicWriteType.WithResponse, CharacteristicPropertyType.Write | CharacteristicPropertyType.WriteWithoutResponse)]
        [InlineData(CharacteristicWriteType.WithoutResponse, CharacteristicWriteType.Default, CharacteristicPropertyType.WriteWithoutResponse)]
        [InlineData(CharacteristicWriteType.WithoutResponse, CharacteristicWriteType.WithoutResponse, CharacteristicPropertyType.Write | CharacteristicPropertyType.WriteWithoutResponse)]
        public async Task Write_WriteType(CharacteristicWriteType expectedWriteType, CharacteristicWriteType currentWriteType, CharacteristicPropertyType currentProperties)
        {
            var characteristic = new CharacteristicMock
            {
                MockPropterties = currentProperties,
                WriteType = currentWriteType
            };

            await characteristic.WriteAsync(new byte[0]);
            var writtenType = characteristic.WriteHistory.First().WriteType;

            Assert.Equal(expectedWriteType, writtenType);
        }

        [Fact(DisplayName = "WriteAsync should throw ArgumentNullException if value is null.")]
        public async Task WriteAsync_null_value()
        {
            var characteristic = new CharacteristicMock();
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await characteristic.WriteAsync(null);
            });
        }

        [Fact(DisplayName = "WriteAsync should throw InvalidOperationException if not writable.")]
        public async Task WriteAsync_not_writable()
        {
            var characteristic = new CharacteristicMock();
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await characteristic.WriteAsync(new byte[0]);
            });
        }
    }
}