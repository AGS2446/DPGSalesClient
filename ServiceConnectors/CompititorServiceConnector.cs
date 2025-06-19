using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompetitorProxy;
using Microsoft.AspNetCore.Http;

namespace DPGSalesClient.ServiceConnectors
{
    public class CompititorServiceConnector
    {
        private CompetitorProxyServiceClient _serComp = new CompetitorProxyServiceClient(CompetitorProxyServiceClient.EndpointConfiguration.CompetitorProxySOAPHttpsEndPoint);
        public CompititorServiceConnector(string strRemoteIP, IHttpContextAccessor httpContextAccessor)
        {
            if (!String.IsNullOrEmpty(strRemoteIP))
            {
                if (strRemoteIP.Contains("https"))
                {
                    _serComp = new CompetitorProxyServiceClient(CompetitorProxyServiceClient.EndpointConfiguration.CompetitorProxySOAPHttpsEndPoint, strRemoteIP + "/CompetitorProxyService.svc/SOAP");
                }
                else
                {
                    _serComp = new CompetitorProxyServiceClient(CompetitorProxyServiceClient.EndpointConfiguration.CompetitorProxySOAPEndPoint, strRemoteIP + "/CompetitorProxyService.svc/SOAP");
                    //_serComp.Endpoint.Binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None);
                }
               
            }
            _serComp.Endpoint.EndpointBehaviors.Add(new ServiceBehaviors.TokenEndpointBehavior(httpContextAccessor));            
        }

        public async Task<List<AGS_DropdownEntity>> FillCompititors()
        {
           return await _serComp.FillCompetitorsAsync();
        }
        
    }
}
