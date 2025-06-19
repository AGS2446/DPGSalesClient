using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpportunityProxy;
using Microsoft.AspNetCore.Http;

namespace DPGSalesClient.ServiceConnectors
{
    public class OpportunityServiceConnector
    {
        private OpportunityProxyServiceClient _serOpp = new OpportunityProxyServiceClient(OpportunityProxyServiceClient.EndpointConfiguration.OpportunityProxySOAPEndPoint);
        public OpportunityServiceConnector(string strRemoteIP, IHttpContextAccessor httpContextAccessor)
        {
            if (!String.IsNullOrEmpty(strRemoteIP))
            {
                if (strRemoteIP.Contains("https"))
                {
                  //  _serOpp = new OpportunityProxyServiceClient(OpportunityProxyServiceClient.EndpointConfiguration.OpportunityProxySOAPEndPoint, strRemoteIP + "/OpportunityProxyService.svc/SOAP");
                }
                else
                {
                    _serOpp = new OpportunityProxyServiceClient(OpportunityProxyServiceClient.EndpointConfiguration.OpportunityProxySOAPEndPoint, strRemoteIP + "/OpportunityProxyService.svc/SOAP");
                   // _serOpp.Endpoint.Binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None);
                }
               
            }
            _serOpp.Endpoint.EndpointBehaviors.Add(new ServiceBehaviors.TokenEndpointBehavior(httpContextAccessor));
           
        }

        public async Task<string> CreateOpportunity(AGS_Opportunity newObj)
        {
            return await _serOpp.CreateAsync(newObj);
        }
        public async Task<bool> DeleteOpportunity(string strId)
        {
            return await _serOpp.DeleteAsync(strId);            
        }

        public async Task<bool> UpdateOpportunity(string strId, AGS_Opportunity model)
        {
            return await _serOpp.UpdateAsync(strId, model);
        }
        public async Task<AGS_Opportunity> RetriveOpportunity(string strId)
        {
            return await _serOpp.RetrieveAsync(strId);
        }
        public async Task<List<AGS_Opportunity>> RetriveOpportunities(string sortBy, string sortOrder, int pageSize, string marker, int currentIndex)
        {
            return await _serOpp.RetrieveAllAsync(sortBy, sortOrder, pageSize, marker, currentIndex);
        }
        public async Task<List<AGS_Opportunity>> SearchOpportunities(string strKey)
        {
            return await _serOpp.SearchAsync(strKey);           
        }
        public async Task<List<AGS_Opportunity>> OpportunityReport(CrmObjectReportRequest objRequest)
        {
            return await _serOpp.ReportDataAsync(objRequest);
        }
        public async Task<List<AGS_OpportunityHistory>> OpportunityHistory(string id)
        {
            return await _serOpp.HistoryAsync(id);
        }
        public async Task<bool> UploadContractValue(AGS_UploadContractValue model)
        {
            return await _serOpp.UploadContractValueAsync(model);
        }
    }
}
