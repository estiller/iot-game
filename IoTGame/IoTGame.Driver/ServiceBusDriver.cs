using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IoTGame.Constants;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace IoTGame.Driver
{
    public class ServiceBusDriver : IDriver
    {
        private TopicClient _topicClient;
        private ISubscriptionClient _subscriptionClient;


        public Task StartAsync()
        {
            _topicClient = new TopicClient(ServiceBusConstants.ConnectionString, ServiceBusConstants.DriveCommandTopicName);

            _subscriptionClient = new SubscriptionClient(ServiceBusConstants.ConnectionString, ServiceBusConstants.ReportBackTopicName, ServiceBusConstants.ControlSubscriptionName, ReceiveMode.ReceiveAndDelete);
            _subscriptionClient.RegisterMessageHandler(HandleMessageAsync, new MessageHandlerOptions(HandleExceptionAsync) { MaxConcurrentCalls = 1 });

            return Task.CompletedTask;
        }

        public async Task DriveAsync(DriveCommand drive)
        {
            var json = JsonConvert.SerializeObject(drive);
            var message = new Message(Encoding.UTF8.GetBytes(json))
            {
                UserProperties = {["SelfSerialized"] = true}
            };
            await _topicClient.SendAsync(message);
        }

        public async Task StopAsync()
        {
            await _subscriptionClient.CloseAsync();
            _subscriptionClient = null;

            await _topicClient.CloseAsync();
            _topicClient = null;
        }

        public event EventHandler<ReportBackEventArgs> ReportBackAvailable;

        private void OnReportBackAvailable(string playerId, int distanceCm, decimal voltage)
        {
            var handler = ReportBackAvailable;
            handler?.Invoke(this, new ReportBackEventArgs(playerId, distanceCm, voltage));
        }

        private Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
        {
            DeserializeMessage(message.Body, out string playerId, out int distanceCm, out decimal voltage);
            OnReportBackAvailable(playerId, distanceCm, voltage);
            return Task.CompletedTask;

            void DeserializeMessage(byte[] messageBody, out string pId, out int d, out decimal v)
            {
                using (var stream = new MemoryStream(messageBody))
                {
                    var reader = new BinaryReader(stream);
                    pId = reader.ReadString();
                    d = reader.ReadInt32();
                    v = reader.ReadDecimal();
                }
            }
        }

        private Task HandleExceptionAsync(ExceptionReceivedEventArgs args)
        {
            Debug.WriteLine($"An exception occured while reading from service bus: {args.Exception}");
            return Task.CompletedTask;
        }
    }

    public class ReportBackEventArgs : EventArgs
    {
        public ReportBackEventArgs(string playerId, int distanceCm, decimal voltage)
        {
            PlayerId = playerId;
            DistanceCm = distanceCm;
            Voltage = voltage;
        }

        public string PlayerId { get; set; }
        public int DistanceCm { get; }
        public decimal Voltage { get; }
    }
}