using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseOrderProxy;
using Microsoft.AspNetCore.Http;

namespace DPGSalesClient.ServiceConnectors
{
    public class PurchaseOrderServiceConnector
    {
        private PurchaseOrderProxyServiceClient _serPurchaseOrder = new PurchaseOrderProxyServiceClient(PurchaseOrderProxyServiceClient.EndpointConfiguration.PurchaseOrderProxySOAPHttpsEndPoint);
        public PurchaseOrderServiceConnector(string strRemoteIP, IHttpContextAccessor httpContextAccessor)
        {
            if (!String.IsNullOrEmpty(strRemoteIP))
            {
                if (strRemoteIP.Contains("https"))
                {
                    _serPurchaseOrder = new PurchaseOrderProxyServiceClient(PurchaseOrderProxyServiceClient.EndpointConfiguration.PurchaseOrderProxySOAPHttpsEndPoint, strRemoteIP + "/PurchaseOrderProxyService.svc/SOAP");
                }
                else
                {
                    _serPurchaseOrder = new PurchaseOrderProxyServiceClient(PurchaseOrderProxyServiceClient.EndpointConfiguration.PurchaseOrderProxySOAPEndPoint, strRemoteIP + "/PurchaseOrderProxyService.svc/SOAP");
                    //_serPurchaseOrder.Endpoint.Binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None);
                }
               
            }
            _serPurchaseOrder.Endpoint.EndpointBehaviors.Add(new ServiceBehaviors.TokenEndpointBehavior(httpContextAccessor));
        }
        public async Task<List<AGS_PurchaseOrder>> GetSAPOrders()
        {
            return await _serPurchaseOrder.RetrieveAllAsync("", "", 0, "", 0);
        }

        public async Task<string> Create(AGS_PurchaseOrder newObj)
        {
            return await _serPurchaseOrder.CreateAsync(newObj);
        }

        public async Task<List<AGS_PurchaseOrder>> Search(string strKey)
        {
            return await _serPurchaseOrder.SearchAsync(strKey);
        }
    }
}
