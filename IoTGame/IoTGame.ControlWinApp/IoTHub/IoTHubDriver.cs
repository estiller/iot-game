using System;
using System.Text;
using System.Threading.Tasks;
using IoTGame.Constants;
using IoTGame.Driver;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace IoTGame.ControlWinApp.IoTHub
{
    public class IoTHubDriver : IDriver
    {
        private DeviceClient _deviceClient;

        public async Task StartAsync()
        {
            _deviceClient = DeviceClient.Create(IoTHubConstants.IoTHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(IoTHubConstants.ControlDeviceId, IoTHubConstants.ControlDeviceKey), TransportType.Mqtt);
            await _deviceClient.OpenAsync();
        }

        public async Task DriveAsync(DriveCommand drive)
        {
            var json = JsonConvert.SerializeObject(drive);
            var message = new Message(Encoding.UTF8.GetBytes(json));
            await _deviceClient.SendEventAsync(message);
        }

        public async Task StopAsync()
        {
            await _deviceClient.CloseAsync();
            _deviceClient = null;
        }

        public event EventHandler<ReportBackEventArgs> ReportBackAvailable;

        private void OnReportBackAvailable(string playerId, int distanceCm, decimal voltage)
        {
            var handler = ReportBackAvailable;
            handler?.Invoke(this, new ReportBackEventArgs(playerId, distanceCm, voltage));
        }
    }
}