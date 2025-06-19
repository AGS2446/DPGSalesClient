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
    public class EnquiryApprovalReportController : Controller
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
        public EnquiryApprovalReportController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor)
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
            bool blFirstReq = false;
            var prevController=HttpContext.Session.GetString("Controller");
            HttpContext.Session.SetString("Controller", "EnquiryApprovalReport");

            if(prevController!= "EnquiryApprovalReport")
            {
                blFirstReq = true;
            }

            EnquiryApprovalIndexModel objReq = new EnquiryApprovalIndexModel();
            try
            {
                if (HttpContext.Session.CheckSession("NavigationList"))
                {
                    string strDateFrom = "";
                    if (DateTime.Now.Month < 4)
                    {
                        strDateFrom = "01/04/" + (DateTime.Now.Year - 1);
                    }
                    else
                    {
                        strDateFrom = "01/04/" + DateTime.Now.Year;
                    }
                    var objReqObj = HttpContext.Session.GetObjectFromJson<EnquiryApprovalIndexModel>("EAR");

                    objReq.DivisionList = await _serClient.GetClients();
                    objReq.BranchList = await _serOrganization.GetBranches();
                    //  objReq.DateFrom = DateTime.Now.AddDays(-2).ToString("dd/MM/yyyy") + " - " + DateTime.Now.ToString("dd/MM/yyyy");
                    //objReq.DateFrom = DateTime.Now.AddDays(-2).ToString("dd/MM/yyyy").Replace("-","/");                    
                    objReq.DateFrom = strDateFrom;
                    objReq.DateTo = DateTime.Now.ToString("dd/MM/yyyy").Replace("-", "/");

                    if (objReq.DivisionList != null && objReq.DivisionList.Count > 0)
                        objReq.DivisionList.Insert(0, new ClientProxy.AGS_DropdownEntity { Text = "SELECT", Value = "" });

                    if (objReq.BranchList != null && objReq.BranchList.Count > 0)
                        objReq.BranchList.Insert(0, new OrganizationProxy.AGS_DropdownEntity { Text = "SELECT", Value = "" });

                    var objReqIn = new ReportProxy.CrmObjectReportRequest();
                    if (objReqObj != null)
                    {
                        objReq.Branch = objReqObj.Branch;
                        objReq.Division = objReqObj.Division;
                        objReq.DateFrom= objReqObj.DateFrom;
                        objReq.DateTo = objReqObj.DateTo;

                        objReqIn.Branch = objReqObj.Branch;
                        objReqIn.Division = objReqObj.Division;
                        objReqIn.DocumentId = objReqObj.EnquiryNo;
                        // objReqIn.DateFrom = objReqObj.DateFrom;
                        //objReqIn.DateFrom = SalesStaticMethods.ConvertDate(objReqObj.DateFrom.Split('-')[0]);
                        //objReqIn.DateTo = SalesStaticMethods.ConvertDate(objReqObj.DateFrom.Split('-')[1]);

                        objReqIn.DateFrom = SalesStaticMethods.ConvertDate(objReqObj.DateFrom);
                        objReqIn.DateTo = SalesStaticMethods.ConvertDate(objReqObj.DateTo);
                    }
                    else
                    {
                        //objReqIn.DateFrom = DateTime.Now.AddDays(-2);
                        objReqIn.DateFrom = Convert.ToDateTime(strDateFrom);
                        objReqIn.DateTo= DateTime.Now;                        
                    }

                    if (blFirstReq)
                    {
                        HttpContext.Session.ClearSession("EAR");
                      
                        objReqIn = new ReportProxy.CrmObjectReportRequest();
                        //objReqIn.DateFrom = DateTime.Now.AddDays(-2);
                        objReqIn.DateFrom = Convert.ToDateTime(strDateFrom);
                        objReqIn.DateTo = DateTime.Now;

                        objReq.Branch = null;
                        objReq.Division = null;
                        objReq.EnquiryNo = null;
                        // objReq.DateFrom = DateTime.Now.AddDays(-2).ToString("dd/MM/yyyy") + " - " + DateTime.Now.ToString("dd/MM/yyyy");
                        // objReq.DateFrom = DateTime.Now.AddDays(-2).ToString("dd/MM/yyyy").Replace("-", "/");
                        objReq.DateFrom = strDateFrom;
                        objReq.DateTo = DateTime.Now.ToString("dd/MM/yyyy").Replace("-", "/");

                    }

                    HttpContext.Session.SetObjectAsJson("EAR", objReq);

                    var lsPendingEnquiry = await _serReport.GetOpportunityApproval(objReqIn);

                    if (lsPendingEnquiry != null)
                    {
                        objReq.EnquiryList= lsPendingEnquiry.Select(y => new EnquiryViewReportModel
                        {
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
                    return View("Index", objReq);
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


            return View();
        }
              
        public async Task<IActionResult> Search(EnquiryApprovalIndexModel objInput)
        {
            try
            {
                var objReq = HttpContext.Session.GetObjectFromJson<EnquiryApprovalIndexModel>("EAR");
                objInput.BranchList = objReq.BranchList;
                objInput.DivisionList = objReq.DivisionList;

                var objReqInput = new ReportProxy.CrmObjectReportRequest();
                objReqInput.Branch = objInput.Branch;
                objReqInput.Division = objInput.Division;
                objReqInput.DocumentId = objInput.EnquiryNo;           
                //objReqInput.DateFrom = SalesStaticMethods.ConvertDate(objInput.DateFrom.Split('-')[0]);
                //objReqInput.DateTo = SalesStaticMethods.ConvertDate(objInput.DateFrom.Split('-')[1]);

                objReqInput.DateFrom = SalesStaticMethods.ConvertDate(objInput.DateFrom);
                objReqInput.DateTo = SalesStaticMethods.ConvertDate(objInput.DateTo);


                HttpContext.Session.SetObjectAsJson("EAR", objInput);

                var lsPendingEnquiry = await _serReport.GetOpportunityApproval(objReqInput);

                if (lsPendingEnquiry != null)
                {
                    objInput.EnquiryList = lsPendingEnquiry.Select(y => new EnquiryViewReportModel
                    {
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
                        ExpectedFinalYear =GetFinYear(y.MaturityDate),
                        CostSheet = y.AttachmentCount,
                        Contactplan = y.ActivitiyCount

                    }).ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return View("Index", objInput);
        }
        public async Task<IActionResult> ActivityIndex(string enqId,string qteId,string ordId)
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
                    objVM.RefObjectID = enqId+","+ qteId+","+ ordId;
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

                if(!string.IsNullOrEmpty(qteId))
                {
                    var lsQuoteAtt= await _serAttachment.GetAttachments("QUOTATION", qteId);
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
        public async Task<IActionResult> Attachments(string enqId)
        {
            var objAtt = new AttachmentsModel();
            try
            {

                objAtt = await _serAttachment.GetAttachmentModel("OPPORTUNITY", enqId);

            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }


            return View(objAtt);


        }
        [HttpPost]
        public async Task<IActionResult> Attachments(AttachmentsModel objInput)
        {
            try
            {
                var jsObject = Newtonsoft.Json.JsonConvert.DeserializeObject<UploadFile>(objInput.PendingUploadFiles);

                if (_serAttachment.VarifyJsonFileObject(jsObject))
                {
                    var lsFilesUploaded = await _serAttachment.UploadAttachments("OPPORTUNITY", objInput.ActivityId, jsObject);

                    if (lsFilesUploaded.Count > 0)
                    {
                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Enquiry", "Attachments uploaded successfully"));

                        return RedirectToAction("Index", "EnquiryApprovalReport");
                    }
                }
                else
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Enquiry", "Please add atleast one document"));

                    return RedirectToAction("Attachments","EnquiryApprovalReport", new { enqId = objInput.ActivityId });
                }


                //var lsFilesUploaded = new List<string>();
                //if (jsObject.Files.Count > 0)
                //{
                //    for (int intF = 0; intF < jsObject.Files.Count; intF++)
                //    {
                //        if (jsObject.Files[intF].Name != "")
                //        {
                //            var blRes = await _serFile.UploadFile(new FileManagerProxy.AGS_Attachment_Upload
                //            {
                //                Name = jsObject.Files[intF].Name,
                //                DocumentID = objInput.ActivityId,
                //                ObjectName = "OPPORTUNITY",
                //                Base = jsObject.Files[intF].File
                //            });

                //            if (blRes)
                //            {
                //                lsFilesUploaded.Add(jsObject.Files[intF].Name + " - Uploaded");
                //            }
                //        }

                //    }
                //}


                //if (lsFilesUploaded.Count > 0)
                //{


                //    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Enquiry", "Attachments uploaded successfully"));

                //    return RedirectToAction("Index");
                //}
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }


            return View(objInput);
        }

        public async Task<IActionResult> RejectEnquiry(string enqId, string status,string remarks)
        {
            string resStatus = "FAILED";
            try
            {
               

                //if (status.ToUpper() == "REJECT")
                //{
                    var blRes = await _serOpportunity.UpdateOpportunity(enqId, new OpportunityProxy.AGS_Opportunity { Status = "REJECTED", Reason = "Rejected enquiry reopen",Remarks=remarks });
                    if (blRes)
                    {
                    //popup

                      TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Enquiry", "Enquiry rejected successfully"));
                      resStatus = "SUCCESS";                    
                    }
                    else {
                    resStatus = "FAILED";
                }
                //}
                //else
                //{
                //    //Approve
                //    return RedirectToAction("ConvertToQuote", new { enqID = enqId });

                //}

                return Json(new {status=resStatus });
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }


            return Json(new { status = resStatus });
        }

        public async Task<IActionResult> ApprovalEnquiry(string enqId)
        {
            string resStatus = "FAILED";
            try
            {

                var blRes = await _serOpportunity.UpdateOpportunity(enqId, new OpportunityProxy.AGS_Opportunity { Status = "PENDING FOR APPROVAL" });
                if (blRes)
                {
                    //popup

                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Enquiry", "Enquiry updated successfully"));
                    resStatus = "SUCCESS";
                }
                else
                {
                    resStatus = "FAILED";
                }
              

                return Json(new { status = resStatus });
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }


            return Json(new { status = resStatus });
        }
        public async Task<IActionResult> CheckAuthorizeToApprove(string enqId,string totalVal,string processName)
        {
            bool blResult = false;
            try
            {
                var dblValueInCrs = (Convert.ToDouble(totalVal)/100);

                blResult = await _serRole.ValidateApprovalAmount(processName, dblValueInCrs);//Recommand
            }
            catch (Exception ex)
            {
            }

            return Json(new {status= blResult?"SUCCESS":"FAILED" });
        }
      
        public async Task<IActionResult> ConvertToQuote(string enqID)
        {
            try
            {

                var objConQuote = new EnquiryConvertQuoteModel();
                var objEnq = await _serOpportunity.RetriveOpportunity(enqID);
                if (objEnq != null)
                {
                    objConQuote.EnquiryID = objEnq.CRMOpportunityID;
                  //  objConQuote.EnquiryDocumentCreatedDate = objEnq.DocumentCreatedDate.HasValue ? objEnq.DocumentCreatedDate.Value.ToString("dd/MM/yyyy") : "";
                    objConQuote.QuoteProducts = objEnq.OpportunityProducts.Select(x => new QuoteProduct
                    {
                        BusinessSegment = x.MaterialGroup,
                        ProductSeg = x.ProductName,
                        ProductSegID = x.ProductID,
                        Quantity = x.Quantity,
                        TotalTonnage = x.TotalTonnageQuantity,
                        TotalValue = x.TotalValue
                    }).ToList();

                    return View(objConQuote);
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ConvertToQuote(EnquiryConvertQuoteModel objInput)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var enqObj = await _serOpportunity.RetriveOpportunity(objInput.EnquiryID);
                    if (enqObj != null)
                    {

                        var objNew = new QuotationProxy.AGS_Quotation();

                        objNew.Division = enqObj.Division;
                        objNew.DivisionName = enqObj.DivisionName;
                        objNew.Region = enqObj.Region;
                        objNew.RegionName = enqObj.RegionName;
                        objNew.Branch = enqObj.Branch;
                        objNew.BranchName = enqObj.BranchName;
                        objNew.SalesOffice = enqObj.SalesOffice;
                        objNew.PlantID = enqObj.PlantID;
                        objNew.PlantName = enqObj.PlantName;

                        objNew.AccountID = enqObj.AccountID;
                        objNew.AccountName = enqObj.AccountName;
                        objNew.CustomerClassification = enqObj.CustomerClassification;
                        objNew.CustomerClassificationId = enqObj.CustomerClassificationId;
                        objNew.CustomerSegment = enqObj.CustomerSegment;
                        objNew.CustomerSegmentID = enqObj.CustomerSegmentID;
                        objNew.SubSegment = enqObj.SubSegment;
                        objNew.SubSegmentID = enqObj.SubSegmentID;
                        objNew.CustomerType = enqObj.CustomerType;
                        objNew.CustomerTypeID = enqObj.CustomerTypeID;


                        objNew.BusinessSegmentID = enqObj.BusinessSegmentID;
                        objNew.BusinessSegment = enqObj.BusinessSegment;

                        objNew.Classification1 = enqObj.Classification1;
                        objNew.Classification2 = enqObj.Classification2;
                        objNew.Classification3 = enqObj.Classification3;
                        objNew.Classification3ID = enqObj.Classification3ID;
                        objNew.Classification4 = enqObj.Classification4;
                        objNew.Consultant = enqObj.Consultant;
                        objNew.ContractValue = enqObj.ContractValue;
                        objNew.CreatedOn = DateTime.Now;
                        // objNew.Currency = "";
                        objNew.Architect = enqObj.Architect;

                     //   objNew.DocumentCreatedDate = enqObj.DocumentCreatedDate;

                        objNew.Probability = enqObj.Probability;
                        objNew.ProductRequired = enqObj.ProductRequired;
                        objNew.ProjectName = enqObj.ProjectName;

                        objNew.SourceType = enqObj.SourceType;
                        objNew.Status = "OPEN";
                        objNew.Tonnage = Convert.ToDouble(enqObj.Tonnage);
                        objNew.TotalValue = enqObj.TotalValue;
                        objNew.UserID = enqObj.UserID;
                        objNew.Username = enqObj.Username;
                        objNew.CRMLeadID = enqObj.CRMLEADID;
                        objNew.CRMOppotunityID = enqObj.CRMOpportunityID;

                     //   objNew.QuoteValidityDate = SalesStaticMethods.ConvertDate(objInput.QuoteValidityDate);
                        objNew.QuoteMaturityDate = SalesStaticMethods.ConvertDate(objInput.QuoteMaturityDate);
                        objNew.OfferDate = SalesStaticMethods.ConvertDate(objInput.OfferDate);

                        if (enqObj.OpportunityProducts != null && enqObj.OpportunityProducts.Count > 0)
                        {
                            objNew.QuoteProducts = enqObj.OpportunityProducts.Select(x => new QuotationProxy.AGS_QuoteProduct
                            {
                                EntityState = QuotationProxy.EntityState.Added,
                                MaterialGroup = x.MaterialGroup,
                                ProductID = x.ProductID,
                                ProductName = x.ProductName,
                                Quantity = x.Quantity,
                                TotalTonnageQuantity = x.TotalTonnageQuantity,
                                TotalValue = x.TotalValue
                            }).ToList();
                        }

                        var strRes = await _serQuote.CreateQuote(objNew);
                        if (strRes != "")
                        {
                            TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Enquiry", "Enquiry("+enqObj.CRMOpportunityID+") has been converted to Quote("+strRes+") successfully"));

                            return RedirectToAction("Index");
                        }
                    }
                }
                else
                {

                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }
            return View();
        }


        public async Task<IActionResult> GetAttachments(string strEnqId)
        {
            try
            {
                //var lsFiles = await _serFile.GetFiles("OPPORTUNITY", strEnqId);
                //if (lsFiles != null && lsFiles.Count > 0)
                //{
                //    return Json(lsFiles);
                //}

            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return Json(null);
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
                    strRes = year+"-"+ (year+1).ToString().Substring(2,2);
              
                else if (dt.Value.Month >= 1 && dt.Value.Month < 4)
                    strRes = (year-1) + "-" + (year).ToString().Substring(2, 2);
            }
            return strRes;
        }


        #region Excel Export

        [HttpPost]
        public async Task<IActionResult> ReportExcel()
        {

            var objReq = HttpContext.Session.GetObjectFromJson<EnquiryApprovalIndexModel>("EAR");
          
            string sTempPathFolder = Path.GetTempPath();
            string sFileName = @"EnquiryApprovalReport.xlsx";
            FileInfo file = new FileInfo(Path.Combine(sTempPathFolder, sFileName));
            if (file.Exists)
            {
                file.Delete();
                file = new FileInfo(Path.Combine(sTempPathFolder, sFileName));
            }

            try
            {
               
                var objReqInput = new ReportProxy.CrmObjectReportRequest();
                objReqInput.Branch = objReq.Branch;
                objReqInput.Division = objReq.Division;
                objReqInput.DateFrom = SalesStaticMethods.ConvertDate(objReq.DateFrom);
                objReqInput.DateTo = SalesStaticMethods.ConvertDate(objReq.DateTo);
                var lsEnqApproval = await _serReport.GetOpportunityApproval(objReqInput);
                

                int headerCol = 1;
                int row = 1;
         
                var lsExpColumns = new List<string> {
                    "Division",
                    "Branch",
                    "Client",
                    "Segment",
                    "SubSegment",
                    "Value(Cr.)",
                    "Status",
                    "Expected FinalQTR",
                    "Expected FinalQTR",
                    "Lost Price",
                    "Remarks",
                    "ContactPlan",
                    "CostSheet"
                };

                using (ExcelPackage package = new ExcelPackage(file))
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
                        worksheet.Cells[row, DataCol].Value = lsEnqApproval[i].Division;
                        worksheet.Cells[row, DataCol + 1].Value = lsEnqApproval[i].Branch;
                        worksheet.Cells[row, DataCol + 2].Value = lsEnqApproval[i].CustomerName;
                        worksheet.Cells[row, DataCol + 3].Value = lsEnqApproval[i].CustomerSegment;
                        worksheet.Cells[row, DataCol + 4].Value = lsEnqApproval[i].SubSegment;
                        worksheet.Cells[row, DataCol + 5].Value = lsEnqApproval[i].TotalValue;
                        worksheet.Cells[row, DataCol + 6].Value = lsEnqApproval[i].Status;
                        worksheet.Cells[row, DataCol + 7].Value = GetQuarter(lsEnqApproval[i].MaturityDate);
                        worksheet.Cells[row, DataCol + 8].Value = GetFinYear(lsEnqApproval[i].MaturityDate);
                        worksheet.Cells[row, DataCol + 9].Value = lsEnqApproval[i].LostPrice;
                        worksheet.Cells[row, DataCol + 10].Value = lsEnqApproval[i].Remarks;
                        worksheet.Cells[row, DataCol + 11].Value = lsEnqApproval[i].ActivitiyCount;
                        worksheet.Cells[row, DataCol + 12].Value = lsEnqApproval[i].AttachmentCount;                      
                        row++;
                    }

                    package.Save();
                }
                FileContentResult response = new FileContentResult(System.IO.File.ReadAllBytes(Path.Combine(sTempPathFolder, sFileName)), "application/vnd.ms-excel");
                return response;
            }
            catch (Exception ex)
            {

            }
            return null;
            
        }

        #endregion
    }
}
