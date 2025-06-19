using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using DPGSalesClient.Models;
using ReportProxy;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using System.IO;
using Microsoft.Extensions.Options;

namespace DPGSalesClient.Controllers
{
    public class ProjectOrderBookedReportController : Controller
    {
        #region Varaibles

        ServiceConnectors.ReportServiceConnector _serReport = null;
        ServiceConnectors.OrganizationServiceConnector _serOrg = null;
        ServiceConnectors.ClientServiceConnector _serClnt = null;
        ServiceConnectors.OrderServiceConnector _serOrd = null;

        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        #endregion

        #region Constructure
        public ProjectOrderBookedReportController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor)
        {
            string strIp = SalesStaticMethods.GetRemoteIp(appSettings);
            _serReport = new ServiceConnectors.ReportServiceConnector(strIp, httpContextAccessor);
            _serOrg = new ServiceConnectors.OrganizationServiceConnector(strIp, httpContextAccessor);
            _serClnt = new ServiceConnectors.ClientServiceConnector(strIp, httpContextAccessor);
            _serOrd = new ServiceConnectors.OrderServiceConnector(strIp, httpContextAccessor);
            _logger = loggerFactory.CreateLogger<ProjectOrderBookedReportController>();
            _hostingEnvironment = hostingEnvironment;
        }
        #endregion

        /******************************************************************
        ******************* Sales Report **********************************
        ******************************************************************/


        #region Project Order Booked Report

        public async Task<IActionResult> Index()
        {
            HttpContext.Session.SetString("Controller", "ProjectOrderBookedReport");
            ViewBag.StartDate = DateTime.Now.ToString("MM/yyyy");
            var clientResult = await _serClnt.GetClients();
            ViewBag.Divisions = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(clientResult.Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Text = x.Text, Value = x.Value }).ToList(), "Value", "Text");
            var bracnchResult = await _serOrg.GetBranches();
            ViewBag.Branches = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(bracnchResult.Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Text = x.Text, Value = x.Value }).ToList(), "Value", "Text");
            return View();
        }

        #endregion

        #region Child Entities        

        [HttpPost]
        public async Task<JsonResult> GetProjectOrderBookedReport(string startDate, string division, string branch)
        {
            ProjectOrderBookedReportRequest objReq = new ProjectOrderBookedReportRequest()
            {
                Branch = branch,
                Division = division
            };

            if (startDate != null && startDate.Trim().Length > 0)
            {
                objReq.StartDate = DateTime.ParseExact(startDate.Trim(), "MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                objReq.StartDate = DateTime.Now;
            }
            var result = await _serReport.ProjectOrderBookedReportAsync(objReq);
            if (result.Count > 0)
            {
                BarViewModel barViewModel = new BarViewModel();
                barViewModel.labelData = result.Select(d => d.OrderType).ToList();
                barViewModel.c_PrevMonthData = result.Select(d => d.C_PrevMonth.Value).ToList();
                barViewModel.c_CurrentMonthData = result.Select(d => d.C_CurrentMonth.Value).ToList();
                barViewModel.c_UptoCurrentMonthData = result.Select(d => d.C_UptoCurrentMonth.Value).ToList();
                barViewModel.p_PrevMonthData = result.Select(d => d.P_PrevMonth.Value).ToList();
                barViewModel.p_CurrentMonthData = result.Select(d => d.P_CurrentMonth.Value).ToList();
                barViewModel.p_UptoCurrentMonthData = result.Select(d => d.P_UptoCurrentMonth.Value).ToList();
                return Json(barViewModel);
            }
            else
            {
                return Json(null);
            }
        }

        #endregion
    }
}
