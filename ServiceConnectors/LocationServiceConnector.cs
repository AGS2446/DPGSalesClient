using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocationProxy;
using Microsoft.AspNetCore.Http;

namespace DPGSalesClient.ServiceConnectors
{
    public class LocationServiceConnector
    {
        private LocationProxyServiceClient _serLocation = new LocationProxyServiceClient(LocationProxyServiceClient.EndpointConfiguration.LocationProxySOAPEndPoint);
        public LocationServiceConnector(string strRemoteIP, IHttpContextAccessor httpContextAccessor)
        {
            if (!String.IsNullOrEmpty(strRemoteIP))
            {
                if (strRemoteIP.Contains("https"))
                {
                    _serLocation = new LocationProxyServiceClient(LocationProxyServiceClient.EndpointConfiguration.LocationProxySOAPEndPoint, strRemoteIP + "/LocationProxyService.svc/SOAP");
                }
                else
                {
                    _serLocation = new LocationProxyServiceClient(LocationProxyServiceClient.EndpointConfiguration.LocationProxySOAPEndPoint, strRemoteIP + "/LocationProxyService.svc/SOAP");
                    //_serLocation.Endpoint.Binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None);
                }

            }
            _serLocation.Endpoint.EndpointBehaviors.Add(new ServiceBehaviors.TokenEndpointBehavior(httpContextAccessor));
        }

        public async Task<AGS_Location> RetreiveLocationDetails()
        {
            return await _serLocation.RetrieveLocationDetailsAsync();
        }
        public async Task<AGS_Location> RetreiveLocationDetailsByOrg(string orgid,string clientId)
        {
            return await _serLocation.RetrieveLocationDetailsByOrgAsync(orgid, clientId);
        }
    }
}
