using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderProxy;
using Microsoft.AspNetCore.Http;

namespace DPGSalesClient.ServiceConnectors
{
    public class OrderServiceConnector
    {
        private OrderProxyServiceClient _serOrder = new OrderProxyServiceClient(OrderProxyServiceClient.EndpointConfiguration.OrderProxySOAPEndPoint);
        public OrderServiceConnector(string strRemoteIP, IHttpContextAccessor httpContextAccessor)
        {
            if (!String.IsNullOrEmpty(strRemoteIP))
            {
                if (strRemoteIP.Contains("https"))
                {
                    _serOrder = new OrderProxyServiceClient(OrderProxyServiceClient.EndpointConfiguration.OrderProxySOAPEndPoint, strRemoteIP + "/OrderProxyService.svc/SOAP");
                }
                else
                {
                    _serOrder = new OrderProxyServiceClient(OrderProxyServiceClient.EndpointConfiguration.OrderProxySOAPEndPoint, strRemoteIP + "/OrderProxyService.svc/SOAP");
                   // _serOrder.Endpoint.Binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None);
                }
                
            }
            _serOrder.Endpoint.EndpointBehaviors.Add(new ServiceBehaviors.TokenEndpointBehavior(httpContextAccessor));
           
        }

      

        public async Task<string> CreateOrder(AGS_Order newObj)
        {
            return await _serOrder.CreateAsync(newObj);
        }



        public async Task<bool> DeleteOrder(string strId)
        {
            return await _serOrder.DeleteAsync(strId);
            
        }

        public async Task<bool> UpdateOrder(string strId, AGS_Order model)
        {
            return await _serOrder.UpdateAsync(strId, model);
        }
        public async Task<AGS_Order> RetriveOrder(string strId)
        {
            return await _serOrder.RetrieveAsync(strId);
        }
        public async Task<List<AGS_Order>> RetriveOrders(string sortBy, string sortOrder, int pageSize, string marker, int currentIndex)
        {
            return await _serOrder.RetrieveAllAsync(sortBy, sortOrder, pageSize, marker, currentIndex);
        }


        public async Task<List<AGS_Order>> SearchOrders(string strKey)
        {
            return await _serOrder.SearchAsync(strKey);           
        }

        public async Task<List<AGS_Order>> OrderReport(CrmObjectReportRequest objRequest)
        {
            return await _serOrder.ReportDataAsync(objRequest);
        }

        public async Task<List<AGS_Order>> OrderWinLossReport(WinlossReportRequest objRequest)
        {
            return await _serOrder.WinlossReportAsync(objRequest);
        }

        public async Task<List<string>> OrdersUplaod(List<AGS_UploadOrder> objRequest)
        {
            return await _serOrder.UploadAsync(objRequest);
        }

        public async Task<bool> UploadContractValue(AGS_UploadContractValue model)
        {
            return await _serOrder.UploadContractValueAsync(model);
        }
    }
}
