using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeadProxy;
using Microsoft.AspNetCore.Http;

namespace DPGSalesClient.ServiceConnectors
{
    public class LeadServiceConnector
    {

        private LeadProxyServiceClient _leadCli = new LeadProxyServiceClient(LeadProxyServiceClient.EndpointConfiguration.LeadProxySOAPEndPoint);
        public LeadServiceConnector(string strRemoteIP, IHttpContextAccessor httpContextAccessor)
        {
            if (!String.IsNullOrEmpty(strRemoteIP))
            {
                if (strRemoteIP.Contains("https"))
                {
                    _leadCli = new LeadProxyServiceClient(LeadProxyServiceClient.EndpointConfiguration.LeadProxySOAPEndPoint, strRemoteIP + "/LeadProxyService.svc/SOAP");
                }
                else
                {
                    _leadCli = new LeadProxyServiceClient(LeadProxyServiceClient.EndpointConfiguration.LeadProxySOAPEndPoint, strRemoteIP + "/LeadProxyService.svc/SOAP");
                    //_leadCli.Endpoint.Binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None);
                }
           
            }
            _leadCli.Endpoint.EndpointBehaviors.Add(new ServiceBehaviors.TokenEndpointBehavior(httpContextAccessor));
        }

        public async Task<List<AGS_Lead>> GetLeadsAsync()
        {
            try
            {
                return await _leadCli.RetrieveAllAsync("", "", 0, "", 0);
            }
            catch (Exception ex)
            {
            }
            return null;
        }


        public async Task<string> CreateLead(AGS_Lead newObj)
        {
            try
            {
                return await _leadCli.CreateAsync(newObj);
            }
            catch (Exception ex)
            {
            }
            return null;
        }

    

        public async Task<bool> DeleteLead(string strId)
        {
            try
            {
                return await _leadCli.DeleteAsync(strId);
            }
            catch (Exception ex)
            {
            }
            return false;
           
        }

        public async Task<bool> UpdateLead(string strId, AGS_Lead model)
        {
            try
            {
                return await _leadCli.UpdateAsync(strId, model);
            }
            catch (Exception ex)
            {
            }
            return false;
           
        }
        public async Task<AGS_Lead> RetriveLead(string strId)
        {
            return await _leadCli.RetrieveAsync(strId);
        }
        public async Task<List<AGS_Lead>> RetriveLeads(string sortBy, string sortOrder, int pageSize, string marker, int currentIndex)
        {
            return await _leadCli.RetrieveAllAsync(sortBy, sortOrder, pageSize, marker, currentIndex);
        }

     
        public async Task<List<AGS_Lead>> SearchLeads(string strKey)
        {
            return await _leadCli.SearchAsync(strKey);
        }

        public async Task<List<AGS_Lead>> LeadReport(CrmObjectReportRequest objRequest)
        {
            return await _leadCli.ReportDataAsync(objRequest);
        }
        public async Task<List<AGS_LeadHistory>> LeadHistory(string strLeadID)
        {
            return await _leadCli.HistoryAsync(strLeadID);
        }

    }
}
