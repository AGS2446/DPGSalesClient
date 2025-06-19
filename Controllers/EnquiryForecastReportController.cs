using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using static DPGSalesClient.Models.EnquiryForeCastReportModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using DPGSalesClient.Models;
using System.IO;
using OfficeOpenXml;
using System.Globalization;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Drawing;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace DPGSalesClient.Controllers
{
    public class EnquiryForecastReportController : Controller
    {
        #region Varaibles

        ServiceConnectors.OpportunityServiceConnector _enqforecastReport = null;
        ServiceConnectors.ClientServiceConnector _enqforecastReportDivisions = null;
        ServiceConnectors.OrganizationServiceConnector _enqforecastReportBranches = null;
        ServiceConnectors.ReportServiceConnector _enqforecastReportResponce = null;

        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        #endregion

        #region Constructure
        public EnquiryForecastReportController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor)
        {
            string strIp = SalesStaticMethods.GetRemoteIp(appSettings);

            _enqforecastReport = new ServiceConnectors.OpportunityServiceConnector(strIp, httpContextAccessor);
            _enqforecastReportDivisions = new ServiceConnectors.ClientServiceConnector(strIp, httpContextAccessor);
            _enqforecastReportBranches = new ServiceConnectors.OrganizationServiceConnector(strIp, httpContextAccessor);
            _enqforecastReportResponce = new ServiceConnectors.ReportServiceConnector(strIp, httpContextAccessor);

            _logger = loggerFactory.CreateLogger<EnquiryForecastReportController>();
            _hostingEnvironment = hostingEnvironment;
        }
        #endregion

        #region ReportGet
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            HttpContext.Session.SetString("Controller", "EnquiryForecastReport");
            ReportViewModel model = new ReportViewModel();
            var result_Divisions = await _enqforecastReportDivisions.GetClients();

            var divisions = result_Divisions.Select(x => new SelectListItem { Text = x.Text, Value = x.Value }).Distinct().ToList();

            if (divisions.Count > 0)
            {
                divisions.Insert(0, new SelectListItem { Text = "SELECT", Value = "" });
            }
            model.DivisionList = divisions;
            var result_branches = await _enqforecastReportBranches.GetBranches();
            var branches = result_branches.Select(x => new SelectListItem { Text = x.Text, Value = x.Value }).Distinct().ToList();
            if (branches.Count > 0)
            {
                branches.Insert(0, new SelectListItem { Text = "SELECT", Value = "" });
            }
            model.BranchList = branches;

            return View(model);

        }
        #endregion

        #region Line Chart report
        [HttpPost]
        public async Task<JsonResult> GetEnquiryForecastReport(string DateFrom,string DateTo, string Division, string Branch)
        {
            try
            {
                ReportProxy.CrmObjectReportRequest objRequest = new ReportProxy.CrmObjectReportRequest();
                objRequest.Division = Division;
                objRequest.Branch = Branch;
                objRequest.MaturityDateFrom = SalesStaticMethods.ConvertDate(DateFrom);
                objRequest.MaturityDateTo = SalesStaticMethods.ConvertDate(DateTo);
                List<ReportProxy.CrmObjectReportResponse> responce = new List<ReportProxy.CrmObjectReportResponse>();
                responce = await _enqforecastReportResponce.OpportunityDueDateReport(objRequest);

                if (responce != null)
                {
                    LineChartViewModel report = new LineChartViewModel();
                    if (responce.Count > 12)
                    {
                        responce = responce.OrderBy(x => x.Year).ToList();
                        report.Month = responce.GroupBy(x => x.Year).Select(y => (double?)Math.Round(y.Sum(x => x.Total ?? 0))).ToList();
                        report.label = responce.GroupBy(x => x.Year).Select(y => y.Key.ToString()).ToList();

                    }
                    else
                    {
                        responce = responce.OrderBy(x => x.Month).OrderBy(x => x.Year).ToList();
                        report.Month = responce.Select(x => (double?)Math.Round(x.Total ?? 0)).ToList();
                        report.label = responce.Select(x => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Month) + " - " + x.Year).ToList();
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

        #region Export
        [HttpPost]
        public async Task<IActionResult> EnquiryForecastReportExcel(EnquiryForeCastReportModel.ReportViewModel model)
        {
            List<EnquiryForecastExport> forecastEnquiry = new List<EnquiryForecastExport>();

            HttpContext.Session.SetString("Controller", "EnquiryForecastReport");
            try
            {

                OpportunityProxy.CrmObjectReportRequest objInput = new OpportunityProxy.CrmObjectReportRequest();
                objInput.Branch = model.Branch;
                objInput.Division = model.Division;
                objInput.MaturityDateFrom = SalesStaticMethods.ConvertDate(model.DateFrom);
                objInput.MaturityDateTo = SalesStaticMethods.ConvertDate(model.DateTo);
                var result = await _enqforecastReport.OpportunityReport(objInput);

                forecastEnquiry = result.Select(x => new EnquiryForecastExport
                {
                    Division = x.DivisionName,
                    Region = x.RegionName,
                    Branch = x.BranchName,
                    CustomerName = x.AccountName,
                    ProjectName = x.ProjectName,
                    Tonnage = x.Tonnage,
                    CV = x.ContractValue,
                    ProductRequired = x.ProductRequired,
                    CustomerType = x.CustomerType,
                    Status = x.Status,
                    Probability = x.Probability,
                    Consultant = x.Consultant,
                    Architect = x.Architect,
                    EnquiryCreatedDate = x.CreatedOn.HasValue ? x.CreatedOn.Value.ToString("dd.MM.yyyy").Replace('-', '.') : "",
                    ModifiedDate = x.ModifiedOn.HasValue ? x.ModifiedOn.Value.ToString("dd.MM.yyyy").Replace('-', '.') : "",
                    EnquiryMaturityDate = x.EnquiryMaturityDate.HasValue ? x.EnquiryMaturityDate.Value.ToString("dd.MM.yyyy").Replace('-', '.') : "",
                    SalesEngg = x.Username
                }).ToList();

                using(var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add( "Report" );

                    // Use LINQ to project data into the worksheet
                    var tableBody = worksheet.Cells[ "A1:Q1" ].LoadFromCollection(
                        from f in forecastEnquiry
                        select new
                        {
                            f.Division,
                            f.Region,
                            f.Branch,
                            f.CustomerName,
                            f.ProjectName,
                            f.Tonnage,
                            f.CV,
                            f.ProductRequired,
                            f.CustomerType,
                            f.Status,
                            f.Probability,
                            f.Consultant,
                            f.Architect,
                            f.EnquiryCreatedDate,
                            f.ModifiedDate,
                            f.EnquiryMaturityDate,
                            f.SalesEngg                            

                        }, true );
                    // Formatting
                    var header = worksheet.Cells[ "A1:Q1" ];
                    header.Style.Font.Bold = true;
                    header.Style.Font.Color.SetColor( Color.Black );
                    header.Style.WrapText = true;

                    FileContentResult response = new FileContentResult( package.GetAsByteArray(), "application/vnd.ms-excel" ) { FileDownloadName = "EnquiryDueDateReport.xlsx" };
                    return response;

                }
                #region Old Code

                //int headerCol = 1;
                //            int row = 1;


                //            PropertyInfo[] properties = typeof(EnquiryForecastExport).GetProperties();
                //            using (MemoryStream ms = new MemoryStream())
                //            {
                //                using (ExcelPackage package = new ExcelPackage(ms))
                //                {
                //                    // add a new worksheet to the empty workbook
                //                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");

                //                    //First add the headers
                //                    var styleHeaderCell = worksheet.Workbook.Styles.CreateNamedStyle("HeaderCell");
                //                    styleHeaderCell.Style.Font.Bold = true;

                //                    foreach (var prop in properties)
                //                    {
                //                        worksheet.Cells[row, headerCol].Value = prop.Name;
                //                        worksheet.Cells[row, headerCol].StyleName = styleHeaderCell.Name;
                //                        headerCol++;
                //                    }


                //                    row++;
                //                    for (int i = 0; i < forecastEnquiry.Count; i++)
                //                    {
                //                        int itemCol = 1;
                //                        foreach (var prop in properties)
                //                        {
                //                            worksheet.Cells[row, itemCol].Value = GetPropValue(forecastEnquiry[i], prop.Name);
                //                            itemCol++;
                //                        }

                //                        row++;
                //                    }

                //                    package.Save(); //Save the workbook.
                //                }
                //                FileContentResult response = new FileContentResult(ms.ToArray(), "application/vnd.ms-excel") { FileDownloadName = "EnquiryDueDateReport.xlsx" };
                //                return response;
                //            }

                #endregion
            }
			catch (Exception ex)
            {

            }
            return null;
        }

        #endregion

        #region Private Methods

        private static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        #endregion
    }
}
