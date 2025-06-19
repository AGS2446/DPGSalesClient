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
    public class StrikeQuoteReportController : Controller
    {
        ServiceConnectors.ReportServiceConnector _ReportProxyServiceConnector = null;
        ServiceConnectors.ClientServiceConnector _serClient = null;
        ServiceConnectors.OrganizationServiceConnector _serOrganization = null;
        ServiceConnectors.QuotationServiceConnector _serQuote = null;

        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;
        // GET: /<controller>/

        public StrikeQuoteReportController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor)
        {
            string strIp = SalesStaticMethods.GetRemoteIp(appSettings);

            _ReportProxyServiceConnector = new ServiceConnectors.ReportServiceConnector(strIp, httpContextAccessor);
            _serClient = new ServiceConnectors.ClientServiceConnector(strIp, httpContextAccessor);
            _serOrganization = new ServiceConnectors.OrganizationServiceConnector(strIp, httpContextAccessor);
            _serQuote = new ServiceConnectors.QuotationServiceConnector(strIp, httpContextAccessor);



            _logger = loggerFactory.CreateLogger<StrikeQuoteReportController>();
            _hostingEnvironment = hostingEnvironment;
        }




        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }


        #region QuoteReport
        public async Task<IActionResult> QuoteReport()
        {
            HttpContext.Session.SetString("Controller", "StrikeQuoteReport");

            StQuoteReportSearchModel searchModel = new StQuoteReportSearchModel();
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


        #region QuoteReport
        [HttpPost]
        public async Task<JsonResult> GetStrikeQuoteReport(string DateFrom, string DateTo, string Division, string Branch)
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
                responce = await _ReportProxyServiceConnector.QuotationReportChart(objRequest);
                if (responce != null)
                {

                    StQuoteReportChartModel report = new StQuoteReportChartModel();
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


        #region Excel Export
        [HttpPost]
        public async Task<IActionResult> StrikeQuoteReportExcel(OpportunityReportSearchModel searchModel)
        {
            StQuotationReportExport lead = new StQuotationReportExport();

          

            try
            {
                List<AGS_Quotation> lstOpportunity = new List<AGS_Quotation>();
                QuotationProxy.CrmObjectReportRequest objRequest = new QuotationProxy.CrmObjectReportRequest();
                objRequest.Division = searchModel.Division;
                objRequest.Branch = searchModel.Branch;
                objRequest.Status = "Converted";
                objRequest.DateFrom = SalesStaticMethods.ConvertDate(searchModel.DateFrom);
                objRequest.DateTo = SalesStaticMethods.ConvertDate(searchModel.DateTo);
                lstOpportunity = await _serQuote.QuotationReport(objRequest);



                int headerCol = 1;
                int row = 1;

                PropertyInfo[] properties = typeof(StQuotationReportExport).GetProperties();
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
                        for (int i = 0; i < lstOpportunity.Count; i++)
                        {
                            int DataCol = 1;
                            worksheet.Cells[row, DataCol].Value = lstOpportunity[i].DivisionName;
                            worksheet.Cells[row, DataCol + 1].Value = lstOpportunity[i].RegionName;
                            worksheet.Cells[row, DataCol + 2].Value = lstOpportunity[i].BranchName;
                            worksheet.Cells[row, DataCol + 3].Value = lstOpportunity[i].Priority;
                            worksheet.Cells[row, DataCol + 4].Value = lstOpportunity[i].PlantID + "-" + lstOpportunity[i].PlantName;
                            worksheet.Cells[row, DataCol + 5].Value = lstOpportunity[i].SalesOffice;
                            worksheet.Cells[row, DataCol + 6].Value = lstOpportunity[i].AccountName;
                            worksheet.Cells[row, DataCol + 7].Value = lstOpportunity[i].ProjectName;
                            worksheet.Cells[row, DataCol + 8].Value = lstOpportunity[i].ContractValue;
                            worksheet.Cells[row, DataCol + 9].Value = lstOpportunity[i].BusinessSegment;
                            worksheet.Cells[row, DataCol + 10].Value = lstOpportunity[i].CustomerSegment;
                            worksheet.Cells[row, DataCol + 11].Value = lstOpportunity[i].SubSegment;
                            worksheet.Cells[row, DataCol + 12].Value = lstOpportunity[i].CustomerType;
                            worksheet.Cells[row, DataCol + 13].Value = lstOpportunity[i].Status;
                            worksheet.Cells[row, DataCol + 14].Value = lstOpportunity[i].ProductRequired;
                            worksheet.Cells[row, DataCol + 15].Value = lstOpportunity[i].Currency;
                            worksheet.Cells[row, DataCol + 16].Value = lstOpportunity[i].Classification1;
                            worksheet.Cells[row, DataCol + 17].Value = lstOpportunity[i].Classification2;
                            worksheet.Cells[row, DataCol + 18].Value = lstOpportunity[i].Classification3;
                            worksheet.Cells[row, DataCol + 19].Value = lstOpportunity[i].Classification4;
                            worksheet.Cells[row, DataCol + 20].Value = lstOpportunity[i].Probability;
                            worksheet.Cells[row, DataCol + 21].Value = lstOpportunity[i].CustomerClassification;
                            worksheet.Cells[row, DataCol + 22].Value = lstOpportunity[i].CRMQuotationID;

                            worksheet.Cells[row, DataCol + 23].Value = lstOpportunity[i].DocumentCreatedDate;
                            worksheet.Cells[row, DataCol + 23].Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";

                            worksheet.Cells[row, DataCol + 24].Value = lstOpportunity[i].QuoteMaturityDate;
                            worksheet.Cells[row, DataCol + 24].Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";

                            worksheet.Cells[row, DataCol + 25].Value = lstOpportunity[i].QuoteValidityDate;
                            worksheet.Cells[row, DataCol + 25].Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";

                            worksheet.Cells[row, DataCol + 26].Value = lstOpportunity[i].ModifiedOn;
                            worksheet.Cells[row, DataCol + 26].Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";

                            worksheet.Cells[row, DataCol + 27].Value = lstOpportunity[i].Username;
                            worksheet.Cells[row, DataCol + 28].Value = lstOpportunity[i].Architect;
                            worksheet.Cells[row, DataCol + 29].Value = lstOpportunity[i].Consultant;
                            worksheet.Cells[row, DataCol + 30].Value = lstOpportunity[i].Description;

                            row++;
                        }

                        package.Save();
                    }
                    FileContentResult response = new FileContentResult(ms.ToArray(), "application/vnd.ms-excel") { FileDownloadName= "StrikeQuoteReport.xlsx" };
                    return response;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        #endregion



    }
}
