using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using DPGSalesClient.Models;
using System.Reflection;
using OfficeOpenXml;
using QuotationProxy;
using System.IO;
using System.Globalization;
using Microsoft.Extensions.Options;
using System.Drawing;
// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace DPGSalesClient.Controllers
{
    public class QuotationReportController : Controller
    {
        #region varaibles

        ServiceConnectors.ClientServiceConnector _serClient = null;
        ServiceConnectors.OrganizationServiceConnector _serOrganization = null;
        ServiceConnectors.QuotationServiceConnector _serQuotation = null;
        ServiceConnectors.ReportServiceConnector _serReport = null;

        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        #endregion

        #region Constructor
        public QuotationReportController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor)
        {
            string strIp = SalesStaticMethods.GetRemoteIp(appSettings);

            _serClient = new ServiceConnectors.ClientServiceConnector(strIp, httpContextAccessor);
            _serOrganization = new ServiceConnectors.OrganizationServiceConnector(strIp, httpContextAccessor);
            _serQuotation = new ServiceConnectors.QuotationServiceConnector(strIp, httpContextAccessor);
            _serReport = new ServiceConnectors.ReportServiceConnector(strIp, httpContextAccessor);

            _logger = loggerFactory.CreateLogger<QuotationReportController>();
            _hostingEnvironment = hostingEnvironment;
        }
        #endregion
       
        #region Report

        public async Task<IActionResult> Report()
        {
            HttpContext.Session.SetString("Controller", "QuotationReport");

            QuotationReportSearchModel searchModel = new QuotationReportSearchModel();
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
        public async Task<IActionResult> QuotationReportExcel(OpportunityReportSearchModel searchModel)
        {
            
            List<QuotationReportExport> lsquotation = new List<QuotationReportExport>();
            try
            {
                QuotationProxy.CrmObjectReportRequest objRequest = new QuotationProxy.CrmObjectReportRequest();
                objRequest.Division = searchModel.Division;
                objRequest.Branch = searchModel.Branch;
                objRequest.DateFrom = SalesStaticMethods.ConvertDate( searchModel.DateFrom );
                objRequest.DateTo = SalesStaticMethods.ConvertDate( searchModel.DateTo );
                var result = await _serQuotation.QuotationReport( objRequest );


                lsquotation = result.Select( x => new QuotationReportExport
                {
                    Division = x.DivisionName,
                    Region = x.RegionName,
                    Branch = x.BranchName,
                    Priority = x.Priority,
                    Plant = x.PlantID + "-" + x.PlantName,
                    SalesOffice = x.SalesOffice,
                    CustomerName = x.AccountName,
                    ProjectName = x.ProjectName,
                    CV = x.ContractValue,
                    BusinessSeg = x.BusinessSegment,
                    CustomerSeg = x.CustomerSegment,
                    CustomerSubSeg = x.SubSegment,
                    CustomerType = x.CustomerType,
                    Status = x.Status,
                    ProductRequired = x.ProductRequired,
                    Currency = x.Currency,
                    Classification1 = x.Classification1,
                    Classification2 = x.Classification2,
                    Classification3 = x.Classification3,
                    Classification4 = x.Classification4,
                    Probability = x.Probability,
                    CustomerClassification = x.CustomerClassification,
                    CRMQuotationID = x.CRMQuotationID,
                    DocumentCreatedDate = x.CreatedOn.HasValue ? x.CreatedOn.Value.ToString( "dd.MM.yyyy" ).Replace( '-', '.' ) : "",
                    QuoteMaturityDate = x.QuoteMaturityDate.HasValue ? x.QuoteMaturityDate.Value.ToString( "dd.MM.yyyy" ).Replace( '-', '.' ) : "",
                    QuoteValidityDate = x.QuoteValidityDate.HasValue ? x.QuoteValidityDate.Value.ToString( "dd.MM.yyyy" ).Replace( '-', '.' ) : "",
                    OfferDate = (x.Status.ToUpper() == "CANCELLED" || x.Status.ToUpper() == "DEFERRED" || x.Status.ToUpper()== "BUDGETARY")?( x.OfferDate.HasValue ? x.OfferDate.Value.ToString( "dd.MM.yyyy" ).Replace( '-', '.' ) : ""):"",
                    ModifiedOn = x.ModifiedOn.HasValue ? x.ModifiedOn.Value.ToString("dd.MM.yyyy").Replace('-', '.') : "",
                    Username = x.Username,
                    Architect = x.Architect,
                    Consultant = x.Consultant,
                    Description = x.Description,
                    ApprovedBy = x.Channel,
                    Tonnage=x.Tonnage,
                    EnquiryNo=x.CRMOppotunityID,
                    Remarks=x.Remarks
                }).ToList();

                using(var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add( "Report" );

                    // Use LINQ to project data into the worksheet
                    var tableBody = worksheet.Cells[ "A1:AJ1" ].LoadFromCollection(
                        from f in lsquotation
                        select new
                        {
                            f.Division,
                            f.Region,
                            f.Branch,
                            f.Priority,
                            f.Plant,
                            f.SalesOffice,
                            f.CustomerName,
                            f.ProjectName,
                            f.CV,
                            f.BusinessSeg,
                            f.CustomerSeg,
                            f.CustomerSubSeg,
                            f.CustomerType,
                            f.Status,
                            f.ProductRequired,
                            f.Currency,
                            f.Classification1,
                            f.Classification2,
                            f.Classification3,
                            f.Classification4,
                            f.Probability,
                            f.CustomerClassification,
                            f.CRMQuotationID,
                            f.DocumentCreatedDate,
                            f.QuoteMaturityDate,
                            f.QuoteValidityDate,
                            f.OfferDate,
                            f.ModifiedOn,
                            f.Username,
                            f.Architect,
                            f.Consultant,
                            f.Description,
                            f.ApprovedBy,
                            f.Tonnage,
                            f.EnquiryNo,
                            f.Remarks

                        }, true );

                    // Formatting
                    var header = worksheet.Cells[ "A1:AJ1" ];
                    header.Style.Font.Bold = true;
                    header.Style.Font.Color.SetColor( Color.Black );
                    //header.Style.WrapText = true;

                    header[ "AA1" ].Value = "QuoteCancelled/Budgetary/Defferred Date";

                    FileContentResult response = new FileContentResult( package.GetAsByteArray(), "application/vnd.ms-excel" ) { FileDownloadName = "QuotationReport.xlsx" };
                    return response;

                }

                #region Old Code

                //int headerCol = 1;
                //            int row = 1;

                //            PropertyInfo[] properties = typeof(QuotationReportExport).GetProperties();
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
                //		for(int i = 0; i < lsquotation.Count; i++)
                //		{
                //			int DataCol = 1;
                //			worksheet.Cells[ row, DataCol ].Value = result[ i ].DivisionName;
                //			worksheet.Cells[ row, DataCol + 1 ].Value = result[ i ].RegionName;
                //			worksheet.Cells[ row, DataCol + 2 ].Value = result[ i ].BranchName;
                //			worksheet.Cells[ row, DataCol + 3 ].Value = result[ i ].Priority;
                //			worksheet.Cells[ row, DataCol + 4 ].Value = result[ i ].PlantID + "-" + result[ i ].PlantName;
                //			worksheet.Cells[ row, DataCol + 5 ].Value = result[ i ].SalesOffice;
                //			worksheet.Cells[ row, DataCol + 6 ].Value = result[ i ].AccountName;
                //			worksheet.Cells[ row, DataCol + 7 ].Value = result[ i ].ProjectName;
                //			worksheet.Cells[ row, DataCol + 8 ].Value = result[ i ].ContractValue;
                //			worksheet.Cells[ row, DataCol + 9 ].Value = result[ i ].BusinessSegment;
                //			worksheet.Cells[ row, DataCol + 10 ].Value = result[ i ].CustomerSegment;
                //			worksheet.Cells[ row, DataCol + 11 ].Value = result[ i ].SubSegment;
                //			worksheet.Cells[ row, DataCol + 12 ].Value = result[ i ].CustomerType;
                //			worksheet.Cells[ row, DataCol + 13 ].Value = result[ i ].Status;
                //			worksheet.Cells[ row, DataCol + 14 ].Value = result[ i ].ProductRequired;

                //			worksheet.Cells[ row, DataCol + 16 ].Value = result[ i ].Currency;
                //			worksheet.Cells[ row, DataCol + 16 ].Value = result[ i ].Classification1;
                //			worksheet.Cells[ row, DataCol + 16 ].Value = result[ i ].Classification2;
                //			worksheet.Cells[ row, DataCol + 16 ].Value = result[ i ].Classification3;
                //			worksheet.Cells[ row, DataCol + 16 ].Value = result[ i ].Classification4;

                //			worksheet.Cells[ row, DataCol + 16 ].Value = result[ i ].Probability;

                //			worksheet.Cells[ row, DataCol + 16 ].Value = result[ i ].CustomerClassification;

                //			worksheet.Cells[ row, DataCol + 16 ].Value = result[ i ].CRMQuotationID;
                //			worksheet.Cells[ row, DataCol + 16 ].Value = result[ i ].Probability;
                //			worksheet.Cells[ row, DataCol + 16 ].Value = result[ i ].Probability;
                //			worksheet.Cells[ row, DataCol + 16 ].Value = result[ i ].Probability;
                //			worksheet.Cells[ row, DataCol + 16 ].Value = result[ i ].Probability;

                //			worksheet.Cells[ row, DataCol + 17 ].Value = result[ i ].CreatedOn;
                //			worksheet.Cells[ row, DataCol + 17 ].Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";

                //			worksheet.Cells[ row, DataCol + 18 ].Value = result[ i ].EnquiryMaturityDate;
                //			worksheet.Cells[ row, DataCol + 18 ].Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";

                //			worksheet.Cells[ row, DataCol + 19 ].Value = result[ i ].EnquiryValidityDate;
                //			worksheet.Cells[ row, DataCol + 19 ].Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";

                //			worksheet.Cells[ row, DataCol + 20 ].Value = result[ i ].ModifiedOn;
                //			worksheet.Cells[ row, DataCol + 20 ].Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";

                //			worksheet.Cells[ row, DataCol + 21 ].Value = result[ i ].Username;
                //			worksheet.Cells[ row, DataCol + 22 ].Value = result[ i ].Architect;

                //			worksheet.Cells[ row, DataCol + 23 ].Value = result[ i ].Consultant;


                //			worksheet.Cells[ row, DataCol + 24 ].Value = result[ i ].Description;


                //			worksheet.Cells[ row, DataCol + 25 ].Value = result[ i ].SAPOpportunityID;


                //			worksheet.Cells[ row, DataCol + 26 ].Value = result[ i ].Tonnage;
                //			worksheet.Cells[ row, DataCol + 27 ].Value = result[ i ].CustomerClassification;
                //			worksheet.Cells[ row, DataCol + 28 ].Value = result[ i ].Classification3;
                //			worksheet.Cells[ row, DataCol + 29 ].Value = result[ i ].Classification4;
                //			worksheet.Cells[ row, DataCol + 30 ].Value = result[ i ].Remarks;
                //			row++;
                //		}

                //		package.Save();
                //                }
                //                FileContentResult response = new FileContentResult(ms.ToArray(), "application/vnd.ms-excel") { FileDownloadName= "QuotationReport.xlsx" };
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
        public async Task<JsonResult> GetQuotationReportChart(string DateFrom, string DateTo, string Division, string Branch)
        {
            try
            {
                ReportProxy.CrmObjectReportRequest objRequest = new ReportProxy.CrmObjectReportRequest();
                objRequest.Division = Division;
                objRequest.Branch = Branch;
                objRequest.DateFrom = SalesStaticMethods.ConvertDate(DateFrom);
                objRequest.DateTo = SalesStaticMethods.ConvertDate(DateTo);

                List<ReportProxy.CrmObjectReportResponse> responce = new List<ReportProxy.CrmObjectReportResponse>();
                responce = await _serReport.QuotationReportChart(objRequest);

                if (responce != null)
                {
                    
                    QuotationReportChartModel report = new QuotationReportChartModel();
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
