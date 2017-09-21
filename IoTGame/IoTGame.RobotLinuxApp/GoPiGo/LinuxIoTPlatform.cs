using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using IoTGame.GoPiGo.Platform;
using Polly;

namespace IoTGame.RobotLinuxApp.GoPiGo
{
    public class LinuxIoTPlatform : IPlatform
    {
        private const string I2CName = "/dev/i2c-1";
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

        private int _deviceHandle;
        private SemaphoreSlim _deviceLock;

        public Task OpenAsync()
        {
            if (_deviceHandle != 0)
                return Task.CompletedTask;

            const int openReadWrite = 2;
            _deviceHandle = Open(I2CName, openReadWrite);
            _deviceLock = new SemaphoreSlim(1);

            const int i2CSlave = 0x0703; // constant, even for different devices
            var deviceReturnCode = Ioctl(_deviceHandle, i2CSlave, GoPiGoAddress);
            if (deviceReturnCode != 0)
                throw new Exception("An error occured while opening the I2C device");
            return Task.CompletedTask;
        }

        public async Task WriteAsync(byte command, byte firstParam = 0, byte secondParam = 0, byte thirdParam = 0)
        {
            await _deviceLock.WaitAsync();
            try
            {
                var writeBuffer = new[] { command, firstParam, secondParam, thirdParam };
                bool success = RetryPolicy.Execute(() => TryWrite(writeBuffer));
                if (!success)
                    throw new Exception("Could not complete write operation");
            }
            finally
            {
                _deviceLock.Release();
            }
        }

        public async Task<byte[]> WriteReadAsync(byte command, int bytesToRead, byte firstParam = 0, byte secondParam = 0, byte thirdParam = 0)
        {
            await _deviceLock.WaitAsync();
            try
            {
                var writeBuffer = new[] { command, firstParam, secondParam, thirdParam };
                var outputBuffer = new byte[bytesToRead];
                bool success = await RetryAsyncPolicy.ExecuteAsync(() => TryWriteRead(writeBuffer, outputBuffer, bytesToRead));
                if (!success) throw new Exception("Could not complete write-read operation");
                return outputBuffer;
            }
            finally
            {
                _deviceLock.Release();
            }
        }

        private bool TryWrite(byte[] writeBuffer)
        {
            var writeResult = Write(_deviceHandle, writeBuffer, writeBuffer.Length);
            bool success = writeResult == writeBuffer.Length;
            return success;
        }

        private async Task<bool> TryWriteRead(byte[] writeBuffer, byte[] outputBuffer, int bytesToRead)
        {
            var writeResult = Write(_deviceHandle, writeBuffer, writeBuffer.Length);
            if (writeResult != writeBuffer.Length)
                return false;

            await Task.Delay(OperationDelayMs);

            for (int i = 0; i < bytesToRead; i++)
            {
                var readBuffer = new byte[1];
                var readResult = Read(_deviceHandle, readBuffer, 1);
                if (readResult != 1)
                    return false;
                outputBuffer[i] = readBuffer[0];
            }
            return true;
        }

        [DllImport("libc.so.6", EntryPoint = "open")]
        private static extern int Open(string fileName, int mode);

        [DllImport("libc.so.6", EntryPoint = "ioctl", SetLastError = true)]
        private static extern int Ioctl(int fd, int request, int data);

        [DllImport("libc.so.6", EntryPoint = "read", SetLastError = true)]
        private static extern int Read(int handle, byte[] data, int length);

        [DllImport("libc.so.6", EntryPoint = "write", SetLastError = true)]
        private static extern int Write(int handle, byte[] data, int length);
    }
}