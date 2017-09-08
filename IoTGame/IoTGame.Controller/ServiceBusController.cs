using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IoTGame.Constants;
using IoTGame.Driver;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace IoTGame.Controller
{
    public class ServiceBusController : IController
    {
        private readonly IDriver _driver;
        private readonly string _subscriptionName;

        private ISubscriptionClient _subscriptionClient;

        public ServiceBusController(IDriver driver, string subscriptionName)
        {
            _driver = driver;
            _subscriptionName = subscriptionName;
        }

        public async Task StartAsync()
        {
            await _driver.StartAsync();

            _subscriptionClient = new SubscriptionClient(ServiceBusConstants.ConnectionString, ServiceBusConstants.DriveCommandTopicName, _subscriptionName, ReceiveMode.ReceiveAndDelete);
            _subscriptionClient.RegisterMessageHandler(HandleMessageAsync, new MessageHandlerOptions(HandleExceptionAsync) { MaxConcurrentCalls = 1});
        }

        public async Task StopAsync()
        {
            await _subscriptionClient.CloseAsync();
            _subscriptionClient = null;

            await _driver.StopAsync();
        }

        private Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
        {
            var json = Encoding.UTF8.GetString(message.Body);
            var command = JsonConvert.DeserializeObject<DriveCommand>(json);
            return _driver.DriveAsync(command);
        }

        private Task HandleExceptionAsync(ExceptionReceivedEventArgs args)
        {
            Debug.WriteLine($"An exception occured while reading from service bus: {args.Exception}");
            return Task.CompletedTask;
        }
    }
}