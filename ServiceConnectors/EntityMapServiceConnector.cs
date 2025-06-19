using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EntityMapProxy;
using Microsoft.AspNetCore.Http;

namespace DPGSalesClient.ServiceConnectors
{
    public class EntityMapServiceConnector
    {
        private EntityMapProxyServiceClient _serEntityMap = new EntityMapProxyServiceClient(EntityMapProxyServiceClient.EndpointConfiguration.EntityMapProxySOAPHttpsEndPoint);
        public EntityMapServiceConnector(string strRemoteIP, IHttpContextAccessor httpContextAccessor)
        {
            if (!String.IsNullOrEmpty(strRemoteIP))
            {
                if (strRemoteIP.Contains("https"))
                {
                    _serEntityMap = new EntityMapProxyServiceClient(EntityMapProxyServiceClient.EndpointConfiguration.EntityMapProxySOAPHttpsEndPoint, strRemoteIP + "/EntityMapProxyService.svc/SOAP");
                }
                else
                {
                    _serEntityMap = new EntityMapProxyServiceClient(EntityMapProxyServiceClient.EndpointConfiguration.EntityMapProxySOAPEndPoint, strRemoteIP + "/EntityMapProxyService.svc/SOAP");
                    //_serEntityMap.Endpoint.Binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None);
                }
                
            }
            _serEntityMap.Endpoint.EndpointBehaviors.Add(new ServiceBehaviors.TokenEndpointBehavior(httpContextAccessor));          
        }

        public async Task<List<AGS_EntityMap>> RetriveAll()
        {
           return await _serEntityMap.RetrieveAllAsync();
        }
        public async Task<List<AGS_EntityMap>> RetriveByName(string objectName,string AttributeName)
        {
            return await _serEntityMap.RetrieveByAttributeNameAsync(objectName, AttributeName);
        }
        public async Task<AGS_EntityMap> RetriveById(string id)
        {
            return await _serEntityMap.RetrieveByIDAsync(id);
        }
        public async Task<List<AGS_EntityMap>> RetriveByObjectName(string objectName)
        {
            return await _serEntityMap.RetrieveByObjectNameAsync(objectName);
        }
        public async Task<List<AGS_EntityMap>> RetriveByParentId(string objectName, string attributeName,string parentId)
        {
            return await _serEntityMap.RetrieveByParentAsync(objectName,attributeName,parentId);
        }

    }
}
