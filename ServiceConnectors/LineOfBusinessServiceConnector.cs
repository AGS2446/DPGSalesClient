using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LineOfBusinessProxy;
using Microsoft.AspNetCore.Http;

namespace DPGSalesClient.ServiceConnectors
{
    public class LineOfBusinessServiceConnector
    {
        private LineOfBusinessProxyServiceClient _serLineOfBusiness = new LineOfBusinessProxyServiceClient(LineOfBusinessProxyServiceClient.EndpointConfiguration.LineOfBusinessServiceSOAPHttpsEndPoint);
        public LineOfBusinessServiceConnector(string strRemoteIP, IHttpContextAccessor httpContextAccessor)
        {
            if (!String.IsNullOrEmpty(strRemoteIP))
            {
                if (strRemoteIP.Contains("https"))
                {
                    _serLineOfBusiness = new LineOfBusinessProxyServiceClient(LineOfBusinessProxyServiceClient.EndpointConfiguration.LineOfBusinessServiceSOAPHttpsEndPoint, strRemoteIP + "/LineOfBusinessProxyService.svc/SOAP");
                }
                else
                {
                    _serLineOfBusiness = new LineOfBusinessProxyServiceClient(LineOfBusinessProxyServiceClient.EndpointConfiguration.LineOfBusinessServiceSOAPEndPoint, strRemoteIP + "/LineOfBusinessProxyService.svc/SOAP");
                   // _serLineOfBusiness.Endpoint.Binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None);
                }
               
            }
            _serLineOfBusiness.Endpoint.EndpointBehaviors.Add(new ServiceBehaviors.TokenEndpointBehavior(httpContextAccessor));
        }

        public async Task<List<AGS_DropdownEntity>> GetLineOfBusinessAsync()
        {
            return await _serLineOfBusiness.FillLineOfBusinessAsync();
        }
    }
}
