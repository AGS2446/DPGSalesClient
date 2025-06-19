using Microsoft.AspNetCore.Http;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace DPGSalesClient.ServiceBehaviors
{

    public class Token
    {
        public static string Value { get; set; }
    }

    internal class TokenEndpointBehavior : IEndpointBehavior
    {
        private IHttpContextAccessor _httpContextAccessor;
        public TokenEndpointBehavior(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.ClientMessageInspectors.Add(new TokenExecutionInspector(_httpContextAccessor));
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }

        public void Validate(ServiceEndpoint endpoint) { }
    }

    internal class TokenExecutionInspector : IClientMessageInspector
    {
        private MessageHeaders headers;
        private IHttpContextAccessor _httpContextAccessor;
        public TokenExecutionInspector(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {

            HttpRequestMessageProperty requestMessage;
            if (request.Properties.ContainsKey(HttpRequestMessageProperty.Name))
            {
                requestMessage = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];
            }
            else
            {
                requestMessage = new HttpRequestMessageProperty();
                request.Properties.Add(HttpRequestMessageProperty.Name, requestMessage);
            }
            requestMessage.Headers["Token"] = _httpContextAccessor.HttpContext.Session.GetString("Token");
            return null;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {

        }
    }
}
