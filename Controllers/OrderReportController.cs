using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using DPGSalesClient.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Reflection;
using OfficeOpenXml;
using OrderProxy;
using System.Globalization;
using Microsoft.Extensions.Options;
using System.Drawing;
// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace DPGSalesClient.Controllers
{
    public class OrderReportController : Controller
    {

        #region varaibles

        ServiceConnectors.ClientServiceConnector _serClient = null;
        ServiceConnectors.OrganizationServiceConnector _serOrganization = null;
        ServiceConnectors.OrderServiceConnector _serOrder = null;
        ServiceConnectors.ReportServiceConnector _serReport = null;

        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        #endregion

        #region Constructor
        public OrderReportController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor)
        {
            string strIp = SalesStaticMethods.GetRemoteIp(appSettings);

            _serClient = new ServiceConnectors.ClientServiceConnector(strIp, httpContextAccessor);
            _serOrganization = new ServiceConnectors.OrganizationServiceConnector(strIp, httpContextAccessor);
            _serOrder = new ServiceConnectors.OrderServiceConnector(strIp, httpContextAccessor);
            _serReport = new ServiceConnectors.ReportServiceConnector(strIp, httpContextAccessor);

            _logger = loggerFactory.CreateLogger<OrderReportController>();
            _hostingEnvironment = hostingEnvironment;
        }

        #endregion       

        #region Report

        public async Task<IActionResult> Report()
        {
            HttpContext.Session.SetString("Controller", "OrderReport");

            OrderReportSearchModel searchModel = new OrderReportSearchModel();
            var objList = new List<SelectListItem>();

            try
            {
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
            }
            catch (Exception ex)
            {

            }
            return View(searchModel);
        }

        #endregion

        #region Excel Export

        [HttpPost]
        public async Task<IActionResult> OrderReportExcel(OrderReportSearchModel searchModel)
        {
            List<OrderReportExport> orderReport = new List<OrderReportExport>();

            //string sTempPathFolder = Path.GetTempPath();
            //string sFileName = @"OrderReport.xlsx";
            //FileInfo file = new FileInfo(Path.Combine(sTempPathFolder, sFileName));
            //if (file.Exists)
            //{
            //    file.Delete();
            //    file = new FileInfo(Path.Combine(sTempPathFolder, sFileName));
            //}

            try
            {
                OrderProxy.WinlossReportRequest objRequest = new OrderProxy.WinlossReportRequest();
                objRequest.Division = searchModel.Division;
                objRequest.Branch = searchModel.Branch;
                objRequest.Wonlose = searchModel.WonLostStatus;

                objRequest.DateFrom = SalesStaticMethods.ConvertDate(searchModel.DateFrom);
                objRequest.DateTo = SalesStaticMethods.ConvertDate(searchModel.DateTo);
                var result = await _serOrder.OrderWinLossReport(objRequest);
                orderReport = result.Select(x => new OrderReportExport
                {
                    Division = x.DivisionName,
                    Region = x.RegionName,
                    Branch = x.BranchName,
                    CustomerName = x.AccountName,
                    ProjectName = x.ProjectName,
                    CV = x.ContractValue,
                    BusinessSeg = x.BusinessSegment,
                    CustomerSeg = x.CustomerSegment,
                    CustomerSubSeg = x.SubSegment,
                    CustomerType = x.CustomerType,
                    Status = x.Status,
                    ProductRequired = x.ProductRequired,
                    CRMOrderID = x.CRMOrderID,
                    OrderStatus = x.Wonlose,
                    DocumentCreatedDate = x.DocumentCreatedDate.HasValue ? x.DocumentCreatedDate.Value.ToString("dd.MM.yyyy").Replace('-', '.') : "",
                    CreatedOn = x.CreatedOn.HasValue ? x.CreatedOn.Value.ToString("dd.MM.yyyy").Replace('-', '.') : "",
                    ModifiedOn = x.ModifiedOn.HasValue ? x.ModifiedOn.Value.ToString("dd.MM.yyyy").Replace('-', '.') : "",
                    Username = x.Username,
                    Architect = x.Architect,
                    Consultant = x.Consultant,
                    Description = x.Description,
                    CompetitorName = x.CompetitorName,
                    Reason = x.Reason,
                    Price = x.CompetitorPrice,
                    EnquiryNo = x.CRMOPPORTUNITYID,
                    QuoteNo = x.CRMQUOTATIONID,
                    CustomerClassification = x.CustomerClassification,
                    Classification3 = x.Classification3,
                    Classification4 = x.Classification4,

                }).ToList();

                using(var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add( "Report" );

                    // Use LINQ to project data into the worksheet
                    var tableBody = worksheet.Cells[ "A1:AC1" ].LoadFromCollection(
                        from f in orderReport
                        select new
                        {
                            f.Division,
                            f.Region,
                            f.Branch,
                            f.CustomerName,
                            f.ProjectName,
                            f.CV,
                            f.BusinessSeg,
                            f.CustomerSeg,
                            f.CustomerSubSeg,
                            f.CustomerType,
                            f.Status,
                            f.ProductRequired,
                            f.CRMOrderID,
                            f.OrderStatus,
                            f.DocumentCreatedDate,
                            f.CreatedOn,
                            f.ModifiedOn,
                            f.Username,
                            f.Architect,
                            f.Consultant,                            
                            f.Description,
                            f.CompetitorName,
                            f.Reason,
                            f.Price,
                            f.EnquiryNo,
                            f.QuoteNo,
                            f.CustomerClassification,
                            f.Classification3,
                            f.Classification4

                        }, true );

                    // Formatting
                    var header = worksheet.Cells[ "A1:AC1" ];
                    header.Style.Font.Bold = true;
                    header.Style.Font.Color.SetColor( Color.Black );
                    //header.Style.WrapText = true;

                    FileContentResult response = new FileContentResult( package.GetAsByteArray(), "application/vnd.ms-excel" ) { FileDownloadName = "OrderReport.xlsx" };
                    return response;

                }

                #region Old Code
                //int headerCol = 1;
                //            int row = 1;

                //            PropertyInfo[] properties = typeof(OrderReportExport).GetProperties();
                //            using (MemoryStream ms = new MemoryStream())
                //            {
                //                using (ExcelPackage package = new ExcelPackage(ms))
                //                {

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
                //                    for(int i = 0; i < orderReport.Count; i++)
                //                    {
                //                        int DataCol = 1;

                //                        //worksheet.Cells[ row, DataCol ].Value = orderReport[ i ].Division;
                //                        //worksheet.Cells[ row, DataCol + 1 ].Value = orderReport[ i ].Region;
                //                        //worksheet.Cells[ row, DataCol + 2 ].Value = orderReport[ i ].Branch;
                //                        //worksheet.Cells[ row, DataCol + 3 ].Value = orderReport[ i ].CustomerName;
                //                        //worksheet.Cells[ row, DataCol + 4 ].Value = orderReport[ i ].ProjectName;
                //                        //worksheet.Cells[ row, DataCol + 5 ].Value = orderReport[ i ].CV;
                //                        //worksheet.Cells[ row, DataCol + 6 ].Value = orderReport[ i ].BusinessSegment;
                //                        //worksheet.Cells[ row, DataCol + 7 ].Value = orderReport[ i ].CustomerSegment;
                //                        //worksheet.Cells[ row, DataCol + 8 ].Value = orderReport[ i ].SubSegment;
                //                        //worksheet.Cells[ row, DataCol + 9 ].Value = orderReport[ i ].CustomerType;
                //                        //worksheet.Cells[ row, DataCol + 10 ].Value = orderReport[ i ].CustomerSegment;
                //                        //worksheet.Cells[ row, DataCol + 11 ].Value = orderReport[ i ].SubSegment;
                //                        //worksheet.Cells[ row, DataCol + 12 ].Value = orderReport[ i ].CustomerType;
                //                        //worksheet.Cells[ row, DataCol + 13 ].Value = orderReport[ i ].Status;
                //                        //worksheet.Cells[ row, DataCol + 14 ].Value = orderReport[ i ].ProductRequired;
                //                        //worksheet.Cells[ row, DataCol + 15 ].Value = orderReport[ i ].Probability;
                //                        //worksheet.Cells[ row, DataCol + 16 ].Value = orderReport[ i ].CRMOpportunityID;

                //                        //worksheet.Cells[ row, DataCol + 17 ].Value = orderReport[ i ].CreatedOn;
                //                        //worksheet.Cells[ row, DataCol + 17 ].Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";

                //                        //worksheet.Cells[ row, DataCol + 18 ].Value = orderReport[ i ].EnquiryMaturityDate;
                //                        //worksheet.Cells[ row, DataCol + 18 ].Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";

                //                        //worksheet.Cells[ row, DataCol + 19 ].Value = orderReport[ i ].EnquiryValidityDate;
                //                        //worksheet.Cells[ row, DataCol + 19 ].Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";

                //                        //worksheet.Cells[ row, DataCol + 20 ].Value = orderReport[ i ].ModifiedOn;
                //                        //worksheet.Cells[ row, DataCol + 20 ].Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";

                //                        //worksheet.Cells[ row, DataCol + 21 ].Value = orderReport[ i ].Username;
                //                        //worksheet.Cells[ row, DataCol + 22 ].Value = orderReport[ i ].Architect;

                //                        //worksheet.Cells[ row, DataCol + 23 ].Value = orderReport[ i ].Consultant;


                //                        row++;
                //                    }

                //                    package.Save();
                //                }
                //                FileContentResult response = new FileContentResult(ms.ToArray(), "application/vnd.ms-excel") { FileDownloadName = "OrderReport.xlsx" };
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

        #region Chart Report
        [HttpPost]
        public async Task<JsonResult> GetOrderReportChart(string DateFrom, string DateTo, string Division, string Branch, string WonLostStatus)
        {
            try
            {
                ReportProxy.WinlossReportRequest objRequest = new ReportProxy.WinlossReportRequest();
                //  ReportProxy.CrmObjectReportRequest objRequest = new ReportProxy.CrmObjectReportRequest();
                objRequest.Division = Division;
                objRequest.Branch = Branch;
                objRequest.Wonlose = WonLostStatus;
                objRequest.DateFrom = SalesStaticMethods.ConvertDate(DateFrom);
                objRequest.DateTo = SalesStaticMethods.ConvertDate(DateTo);


                List<ReportProxy.WinlossReportResponse> responce = new List<ReportProxy.WinlossReportResponse>();
                responce = await _serReport.OrderWinLostReportChart(objRequest);

                if (responce != null)
                {

                    OrderReportChartModel report = new OrderReportChartModel();
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

        #region Private Methods

        private static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        #endregion

    }
}
