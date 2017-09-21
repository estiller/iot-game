using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IoTGame.Constants;
using IoTGame.Driver;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.InteropExtensions;
using Newtonsoft.Json;

namespace IoTGame.Controller
{
    public class ServiceBusController : IController
    {
        private readonly string _playerId;
        private readonly IDriver _driver;
        private readonly string _subscriptionName;

        private ISubscriptionClient _subscriptionClient;
        private ITopicClient _topicClient;

        public ServiceBusController(string playerId, IDriver driver, string subscriptionName)
        {
            _playerId = playerId;
            _driver = driver;
            _subscriptionName = subscriptionName;
        }

        public async Task StartAsync()
        {
            await _driver.StartAsync();

            _subscriptionClient = new SubscriptionClient(ServiceBusConstants.ConnectionString, ServiceBusConstants.DriveCommandTopicName, _subscriptionName, ReceiveMode.ReceiveAndDelete);
            _subscriptionClient.RegisterMessageHandler(HandleMessageAsync, new MessageHandlerOptions(HandleExceptionAsync) { MaxConcurrentCalls = 1});

            _topicClient = new TopicClient(ServiceBusConstants.ConnectionString, ServiceBusConstants.ReportBackTopicName);
        }

        public async Task StopAsync()
        {
            await _topicClient.CloseAsync();
            _topicClient = null;

            await _subscriptionClient.CloseAsync();
            _subscriptionClient = null;

            await _driver.StopAsync();
        }

        public async Task ReportBackAsync(int distanceCm, decimal voltage)
        {
            if (_playerId == null)
                throw new Exception("PlayerId cannot be null when sending out reports");

            var message = new Message(SerializeReport());
            await _topicClient.SendAsync(message);

            byte[] SerializeReport()
            {
                using (var stream = new MemoryStream())
                {
                    var writer = new BinaryWriter(stream);
                    writer.Write(_playerId);
                    writer.Write(distanceCm);
                    writer.Write(voltage);
                    return stream.ToArray();
                }
            }
        }

        private Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
        {
            var json = IsSelfSerialized(message) ? Encoding.UTF8.GetString(message.Body) : message.GetBody<string>();
            var command = JsonConvert.DeserializeObject<DriveCommand>(json);
            return _driver.DriveAsync(command);

            bool IsSelfSerialized(Message m)
            {
                return m.UserProperties.ContainsKey("SelfSerialized");
            }
        }

        private Task HandleExceptionAsync(ExceptionReceivedEventArgs args)
        {
            Debug.WriteLine($"An exception occured while reading from service bus: {args.Exception}");
            return Task.CompletedTask;
        }
    }
}