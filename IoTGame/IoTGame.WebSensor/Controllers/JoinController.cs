using System;
using System.Web.Http;
using IoTGame.Constants;
using IoTGame.WebSensor.Models;
using Microsoft.ServiceBus;

namespace IoTGame.WebSensor.Controllers
{
    public class JoinController : ApiController
    {
        private static readonly Uri ServiceUri = ServiceBusEnvironment.CreateServiceUri(Uri.UriSchemeHttps, EventHubConstants.Namespace, EventHubConstants.Name);

        public IHttpActionResult Post(JoinRequest data)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var publisherId = CleanupId(data);
            var publisherUri = new Uri(ServiceUri, $"publishers/{publisherId}/").AbsoluteUri;
            return Json(new JoinResponse
            {
                ServiceUri = publisherUri,
                Signature = SharedAccessSignatureTokenProvider.GetSharedAccessSignature(EventHubConstants.PolicyName, EventHubConstants.PolicyKey, publisherUri, TimeSpan.FromHours(1))
            });
        }

        private static string CleanupId(JoinRequest data)
        {
            return data.Email.Replace("@", "__").Replace('.', '_').Replace('+', '_');
        }
    }
}
