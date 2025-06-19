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
using OpportunityProxy;
using System.Globalization;
using Microsoft.Extensions.Options;
using System.Drawing;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace DPGSalesClient.Controllers
{
    public class OpportunityReportController : Controller
    {
        #region varaibles

        ServiceConnectors.ClientServiceConnector _serClient = null;
        ServiceConnectors.OrganizationServiceConnector _serOrganization = null;
        ServiceConnectors.OpportunityServiceConnector _serOpportunity = null;
        ServiceConnectors.ReportServiceConnector _serReport = null;

        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        #endregion

        #region Constructor
        public OpportunityReportController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor)
        {
            string strIp = SalesStaticMethods.GetRemoteIp(appSettings);
            _serClient = new ServiceConnectors.ClientServiceConnector(strIp, httpContextAccessor);
            _serOrganization = new ServiceConnectors.OrganizationServiceConnector(strIp, httpContextAccessor);
            _serOpportunity = new ServiceConnectors.OpportunityServiceConnector(strIp, httpContextAccessor);
            _serReport = new ServiceConnectors.ReportServiceConnector(strIp, httpContextAccessor);

            _logger = loggerFactory.CreateLogger<OpportunityReportController>();
            _hostingEnvironment = hostingEnvironment;
        }

        #endregion

        #region Report
        public async Task<IActionResult> Report()
        {
            HttpContext.Session.SetString("Controller", "OpportunityReport");

            OpportunityReportSearchModel searchModel = new OpportunityReportSearchModel();
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
        public async Task<IActionResult> OpportunityReportExcel(OpportunityReportSearchModel searchModel)
        {
            List<OpportunityReportExport> enquiryReport = new List<OpportunityReportExport>();
            try
            {
                OpportunityProxy.CrmObjectReportRequest objRequest = new OpportunityProxy.CrmObjectReportRequest();
                objRequest.Division = searchModel.Division;
                objRequest.Branch = searchModel.Branch;
                objRequest.DateFrom = SalesStaticMethods.ConvertDate( searchModel.DateFrom );
                objRequest.DateTo = SalesStaticMethods.ConvertDate( searchModel.DateTo );
                var result = await _serOpportunity.OpportunityReport( objRequest );

				#region new Code

				enquiryReport = result.Select( x => new OpportunityReportExport
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
					Probability = x.Probability,
					CRMOpportunityID = x.CRMOpportunityID,
					CreatedDate = x.CreatedOn.HasValue ? x.CreatedOn.Value.ToString( "dd.MM.yyyy" ).Replace( '-', '.' ) : "",
					EnquiryMaturityDate = x.EnquiryMaturityDate.HasValue ? x.EnquiryMaturityDate.Value.ToString( "dd.MM.yyyy" ).Replace( '-', '.' ) : "",
					EnquiryValidityDate = x.EnquiryValidityDate.HasValue ? x.EnquiryValidityDate.Value.ToString( "dd.MM.yyyy" ).Replace( '-', '.' ) : "",
					ModifiedOn = x.ModifiedOn.HasValue ? x.ModifiedOn.Value.ToString( "dd.MM.yyyy" ).Replace( '-', '.' ) : "",
					Username = x.Username,
					Architect = x.Architect,
					Consultant = x.Consultant,
					Description = x.Description,
					SAPEnquiryNo = x.SAPOpportunityID,
					Tonnage = x.Tonnage,
					CustomerClassification = x.CustomerClassification,
					Classification3 = x.Classification3,
					Classification4 = x.Classification4,
					Remarks = x.Remarks

				} ).ToList();

				using(var package = new ExcelPackage())
				{
					var worksheet = package.Workbook.Worksheets.Add( "Report" );

					// Use LINQ to project data into the worksheet
					var tableBody = worksheet.Cells[ "A1:AB1" ].LoadFromCollection(
						from f in enquiryReport
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
							f.Probability,
							f.CRMOpportunityID,
							f.CreatedDate,
							f.EnquiryMaturityDate,
							f.EnquiryValidityDate,
							f.ModifiedOn,
							f.Username,
							f.Architect,
							f.Consultant,
							f.Description,
							f.SAPEnquiryNo,
							f.Tonnage,
							f.CustomerClassification,
							f.Classification3,
							f.Classification4,
							f.Remarks

						}, true );
					// Formatting
					var header = worksheet.Cells[ "A1:AB1" ];
					header.Style.Font.Bold = true;
					header.Style.Font.Color.SetColor( Color.Black );
					//header.Style.WrapText = true;

					header[ "Q1" ].Value = "EnquiryCancelled/Defferred Date";
					FileContentResult response = new FileContentResult( package.GetAsByteArray(), "application/vnd.ms-excel" ) { FileDownloadName = "EnquiryReport.xlsx" };
					return response;

				}

				#endregion

				#region Old Code
				//int headerCol = 1;
				//int row = 1;

				//PropertyInfo[] properties = typeof( OpportunityReportExport ).GetProperties();
				//using(MemoryStream ms = new MemoryStream())
				//{
				//	using(ExcelPackage package = new ExcelPackage( ms ))
				//	{
				//		ExcelWorksheet worksheet = package.Workbook.Worksheets.Add( "Report" );
				//		//First add the headers
				//		var styleHeaderCell = worksheet.Workbook.Styles.CreateNamedStyle( "HeaderCell" );
				//		styleHeaderCell.Style.Font.Bold = true;

				//		foreach(var prop in properties)
				//		{
				//			worksheet.Cells[ row, headerCol ].Value = prop.Name;
				//			worksheet.Cells[ row, headerCol ].StyleName = styleHeaderCell.Name;
				//			headerCol++;
				//		}
				//		row++;
				//		for(int i = 0; i < result.Count; i++)
				//		{
				//			int DataCol = 1;
				//			worksheet.Cells[ row, DataCol ].Value = result[ i ].DivisionName;
				//			worksheet.Cells[ row, DataCol + 1 ].Value = result[ i ].RegionName;
				//			worksheet.Cells[ row, DataCol + 2 ].Value = result[ i ].BranchName;
				//			worksheet.Cells[ row, DataCol + 3 ].Value = result[ i ].AccountName;
				//			worksheet.Cells[ row, DataCol + 4 ].Value = result[ i ].ProjectName;
				//			worksheet.Cells[ row, DataCol + 5 ].Value = result[ i ].ContractValue;
				//			worksheet.Cells[ row, DataCol + 6 ].Value = result[ i ].BusinessSegment;
				//			worksheet.Cells[ row, DataCol + 7 ].Value = result[ i ].CustomerSegment;
				//			worksheet.Cells[ row, DataCol + 8 ].Value = result[ i ].SubSegment;
				//			worksheet.Cells[ row, DataCol + 9 ].Value = result[ i ].CustomerType;
				//			worksheet.Cells[ row, DataCol + 10 ].Value = result[ i ].CustomerSegment;
				//			worksheet.Cells[ row, DataCol + 11 ].Value = result[ i ].SubSegment;
				//			worksheet.Cells[ row, DataCol + 12 ].Value = result[ i ].CustomerType;
				//			worksheet.Cells[ row, DataCol + 13 ].Value = result[ i ].Status;
				//			worksheet.Cells[ row, DataCol + 14 ].Value = result[ i ].ProductRequired;
				//			worksheet.Cells[ row, DataCol + 15 ].Value = result[ i ].Probability;
				//			worksheet.Cells[ row, DataCol + 16 ].Value = result[ i ].CRMOpportunityID;

				//			worksheet.Cells[ row, DataCol + 17 ].Value = result[ i ].CreatedOn;
				//			worksheet.Cells[ row, DataCol + 17 ].Style.Numberformat.Format = "dd.MM.yyyy";

				//			worksheet.Cells[ row, DataCol + 18 ].Value = result[ i ].EnquiryMaturityDate;
				//			worksheet.Cells[ row, DataCol + 18 ].Style.Numberformat.Format = "dd.MM.yyyy";

				//			worksheet.Cells[ row, DataCol + 19 ].Value = result[ i ].EnquiryValidityDate;
				//			worksheet.Cells[ row, DataCol + 19 ].Style.Numberformat.Format = "dd.MM.yyyy";

				//			worksheet.Cells[ row, DataCol + 20 ].Value = result[ i ].ModifiedOn;
				//			worksheet.Cells[ row, DataCol + 20 ].Style.Numberformat.Format = "dd.MM.yyyy";

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
				//	}
				//	FileContentResult response = new FileContentResult( ms.ToArray(), "application/vnd.ms-excel" ) { FileDownloadName = "EnquiryReport.xlsx" };
				//	return response;
				//}

				#endregion
			}
			catch(Exception ex)
            {


            }

            return null;

        }

        #endregion

        #region Chart Report

        [HttpPost]
        public async Task<JsonResult> GetOpportunityReportChart(string DateFrom, string DateTo, string Division, string Branch)
        {
            try
            {
                ReportProxy.CrmObjectReportRequest objRequest = new ReportProxy.CrmObjectReportRequest();
                objRequest.Division = Division;
                objRequest.Branch = Branch;
                objRequest.DateFrom = SalesStaticMethods.ConvertDate(DateFrom);
                objRequest.DateTo = SalesStaticMethods.ConvertDate(DateTo);


                List<ReportProxy.CrmObjectReportResponse> responce = new List<ReportProxy.CrmObjectReportResponse>();
                responce = await _serReport.OpportunityReportChart(objRequest);

                if (responce != null)
                {

                    OpportunityReportChartModel report = new OpportunityReportChartModel();
                    if (responce.Count > 12)
                    {
                        responce = responce.OrderBy(x => x.Year).ToList();
                        report.monthData = responce.GroupBy(x => x.Year).Select(g => (double?)Math.Round(g.Sum(x => x.Total ?? 0))).ToList();
                        report.labelData = responce.GroupBy(x => x.Year).Select(g => g.Key.ToString()).ToList();

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
