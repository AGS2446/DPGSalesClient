using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuotationProxy;
using Microsoft.AspNetCore.Http;

namespace DPGSalesClient.ServiceConnectors
{
    public class QuotationServiceConnector
    {
        private QuotationProxyServiceClient _serQuote = new QuotationProxyServiceClient(QuotationProxyServiceClient.EndpointConfiguration.QuotationProxySOAPEndPoint);
        public QuotationServiceConnector(string strRemoteIP, IHttpContextAccessor httpContextAccessor)
        {
            if (!String.IsNullOrEmpty(strRemoteIP))
            {
                if (strRemoteIP.Contains("https"))
                {
                    _serQuote = new QuotationProxyServiceClient(QuotationProxyServiceClient.EndpointConfiguration.QuotationProxySOAPEndPoint, strRemoteIP + "/QuotationProxyService.svc/SOAP");
                }
                else
                {
                    _serQuote = new QuotationProxyServiceClient(QuotationProxyServiceClient.EndpointConfiguration.QuotationProxySOAPEndPoint, strRemoteIP + "/QuotationProxyService.svc/SOAP");
                    //_serQuote.Endpoint.Binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None);
                }
                
            }
            _serQuote.Endpoint.EndpointBehaviors.Add(new ServiceBehaviors.TokenEndpointBehavior(httpContextAccessor));
           
        }

      

        public async Task<string> CreateQuote(AGS_Quotation newObj)
        {
            return await _serQuote.CreateAsync(newObj);
        }



        public async Task<bool> DeleteQuote(string strId)
        {
            return await _serQuote.DeleteAsync(strId);
            
        }

        public async Task<bool> UpdateQuote(string strId, AGS_Quotation model)
        {
            return await _serQuote.UpdateAsync(strId, model);
        }
        public async Task<AGS_Quotation> RetriveQuote(string strId)
        {
            return await _serQuote.RetrieveAsync(strId);
        }
        public async Task<List<AGS_Quotation>> RetriveQuotes(string sortBy, string sortOrder, int pageSize, string marker, int currentIndex)
        {
            return await _serQuote.RetrieveAllAsync(sortBy, sortOrder, pageSize, marker, currentIndex);
        }


        public async Task<List<AGS_Quotation>> SearchQuotes(string strKey)
        {
            return await _serQuote.SearchAsync(strKey);           
        }
        public async Task<List<AGS_Quotation>> QuotationReport(CrmObjectReportRequest request)
        {
            return await _serQuote.ReportDataAsync(request);
        }
        public async Task<bool> UploadContractValue(AGS_UploadContractValue model)
        {
            return await _serQuote.UploadContractValueAsync(model);
        }
        public async Task<List<AGS_QuotationHistory>> QuotationHistory(string id)
        {
            return await _serQuote.HistoryAsync(id);
        }
    }
}
