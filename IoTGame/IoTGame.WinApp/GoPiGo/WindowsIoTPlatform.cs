using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using IoTGame.GoPiGo.Platform;
using Polly;

namespace IoTGame.WinApp.GoPiGo
{
    public class WindowsIoTPlatform : IPlatform
    {
        private const string I2CName = "I2C1";
        private const byte GoPiGoAddress = 0x08;
        private static readonly TimeSpan OperationDelayMs = TimeSpan.FromMilliseconds(10);

        private static readonly Policy<bool> RetryPolicy = Policy
            .HandleResult<bool>(success => !success)
            .WaitAndRetry(5, i =>
            {
                Debug.WriteLine($"An operation has failed. Retry {i}.");
                return OperationDelayMs;
            });

        private static readonly Policy<bool> RetryAsyncPolicy = Policy
            .HandleResult<bool>(success => !success)
            .WaitAndRetryAsync(5, i =>
            {
                Debug.WriteLine($"An async operation has failed. Retry {i}.");
                return OperationDelayMs;
            });

        private I2cDevice _device;

        public async Task OpenAsync()
        {
            if (_device != null)
                return;

            var settings = new I2cConnectionSettings(GoPiGoAddress)
            {
                BusSpeed = I2cBusSpeed.StandardMode
            };

            var deviceInformation = await GetDeviceInformation();
            _device = await I2cDevice.FromIdAsync(deviceInformation.Id, settings);
        }

        public Task WriteAsync(byte command, byte firstParam = 0, byte secondParam = 0, byte thirdParam = 0)
        {
            var writeBuffer = new[] {command, firstParam, secondParam, thirdParam};
            bool success = RetryPolicy.Execute(() => TryWrite(writeBuffer));
            return success ? Task.CompletedTask : Task.FromException<Exception>(new Exception("Could not complete write operation"));
        }

        public async Task<byte[]> WriteReadAsync(byte command, int bytesToRead, byte firstParam = 0, byte secondParam = 0, byte thirdParam = 0)
        {
            var writeBuffer = new[] {command, firstParam, secondParam, thirdParam};
            var outputBuffer = new byte[bytesToRead];
            bool success = await RetryAsyncPolicy.ExecuteAsync(() => TryWriteRead(writeBuffer, outputBuffer, bytesToRead));
            if (!success) throw new Exception("Could not complete write-read operation");
            return outputBuffer;
        }

        private static async Task<DeviceInformation> GetDeviceInformation()
        {
            var advancedQuerySyntax = I2cDevice.GetDeviceSelector(I2CName);
            var devices = await DeviceInformation.FindAllAsync(advancedQuerySyntax);
            return devices[0];
        }

        private bool TryWrite(byte[] writeBuffer)
        {
            var writeTransferResult = _device.WritePartial(writeBuffer);
            bool success = writeTransferResult.Status == I2cTransferStatus.FullTransfer;
            return success;
        }

        private async Task<bool> TryWriteRead(byte[] writeBuffer, byte[] outputBuffer, int bytesToRead)
        {
            var writeTransferResult = _device.WritePartial(writeBuffer);
            if (writeTransferResult.Status != I2cTransferStatus.FullTransfer)
                return false;

            await Task.Delay(OperationDelayMs);

            for (int i = 0; i < bytesToRead; i++)
            {
                var readBuffer = new byte[1];
                var readTransferResult = _device.ReadPartial(readBuffer);
                if (readTransferResult.Status != I2cTransferStatus.FullTransfer)
                    return false;
                outputBuffer[i] = readBuffer[0];
            }
            return true;
        }
    }
}