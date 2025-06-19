using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductProxy;
using Microsoft.AspNetCore.Http;

namespace DPGSalesClient.ServiceConnectors
{
    public class ProductServiceConnector
    {
        private ProductProxyServiceClient _serProduct = new ProductProxyServiceClient(ProductProxyServiceClient.EndpointConfiguration.ProductProxySOAPHttpsEndPoint);
        public ProductServiceConnector(string strRemoteIP, IHttpContextAccessor httpContextAccessor)
        {
            if (!String.IsNullOrEmpty(strRemoteIP))
            {
                if (strRemoteIP.Contains("https"))
                {
                    _serProduct = new ProductProxyServiceClient(ProductProxyServiceClient.EndpointConfiguration.ProductProxySOAPHttpsEndPoint, strRemoteIP + "/ProductProxyService.svc/SOAP");
                }
                else
                {
                    _serProduct = new ProductProxyServiceClient(ProductProxyServiceClient.EndpointConfiguration.ProductProxySOAPEndPoint, strRemoteIP + "/ProductProxyService.svc/SOAP");
                    //_serProduct.Endpoint.Binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None);
                }
               
            }
            _serProduct.Endpoint.EndpointBehaviors.Add(new ServiceBehaviors.TokenEndpointBehavior(httpContextAccessor));            
        }

        public async Task<List<AGS_DropdownEntity>> GetProducts()
        {
            return await _serProduct.FillProductsAsync();
        }

        public async Task<List<AGS_Product>> GetProductsList()
        {
            return await _serProduct.RetrieveAllAsync(null, null, 0, null, 0);

        }
        public async Task<List<AGS_Product>> SearchProducts(string strKey)
        {
            return await _serProduct.SearchAsync(strKey);
        }

    }
}
