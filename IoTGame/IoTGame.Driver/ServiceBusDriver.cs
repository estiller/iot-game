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
            var message = new Message(Encoding.UTF8.GetBytes(json));
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

        private void OnReportBackAvailable(int distanceCm, decimal voltage)
        {
            var handler = ReportBackAvailable;
            handler?.Invoke(this, new ReportBackEventArgs(distanceCm, voltage));
        }

        private Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
        {
            DeserializeMessage(message.Body, out int distanceCm, out decimal voltage);
            OnReportBackAvailable(distanceCm, voltage);
            return Task.CompletedTask;

            void DeserializeMessage(byte[] messageBody, out int d, out decimal v)
            {
                using (var stream = new MemoryStream(messageBody))
                {
                    var writer = new BinaryReader(stream);
                    d = writer.ReadInt32();
                    v = writer.ReadDecimal();
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
        public ReportBackEventArgs(int distanceCm, decimal voltage)
        {
            DistanceCm = distanceCm;
            Voltage = voltage;
        }

        public int DistanceCm { get; }
        public decimal Voltage { get; }
    }
}