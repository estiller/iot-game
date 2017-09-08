using System.Text;
using System.Threading.Tasks;
using IoTGame.Constants;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace IoTGame.Driver
{
    public class ServiceBusDriver : IDriver
    {
        private readonly TopicClient _topicClient;

        public ServiceBusDriver()
        {
            _topicClient = new TopicClient(ServiceBusConstants.ConnectionString, ServiceBusConstants.DriveCommandTopicName);
        }

        public Task StartAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DriveAsync(DriveCommand drive)
        {
            var json = JsonConvert.SerializeObject(drive);
            var message = new Message(Encoding.UTF8.GetBytes(json));
            await _topicClient.SendAsync(message);
        }

        public Task StopAsync()
        {
            return Task.CompletedTask;
        }
    }
}