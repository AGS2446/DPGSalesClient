using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EquipmentProxy;
using Microsoft.AspNetCore.Http;

namespace DPGSalesClient.ServiceConnectors
{
    public class EquipmentServiceConnector
    {
        private EquipmentProxyServiceClient _serEquipment = new EquipmentProxyServiceClient(EquipmentProxyServiceClient.EndpointConfiguration.EquipmentProxySOAPHttpsEndPoint);

        public EquipmentServiceConnector(string strRemoteIP, IHttpContextAccessor httpContextAccessor)
        {
            if (!String.IsNullOrEmpty(strRemoteIP))
            {
                if (strRemoteIP.Contains("https"))
                {
                    _serEquipment = new EquipmentProxyServiceClient(EquipmentProxyServiceClient.EndpointConfiguration.EquipmentProxySOAPHttpsEndPoint, strRemoteIP + "/EquipmentProxyService.svc/SOAP");
                }
                else
                {
                    _serEquipment = new EquipmentProxyServiceClient(EquipmentProxyServiceClient.EndpointConfiguration.EquipmentProxySOAPEndPoint, strRemoteIP + "/EquipmentProxyService.svc/SOAP");
                    //_serEquipment.Endpoint.Binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None);
                }
                
            }
            _serEquipment.Endpoint.EndpointBehaviors.Add(new ServiceBehaviors.TokenEndpointBehavior(httpContextAccessor));
        }
        public async Task<List<AGS_Equipment>> GetEquipments(string strKey)
        {
            return await _serEquipment.SearchAsync(strKey);
        }

        public async Task<List<AGS_DropdownEntity>> GetPlants(string strKey)
        {
            return await _serEquipment.FillEquipmentPlantsAsync(strKey);
        }
    }
}
