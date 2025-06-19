using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using DPGSalesClient.Models;
using System.IO;
using LeadProxy;
using OfficeOpenXml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using OpportunityProxy;
using OrderProxy;
using QuotationProxy;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;


// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace DPGSalesClient.Controllers
{
    public class StrikeLeadReportController : Controller
    {
        ServiceConnectors.ReportServiceConnector _ReportProxyServiceConnector = null;
        ServiceConnectors.LeadServiceConnector _serLead = null;
        ServiceConnectors.ClientServiceConnector _serClient = null;
        ServiceConnectors.OrganizationServiceConnector _serOrganization = null;

        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;
        // GET: /<controller>/

        public StrikeLeadReportController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor)
        {
            string strIp = SalesStaticMethods.GetRemoteIp(appSettings);
            _ReportProxyServiceConnector = new ServiceConnectors.ReportServiceConnector(strIp, httpContextAccessor);
            _serLead = new ServiceConnectors.LeadServiceConnector(strIp, httpContextAccessor);
            _serClient = new ServiceConnectors.ClientServiceConnector(strIp, httpContextAccessor);
            _serOrganization = new ServiceConnectors.OrganizationServiceConnector(strIp, httpContextAccessor);


            _logger = loggerFactory.CreateLogger<StrikeLeadReportController>();
            _hostingEnvironment = hostingEnvironment;

        }


        public IActionResult Index()
        {
            return View();
        }
        #region Lead
        public async Task<IActionResult> LeadReport()
        {
            HttpContext.Session.SetString("Controller", "StrikeLeadReport");

            StLeadReportSearchModel searchModel = new StLeadReportSearchModel();
            var objList = new List<SelectListItem>();
            var lstDropdownEntity = await _serClient.GetClients();
            if (lstDropdownEntity != null && lstDropdownEntity.Count > 0)
            {
                objList = lstDropdownEntity.Select(x => new SelectListItem { Text = x.Text, Value = x.Value }).ToList();
                searchModel.DivisionList = new SelectList(objList, "Value", "Text");
            }
            var lstBranches = await _serOrganization.GetBranches();
            if (lstBranches != null && lstBranches.Count > 0)
            {
                objList = lstBranches.Select(x => new SelectListItem { Text = x.Text, Value = x.Value }).ToList();
                searchModel.BranchList = new SelectList(objList, "Value", "Text");
            }
            return View(searchModel);
        }
        #endregion
        #region Lead
        [HttpPost]
        public async Task<JsonResult> GetStrikeLeadReport(string DateFrom, string DateTo, string Division, string Branch)
        {

            try
            {
                ReportProxy.CrmObjectReportRequest objRequest = new ReportProxy.CrmObjectReportRequest();
                objRequest.Division = Division;
                objRequest.Branch = Branch;
                objRequest.DateFrom = SalesStaticMethods.ConvertDate(DateFrom);
                objRequest.DateTo = SalesStaticMethods.ConvertDate(DateTo);
                objRequest.Status = "Converted";

                List<ReportProxy.CrmObjectReportResponse> responce = new List<ReportProxy.CrmObjectReportResponse>();
                responce = await _ReportProxyServiceConnector.LeadReportChart(objRequest);

                if (responce != null)
                {

                    StLeadReportChartModel report = new StLeadReportChartModel();
                    if (responce.Count > 12)
                    {
                        responce = responce.OrderBy(x => x.Year).ToList();
                        report.labelData = responce.GroupBy(x => x.Year).Select(g => g.Key.ToString()).ToList();
                        report.monthData = responce.GroupBy(x => x.Year).Select(g => (double?)Math.Round(g.Sum(x => x.Total ?? 0))).ToList();
                    }
                    else
                    {
                        responce = responce.OrderBy(x => x.Month).OrderBy(x => x.Year).ToList();

                        report.monthData = responce.Select(x => (double?)Math.Round(x.Total ?? 0)).ToList();
                        report.labelData = responce.Select(x => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Month) + " - " + x.Year).ToList();
                    }
                    return Json(report);
                }
            }
            catch (Exception ex)
            {
                                
            }
           
            
            return Json(null);
        }
        #endregion
        [HttpPost]
        public async Task<IActionResult> StrikeLeadReportExcel(LeadReportSearchModel searchModel)
        {
            StLeadReportExport lead = new StLeadReportExport();
           
            List<AGS_Lead> lstLead = new List<AGS_Lead>();
            LeadProxy.CrmObjectReportRequest objRequest = new LeadProxy.CrmObjectReportRequest();
            objRequest.Division = searchModel.Division;
            objRequest.Branch = searchModel.Branch;
            objRequest.Status = "Converted";
            objRequest.DateFrom = SalesStaticMethods.ConvertDate(searchModel.DateFrom);
            objRequest.DateTo = SalesStaticMethods.ConvertDate(searchModel.DateTo);
            lstLead = await _serLead.LeadReport(objRequest);

            int headerCol = 1;
            int row = 1;

            PropertyInfo[] properties = typeof(StLeadReportExport).GetProperties();
            using (MemoryStream ms = new MemoryStream())
            {
                using (ExcelPackage package = new ExcelPackage(ms))
                {

                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");
                    //First add the headers
                    var styleHeaderCell = worksheet.Workbook.Styles.CreateNamedStyle("HeaderCell");
                    styleHeaderCell.Style.Font.Bold = true;


                    foreach (var prop in properties)
                    {
                        worksheet.Cells[row, headerCol].Value = prop.Name;
                        worksheet.Cells[row, headerCol].StyleName = styleHeaderCell.Name;
                        headerCol++;
                    }
                    row++;
                    for (int i = 0; i < lstLead.Count; i++)
                    {
                        int DataCol = 1;
                        worksheet.Cells[row, DataCol].Value = lstLead[i].DivisionName;
                        worksheet.Cells[row, DataCol + 1].Value = lstLead[i].RegionName;
                        worksheet.Cells[row, DataCol + 2].Value = lstLead[i].BranchName;
                        worksheet.Cells[row, DataCol + 3].Value = lstLead[i].Priority;
                        worksheet.Cells[row, DataCol + 4].Value = lstLead[i].PlantID + "-" + lstLead[i].PlantName;
                        worksheet.Cells[row, DataCol + 5].Value = lstLead[i].SalesOffice;
                        worksheet.Cells[row, DataCol + 6].Value = lstLead[i].AccountName;
                        worksheet.Cells[row, DataCol + 7].Value = lstLead[i].ProjectName;
                        worksheet.Cells[row, DataCol + 8].Value = lstLead[i].ContractValue;
                        worksheet.Cells[row, DataCol + 9].Value = lstLead[i].BusinessSegment;
                        worksheet.Cells[row, DataCol + 10].Value = lstLead[i].CustomerSegment;
                        worksheet.Cells[row, DataCol + 11].Value = lstLead[i].SubSegment;
                        worksheet.Cells[row, DataCol + 12].Value = lstLead[i].CustomerType;
                        worksheet.Cells[row, DataCol + 13].Value = lstLead[i].Status;
                        worksheet.Cells[row, DataCol + 14].Value = lstLead[i].ProductRequired;
                        worksheet.Cells[row, DataCol + 15].Value = lstLead[i].Currency;
                        worksheet.Cells[row, DataCol + 16].Value = lstLead[i].Classification1;
                        worksheet.Cells[row, DataCol + 17].Value = lstLead[i].Classification2;
                        worksheet.Cells[row, DataCol + 18].Value = lstLead[i].Classification3;
                        worksheet.Cells[row, DataCol + 19].Value = lstLead[i].Classification4;
                        worksheet.Cells[row, DataCol + 20].Value = lstLead[i].Probability;
                        worksheet.Cells[row, DataCol + 21].Value = lstLead[i].CustomerClassification;
                        worksheet.Cells[row, DataCol + 22].Value = lstLead[i].CRMLeadID;

                        worksheet.Cells[row, DataCol + 23].Value = lstLead[i].DocumentCreatedDate;
                        worksheet.Cells[row, DataCol + 23].Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";

                        worksheet.Cells[row, DataCol + 24].Value = lstLead[i].LeadMaturityDate;
                        worksheet.Cells[row, DataCol + 24].Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";

                        worksheet.Cells[row, DataCol + 25].Value = lstLead[i].ModifiedOn;
                        worksheet.Cells[row, DataCol + 25].Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";

                        worksheet.Cells[row, DataCol + 26].Value = lstLead[i].Username;
                        worksheet.Cells[row, DataCol + 27].Value = lstLead[i].Architect;
                        worksheet.Cells[row, DataCol + 28].Value = lstLead[i].Consultant;
                        worksheet.Cells[row, DataCol + 29].Value = lstLead[i].Description;

                        row++;
                    }

                    package.Save();
                }
                FileContentResult response = new FileContentResult(ms.ToArray(), "application/vnd.ms-excel") {FileDownloadName= "StrikeLeadReport.xlsx" };
                return response;
            }
        }    
        
    }
}
