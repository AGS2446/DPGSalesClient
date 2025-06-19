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
using System.Text.RegularExpressions;

namespace DPGSalesClient.Controllers
{
    public class SalesReportController : Controller
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

        public SalesReportController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor)
        {
            string strIp = SalesStaticMethods.GetRemoteIp(appSettings);
            _serReport = new ServiceConnectors.ReportServiceConnector(strIp, httpContextAccessor);
            _serOrg = new ServiceConnectors.OrganizationServiceConnector(strIp, httpContextAccessor);
            _serClnt = new ServiceConnectors.ClientServiceConnector(strIp, httpContextAccessor);
            _serOrd = new ServiceConnectors.OrderServiceConnector(strIp, httpContextAccessor);
            _logger = loggerFactory.CreateLogger<SalesReportController>();
            _hostingEnvironment = hostingEnvironment;
        }
        #endregion

        /******************************************************************
        ******************* Sales Report **********************************
        ******************************************************************/        
        #region Sales Report

        public async Task<IActionResult> Index()
        {
            try
            {
                HttpContext.Session.SetString("Controller", "SalesReport");
                var resultClnts = await _serClnt.GetClients();
                var lsobjListClnts = resultClnts.Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Text = x.Text, Value = x.Value }).ToList();
                ViewBag.Divisions = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(lsobjListClnts, "Value", "Text");

                var result = await _serOrg.GetBranches();
                var lsobjList = result.Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Text = x.Text, Value = x.Value }).ToList();
                ViewBag.Branches = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(lsobjList, "Value", "Text");
                ViewBag.DateFrom = "01/" + DateTime.Now.ToString("MM/yyyy");
                ViewBag.DateTo= DateTime.Now.ToString("dd/MM/yyyy");


                return View();
            }
            catch (Exception ex)
            {
                                
            }
            return View();
        }

        #endregion

        #region Child Entities

        [HttpPost]
        public async Task<JsonResult> GetSalesReport(string dateFrom, string dateTo, string division,string branch)
        {

           
                List<ReportProxy.SalesReportResponse> lsSalesReport = new List<ReportProxy.SalesReportResponse>();
                DateTime dtFromDate = new DateTime();
                DateTime dtToDate = DateTime.Now;
                if (dateFrom == null || dateTo == null)
                {
                    lsSalesReport = await _serReport.SalesReportAsync(new SalesReportRequest() { DateFrom = new DateTime(dtToDate.Year, dtToDate.Month, 1), DateTo = dtToDate });
                }
                else
                {
                    try
                    {
                        dtFromDate = DateTime.ParseExact(dateFrom.Trim(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                        dtToDate = DateTime.ParseExact(dateTo.Trim(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                        if (division != null && division.Trim().Length > 0 && branch == null)
                        {
                            lsSalesReport = await _serReport.SalesReportAsync(new SalesReportRequest() { DateFrom = dtFromDate, DateTo = dtToDate, Division = division.Replace(";", "") });
                        }
                        else if (division != null && division.Trim().Length > 0 && branch != null && branch.Trim().Length > 0)
                        {
                            lsSalesReport = await _serReport.SalesReportAsync(new SalesReportRequest() { DateFrom = dtFromDate, DateTo = dtToDate, Division = division.Replace(";",""), Branch = branch.Replace(";","") });
                        }
                        else if (branch != null && branch.Trim().Length > 0)
                        {
                            lsSalesReport = await _serReport.SalesReportAsync(new SalesReportRequest() { DateFrom = dtFromDate, DateTo = dtToDate, Branch = branch.Replace(";","") });
                        }
                        else
                        {
                            lsSalesReport = await _serReport.SalesReportAsync(new SalesReportRequest() { DateFrom = dtFromDate, DateTo = dtToDate });
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
                if (lsSalesReport != null)
                {
                    var result = lsSalesReport.Select(y => new DonutViewModel { label = y.Name, value = y.OrderValue ?? 0 }).ToList();
                    foreach (var item in result)
                    {

                        if (item.label.Contains("Voltas"))
                        {
                            item.color = "#f56954";
                            item.highlight = "#f56954";
                        }
                        else if (item.label.Contains("BLUE STAR LTD"))
                        {
                            item.color = "#00a65a";
                            item.highlight = "#00a65a";
                        }
                        else if (item.label.Contains("STERLING & WILSON PVT LTD"))
                        {
                            item.color = "#f39c12";
                            item.highlight = "#f39c12";
                        }
                        else if (item.label.Contains("ETA ENGINEERING PVT LTD"))
                        {
                            item.color = "#00c0ef";
                            item.highlight = "#00c0ef";
                        }
                        else
                        {
                            item.color = "#9a9a9a";
                            item.highlight = "#9a9a9a";
                        }
                    }

                    return Json(result);
                }
          
            return Json(null);
        }


        #region Export Method

        [HttpPost]
        public async Task<IActionResult> ExportSalesReport(string name, FormCollection formData)
        {
            OrderProxy.CrmObjectReportRequest reportRequest = new OrderProxy.CrmObjectReportRequest();
            string dateFrom = HttpContext.Request.Form["dateFrom"];
            string dateTo = HttpContext.Request.Form["dateTo"];
            string division = HttpContext.Request.Form["division"];
            string branch = HttpContext.Request.Form["branch"];
            string legendexport = HttpContext.Request.Form["legendexport"];
            DateTime dtFromDate = new DateTime();
            DateTime dtToDate = DateTime.Now;

            if (dateFrom == null || dateTo==null)
            {
                reportRequest.DateFrom = new DateTime(dtToDate.Year, dtToDate.Month, 1);
                reportRequest.DateTo = dtToDate;
            }
            else
            {
                try
                {
                    dtFromDate = DateTime.ParseExact(dateFrom, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    dtToDate = DateTime.ParseExact(dateTo, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    reportRequest.DateFrom = dtFromDate;
                    reportRequest.DateTo = dtToDate;
                    if (division != null && division.Trim().Length > 0)
                    {
                        reportRequest.Division = division;
                    }

                    if (branch != null && branch.Trim().Length > 0)
                    {
                        reportRequest.Branch = branch;
                    }
                }
                catch (Exception ex)
                {

                }
            }
            reportRequest.Competitor = legendexport;
            var lsSalesReport = await _serOrd.OrderReport(reportRequest);
            if (lsSalesReport != null)
            {
                string sTempPathFolder = Path.GetTempPath();
                string sFileName = @"SalesReport.xlsx";
                FileInfo file = new FileInfo(Path.Combine(sTempPathFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sTempPathFolder, sFileName));
                }


                using (ExcelPackage package = new ExcelPackage(file))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");
                    //First add the headers
                    var styleHeaderCell = worksheet.Workbook.Styles.CreateNamedStyle("HeaderCell");
                    styleHeaderCell.Style.Font.Bold = true;

                    int row = 1;

                    #region  Header

                    worksheet.Cells[row, 1].Value = "Order ID";
                    worksheet.Cells[row, 1].StyleName = styleHeaderCell.Name;

                    worksheet.Cells[row, 2].Value = "Account ID";
                    worksheet.Cells[row, 2].StyleName = styleHeaderCell.Name;

                    worksheet.Cells[row, 3].Value = "Account Name";
                    worksheet.Cells[row, 3].StyleName = styleHeaderCell.Name;

                    worksheet.Cells[row, 4].Value = "Business Segment";
                    worksheet.Cells[row, 4].StyleName = styleHeaderCell.Name;

                    worksheet.Cells[row, 5].Value = "Sub Segment";
                    worksheet.Cells[row, 5].StyleName = styleHeaderCell.Name;

                    worksheet.Cells[row, 6].Value = "Competitor";
                    worksheet.Cells[row, 6].StyleName = styleHeaderCell.Name;

                    worksheet.Cells[row, 7].Value = "Reason";
                    worksheet.Cells[row, 7].StyleName = styleHeaderCell.Name;

                    worksheet.Cells[row, 8].Value = "Price";
                    worksheet.Cells[row, 8].StyleName = styleHeaderCell.Name;

                    worksheet.Cells[row, 9].Value = "Order Type";
                    worksheet.Cells[row, 9].StyleName = styleHeaderCell.Name;

                    worksheet.Cells[row, 10].Value = "Contract Value";
                    worksheet.Cells[row, 10].StyleName = styleHeaderCell.Name;

                    worksheet.Cells[row, 11].Value = "User ID";
                    worksheet.Cells[row, 11].StyleName = styleHeaderCell.Name;

                    worksheet.Cells[row, 12].Value = "Username";
                    worksheet.Cells[row, 12].StyleName = styleHeaderCell.Name;

                    worksheet.Cells[row, 13].Value = "Tonnage";
                    worksheet.Cells[row, 13].StyleName = styleHeaderCell.Name;

                    worksheet.Cells[row, 14].Value = "Product Name";
                    worksheet.Cells[row, 14].StyleName = styleHeaderCell.Name;

                    worksheet.Cells[row, 15].Value = "Quantity";
                    worksheet.Cells[row, 15].StyleName = styleHeaderCell.Name;

                    worksheet.Cells[row, 16].Value = "Status";
                    worksheet.Cells[row, 16].StyleName = styleHeaderCell.Name; 

                    #endregion

                    row++;
                    for (int i = 0; i < lsSalesReport.Count; i++)
                    {
                        if (lsSalesReport[i].OrderProducts != null && lsSalesReport[i].OrderProducts.Count > 0)
                        {
                            for (int j = 0; j < lsSalesReport[i].OrderProducts.Count; j++)
                            {
                                worksheet.Cells[row, 1].Value = lsSalesReport[i].CRMOrderID;
                                worksheet.Cells[row, 2].Value = lsSalesReport[i].AccountID;
                                worksheet.Cells[row, 3].Value = lsSalesReport[i].AccountName;
                                worksheet.Cells[row, 4].Value = lsSalesReport[i].BusinessSegment;
                                worksheet.Cells[row, 5].Value = lsSalesReport[i].SubSegment;
                                worksheet.Cells[row, 6].Value = lsSalesReport[i].CompetitorName;
                                worksheet.Cells[row, 7].Value = lsSalesReport[i].Reason;
                                worksheet.Cells[row, 8].Value = lsSalesReport[i].CompetitorPrice;
                                worksheet.Cells[row, 9].Value = lsSalesReport[i].OrderType;
                                worksheet.Cells[row, 10].Value = lsSalesReport[i].ContractValue;
                                worksheet.Cells[row, 11].Value = lsSalesReport[i].UserID;
                                worksheet.Cells[row, 12].Value = lsSalesReport[i].Username;
                                worksheet.Cells[row, 13].Value = lsSalesReport[i].Tonnage;
                                worksheet.Cells[row, 14].Value = lsSalesReport[i].OrderProducts[j].ProductName;
                                worksheet.Cells[row, 15].Value = lsSalesReport[i].OrderProducts[j].Quantity;
                                worksheet.Cells[row, 16].Value = lsSalesReport[i].Wonlose;
                                row++;
                            }
                        }
                        else
                        {
                            worksheet.Cells[row, 1].Value = lsSalesReport[i].CRMOrderID;
                            worksheet.Cells[row, 2].Value = lsSalesReport[i].AccountID;
                            worksheet.Cells[row, 3].Value = lsSalesReport[i].AccountName;
                            worksheet.Cells[row, 4].Value = lsSalesReport[i].BusinessSegment;
                            worksheet.Cells[row, 5].Value = lsSalesReport[i].SubSegment;
                            worksheet.Cells[row, 6].Value = lsSalesReport[i].CompetitorName;
                            worksheet.Cells[row, 7].Value = lsSalesReport[i].Reason;
                            worksheet.Cells[row, 8].Value = lsSalesReport[i].CompetitorPrice;
                            worksheet.Cells[row, 9].Value = lsSalesReport[i].OrderType;
                            worksheet.Cells[row, 10].Value = lsSalesReport[i].ContractValue;
                            worksheet.Cells[row, 11].Value = lsSalesReport[i].UserID;
                            worksheet.Cells[row, 12].Value = lsSalesReport[i].Username;
                            worksheet.Cells[row, 13].Value = lsSalesReport[i].Tonnage;
                            row++;
                        }

                    }

                    package.Save();
                }
                FileContentResult response = new FileContentResult(System.IO.File.ReadAllBytes(Path.Combine(sTempPathFolder, sFileName)), "application/vnd.ms-excel");
                return response;
            }
            return null;
        }

        #endregion

        #endregion
    }
}
