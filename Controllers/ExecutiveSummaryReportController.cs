using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using DPGSalesClient.Models;
using Microsoft.Extensions.Options;
using System.IO;
using OfficeOpenXml;

namespace DPGSalesClient.Controllers
{
    public class ExecutiveSummaryReportController : Controller
    {
        #region Varaibles

        ServiceConnectors.OpportunityServiceConnector _serOpportunity = null;
        ServiceConnectors.ReportServiceConnector _serReport = null;
        ServiceConnectors.QuotationServiceConnector _serQuote = null;
        ServiceConnectors.ClientServiceConnector _serClient = null;
        ServiceConnectors.OrganizationServiceConnector _serOrganization = null;
        ServiceConnectors.RoleServiceConnector _serRole = null;

        Models.BusinessLogic.ActivityBL _serActivity = null;
        Models.BusinessLogic.AttachmentBL _serAttachment = null;
        Models.BusinessLogic.CustomerBL _serCustomer = null;

        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        #endregion

        #region Constructor
        public ExecutiveSummaryReportController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor)
        {
            string strIp = SalesStaticMethods.GetRemoteIp(appSettings);

            _serOpportunity = new ServiceConnectors.OpportunityServiceConnector(strIp, httpContextAccessor);
            _serReport = new ServiceConnectors.ReportServiceConnector(strIp, httpContextAccessor);
            _serQuote = new ServiceConnectors.QuotationServiceConnector(strIp, httpContextAccessor);
            _serClient = new ServiceConnectors.ClientServiceConnector(strIp, httpContextAccessor);
            _serOrganization = new ServiceConnectors.OrganizationServiceConnector(strIp, httpContextAccessor);
            _serRole = new ServiceConnectors.RoleServiceConnector(strIp, httpContextAccessor);

            _serActivity = new Models.BusinessLogic.ActivityBL(strIp, httpContextAccessor);
            _serAttachment = new Models.BusinessLogic.AttachmentBL(strIp, httpContextAccessor);
            _serCustomer = new Models.BusinessLogic.CustomerBL(strIp, httpContextAccessor);

            _logger = loggerFactory.CreateLogger<EnquiryApprovalReportController>();
            _hostingEnvironment = hostingEnvironment;
        }

        #endregion

        /******************************************************************
         * ****************** ENQUIRY REPORT **********************************
         ******************************************************************/
        public async Task<IActionResult> Index()
        {
            bool postBack = false;
            var prevController = HttpContext.Session.GetString("Controller");
            HttpContext.Session.SetString("Controller", "ExecutiveSummaryReport");

            if (prevController != null && prevController == "ExecutiveSummaryReport")
            {
                postBack = true;
            }

           

            EnquiryApprovalIndexModel viewModel = new EnquiryApprovalIndexModel();
            try
            {
                if (HttpContext.Session.CheckSession("NavigationList"))
                {
                    viewModel.DivisionList = await _serClient.GetClients();
                    viewModel.BranchList = await _serOrganization.GetBranches();
                    if (viewModel.DivisionList != null && viewModel.DivisionList.Count > 0)
                        viewModel.DivisionList.Insert(0, new ClientProxy.AGS_DropdownEntity { Text = "SELECT", Value = "" });
                    if (viewModel.BranchList != null && viewModel.BranchList.Count > 0)
                        viewModel.BranchList.Insert(0, new OrganizationProxy.AGS_DropdownEntity { Text = "SELECT", Value = "" });

                    
                    var sessionViewModel = HttpContext.Session.GetObjectFromJson<EnquiryApprovalIndexModel>("ESR");
                    if (postBack && sessionViewModel != null)
                    { 
                        viewModel.Branch = sessionViewModel.Branch;
                        viewModel.Division = sessionViewModel.Division;
                        viewModel.DateFrom = sessionViewModel.DateFrom;
                        viewModel.DateTo = sessionViewModel.DateTo;
                    }
                    else
                    { 
                        string strDateFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("dd/MM/yyyy").Replace("-", "/");
                        viewModel.DateFrom = strDateFrom;
                        viewModel.DateTo = DateTime.Now.ToString("dd/MM/yyyy").Replace("-", "/");
                    }
                    
                    var requestModel = new ReportProxy.CrmObjectReportRequest
                    {
                        DateFrom = SalesStaticMethods.ConvertDate(viewModel.DateFrom),
                        DateTo = SalesStaticMethods.ConvertDate(viewModel.DateTo),
                        Branch = viewModel.Branch,
                        Division = viewModel.Division
                    };
                    

                    var summaryReportResponses = await _serReport.OpportunitySummaryReport(requestModel);
                    
                    if (summaryReportResponses != null)
                    {
                        
                        viewModel.EnquiryList = summaryReportResponses.Select(y => new EnquiryViewReportModel
                        {
                            ID = y.Id,
                            EnquiryID = y.EnquiryID,
                            OrderID = y.OrderID,
                            QuoteID = y.QuoteID,
                            Division = y.Division,
                            Branch = y.Branch,
                            CustomerName = y.CustomerName,
                            Segment = y.CustomerSegment,
                            SubSegment = y.SubSegment,
                            TotalValue = y.TotalValue,
                            Status = y.Status,
                            LostPrice = y.LostPrice,
                            Remarks = y.Remarks,
                            ExpectedFinalQtr = GetQuarter(y.MaturityDate),
                            ExpectedFinalYear = GetFinYear(y.MaturityDate),
                            CostSheet = y.AttachmentCount,
                            Contactplan = y.ActivitiyCount

                        }).ToList();

                       
                    }

                }
                else
                {
                    return RedirectToAction("Login", "Auth");
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }
            HttpContext.Session.SetObjectAsJson("ESR", viewModel);
            return View(viewModel);

        }

        public async Task<IActionResult> Search(EnquiryApprovalIndexModel viewModel)
        {
            try
            {
                var sessionViewModel = HttpContext.Session.GetObjectFromJson<EnquiryApprovalIndexModel>("ESR");
                if (sessionViewModel != null)
                {
                    viewModel.BranchList = sessionViewModel.BranchList;
                    viewModel.DivisionList = sessionViewModel.DivisionList;
                }
                else
                {
                    viewModel.DivisionList = await _serClient.GetClients();
                    viewModel.BranchList = await _serOrganization.GetBranches();
                    if (viewModel.DivisionList != null && viewModel.DivisionList.Count > 0)
                        viewModel.DivisionList.Insert(0, new ClientProxy.AGS_DropdownEntity { Text = "SELECT", Value = "" });
                    if (viewModel.BranchList != null && viewModel.BranchList.Count > 0)
                        viewModel.BranchList.Insert(0, new OrganizationProxy.AGS_DropdownEntity { Text = "SELECT", Value = "" });
                }

                var requestModel = new ReportProxy.CrmObjectReportRequest();
                requestModel.Branch = viewModel.Branch;
                requestModel.Division = viewModel.Division;

                requestModel.DateFrom = SalesStaticMethods.ConvertDate(viewModel.DateFrom);
                requestModel.DateTo = SalesStaticMethods.ConvertDate(viewModel.DateTo);


               

                var lsPendingEnquiry = await _serReport.OpportunitySummaryReport(requestModel);

                if (lsPendingEnquiry != null)
                {
                    viewModel.EnquiryList = lsPendingEnquiry.Select(y => new EnquiryViewReportModel
                    {
                        ID = y.Id,
                        EnquiryID = y.EnquiryID,
                        OrderID = y.OrderID,
                        QuoteID = y.QuoteID,
                        Division = y.Division,
                        Branch = y.Branch,
                        CustomerName = y.CustomerName,
                        Segment = y.CustomerSegment,
                        SubSegment = y.SubSegment,
                        TotalValue = y.TotalValue,
                        Status = y.Status,
                        LostPrice = y.LostPrice,
                        Remarks = y.Remarks,
                        ExpectedFinalQtr = GetQuarter(y.MaturityDate),
                        ExpectedFinalYear = GetFinYear(y.MaturityDate),
                        CostSheet = y.AttachmentCount,
                        Contactplan = y.ActivitiyCount

                    }).ToList();
                }
            }
            catch (Exception ex)
            {

            }
            HttpContext.Session.SetObjectAsJson("ESR", viewModel);
            return View("Index", viewModel);
        }
        public async Task<IActionResult> ActivityIndex(string enqId, string qteId, string ordId)
        {
            var objVM = new ContactPlanViewModel();
            try
            {
                var enqAct = await _serActivity.GetAllActivitiesByDocumentID(enqId);
                if (enqAct != null)
                {
                    objVM = enqAct;
                }
                if (!String.IsNullOrEmpty(qteId))
                {
                    if (objVM.ActivityList == null)
                        objVM.ActivityList = new List<ContactPlanViewModel.ActivityListItem>();

                    var qteAct = await _serActivity.GetAllActivitiesByDocumentID(qteId);
                    if (qteAct != null)
                    {
                        objVM.ActivityList.AddRange(qteAct.ActivityList);
                    }

                }
                if (!String.IsNullOrEmpty(ordId))
                {
                    if (objVM.ActivityList == null)
                        objVM.ActivityList = new List<ContactPlanViewModel.ActivityListItem>();
                    var ordAct = await _serActivity.GetAllActivitiesByDocumentID(ordId);
                    if (ordAct != null)
                    {
                        objVM.ActivityList.AddRange(ordAct.ActivityList);
                    }
                }


                if (objVM != null)
                {
                    objVM.RefObjectID = enqId + "," + qteId + "," + ordId;
                    return View(objVM);
                }

            }
            catch (TimeoutException tex)
            {

            }
            catch (Exception ex)
            {
            }

            return View(objVM);
        }
        public async Task<IActionResult> ActivityDetails(string activityId, string enqId, string qteId, string ordId)
        {
            var detailsObject = new ContactPlanDetailsModel();
            try
            {
                detailsObject = await _serActivity.ActvityDetailsObject("ACTIVITY", activityId);

            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }
            return View(detailsObject);
        }

        public async Task<IActionResult> AttachmentHistory(string enqId, string qteId, string ordId)
        {
            var enqAttHist = new EnquiryAttachmentHistoryModel();
            try
            {
                enqAttHist.Attachments = new List<DownloadFileObject>();

                enqAttHist.EnquiryID = enqId;
                enqAttHist.Attachments = await _serAttachment.GetAttachments("OPPORTUNITY", enqId);

                if (!string.IsNullOrEmpty(qteId))
                {
                    var lsQuoteAtt = await _serAttachment.GetAttachments("QUOTATION", qteId);
                    if (lsQuoteAtt != null && lsQuoteAtt.Count > 0)
                        enqAttHist.Attachments.AddRange(lsQuoteAtt);
                }

                if (!string.IsNullOrEmpty(ordId))
                {
                    var lsOrderAtt = await _serAttachment.GetAttachments("ORDER", ordId);
                    if (lsOrderAtt != null && lsOrderAtt.Count > 0)
                        enqAttHist.Attachments.AddRange(lsOrderAtt);
                }

                enqAttHist.Attachments = enqAttHist.Attachments.OrderByDescending(x => x.CreatedOn).ToList();
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }
            return View(enqAttHist);
        }

        public async Task<IActionResult> UpdateTotalValue(string strId, double totalValue)
        {
            string strResult = "";
            try
            {
                ReportProxy.SummaryReportUpdateRequest objReq = new ReportProxy.SummaryReportUpdateRequest()
                {
                    Id = strId,
                    Value = totalValue
                };
                var serVal = await _serReport.OpportunitySummaryReportUpdate(objReq);
                if (serVal)
                {
                    strResult = "SUCCESS";
                }
                else
                {
                    strResult = "FAILED";
                }

            }
            catch (Exception ex)
            {

            }
            return Json(new { status = strResult });
        }
        public string GetQuarter(DateTime? dt)
        {
            string strRes = "";
            if (dt.HasValue)
            {
                if (dt.Value.Month >= 4 && dt.Value.Month < 7)
                    strRes = "Q1";
                else if (dt.Value.Month >= 7 && dt.Value.Month < 10)
                    strRes = "Q2";
                else if (dt.Value.Month >= 10 && dt.Value.Month <= 12)
                    strRes = "Q3";
                else if (dt.Value.Month >= 1 && dt.Value.Month < 4)
                    strRes = "Q4";
            }
            return strRes;
        }

        public string GetFinYear(DateTime? dt)
        {
            string strRes = "";
            if (dt.HasValue)
            {
                int year = dt.Value.Year;

                if (dt.Value.Month >= 4 && dt.Value.Month <= 12)
                    strRes = year + "-" + (year + 1).ToString().Substring(2, 2);

                else if (dt.Value.Month >= 1 && dt.Value.Month < 4)
                    strRes = (year - 1) + "-" + (year).ToString().Substring(2, 2);
            }
            return strRes;
        }


        #region Excel Export

        [HttpPost]
        public async Task<IActionResult> ReportExcel()
        {
            var sessionViewModel = HttpContext.Session.GetObjectFromJson<EnquiryApprovalIndexModel>("ESR");

            try
            {
                if (sessionViewModel != null)
                {
                    var objReqInput = new ReportProxy.CrmObjectReportRequest();
                    objReqInput.Branch = sessionViewModel.Branch;
                    objReqInput.Division = sessionViewModel.Division;

                    //objReqInput.DateFrom = SalesStaticMethods.ConvertDate(objReq.DateFrom.Split('-')[0]);
                    //objReqInput.DateTo = SalesStaticMethods.ConvertDate(objReq.DateFrom.Split('-')[1]);

                    objReqInput.DateFrom = SalesStaticMethods.ConvertDate(sessionViewModel.DateFrom);
                    objReqInput.DateTo = SalesStaticMethods.ConvertDate(sessionViewModel.DateTo);
                    var lsEnqApproval = await _serReport.OpportunitySummaryReport(objReqInput);


                    int headerCol = 1;
                    int row = 1;

                    var lsExpColumns = new List<string> {
                    "EnquiryID",
                    "QuoteID",
                    "OrderID",
                    "Division",
                    "Branch",
                    "CustomerName",
                    "Segment",
                    "SubSegment",
                    "Value(Cr.)",
                    "Status",
                    "Expected FinalQTR",
                    "Expected FinalQTR",
                    "Lost Price",
                    "Remarks",
                    "ContactPlan",
                    "CostSheet",
                    "Username",
                    "ApprovedByName"
                };
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (ExcelPackage package = new ExcelPackage(ms))
                        {

                            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");
                            //First add the headers
                            var styleHeaderCell = worksheet.Workbook.Styles.CreateNamedStyle("HeaderCell");
                            styleHeaderCell.Style.Font.Bold = true;


                            foreach (var prop in lsExpColumns)
                            {
                                worksheet.Cells[row, headerCol].Value = prop;
                                worksheet.Cells[row, headerCol].StyleName = styleHeaderCell.Name;
                                headerCol++;
                            }
                            row++;
                            for (int i = 0; i < lsEnqApproval.Count; i++)
                            {
                                int DataCol = 1;
                                worksheet.Cells[row, DataCol].Value = lsEnqApproval[i].EnquiryID;
                                worksheet.Cells[row, DataCol + 1].Value = lsEnqApproval[i].QuoteID;
                                worksheet.Cells[row, DataCol + 2].Value = lsEnqApproval[i].OrderID;
                                worksheet.Cells[row, DataCol + 3].Value = lsEnqApproval[i].Division;
                                worksheet.Cells[row, DataCol + 4].Value = lsEnqApproval[i].Branch;
                                worksheet.Cells[row, DataCol + 5].Value = lsEnqApproval[i].CustomerName;
                                worksheet.Cells[row, DataCol + 6].Value = lsEnqApproval[i].CustomerSegment;
                                worksheet.Cells[row, DataCol + 7].Value = lsEnqApproval[i].SubSegment;
                                worksheet.Cells[row, DataCol + 8].Value = lsEnqApproval[i].TotalValue;
                                worksheet.Cells[row, DataCol + 9].Value = lsEnqApproval[i].Status;
                                worksheet.Cells[row, DataCol + 10].Value = GetQuarter(lsEnqApproval[i].MaturityDate);
                                worksheet.Cells[row, DataCol + 11].Value = GetFinYear(lsEnqApproval[i].MaturityDate);
                                worksheet.Cells[row, DataCol + 12].Value = lsEnqApproval[i].LostPrice;
                                worksheet.Cells[row, DataCol + 13].Value = lsEnqApproval[i].Remarks;
                                worksheet.Cells[row, DataCol + 14].Value = lsEnqApproval[i].ActivitiyCount;
                                worksheet.Cells[row, DataCol + 15].Value = lsEnqApproval[i].AttachmentCount;
                                worksheet.Cells[row, DataCol + 16].Value = lsEnqApproval[i].UserName;
                                worksheet.Cells[row, DataCol + 17].Value = lsEnqApproval[i].ApprovedByName;

                                row++;
                            }

                            package.Save();
                        }
                        FileContentResult response = new FileContentResult(ms.ToArray(), "application/vnd.ms-excel") { FileDownloadName = "EnquirySummaryReport.xlsx" };
                        return response;
                    }
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
