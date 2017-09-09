using System;
using System.IO;
using System.Threading.Tasks;
using IoTGame.Constants;
using Microsoft.Azure.Devices;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;

namespace IoTGame.Functions
{
    public static class ReportBack
    {
        private static readonly ServiceClient ServiceClient = ServiceClient.CreateFromConnectionString(IoTHubConstants.ConnectionString);

        [FunctionName("ReportBack")]
        public static async Task Run(
            [ServiceBusTrigger(ServiceBusConstants.ReportBackTopicName, ServiceBusConstants.FunctionSubscriptionName, AccessRights.Manage, Connection = "ServiceBus")]BrokeredMessage message, 
            TraceWriter log)
        {
            var stream = message.GetBody<Stream>();
            byte[] outputBytes;
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                outputBytes = memoryStream.ToArray();
            }

            var output = new Message(outputBytes)
            {
                ExpiryTimeUtc = DateTime.UtcNow + TimeSpan.FromSeconds(10)
            };
            await ServiceClient.SendAsync(IoTHubConstants.ControlDeviceId, output);
        }
    }
}
