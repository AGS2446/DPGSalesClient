using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using DPGSalesClient.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuotationProxy;
using OfficeOpenXml;
using System.IO;
using System.Reflection;
using ReportProxy;
using System.Globalization;
using Microsoft.Extensions.Options;
using System.Drawing;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace DPGSalesClient.Controllers
{
    public class QuotationForecastReportController : Controller
    {
        #region varaibles

        ServiceConnectors.ClientServiceConnector _serClient = null;
        ServiceConnectors.OrganizationServiceConnector _serOrganization = null;
        ServiceConnectors.QuotationServiceConnector _serQuote = null;
        ServiceConnectors.LeadServiceConnector _serLead = null;
        ServiceConnectors.ReportServiceConnector _serReport = null;
        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        #endregion

        #region Constructor
        public QuotationForecastReportController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor)
        {
            string strIp = SalesStaticMethods.GetRemoteIp(appSettings);

            _serClient = new ServiceConnectors.ClientServiceConnector(strIp, httpContextAccessor);
            _serOrganization = new ServiceConnectors.OrganizationServiceConnector(strIp, httpContextAccessor);
            _serQuote = new ServiceConnectors.QuotationServiceConnector(strIp, httpContextAccessor);
            _serLead = new ServiceConnectors.LeadServiceConnector(strIp, httpContextAccessor);
            _serReport = new ServiceConnectors.ReportServiceConnector(strIp, httpContextAccessor);

            _logger = loggerFactory.CreateLogger<QuotationForecastReportController>();
            _hostingEnvironment = hostingEnvironment;
        }

        #endregion
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> QuoteForecastReport()
        {
            HttpContext.Session.SetString("Controller", "QuotationForecastReport");

            QuoteForecastReportModel reportModel = new QuoteForecastReportModel();
            var objList = new List<SelectListItem>();
            var lstDropdownEntity = await _serClient.GetClients();
            if (lstDropdownEntity != null && lstDropdownEntity.Count > 0)
            {
                objList = lstDropdownEntity.Select(x => new SelectListItem { Text = x.Text, Value = x.Value }).ToList();
                reportModel.DivisionList = new SelectList(objList, "Value", "Text");
            }
            var lstBranches = await _serOrganization.GetBranches();
            if (lstBranches != null && lstBranches.Count > 0)
            {
                objList = lstBranches.Select(x => new SelectListItem { Text = x.Text, Value = x.Value }).ToList();
                reportModel.BranchList = new SelectList(objList, "Value", "Text");
            }
            return View(reportModel);
        }
        [HttpPost]

        public async Task<JsonResult> GetQuoteReportChart(string DateFrom, string DateTo, string Division, string Branch)
        {
            try
            {
                ReportProxy.CrmObjectReportRequest objRequest = new ReportProxy.CrmObjectReportRequest();
                objRequest.Division = Division;
                objRequest.Branch = Branch;
                objRequest.MaturityDateFrom = SalesStaticMethods.ConvertDate(DateFrom);
                objRequest.MaturityDateTo = SalesStaticMethods.ConvertDate(DateTo);


                List<ReportProxy.CrmObjectReportResponse> responce = new List<ReportProxy.CrmObjectReportResponse>();
                responce = await _serReport.QuotationDueDateReport(objRequest);

                if (responce != null)
                {

                    QuoteReportChartModel report = new QuoteReportChartModel();
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

        [HttpPost]
        public async Task<IActionResult> QuotationForecastReportExcel(QuoteForecastReportModel reportModel)
        {
            List<QuoteForecastReportExport> quoteForecast = new List<QuoteForecastReportExport>();
            try
            {
                QuotationProxy.CrmObjectReportRequest objRequest = new QuotationProxy.CrmObjectReportRequest();
                objRequest.Division = reportModel.Division;
                objRequest.Branch = reportModel.Branch;
                objRequest.MaturityDateFrom = SalesStaticMethods.ConvertDate(reportModel.DateFrom);
                objRequest.MaturityDateTo = SalesStaticMethods.ConvertDate(reportModel.DateTo);
                var result = await _serQuote.QuotationReport(objRequest);

                quoteForecast = result.Select(x => new QuoteForecastReportExport
                {
                    Division = x.DivisionName,
                    Region = x.RegionName,
                    Branch = x.BranchName,
                    CustomerName = x.AccountName,
                    ProjectName = x.ProjectName,
                    Tonnage = x.Tonnage,
                    CV = x.ContractValue,
                    BusinessSeg = x.BusinessSegment,
                    Customerseg = x.CustomerSegment,
                    CustomerSubseg = x.SubSegment,
                    CustomerType = x.CustomerType,
                    Status = x.Status,
                    ProductRequired = x.ProductRequired,
                    Probability = x.Probability,
                    QuoteCreatedDate = x.CreatedOn.HasValue ? x.CreatedOn.Value.ToString("dd.MM.yyyy").Replace('-', '.') : "",
                    QuoteMaturityDate = x.QuoteMaturityDate.HasValue ? x.QuoteMaturityDate.Value.ToString("dd.MM.yyyy").Replace('-', '.') : "",
                    ModifiedDate = x.ModifiedOn.HasValue ? x.ModifiedOn.Value.ToString("dd.MM.yyyy").Replace('-', '.') : "",
                    QuoteNo = x.CRMQuotationID,
                    EnquiryNumber = x.CRMOppotunityID,
                    LeadNo = x.CRMLeadID,
                    SalesEngg = x.Username,
                    Architect = x.Architect,
                    Consultant = x.Consultant,
                    SourceType = x.SourceType,
                    Remarks = x.Description
                }).ToList();

                using(var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add( "Report" );

                    // Use LINQ to project data into the worksheet
                    var tableBody = worksheet.Cells[ "A1:Y1" ].LoadFromCollection(
                        from f in quoteForecast
                        select new
                        {
                            f.Division,
                            f.Region,
                            f.Branch,                    
                            f.CustomerName,
                            f.ProjectName,
                            f.Tonnage,
                            f.CV,
                            f.BusinessSeg,
                            f.Customerseg,
                            f.CustomerSubseg,
                            f.CustomerType,
                            f.Status,
                            f.ProductRequired,
                            f.Probability,
                            f.QuoteCreatedDate,
                            f.QuoteMaturityDate,
                            f.ModifiedDate,
                            f.QuoteNo,
                            f.EnquiryNumber,
                            f.LeadNo,
                            f.SalesEngg,                          
                            f.Architect,
                            f.Consultant,
                            f.SourceType,                          
                            f.Remarks

                        }, true );

                    // Formatting
                    var header = worksheet.Cells[ "A1:Y1" ];
                    header.Style.Font.Bold = true;
                    header.Style.Font.Color.SetColor( Color.Black );
                    header.Style.WrapText = true;
                    FileContentResult response = new FileContentResult( package.GetAsByteArray(), "application/vnd.ms-excel" ) { FileDownloadName = "QuoteDueDateReport.xlsx" };
                    return response;

                }

				#region old code
				//int headerCol = 1;
    //            int row = 1;

    //            PropertyInfo[] properties = typeof(QuoteForecastReportExport).GetProperties();
    //            using (MemoryStream ms = new MemoryStream())
    //            {
    //                using (ExcelPackage package = new ExcelPackage(ms))
    //                {
    //                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");

    //                    var styleHeaderCell = worksheet.Workbook.Styles.CreateNamedStyle("HeaderCell");
    //                    styleHeaderCell.Style.Font.Bold = true;

    //                    foreach (var prop in properties)
    //                    {
    //                        worksheet.Cells[row, headerCol].Value = prop.Name;
    //                        worksheet.Cells[row, headerCol].StyleName = styleHeaderCell.Name;
    //                        headerCol++;
    //                    }
    //                    row++;
    //                    for (int i = 0; i < quoteForecast.Count; i++)
    //                    {
    //                        int itemCol = 1;
    //                        foreach (var prop in properties)
    //                        {
    //                            worksheet.Cells[row, itemCol].Value = GetPropValue(quoteForecast[i], prop.Name);
    //                            itemCol++;
    //                        }
    //                        row++;
    //                    }

    //                    package.Save();
    //                }
    //                FileContentResult response = new FileContentResult(ms.ToArray(), "application/vnd.ms-excel") { FileDownloadName = "QuoteDueDateReport.xlsx" };
    //                return response;
    //            }
				#endregion
			}
			catch (Exception ex)
            {
                                
            }
            return null;
        }


        #region Private Methods
        private static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        #endregion

    }
}
    
