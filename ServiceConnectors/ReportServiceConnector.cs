using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReportProxy;
using Microsoft.AspNetCore.Http;

namespace DPGSalesClient.ServiceConnectors
{
    public class ReportServiceConnector
    {
        private ReportProxyServiceClient _serReport = new ReportProxyServiceClient(ReportProxyServiceClient.EndpointConfiguration.ReportProxySOAPEndPoint);
        public ReportServiceConnector(string strRemoteIP, IHttpContextAccessor httpContextAccessor)
        {
            if (!String.IsNullOrEmpty(strRemoteIP))
            {
                if (strRemoteIP.Contains("https"))
                {
                   // _serReport = new ReportProxyServiceClient(ReportProxyServiceClient.EndpointConfiguration.ReportProxySOAPHttpsEndPoint, strRemoteIP + "/ReportProxyService.svc/SOAP");
                }
                else
                {
                    _serReport = new ReportProxyServiceClient(ReportProxyServiceClient.EndpointConfiguration.ReportProxySOAPEndPoint, strRemoteIP + "/ReportProxyService.svc/SOAP");
                   // _serReport.Endpoint.Binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None);
                }
               
            }
            _serReport.Endpoint.EndpointBehaviors.Add(new ServiceBehaviors.TokenEndpointBehavior(httpContextAccessor));            
        }

        public async Task<List<OpportunityApprovalReportResponse>> GetOpportunityApproval(CrmObjectReportRequest objInput)
        {
           return await _serReport.OpportunityApprovalReportAsync(objInput);
        }
        public async Task<List<SalesReportResponse>> SalesReportAsync(SalesReportRequest filter)
        {
            return await _serReport.SalesReportAsync(filter);
        }

        public async Task<List<ProjectOrderBookedReportResponse>> ProjectOrderBookedReportAsync(ProjectOrderBookedReportRequest filter)
        {
            return await _serReport.ProjectOrderBookedReportAsync(filter);
        }

        public async Task<List<CrmObjectReportResponse>> LeadReportChart(CrmObjectReportRequest request)
        {
            return await _serReport.LeadReportAsync(request);
        }

        public async Task<List<CrmObjectReportResponse>> OpportunityReportChart(CrmObjectReportRequest request)
        {
            return await _serReport.OpportunityReportAsync(request);
        }
        public async Task<List<CrmObjectReportResponse>> OpportunityDueDateReport(CrmObjectReportRequest request)
        {
            return await _serReport.OpportunityDueReportAsync(request);
        }


        public async Task<List<CrmObjectReportResponse>> QuotationReportChart(CrmObjectReportRequest request)
        {
            return await _serReport.QuotationReportAsync(request);
        }
        public async Task<List<CrmObjectReportResponse>> QuotationDueDateReport(CrmObjectReportRequest request)
        {
            return await _serReport.QuotationDueReportAsync(request);
        }
        public async Task<List<CrmObjectReportResponse>> OrderReportChart(CrmObjectReportRequest request)
        {
            return await _serReport.OrderReportAsync(request);
        }

        public async Task<List<OpportunitySummaryReportResponse>> OpportunitySummaryReport(CrmObjectReportRequest request)
        {
            return await _serReport.OpportunitySummaryReportAsync(request);
        }

        public async Task<bool> OpportunitySummaryReportUpdate(SummaryReportUpdateRequest request)
        {
            return await _serReport.SummaryReportUpdateAsync(request);
        }

        public async Task<List<WinlossReportResponse>> OrderWinLostReportChart(WinlossReportRequest request)
        {
            return await _serReport.WinlossOrderReportAsync(request);
        }

        public async Task<OrderDashboardResponse> OrderDashboard(OrderDashboardRequest request)
        {

            return await _serReport.OrderDashboardAsync(request);
        }

        public async Task<SalesDashboardResponse> SalesDashboard(CrmObjectReportRequest request)
        {
            return await _serReport.SalesDashboardAsync(request);
        }

    }
}
