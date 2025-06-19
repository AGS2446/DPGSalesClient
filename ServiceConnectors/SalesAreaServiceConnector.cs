using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalesAreaProxy;
using Microsoft.AspNetCore.Http;

namespace DPGSalesClient.ServiceConnectors
{
    public class SalesAreaServiceConnector
    {
        private SalesAreaProxyServiceClient _serSalesArea = new SalesAreaProxyServiceClient(SalesAreaProxyServiceClient.EndpointConfiguration.SalesAreaProxySOAPHttpsEndPoint);

        public SalesAreaServiceConnector(string strRemoteIP, IHttpContextAccessor httpContextAccessor)
        {
            if (!String.IsNullOrEmpty(strRemoteIP))
            {
                if (strRemoteIP.Contains("https"))
                {
                    _serSalesArea = new SalesAreaProxyServiceClient(SalesAreaProxyServiceClient.EndpointConfiguration.SalesAreaProxySOAPHttpsEndPoint, strRemoteIP + "/SalesAreaProxyService.svc/SOAP");
                }
                else
                {
                    _serSalesArea = new SalesAreaProxyServiceClient(SalesAreaProxyServiceClient.EndpointConfiguration.SalesAreaProxySOAPEndPoint, strRemoteIP + "/SalesAreaProxyService.svc/SOAP");
                    //_serSalesArea.Endpoint.Binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None);
                }
               
            }
            _serSalesArea.Endpoint.EndpointBehaviors.Add(new ServiceBehaviors.TokenEndpointBehavior(httpContextAccessor));
        }

        public async Task<List<AGS_DropdownEntity>> GetSalesOrg()
        {
            return await _serSalesArea.FillSalesOrgsAsync();
        }
        public async Task<List<AGS_DropdownEntity>> GetDistChannel(string strSalesOrg)
        {
            return await _serSalesArea.FillDistChannelsAsync(strSalesOrg);
        }
        public async Task<List<AGS_DropdownEntity>> GetDivision(string strSalesOrg,string DistChannel)
        {
            return await _serSalesArea.FillDivisionsAsync(strSalesOrg, DistChannel);
        }

    }
}
