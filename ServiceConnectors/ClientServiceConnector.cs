using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClientProxy;
using Microsoft.AspNetCore.Http;

namespace DPGSalesClient.ServiceConnectors
{
    public class ClientServiceConnector
    {
        private ClientProxyServiceClient _serClient = new ClientProxyServiceClient(ClientProxyServiceClient.EndpointConfiguration.ClientServiceSOAPHttpsEndPoint);
        public ClientServiceConnector(string strRemoteIP, IHttpContextAccessor httpContextAccessor)
        {
            if (!String.IsNullOrEmpty(strRemoteIP))
            {
                if (strRemoteIP.Contains("https"))
                {
                    _serClient = new ClientProxyServiceClient(ClientProxyServiceClient.EndpointConfiguration.ClientServiceSOAPHttpsEndPoint, strRemoteIP + "/ClientProxyService.svc/SOAP");
                }
                else
                {
                    _serClient = new ClientProxyServiceClient(ClientProxyServiceClient.EndpointConfiguration.ClientServiceSOAPEndPoint, strRemoteIP + "/ClientProxyService.svc/SOAP");
                   // _serClient.Endpoint.Binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None);
                }

                
            }
            _serClient.Endpoint.EndpointBehaviors.Add(new ServiceBehaviors.TokenEndpointBehavior(httpContextAccessor));
        }

        public async Task<List<AGS_DropdownEntity>> GetClients()
        {
            return await _serClient.FillClientsAsync();
        }
    }
}
