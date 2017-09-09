using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email.DataProvider;
using IoTGame.Constants;
using IoTGame.Driver;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace IoTGame.ControlWinApp.IoTHub
{
    public class IoTHubDriver : IDriver
    {
        private DeviceClient _deviceClient;

        private CancellationTokenSource _cancellationTokenSource;
        private Task _recieveTask;

        public async Task StartAsync()
        {
            _deviceClient = DeviceClient.Create(IoTHubConstants.IoTHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(IoTHubConstants.ControlDeviceId, IoTHubConstants.ControlDeviceKey), TransportType.Mqtt);
            await _deviceClient.OpenAsync();

            _cancellationTokenSource = new CancellationTokenSource();
            _recieveTask = RecieveMessagesAsync(_cancellationTokenSource.Token);
        }

        public async Task DriveAsync(DriveCommand drive)
        {
            var json = JsonConvert.SerializeObject(drive);
            var message = new Message(Encoding.UTF8.GetBytes(json));
            await _deviceClient.SendEventAsync(message);
        }

        public async Task StopAsync()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = null;

            await _recieveTask;
            _recieveTask = null;

            await _deviceClient.CloseAsync();
            _deviceClient = null;
        }

        public event EventHandler<ReportBackEventArgs> ReportBackAvailable;

        private void OnReportBackAvailable(int distanceCm, decimal voltage)
        {
            var handler = ReportBackAvailable;
            handler?.Invoke(this, new ReportBackEventArgs(distanceCm, voltage));
        }

        private async Task RecieveMessagesAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = await _deviceClient.ReceiveAsync(TimeSpan.FromSeconds(5));
                if (message == null)
                    continue;

                var buffer = message.GetBytes();
                using (var stream = new MemoryStream(buffer))
                {
                    var reader = new BinaryReader(stream);
                    var distanceCm = reader.ReadInt32();
                    var voltage = reader.ReadDecimal();
                    OnReportBackAvailable(distanceCm, voltage);
                }
            }
        }
    }
}