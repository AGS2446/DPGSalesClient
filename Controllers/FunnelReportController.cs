using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using DPGSalesClient.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace DPGSalesClient.Controllers
{
    public class FunnelReportController : Controller
    {
        
        #region varaibles

        ServiceConnectors.ClientServiceConnector _serClient = null;
        ServiceConnectors.OrganizationServiceConnector _serOrganization = null;
        ServiceConnectors.LeadServiceConnector _serLead = null;
        ServiceConnectors.ReportServiceConnector _serReport = null;
        ServiceConnectors.EntityMapServiceConnector _serEntity = null;

        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        #endregion


        #region Constructor
        public FunnelReportController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor)
        {
            string strIp = SalesStaticMethods.GetRemoteIp(appSettings);

            _serClient = new ServiceConnectors.ClientServiceConnector(strIp, httpContextAccessor);
            _serOrganization = new ServiceConnectors.OrganizationServiceConnector(strIp, httpContextAccessor);
            _serLead = new ServiceConnectors.LeadServiceConnector(strIp, httpContextAccessor);
            _serReport = new ServiceConnectors.ReportServiceConnector(strIp, httpContextAccessor);
            _serEntity = new ServiceConnectors.EntityMapServiceConnector(strIp, httpContextAccessor);
            _logger = loggerFactory.CreateLogger<LeadReportController>();
            _hostingEnvironment = hostingEnvironment;
        }

        #endregion     
       

        public async Task<IActionResult> Report()
        {
            HttpContext.Session.SetString("Controller", "FunnelReport");

            FunnelReportSearchModel searchModel = new FunnelReportSearchModel();
            var Fromdate = "01/04/2018";

            var objList = new List<SelectListItem>();
            try
            {
                searchModel.DateFrom = Fromdate;
                searchModel.DateTo = DateTime.Now.ToString("dd/MM/yyyy").Replace("-","/");
                var lstDropdownEntity = await _serClient.GetClients();
                if (lstDropdownEntity != null && lstDropdownEntity.Count > 0)
                {
                    objList = lstDropdownEntity.Select(x => new SelectListItem { Text = x.Text, Value = x.Value }).ToList();
                    searchModel.DivisionList = new SelectList(objList, "Value", "Text");
                }
                else
                {
                    searchModel.DivisionList = null;
                }
                var lstBranches = await _serOrganization.GetBranches();
                if (lstBranches != null && lstBranches.Count > 0)
                {
                    objList = lstBranches.Select(x => new SelectListItem { Text = x.Text, Value = x.Value }).ToList();
                    searchModel.BranchList = new SelectList(objList, "Value", "Text");
                }
                else
                {
                    searchModel.BranchList = null;
                }
                var lsEntity = await _serEntity.RetriveByObjectName("LEAD");
                if (lsEntity != null)
                {                     
                    searchModel.SegmentList = SalesStaticMethods.GetSelectlistItemsByName("CUSTOMERSUBSEGMENT", lsEntity, "B");
                    searchModel.SegmentList.RemoveAt(0);
                }
                if (searchModel.SegmentList == null)
                {
                    searchModel.SegmentList = null;
                }
            }
            catch (Exception ex)
            {

            }
            
            return View(searchModel);
        }
        [HttpPost]
        public async Task<IActionResult> GetFunnelReportChart(string Division, string Branch, string Segment, string DateFrom, string DateTo)
        {
            try
            {
                ReportProxy.CrmObjectReportRequest objRequest = new ReportProxy.CrmObjectReportRequest();
                objRequest.Division = Division;
                objRequest.Branch = Branch;
                objRequest.CustomerSubsegment = Segment;
                objRequest.DateFrom = SalesStaticMethods.ConvertDate(DateFrom);
                objRequest.DateTo = SalesStaticMethods.ConvertDate(DateTo);

                ReportProxy.SalesDashboardResponse responce = new ReportProxy.SalesDashboardResponse();
                responce = await _serReport.SalesDashboard(objRequest);
                if (responce != null)
                {
                    return Json(responce);
                }

            }
            catch (Exception ex)
            {

            }
            return Json(null);
        }

        public async Task<IActionResult> AmFunnelChart()
        {
            HttpContext.Session.SetString("Controller", "FunnelReport");

            FunnelReportSearchModel searchModel = new FunnelReportSearchModel();
            var Fromdate = "01/04/2018";

            var objList = new List<SelectListItem>();
            try
            {
                searchModel.DateFrom = Fromdate;
                searchModel.DateTo = DateTime.Now.ToString("dd/MM/yyyy").Replace("-", "/");
                var lstDropdownEntity = await _serClient.GetClients();
                if (lstDropdownEntity != null && lstDropdownEntity.Count > 0)
                {
                    objList = lstDropdownEntity.Select(x => new SelectListItem { Text = x.Text, Value = x.Value }).ToList();
                    searchModel.DivisionList = new SelectList(objList, "Value", "Text");
                }
                else
                {
                    searchModel.DivisionList = null;
                }
                var lstBranches = await _serOrganization.GetBranches();
                if (lstBranches != null && lstBranches.Count > 0)
                {
                    objList = lstBranches.Select(x => new SelectListItem { Text = x.Text, Value = x.Value }).ToList();
                    searchModel.BranchList = new SelectList(objList, "Value", "Text");
                }
                else
                {
                    searchModel.BranchList = null;
                }
                var lsEntity = await _serEntity.RetriveByObjectName("LEAD");
                if (lsEntity != null)
                {
                    searchModel.SegmentList = SalesStaticMethods.GetSelectlistItemsByName("CUSTOMERSUBSEGMENT", lsEntity, "B");
                    searchModel.SegmentList.RemoveAt(0);
                }
                if (searchModel.SegmentList == null)
                {
                    searchModel.SegmentList = null;
                }
            }
            catch (Exception ex)
            {

            }

            return View(searchModel);
        }

    }
}
