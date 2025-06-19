using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GpsProxy;
using Microsoft.AspNetCore.Http;

namespace DPGSalesClient.ServiceConnectors
{
    public class GpsServiceConnector
    {

        private GpsProxyServiceClient _gpsCli = new GpsProxyServiceClient(GpsProxyServiceClient.EndpointConfiguration.GpsProxySOAPHttpsEndPoint);
        public GpsServiceConnector(string strRemoteIP, IHttpContextAccessor httpContextAccessor)
        {
            if (!String.IsNullOrEmpty(strRemoteIP))
            {
                if (strRemoteIP.Contains("https"))
                {
                    _gpsCli = new GpsProxyServiceClient(GpsProxyServiceClient.EndpointConfiguration.GpsProxySOAPHttpsEndPoint, strRemoteIP + "/GpsProxyService.svc/SOAP");
                }
                else
                {
                    _gpsCli = new GpsProxyServiceClient(GpsProxyServiceClient.EndpointConfiguration.GpsProxySOAPEndPoint, strRemoteIP + "/GpsProxyService.svc/SOAP");
                   // _gpsCli.Endpoint.Binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None);
                }
               
            }
            _gpsCli.Endpoint.EndpointBehaviors.Add(new ServiceBehaviors.TokenEndpointBehavior(httpContextAccessor));
        }

        public async Task<List<UserGpsInfo>> GetGpsAsync(GpsFilter filter)
        {
            return await _gpsCli.RetrieveAllAsync(filter);
        }

        public async Task<List<AGS_GpsTrack>> GetGpsDetailsAsync(string userId, string gpsDate)
        {
            return await _gpsCli.RetrieveAsync(userId, gpsDate);
        }


    }
}
