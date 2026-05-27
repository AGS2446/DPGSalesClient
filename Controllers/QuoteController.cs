using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using DPGSalesClient.Models;
using ActivityProxy;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuotationProxy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using AuthorizationProxy;
using DPGSalesClient.CommonLibrary.Exceptions;
using System.IO;
using Microsoft.Extensions.Configuration;
namespace DPGSalesClient.Controllers
{
    public class QuoteController : Controller
    {
        #region Varaibles  

        ServiceConnectors.QuotationServiceConnector _serQuote = null;
        ServiceConnectors.OrderServiceConnector _serOrder = null;
        ServiceConnectors.LocationServiceConnector _serLocation = null;
        ServiceConnectors.CompititorServiceConnector _serCompititor = null;
        ServiceConnectors.EntityMapServiceConnector _serEntity = null;
        ServiceConnectors.UserServiceConnector _serUser = null;
        ServiceConnectors.ProductServiceConnector _serProduct = null;

        Models.BusinessLogic.ActivityBL _serActivity = null;
        Models.BusinessLogic.AttachmentBL _serAttachment = null;
        Models.BusinessLogic.CustomerBL _serCustomer = null;

        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly HashSet<string> ExcludedDivisionIds;
        #endregion

        #region Constructor
        public QuoteController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            string strIp = SalesStaticMethods.GetRemoteIp(appSettings);

            _serQuote = new ServiceConnectors.QuotationServiceConnector(strIp, httpContextAccessor);
            _serOrder = new ServiceConnectors.OrderServiceConnector(strIp, httpContextAccessor);
            _serCompititor = new ServiceConnectors.CompititorServiceConnector(strIp, httpContextAccessor);
            _serLocation = new ServiceConnectors.LocationServiceConnector(strIp, httpContextAccessor);

            _serEntity = new ServiceConnectors.EntityMapServiceConnector(strIp, httpContextAccessor);
            _serUser = new ServiceConnectors.UserServiceConnector(strIp, httpContextAccessor);
            _serProduct = new ServiceConnectors.ProductServiceConnector(strIp, httpContextAccessor);

            _serActivity = new Models.BusinessLogic.ActivityBL(strIp, httpContextAccessor);
            _serAttachment = new Models.BusinessLogic.AttachmentBL(strIp, httpContextAccessor);
            _serCustomer = new Models.BusinessLogic.CustomerBL(strIp, httpContextAccessor);

            _logger = loggerFactory.CreateLogger<QuoteController>();
            _hostingEnvironment = hostingEnvironment;
            ExcludedDivisionIds = configuration.GetSection("ExcludedDivisionIds").Get<HashSet<string>>();
        }

        #endregion

        /******************************************************************
         * ****************** Quote **********************************
         ******************************************************************/                                                                                                                                                                             

        #region Quote
        public async Task<IActionResult> Index()
        {
            HttpContext.Session.SetString("Controller", "Quote");
            var objData = new QuoteViewModel();
            try
            {
                if (HttpContext.Session.CheckSession("NavigationList"))
                {
                    AGS_LoginUserInfo logInfo = HttpContext.Session.GetObjectFromJson<AGS_LoginUserInfo>( "LoginUserInfo" );
                    HttpContext.Session.SetString( "UserRoleId", logInfo.RoleInfo[ 0 ].RoleID );
                    ViewBag.RoleId = logInfo.RoleInfo[ 0 ].RoleID;
                    var lsQuote = await _serQuote.RetriveQuotes("", "", 0, "", 0);
                    if (lsQuote != null)
                    {
                        objData.QuoteList = lsQuote.Select(y => new QuoteViewModel.QuoteViewItemModel { QuoteID = y.CRMQuotationID, EnquiryID = y.CRMOppotunityID, BusineeSegment = y.BusinessSegment, CustomerName = y.AccountName, Status = y.Status, CreatedOn = y.CreatedOn, LeadID = y.CRMLeadID, Division = y.DivisionName, Branch = y.BranchName, Probablity = y.Probability, ContractValue = y.ContractValue, isDirect = !ExcludedDivisionIds.Contains(y.Division) }).ToList();
                        return View("Index", objData);
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


            return View(objData);
        }

        [HttpPost]
        public async Task<IActionResult> Search(string strKey)
        {
            var objData = new QuoteViewModel();
            try
            {
                objData.SearchKey = strKey.Trim();
                var lsData = await _serQuote.SearchQuotes(strKey.Trim());
                ViewBag.RoleId = HttpContext.Session.GetString( "UserRoleId" );
                if (lsData != null && lsData.Count > 0)
                {
                    objData.QuoteList = lsData.Select(y => new QuoteViewModel.QuoteViewItemModel { QuoteID = y.CRMQuotationID, EnquiryID = y.CRMOppotunityID, BusineeSegment = y.BusinessSegment, CustomerName = y.AccountName, Status = y.Status, CreatedOn = y.CreatedOn, LeadID = y.CRMLeadID, Division = y.DivisionName, Branch = y.BranchName, Probablity = y.Probability, ContractValue = y.ContractValue }).ToList();
                    return View("Index", objData);
                }
                else
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Quote", "No Data available"));

                    //var lsQuote = await _serQuote.RetriveQuotes("", "", 0, "", 0);
                    //if (lsQuote != null)
                    //{
                    //    return View("Index", lsQuote.Select(y => new QuoteViewModel { QuoteID = y.CRMQuotationID, EnquiryID = y.CRMOppotunityID, BusineeSegment = y.BusinessSegment, CustomerName = y.AccountName, Status = y.Status, CreatedOn = y.CreatedOn, LeadID = y.CRMLeadID, Division = y.DivisionName, Branch = y.BranchName, Probablity = y.Probability, ContractValue = y.ContractValue }).ToList());
                    //}

                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return View("Index", objData);
        }

        public async Task<IActionResult> Attachments(string qteId)
        {
            var objAtt = new AttachmentsModel();
            try
            {

                objAtt = await _serAttachment.GetAttachmentModel("QUOTATION", qteId);

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
                var allowedExtensions = new[]
{
    ".pdf", ".xlsx", ".txt", ".ppt", ".png", ".jpg", ".jpeg", ".docx", ".doc", ".xls"
};
                var jsObject = Newtonsoft.Json.JsonConvert.DeserializeObject<UploadFile>(objInput.PendingUploadFiles);
                foreach (var file in jsObject.Files)
                {
                    if (string.IsNullOrWhiteSpace(file.Name))
                        continue;

                    string ext = Path.GetExtension(file.Name).ToLower();

                    if (!allowedExtensions.Contains(ext))
                    {
                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Enquiry", $"File type not allowed: {file.Name}"));

                        return RedirectToAction("Attachments", new { enqId = objInput.ActivityId });
                    }
                }
                if (_serAttachment.VarifyJsonFileObject(jsObject))
                {
                    var lsFilesUploaded = await _serAttachment.UploadAttachments("QUOTATION", objInput.ActivityId, jsObject);

                    if (lsFilesUploaded.Count > 0)
                    {
                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Quotation", "Attachments uploaded successfully"));

                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Quotation", "Please add atleast one document"));

                    return RedirectToAction("Attachments", new { qteId = objInput.ActivityId });
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
                //                ObjectName = "QUOTATION",
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

                //    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Quotation", "Attachments uploaded successfully"));

                //    return RedirectToAction("Index");
                //}
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }


            return View(objInput);
        }

        public async Task<IActionResult> DeleteAttachment(int Id, string activityID)
        {
            try
            {
                var res = await _serAttachment.DeleteAttachment(Id);
            }
            catch (TimeoutException tex)
            {

            }
            catch (Exception ex)
            {

            }
            return RedirectToAction("Attachments", new { qteId = activityID });
        }

        public async Task<IActionResult> Details(string qteId, string status)
        {
            var objDetails = new QuoteDetailsModel();
            try
            {
                var lsFiles = await _serAttachment.GetAttachments("QUOTATION", qteId);
                ViewBag.RoleId = HttpContext.Session.GetString( "UserRoleId" );
                var quoteDetails = await _serQuote.RetriveQuote(qteId);
                if (quoteDetails != null)
                {
                    objDetails = new QuoteDetailsModel
                    {
                        QuoteID = quoteDetails.CRMQuotationID,
                        EnquiryID = quoteDetails.CRMOppotunityID,
                        //LeadID = quoteDetails.CRMLeadID,
                        Status = quoteDetails.Status,
                        Division = quoteDetails.DivisionName,
                        Region = quoteDetails.RegionName,
                        Branch = quoteDetails.BranchName,
                        SalesOffice = quoteDetails.SalesOffice,
                        Plant = quoteDetails.PlantName,
                        CustomerCode = quoteDetails.AccountID,
                        CustomerName = quoteDetails.AccountName,
                        CustomerClassification = quoteDetails.CustomerClassification,
                        CustomerSegment = quoteDetails.CustomerSegment,
                        CustomerSubSegment = quoteDetails.SubSegment,
                        CustomerType = quoteDetails.CustomerType,
                        Tonnage = quoteDetails.Tonnage,
                        TotalValue = quoteDetails.TotalValue,
                        BusinessSegment = quoteDetails.BusinessSegment,
                        ProdcutRequired = quoteDetails.ProductRequired,
                        ProjectName = quoteDetails.ProjectName,
                        //Architect = quoteDetails.Architect,
                        //Consultant = quoteDetails.Consultant,
                        ContractValue_IN_LAKHS = quoteDetails.ContractValue,
                        Probability = quoteDetails.Probability,
                        QuoteAssignTo = quoteDetails.Username,
                        QuoteMaturityDate = quoteDetails.QuoteMaturityDate.HasValue ? quoteDetails.QuoteMaturityDate.Value.ToString("dd/MM/yyyy") : "",
                        // QuoteValidityDate = quoteDetails.QuoteMaturityDate.HasValue ? quoteDetails.QuoteMaturityDate.Value.ToString("dd/MM/yyyy") : "",
                        // DocumentCreatedDate = quoteDetails.DocumentCreatedDate.HasValue ? quoteDetails.DocumentCreatedDate.Value.ToString("dd/MM/yyyy") : "",
                        QuoteOfferDate = quoteDetails.OfferDate.HasValue ? quoteDetails.OfferDate.Value.ToString("dd/MM/yyyy") : "",
                        QuoteDescription = quoteDetails.Description,
                        Currency = quoteDetails.Currency,
                        SourceType = quoteDetails.SourceType,
                        Classification1 = quoteDetails.Classification1,
                        Classification2 = quoteDetails.Classification2,
                        Classification3 = quoteDetails.Classification3,
                        Classification4 = quoteDetails.Classification4,
                        Files = lsFiles,
                        isDirect = !ExcludedDivisionIds.Contains(quoteDetails.Division),
                        Versions = quoteDetails.Versions?
    .Select(x => new DPGSalesClient.Models.AGS_VersionHistory
    {
        OldContractValue = x.OldContractValue,
        NewContractValue = x.NewContractValue,
        CreatedBy = x.CreatedBy,
        CreatedDate = x.CreatedDate,
        Remarks=x.Remarks
    }).ToList() ?? new List<DPGSalesClient.Models.AGS_VersionHistory>()
                };
                }


            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }
            HttpContext.Session.SetString("ConvertOrderInfo", "");
            HttpContext.Session.SetString("QuoteNew", "");

            return View(objDetails);
        }

        public async Task<IActionResult> Edit(string qteId, string status, string stBack)
        {
            string Entity = "Lead";
            try
            {
                if (stBack == "BACK")
                {
                    var objNewQte = HttpContext.Session.GetObjectFromJson<QuoteNewModel>("QuoteNew");
                    if (objNewQte.isDirect)
                    {
                        Entity = "ENQUIRY";
                    }
                    if (objNewQte != null)
                    {
                        if (objNewQte.Branch != null)
                        {
                            var lsPlans = await _serLocation.RetreiveLocationDetailsByOrg(objNewQte.Region.Split('#')[1], objNewQte.Division.Split('#')[1]);
                            if (lsPlans != null)
                            {
                                objNewQte.BranchList = lsPlans.Branches.Select(x => new SelectListItemObject { Value = x.Name + "#" + x.ID, Text = x.Name }).ToList();
                            }
                        }
                        if (objNewQte.Plant != null)
                        {
                            var lsPlans = await _serLocation.RetreiveLocationDetailsByOrg(objNewQte.Branch.Split('#')[1], objNewQte.Division.Split('#')[1]);
                            if (lsPlans != null)
                            {
                                objNewQte.PlantsList = lsPlans.Plants.Select(x => new SelectListItemObject { Value = x.Name + "#" + x.Code, Text = x.Name }).ToList();
                            }
                        }
                        if (objNewQte.QuoteAssignTo != null)
                        {
                            var lsUsers = await _serUser.GetAssignedUsers(objNewQte.Division.Split('#')[1], objNewQte.Branch.Split('#')[1]);
                            if (lsUsers != null)
                            {
                                objNewQte.QuoteAssignToList = lsUsers.Select(x => new SelectListItemObject { Value = x.Text + "#" + x.Value, Text = x.Text }).ToList();
                                if (objNewQte.QuoteAssignToList.Count > 1)
                                {
                                    objNewQte.QuoteAssignToList.Insert(0, new SelectListItemObject { Value = "", Text = "SELECT" });
                                }
                            }
                        }
                        if (objNewQte.CustomerSubSegment != null)
                        {
                            var lsSubseg = await _serEntity.RetriveByParentId(Entity, "CUSTOMERSUBSEGMENT", objNewQte.CustomerSegment.Split('#')[0]);
                            if (lsSubseg != null)
                            {
                                objNewQte.CustomerSubSegmentList = lsSubseg.Select(x => new SelectListItemObject { Value = x.PropertyName + "#" + x.PropertyValue, Text = x.PropertyName }).ToList();
                                if (objNewQte.CustomerSubSegmentList.Count > 1)
                                {
                                    objNewQte.CustomerSubSegmentList.Insert(0, new SelectListItemObject { Value = "", Text = "SELECT" });
                                }
                            }
                        }

                        HttpContext.Session.SetObjectAsJson("QuoteNew", objNewQte);
                        return View(objNewQte);
                    }
                }
                else
                {

                    var objNew = new QuoteNewModel();
                    var qteEdit = await _serQuote.RetriveQuote(qteId);
                    if (qteEdit != null)
                    {
                        objNew.QuoteID = qteEdit.CRMQuotationID;
                        objNew.EnquiryID = qteEdit.CRMOppotunityID;
                        //objNew.LeadID = qteEdit.CRMLeadID;
                        objNew.Division = qteEdit.DivisionName + "#" + qteEdit.Division;
                        HttpContext.Session.SetObjectAsJson("quoteDivision", objNew.Division);
                        objNew.Region = qteEdit.RegionName + "#" + qteEdit.Region;
                        objNew.Branch = qteEdit.BranchName + "#" + qteEdit.Branch;
                        objNew.SalesOffice = qteEdit.SalesOffice;
                        objNew.Plant = qteEdit.PlantName + "#" + qteEdit.PlantID;

                        objNew.CustomerCode = qteEdit.AccountID;
                        objNew.CustomerName = qteEdit.AccountName;
                        objNew.CustomerSegment = qteEdit.CustomerSegment + "#" + qteEdit.CustomerSegmentID;
                        objNew.CustomerSubSegment = qteEdit.SubSegment + "#" + qteEdit.SubSegmentID;
                        objNew.CustomerType = qteEdit.CustomerType + "#" + qteEdit.CustomerTypeID;
                        objNew.CustomerClassification = qteEdit.CustomerClassification + "#" + qteEdit.CustomerClassificationId;

                        objNew.Probability = qteEdit.Probability;
                        objNew.ContractValue_IN_LAKHS = qteEdit.ContractValue;
                        objNew.Status = qteEdit.Status;
                        objNew.Tonnage = qteEdit.Tonnage;
                        objNew.TotalValue = qteEdit.TotalValue;
                        objNew.ProjectName = qteEdit.ProjectName;
                        objNew.Currency = qteEdit.Currency;
                        objNew.CurrencyValue = qteEdit.CurrencyValue;
                        //objNew.Architect = qteEdit.Architect;
                        //objNew.Consultant = qteEdit.Consultant;
                        objNew.QuoteDescription = qteEdit.Description;
                        //  objNew.DocumentCreatedDate = qteEdit.DocumentCreatedDate.HasValue ? qteEdit.DocumentCreatedDate.Value.ToString("dd/MM/yyyy") : "";
                        objNew.QuoteMaturityDate = qteEdit.QuoteMaturityDate.HasValue ? qteEdit.QuoteMaturityDate.Value.ToString("dd/MM/yyyy") : "";
                        // objNew.QuoteValidityDate = qteEdit.QuoteValidityDate.HasValue ? qteEdit.QuoteValidityDate.Value.ToString("dd/MM/yyyy") : "";
                        objNew.QuoteOfferDate = qteEdit.OfferDate.HasValue ? qteEdit.OfferDate.Value.ToString("dd/MM/yyyy") : "";
                        objNew.Probability = qteEdit.Probability;
                        objNew.ContractValue_IN_LAKHS = qteEdit.ContractValue;
                        objNew.BusinessSegment = qteEdit.BusinessSegment + "#" + qteEdit.BusinessSegmentID;
                        objNew.ProdcutRequired = qteEdit.ProductRequired;
                        objNew.SourceType = qteEdit.SourceType;
                        //objNew.Classification1 = qteEdit.Classification1;
                        objNew.Classification2 = qteEdit.Classification2;
                        //objNew.Classification3 = qteEdit.Classification3 + "#" + qteEdit.Classification3ID;
                        objNew.Classification4 = qteEdit.Classification4;
                        objNew.QuoteAssignTo = qteEdit.Username + "#" + qteEdit.UserID;

                        objNew.City = qteEdit.City;
                        objNew.State = qteEdit.State;
                        objNew.MobileNumber = qteEdit.MobileNumber;
                        objNew.Pincode = qteEdit.Pincode;
                        objNew.ContactPerson = qteEdit.ContactName;
                        objNew.CustomerAddress = qteEdit.Address1;
                        #region Account details
                        //var objAcc = await _serCustomer.GetCustomers(qteEdit.AccountName);
                        //if (objAcc != null && objAcc.Count > 0)
                        //{
                        //    objNew.City = objAcc[0].city;
                        //    objNew.MobileNumber = objAcc[0].mobilenumber;
                        //    objNew.State = objAcc[0].state;
                        //    objNew.Pincode = objAcc[0].pincode;
                        //    objNew.ContactPerson = objAcc[0].contactperson;
                        //    objNew.CustomerAddress = objAcc[0].customeraddress;

                        //}
                        #endregion

                        #region Location details

                        var lsLocdetails = await _serLocation.RetreiveLocationDetails();

                        if (lsLocdetails != null)
                        {
                            var lsobjList = new List<SelectListItemObject>();
                            //Divisions
                            if (lsLocdetails.Divisions.Count > 0)
                            {
                                lsobjList = lsLocdetails.Divisions.Select(x => new SelectListItemObject { Text = x.Name, Value = x.Name + "#" + x.ID }).ToList();
                                if (lsobjList.Count > 1)
                                    lsobjList.Insert(0, new SelectListItemObject { Text = "SELECT", Value = "" });

                                objNew.DivisionList = lsobjList;
                                // objNew.DivisionList = new SelectList(lsobjList, "Value", "Text");
                            }
                            //Regions
                            if (lsLocdetails.Regions.Count > 0)
                            {
                                lsobjList = new List<SelectListItemObject>();
                                lsobjList = lsLocdetails.Regions.Select(x => new SelectListItemObject { Text = x.Name, Value = x.Name + "#" + x.ID }).ToList();
                                if (lsobjList.Count > 1)
                                {
                                    lsobjList.Insert(0, new SelectListItemObject { Text = "SELECT", Value = "" });
                                }
                                objNew.RegionList = lsobjList;


                            }
                            //Branch
                            var lsBranches = await _serLocation.RetreiveLocationDetailsByOrg(qteEdit.Region, qteEdit.Division);
                            if (lsBranches != null && lsBranches.Branches.Count > 0)
                            {
                                lsobjList = new List<SelectListItemObject>();
                                lsobjList = lsBranches.Branches.Select(x => new SelectListItemObject { Text = x.Name, Value = x.Name + "#" + x.ID }).ToList();

                                if (lsobjList.Count > 1)
                                {
                                    lsobjList.Insert(0, new SelectListItemObject { Text = "SELECT", Value = "" });
                                }
                                objNew.BranchList = lsobjList;

                            }
                            if (objNew.Plant != null)
                            {
                                var lsPlans = await _serLocation.RetreiveLocationDetailsByOrg(qteEdit.Branch, qteEdit.Division);
                                if (lsPlans != null)
                                {
                                    objNew.PlantsList = lsPlans.Plants.Select(x => new SelectListItemObject { Value = x.Name + "#" + x.Code, Text = x.Name }).ToList();
                                }
                            }
                            if (objNew.QuoteAssignTo != null)
                            {
                                var lsUsers = await _serUser.GetAssignedUsers(qteEdit.Division, qteEdit.Branch);
                                if (lsUsers != null)
                                {
                                    objNew.QuoteAssignToList = lsUsers.Select(x => new SelectListItemObject { Value = x.Text + "#" + x.Value, Text = x.Text }).ToList();
                                    if (objNew.QuoteAssignToList.Count > 1)
                                    {
                                        objNew.QuoteAssignToList.Insert(0, new SelectListItemObject { Value = "", Text = "SELECT" });
                                    }
                                }
                            }


                        }
                        #endregion

                        #region EntityMap Details
                        objNew.isDirect = ExcludedDivisionIds.Contains(qteEdit.Division);
                        //string Entity = "Lead";
                        if (objNew.isDirect)
                        {
                            Entity = "ENQUIRY";
                        }

                        var lsEntity = await _serEntity.RetriveByObjectName(Entity);
                        if (lsEntity != null && lsEntity.Count > 0)
                        {
                            objNew.CustomerSegmentList = SalesStaticMethods.GetSelectlistItemsByName("CUSTOMERSEGMENT", lsEntity, "B");
                            objNew.CustomerClassificationList = SalesStaticMethods.GetSelectlistItemsByName("CUSTOMERCLASSIFICATION", lsEntity, "B");
                            objNew.CustomerTypeList = SalesStaticMethods.GetSelectlistItemsByName("CUSTOMERTYPE", lsEntity, "B");
                            objNew.BusinessSegmentList = SalesStaticMethods.GetSelectlistItemsByName("BUSINESSSEGMENT", lsEntity, "B");
                            objNew.ProdcutRequiredList = SalesStaticMethods.GetSelectlistItemsByName("PRODUCTREQUIRED", lsEntity, "P");
                            objNew.ProbabilityList = SalesStaticMethods.GetSelectlistItemsByName("PROBABILITY", lsEntity, "P");
                            objNew.CurrencyList = SalesStaticMethods.GetSelectlistItemsByName("CURRENCY", lsEntity, "P");
                            //objNew.Classification1List = SalesStaticMethods.GetSelectlistItemsByName("CLASSIFICATION1", lsEntity, "P");
                            objNew.Classification2List = SalesStaticMethods.GetSelectlistItemsByName("CLASSIFICATION2", lsEntity, "P");
                            //objNew.Classification3List = SalesStaticMethods.GetSelectlistItemsByName("CLASSIFICATION3", lsEntity, "B");
                            objNew.Classification4List = SalesStaticMethods.GetSelectlistItemsByName("CLASSIFICATION4", lsEntity, "P");
                            objNew.SourceTypeList = SalesStaticMethods.GetSelectlistItemsByName("SOURCETYPE", lsEntity, "P");

                        }

                        if (objNew.CustomerSubSegment != null)
                        {
                            var lsSubseg = await _serEntity.RetriveByParentId(Entity, "CUSTOMERSUBSEGMENT", qteEdit.CustomerSegment);
                            if (lsSubseg != null)
                            {
                                objNew.CustomerSubSegmentList = lsSubseg.Select(x => new SelectListItemObject { Value = x.PropertyName + "#" + x.PropertyValue, Text = x.PropertyName }).ToList();
                                if (objNew.CustomerSubSegmentList.Count > 1)

                                    objNew.CustomerSubSegmentList.Insert(0, new SelectListItemObject { Value = "", Text = "SELECT" });
                            }
                        }
                        #endregion


                        if (qteEdit.QuoteProducts != null && qteEdit.QuoteProducts.Count > 0)
                        {
                            objNew.QuoteProducts = qteEdit.QuoteProducts.Select(x => new QuoteProduct
                            {
                                BusinessSegment = x.MaterialGroup,
                                QuoteID = x.CRMQuoteID,
                                ProductSegID = x.ProductID,
                                ProductSeg = x.ProductName,
                                Quantity = x.Quantity,
                                TotalTonnage = x.TotalTonnageQuantity,
                                TotalValue = x.TotalValue
                            }).ToList();
                        }


                        HttpContext.Session.SetObjectAsJson("QuoteNew", objNew);
                        return View(objNew);
                    }
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Edit(QuoteNewModel objInput)
        {
            try
            {
                var objNewQte = HttpContext.Session.GetObjectFromJson<QuoteNewModel>("QuoteNew");
                if (objNewQte != null)
                {
                    if (objNewQte.QuoteProducts != null && objNewQte.QuoteProducts.Count > 0)
                    {
                        AGS_Quotation objEditQuote = new AGS_Quotation();
                        objEditQuote.CRMQuotationID = objNewQte.QuoteID;
                        objEditQuote.CRMOppotunityID = objNewQte.EnquiryID;
                        //objEditQuote.CRMLeadID = objNewQte.LeadID;

                        objEditQuote.Division = objInput.Division.Split('#')[1];
                        objEditQuote.DivisionName = objInput.Division.Split('#')[0];
                        objEditQuote.Region = objInput.Region.Split('#')[1];
                        objEditQuote.RegionName = objInput.Region.Split('#')[0];
                        objEditQuote.Branch = objInput.Branch.Split('#')[1];
                        objEditQuote.BranchName = objInput.Branch.Split('#')[0];
                        objEditQuote.SalesOffice = objInput.SalesOffice;
                        objEditQuote.PlantID = objInput.Plant.Split('#')[1];
                        objEditQuote.PlantName = objInput.Plant.Split('#')[0];


                        objEditQuote.AccountID = objInput.CustomerCode;
                        objEditQuote.AccountName = objInput.CustomerName;
                        objEditQuote.City = objInput.City;
                        objEditQuote.State = objInput.State;
                        objEditQuote.MobileNumber = objInput.MobileNumber;
                        objEditQuote.Pincode = objInput.Pincode;
                        objEditQuote.ContactName = objInput.ContactPerson;
                        objEditQuote.Address1 = objInput.CustomerAddress;

                        objEditQuote.CustomerSegment = objInput.CustomerSegment.Split('#')[0];
                        objEditQuote.CustomerSegmentID = objInput.CustomerSegment.Split('#')[1];
                        objEditQuote.SubSegment = objInput.CustomerSubSegment.Split('#')[0];
                        objEditQuote.SubSegmentID = objInput.CustomerSubSegment.Split('#')[1];
                        objEditQuote.CustomerType = objInput.CustomerType.Split('#')[0];
                        objEditQuote.CustomerClassification = objInput.CustomerClassification.Split('#')[0];
                        objEditQuote.CustomerClassificationId = objInput.CustomerClassification.Split('#')[1];

                        objEditQuote.BusinessSegmentID = objInput.BusinessSegment.Split('#')[1];
                        objEditQuote.BusinessSegment = objInput.BusinessSegment.Split('#')[0];
                        //objEditQuote.Classification1 = objInput.Classification1;
                        objEditQuote.Classification2 = objInput.Classification2;
                        //objEditQuote.Classification3 = objInput.Classification3.Split('#')[0];
                       // objEditQuote.Classification3ID = objInput.Classification3.Split('#')[1];
                        objEditQuote.Classification4 = objInput.Classification4;
                        objEditQuote.UserID = objInput.QuoteAssignTo.Split('#')[1];
                        objEditQuote.Username = objInput.QuoteAssignTo.Split('#')[0];
                        //objEditQuote.Architect = objInput.Architect;
                        //objEditQuote.Consultant = objInput.Consultant;
                        objEditQuote.SourceType = objInput.SourceType;
                        objEditQuote.ProjectName = objInput.ProjectName;
                        objEditQuote.ProductRequired = objInput.ProdcutRequired;
                        objEditQuote.ContractValue = objInput.ContractValue_IN_LAKHS;
                        objEditQuote.Probability = objInput.Probability;
                        objEditQuote.Currency = objInput.Currency;
                        objEditQuote.CurrencyValue = objInput.CurrencyValue;

                        //Enquiry details
                      //  objEditQuote.QuoteMaturityDate = SalesStaticMethods.ConvertDate(objInput.QuoteMaturityDate);
                        objEditQuote.QuoteMaturityDate = string.IsNullOrWhiteSpace(objInput.QuoteMaturityDate) ? (DateTime?)null : Convert.ToDateTime(objInput.QuoteMaturityDate);
                        // objEditQuote.QuoteValidityDate = SalesStaticMethods.ConvertDate(objInput.QuoteValidityDate);
                        // objEditQuote.DocumentCreatedDate = SalesStaticMethods.ConvertDate(objInput.DocumentCreatedDate);
                        objEditQuote.OfferDate = string.IsNullOrWhiteSpace(objInput.QuoteOfferDate) ? (DateTime?)null : Convert.ToDateTime(objInput.QuoteOfferDate);
                        //objEditQuote.OfferDate = SalesStaticMethods.ConvertDate(objInput.QuoteOfferDate);
                        objEditQuote.Tonnage = objInput.Tonnage.Value;
                        objEditQuote.TotalValue = objInput.TotalValue;
                        objEditQuote.Description = objInput.QuoteDescription;
                        objEditQuote.Status = objInput.Status;

                        objEditQuote.QuoteProducts = new List<AGS_QuoteProduct>();
                        if (objNewQte.QuoteProducts.Count > 0)
                        {
                            var lsProducts = objNewQte.QuoteProducts.Select(x => new AGS_QuoteProduct
                            {
                                MaterialGroup = x.BusinessSegment,
                                ProductID = x.ProductSegID,
                                ProductName = x.ProductSeg,
                                Quantity = x.Quantity,
                                TotalValue = x.TotalValue,
                                EntityState = x.EntityState,
                                TotalTonnageQuantity = x.TotalTonnage
                            }).ToList();


                            objEditQuote.QuoteProducts = lsProducts;
                        }

                        if (objEditQuote.QuoteProducts.Count > 0)
                        {
                            var blRes = await _serQuote.UpdateQuote(objNewQte.QuoteID, objEditQuote);
                            if (blRes)
                            {

                                TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Quote", "Quote (" + objNewQte.QuoteID + ") has been update successfully"));

                                return RedirectToAction("Details", new { qteId = objInput.QuoteID, status = "OPEN" });
                            }
                        }
                        HttpContext.Session.SetString("quoteDivision", "");
                        HttpContext.Session.ClearSession("QuoteNew");
                    }
                    else
                    {

                        objInput.DivisionList = objNewQte.DivisionList;
                        objInput.RegionList = objNewQte.RegionList;
                        objInput.BranchList = objNewQte.BranchList;
                        objInput.PlantsList = objNewQte.PlantsList;
                        objInput.CustomerSegmentList = objNewQte.CustomerSegmentList;
                        objInput.CustomerSubSegmentList = objNewQte.CustomerSubSegmentList;
                        objInput.CustomerTypeList = objNewQte.CustomerTypeList;
                        objInput.CustomerClassificationList = objNewQte.CustomerClassificationList;
                        objInput.ProbabilityList = objNewQte.ProbabilityList;
                        objInput.ProdcutRequiredList = objNewQte.ProdcutRequiredList;
                        objInput.SourceTypeList = objNewQte.SourceTypeList;
                        objInput.CurrencyList = objNewQte.CurrencyList;
                        objInput.QuoteAssignToList = objNewQte.QuoteAssignToList;
                        objInput.Classification1List = objNewQte.Classification1List;
                        objInput.Classification2List = objNewQte.Classification2List;
                        objInput.Classification3List = objNewQte.Classification3List;
                        objInput.Classification4List = objNewQte.Classification4List;
                        objInput.BusinessSegmentList = objNewQte.BusinessSegmentList;
                        objInput.QuoteProducts = objNewQte.QuoteProducts;

                        HttpContext.Session.SetObjectAsJson("QuoteNew", objInput);

                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Quote", "You should provide at least one product"));

                        return RedirectToAction("Edit", new { qteId = objNewQte.QuoteID, stBack = "BACK" });
                    }
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }



            return View(objInput);
        }
        public async Task<IActionResult> NavigateEnquiryItems(QuoteNewModel objInput)
        {
            try
            {
                var objNew = HttpContext.Session.GetObjectFromJson<QuoteNewModel>("QuoteNew");
                if (objNew != null)
                {
                    objInput.EnquiryID = objNew.EnquiryID;
                    objInput.DivisionList = objNew.DivisionList;
                    objInput.RegionList = objNew.RegionList;
                    objInput.BranchList = objNew.BranchList;
                    objInput.PlantsList = objNew.PlantsList;
                    objInput.CustomerSegmentList = objNew.CustomerSegmentList;
                    objInput.CustomerSubSegmentList = objNew.CustomerSubSegmentList;
                    objInput.CustomerTypeList = objNew.CustomerTypeList;
                    objInput.CustomerClassificationList = objNew.CustomerClassificationList;
                    objInput.ProbabilityList = objNew.ProbabilityList;
                    objInput.ProdcutRequiredList = objNew.ProdcutRequiredList;
                    objInput.SourceTypeList = objNew.SourceTypeList;
                    objInput.QuoteAssignToList = objNew.QuoteAssignToList;
                    objInput.Classification1List = objNew.Classification1List;
                    objInput.Classification2List = objNew.Classification2List;
                    objInput.Classification3List = objNew.Classification3List;
                    objInput.Classification4List = objNew.Classification4List;
                    objInput.BusinessSegmentList = objNew.BusinessSegmentList;
                    objInput.QuoteProducts = objNew.QuoteProducts;
                    objInput.CurrencyList = objNew.CurrencyList;
                }

                HttpContext.Session.SetObjectAsJson("QuoteNew", objInput);

                string status = "FAILED";
                if (objInput.BusinessSegment != null)
                {
                    status = "SUCCESS";
                }
                return Json(new { status = status, BusinessSegment = objInput.BusinessSegment, quoteId = objInput.QuoteID });
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return Json(null);

        }
        public async Task<IActionResult> QuoteItems(string BusinessSegment, string qteId)//string BusinesSeg,
        {
            var itemsObject = new QuoteItemModel();
            try
            {
                itemsObject.BusinessSegment = BusinessSegment;
                itemsObject.QuoteID = qteId;
                var objNewEnq = HttpContext.Session.GetObjectFromJson<QuoteNewModel>("QuoteNew");
                if (objNewEnq != null)
                {
                    itemsObject.QuoteID = objNewEnq.EnquiryID;
                    itemsObject.BusinessSegment = objNewEnq.BusinessSegment;
                    itemsObject.Products = objNewEnq.QuoteProducts;
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }



            return View(itemsObject);
        }
        public async Task<IActionResult> AddProduct(string BusinesSeg)
        {
            var newQuoteProduct = new QuoteProduct();
            try
            {
                newQuoteProduct.BusinessSegment = BusinesSeg.Split('#')[0];
                newQuoteProduct.Division = HttpContext.Session.GetObjectFromJson<string>("quoteDivision");
                var lsProds = await _serProduct.GetProducts();
                if (lsProds != null)
                {
                    var lsSelect = lsProds.Select(x => new SelectListItem { Text = x.Text, Value = x.Text + "#" + x.Value }).ToList();
                    if (lsSelect.Count > 1)
                        lsSelect.Insert(0, new SelectListItem { Text = "SELECT", Value = "" });

                    newQuoteProduct.ProductList = new SelectList(lsSelect, "Value", "Text");
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }




            return View(newQuoteProduct);
        }
        [HttpPost]
        public async Task<IActionResult> AddProduct(QuoteProduct objInput)
        {
            try
            {
                var objNewQte = HttpContext.Session.GetObjectFromJson<QuoteNewModel>("QuoteNew");
                if (objNewQte != null)
                {
                    if (objNewQte.QuoteProducts == null)
                    {
                        objNewQte.QuoteProducts = new List<QuoteProduct>();
                    }

                    if (objInput != null)
                    {
                        if (objInput.ProductSeg != null)
                        {
                            objInput.ProductSegID = objInput.ProductSeg.Split('#')[1];
                            objInput.ProductSeg = objInput.ProductSeg.Split('#')[0];
                            objInput.BusinessSegment = objInput.BusinessSegment;

                            var idxObj = objNewQte.QuoteProducts.Where(x => x.ProductSegID == objInput.ProductSegID).FirstOrDefault();
                            if (idxObj != null)
                            {
                                var idx = objNewQte.QuoteProducts.IndexOf(idxObj);

                                if (objNewQte.QuoteProducts[idx].EntityState == EntityState.Deleted && !String.IsNullOrEmpty(objNewQte.QuoteProducts[idx].QuoteID))
                                {
                                    objNewQte.TotalValue += objInput.TotalValue;
                                    objNewQte.Tonnage += objInput.TotalTonnage;
                                    objNewQte.QuoteProducts[idx].EntityState = EntityState.Modified;
                                    objNewQte.QuoteProducts[idx].TotalTonnage = objInput.TotalTonnage;
                                    objNewQte.QuoteProducts[idx].TotalValue = objInput.TotalValue;
                                }
                                else
                                {
                                    objNewQte.QuoteProducts[idx].Quantity += objInput.Quantity;
                                    objNewQte.QuoteProducts[idx].TotalValue += objInput.TotalValue;
                                    objNewQte.QuoteProducts[idx].TotalTonnage += objInput.TotalTonnage;

                                    if (!String.IsNullOrEmpty(objNewQte.QuoteProducts[idx].QuoteID))
                                    {
                                        objNewQte.QuoteProducts[idx].EntityState = EntityState.Modified;
                                    }

                                    objNewQte.TotalValue += objInput.TotalValue;
                                    objNewQte.Tonnage += objInput.TotalTonnage;
                                }
                            }
                            else
                            {

                                objNewQte.TotalValue += objInput.TotalValue;
                                objNewQte.Tonnage += objInput.TotalTonnage;
                                objInput.EntityState = EntityState.Added;
                                objNewQte.QuoteProducts.Add(objInput);
                            }

                        }
                    }




                    HttpContext.Session.SetObjectAsJson("QuoteNew", objNewQte);

                    return RedirectToAction("QuoteItems");
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }


            return View(objInput);
        }
        public async Task<IActionResult> EditProduct(string productId)
        {
            var editQuoteProduct = new QuoteProduct();
            try
            {
                var objNewQte = HttpContext.Session.GetObjectFromJson<QuoteNewModel>("QuoteNew");
                if (objNewQte != null)
                {
                    if (objNewQte.QuoteProducts != null && objNewQte.QuoteProducts.Count > 0)
                    {
                        editQuoteProduct = objNewQte.QuoteProducts.Where(x => x.ProductSegID == productId).FirstOrDefault();

                        var lsSelect = new List<SelectListItem> { new SelectListItem { Text = editQuoteProduct.ProductSeg, Value = editQuoteProduct.ProductSeg + "#" + editQuoteProduct.ProductSegID } };
                        if (lsSelect.Count > 1)
                            lsSelect.Insert(0, new SelectListItem { Text = "SELECT", Value = "" });

                        editQuoteProduct.ProductList = new SelectList(lsSelect, "Value", "Text");

                        editQuoteProduct.ProductSeg = editQuoteProduct.ProductSeg + "#" + editQuoteProduct.ProductSegID;
                       
                    }
                    editQuoteProduct.Division = HttpContext.Session.GetObjectFromJson<string>("quoteDivision");
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return View(editQuoteProduct);
        }
        [HttpPost]
        public async Task<IActionResult> EditProduct(QuoteProduct objInput)
        {
            try
            {
                var objNewQte = HttpContext.Session.GetObjectFromJson<QuoteNewModel>("QuoteNew");
                if (objNewQte != null)
                {
                    if (objNewQte.QuoteProducts == null)
                    {
                        objNewQte.QuoteProducts = new List<QuoteProduct>();
                    }

                    if (objInput != null)
                    {
                        if (objInput.ProductSeg != null)
                        {
                            objInput.ProductSegID = objInput.ProductSeg.Split('#')[1];
                            objInput.ProductSeg = objInput.ProductSeg.Split('#')[0];
                            objInput.BusinessSegment = objInput.BusinessSegment;

                            var idxObj = objNewQte.QuoteProducts.Where(x => x.ProductSegID == objInput.ProductSegID).FirstOrDefault();
                            if (idxObj != null)
                            {
                                var idx = objNewQte.QuoteProducts.IndexOf(idxObj);

                                double? totVal = 0f;
                                double? totTonn = 0;

                                totVal = objInput.TotalValue - objNewQte.QuoteProducts[idx].TotalValue;
                                totTonn = objInput.TotalTonnage - objNewQte.QuoteProducts[idx].TotalTonnage;

                                // objInput.ProductSegmentId = objInput.ProductSegment;
                                objNewQte.TotalValue += totVal;
                                objNewQte.Tonnage += totTonn;

                                objNewQte.QuoteProducts[idx].Quantity = objInput.Quantity;
                                objNewQte.QuoteProducts[idx].TotalValue = objInput.TotalValue;
                                objNewQte.QuoteProducts[idx].TotalTonnage = objInput.TotalTonnage;

                                if (!String.IsNullOrEmpty(objNewQte.QuoteProducts[idx].QuoteID))
                                {
                                    objNewQte.QuoteProducts[idx].EntityState = EntityState.Modified;
                                }
                                else
                                {
                                    objNewQte.QuoteProducts[idx].EntityState = EntityState.Added;
                                }
                            }

                        }
                    }


                    // objNewEnq.strProducts = Newtonsoft.Json.JsonConvert.SerializeObject(objNewEnq.OpportunityProducts);

                    HttpContext.Session.SetObjectAsJson("QuoteNew", objNewQte);

                    return RedirectToAction("QuoteItems");
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return View(objInput);
        }

        public async Task<IActionResult> DeleteProduct(string productId)
        {
            var editQuoteProduct = new QuoteProduct();
            try
            {
                var objNewQte = HttpContext.Session.GetObjectFromJson<QuoteNewModel>("QuoteNew");
                if (objNewQte != null)
                {
                    if (objNewQte.QuoteProducts != null && objNewQte.QuoteProducts.Count > 0)
                    {
                        editQuoteProduct = objNewQte.QuoteProducts.Where(x => x.ProductSegID == productId).FirstOrDefault();

                        var idx = objNewQte.QuoteProducts.IndexOf(editQuoteProduct);

                        objNewQte.TotalValue -= objNewQte.QuoteProducts[idx].TotalValue;
                        objNewQte.Tonnage -= objNewQte.QuoteProducts[idx].TotalTonnage;

                        if (!String.IsNullOrEmpty(objNewQte.QuoteProducts[idx].QuoteID))
                        {
                            objNewQte.QuoteProducts[idx].EntityState = EntityState.Deleted;
                        }
                        else
                        {
                            objNewQte.QuoteProducts.RemoveAt(idx);
                        }


                        // objNewEnq.strProducts = Newtonsoft.Json.JsonConvert.SerializeObject(objNewEnq.OpportunityProducts);

                        HttpContext.Session.SetObjectAsJson("QuoteNew", objNewQte);

                        return RedirectToAction("QuoteItems");
                    }
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }



            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuotationByConfirmation(string qteId, string strAction)
        {
            try
            {
                string strStatus = "";

                switch (strAction.ToUpper())
                {
                    case "CANCELLED":
                        strStatus = "CANCELLED";
                        break;
                    case "DEFERRED":
                        strStatus = "DEFERRED";
                        break;
                    case "BUDGETARY":
                        strStatus = "BUDGETARY";
                        break;
                    case "LOI":
                        strStatus = "LOI";
                        break;
                    case "OPEN":
                        strStatus = "Open";
                        break;
                }

                var blRes = await _serQuote.UpdateQuote(qteId, new AGS_Quotation { Status = strStatus });
                if (blRes)
                {
                    return Json(new { status = "SUCCESS" });
                }
                else
                {
                    //alert message
                    return Json(new { status = "FAILED" });
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return Json(null);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateQuotationByStatusRemarks(string qteId, string strAction,string strRemarks, string strDate)
        {
            try
            {
                string strStatus = "";

                switch (strAction.ToUpper())
                {
                    case "CANCELLED":

                        strStatus = "CANCELLED";
                        break;
                    case "DEFERRED":
                        strStatus = "DEFERRED";
                        break;
                    case "BUDGETARY":
                        strStatus = "BUDGETARY";
                        break;
                        
                }

                AGS_Quotation aGS_Quotation = new AGS_Quotation(); 
                aGS_Quotation.Status = strStatus;
                aGS_Quotation.Remarks = strRemarks;
				try { aGS_Quotation.OfferDate = SalesStaticMethods.ConvertDate( strDate ); }
                
                
                catch(Exception ex)
				{
                    aGS_Quotation.OfferDate = DateTime.Now;

                }


                 var blRes = await _serQuote.UpdateQuote(qteId, aGS_Quotation );
                if (blRes)
                {
                    return Json(new { status = "SUCCESS" });
                }
                else
                {
                    //alert message
                    return Json(new { status = "FAILED" });
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return Json(null);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateContractValue(string qutId, double contractValue,string remarks)
        {
            try
            {
                AGS_UploadContractValue request = new AGS_UploadContractValue();
                request.DocumentId = qutId+"~"+remarks;
                request.ContractValue = contractValue;

                var blRes = await _serQuote.UploadContractValue(request);
                if (blRes)
                {
                    return Json(new { status = "SUCCESS" });
                }
                else
                {
                    //alert message
                    return Json(new { status = "FAILED" });
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }
            return Json(null);

        }


        /******************************************************************
        * ****************** Convert To Order *****************************
        ******************************************************************/

        public async Task<IActionResult> ConvertToOrder(string qteId)
        {
            try
            {
                var logMessage = $"{DateTime.Now} - {System.Reflection.MethodBase.GetCurrentMethod().Name}  - {qteId} - {qteId}";
                GenericExceptionHandler.ErrorLogWrite(logMessage);
                var objConOrder = HttpContext.Session.GetObjectFromJson<QuoteConvertOrderModel>("ConvertOrderInfo");
                if (objConOrder != null)
                {
                    return View(objConOrder);
                }
                else
                {
                    var objConvertToOrder = new QuoteConvertOrderModel();
                    var quoteDetails = await _serQuote.RetriveQuote(qteId);
                    if (quoteDetails != null)
                    {
                        objConvertToOrder.QuoteID = quoteDetails.CRMQuotationID;
                        objConvertToOrder.EnquiryID = quoteDetails.CRMOppotunityID;
                        objConvertToOrder.LeadID = quoteDetails.CRMLeadID;
                        objConvertToOrder.TotalTonnage = quoteDetails.Tonnage;
                        objConvertToOrder.TotalItemValue = quoteDetails.TotalValue;
                        objConvertToOrder.BusinessSegment = quoteDetails.BusinessSegment + "#" + quoteDetails.BusinessSegmentID;
                        objConvertToOrder.ContractValue = quoteDetails.ContractValue;
                        objConvertToOrder.Division = quoteDetails.Division;
                        
                        //objConvertToOrder.QuoteDocumentCreatedDate = quoteDetails.DocumentCreatedDate.HasValue ? quoteDetails.DocumentCreatedDate.Value.ToString("dd/MM/yyyy") : "";

                        objConvertToOrder.OrderProducts = quoteDetails.QuoteProducts.Select(x => new OrderProduct
                        {
                            BusinessSegment = x.MaterialGroup,
                            ProductSeg = x.ProductName,
                            ProductSegID = x.ProductID,
                            Quantity = x.Quantity,
                            TotalTonnage = x.TotalTonnageQuantity
                        }).ToList();

                        var entityObjs = await _serEntity.RetriveByObjectName("Order");
                        if (entityObjs != null && entityObjs.Count > 0)
                        {
                            objConvertToOrder.OrderTypeList = SalesStaticMethods.GetSelectlistItemsByName("OrderType", entityObjs, "P");
                            objConvertToOrder.ReasonsList = SalesStaticMethods.GetSelectlistItemsByName("Reason", entityObjs, "P");
                            objConvertToOrder.WonLossList = SalesStaticMethods.GetSelectlistItemsByName("Status", entityObjs, "P");
                        }

                        var lsComp = await _serCompititor.FillCompititors();
                        if (lsComp != null && lsComp.Count > 0)
                        {
                            objConvertToOrder.CompNameList = lsComp.Select(x => new SelectListItemObject { Value = x.Text, Text = x.Text }).ToList();
                            if (objConvertToOrder.CompNameList.Count > 1)
                                objConvertToOrder.CompNameList.Insert(0, new SelectListItemObject { Value = "", Text = "SELECT" });
                        }

                        HttpContext.Session.SetObjectAsJson("ConvertOrderInfo", objConvertToOrder);

                        return View(objConvertToOrder);
                    }
                }




            }
            catch (Exception ex)
            {
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ConvertToOrder(QuoteConvertOrderModel objInput)
        {
            try
            {
                var qteObj = await _serQuote.RetriveQuote(objInput.QuoteID);
                if (qteObj != null)
                {
                    var objNewOrder = new OrderProxy.AGS_Order();
                    objNewOrder.CRMQUOTATIONID = objInput.QuoteID;
                    objNewOrder.CRMOPPORTUNITYID = objInput.EnquiryID;
                    objNewOrder.CRMLeadID = objInput.LeadID;

                    objNewOrder.Division = qteObj.Division;
                    objNewOrder.DivisionName = qteObj.DivisionName;
                    objNewOrder.Region = qteObj.Region;
                    objNewOrder.RegionName = qteObj.RegionName;
                    objNewOrder.Branch = qteObj.Branch;
                    objNewOrder.BranchName = qteObj.BranchName;
                    objNewOrder.SalesOffice = qteObj.SalesOffice;
                    objNewOrder.PlantID = qteObj.PlantID;
                    objNewOrder.PlantName = qteObj.PlantName;

                    objNewOrder.AccountID = qteObj.AccountID;
                    objNewOrder.AccountName = qteObj.AccountName;
                    objNewOrder.CustomerClassification = qteObj.CustomerClassification;
                    objNewOrder.CustomerClassificationId = qteObj.CustomerClassificationId;
                    objNewOrder.CustomerSegment = qteObj.CustomerSegment;
                    objNewOrder.CustomerSegmentID = qteObj.CustomerSegmentID;
                    objNewOrder.SubSegment = qteObj.SubSegment;
                    objNewOrder.SubSegmentID = qteObj.SubSegmentID;
                    objNewOrder.CustomerType = qteObj.CustomerType;
                    objNewOrder.CustomerTypeID = qteObj.CustomerTypeID;

                    objNewOrder.BusinessSegmentID = qteObj.BusinessSegmentID;
                    objNewOrder.BusinessSegment = qteObj.BusinessSegment;
                    objNewOrder.Classification1 = qteObj.Classification1;
                    objNewOrder.Classification2 = qteObj.Classification2;
                    objNewOrder.Classification3 = qteObj.Classification3;
                    objNewOrder.Classification3ID = qteObj.Classification3ID;
                    objNewOrder.Classification4 = qteObj.Classification4;
                    objNewOrder.Consultant = qteObj.Consultant;
                    objNewOrder.Architect = qteObj.Architect;
                    // objNewOrder.DocumentCreatedDate = qteObj.DocumentCreatedDate;
                    objNewOrder.ProjectName = qteObj.ProjectName;
                    objNewOrder.Currency = qteObj.Currency;
                    objNewOrder.CurrencyValue = qteObj.CurrencyValue;


                    objNewOrder.Status = "OPEN";
                    objNewOrder.UserID = qteObj.UserID;
                    objNewOrder.Username = qteObj.Username;

                    objNewOrder.CRMLeadID = qteObj.CRMLeadID;
                    objNewOrder.CRMOPPORTUNITYID = qteObj.CRMOppotunityID;
                    objNewOrder.CRMQUOTATIONID = qteObj.CRMQuotationID;

                    objNewOrder.ProductRequired = qteObj.ProductRequired;
                    objNewOrder.TotalCost = objInput.TotalCost;
                    objNewOrder.TotalValue = qteObj.TotalValue;
                    //objNewOrder.ContractValue = qteObj.ContractValue;
                    objNewOrder.ContractValue = objInput.ContractValue;

                    if (objInput.WonLossValue.ToUpper() == "WIN")
                    {
                        objNewOrder.GrossMargin = objInput.GrossMargin;
                        objNewOrder.TurnOverValue = objInput.TurnOver;
                        objNewOrder.Tonnage = objInput.TotalTonnage;

                        objNewOrder.CompetitorPrice = null;
                        objNewOrder.CompetitorName = "";
                        objNewOrder.Reason = "";
                    }
                    else if (objInput.WonLossValue.ToUpper() == "LOST")
                    {
                        objNewOrder.CompetitorPrice = objInput.CompititorPrice;
                        objNewOrder.CompetitorName = objInput.CompName;
                        objNewOrder.Reason = objInput.Reasons;

                        objNewOrder.GrossMargin = null;
                        objNewOrder.TurnOverValue = null;
                        objNewOrder.Tonnage = null;
                    }

                    objNewOrder.PoNo = objInput.PONo;
                    objNewOrder.PoDate = SalesStaticMethods.ConvertDate(objInput.PODate);
                    // objNewOrder.DocumentCreatedDate= SalesStaticMethods.ConvertDate(objInput.OrderDocumentCreatedDate);
                    objNewOrder.OrderReferenceDate = qteObj.QuoteMaturityDate;
                    objNewOrder.QuotationDate = qteObj.QuoteMaturityDate;

                    objNewOrder.OrderType = "Branch Order";// objInput.OrderType;
                    objNewOrder.Wonlose = objInput.WonLossValue;
                    objNewOrder.CompetitorName2 = objInput.CompName2;

                    var objConOrder = HttpContext.Session.GetObjectFromJson<QuoteConvertOrderModel>("ConvertOrderInfo");
                    if (objConOrder != null && objConOrder.OrderProducts != null && objConOrder.OrderProducts.Count > 0)
                    {
                        objNewOrder.OrderProducts = objConOrder.OrderProducts.Select(x => new OrderProxy.AGS_OrderProduct
                        {
                            EntityState = OrderProxy.EntityState.Added,
                            MaterialGroup = x.BusinessSegment,
                            ProductID = x.ProductSegID,
                            ProductName = x.ProductSeg,
                            Quantity = x.Quantity,
                            TotalTonnageQuantity = x.TotalTonnage,
                            TurnoverValue = x.TurnoverValue,
                            GrossMargin = x.MarginValue,
                            ContractValue = x.ContractValue,

                        }).ToList();
                    }

                    if (objNewOrder.OrderProducts != null && objNewOrder.OrderProducts.Count > 0)
                    {
                        var strRes = await _serOrder.CreateOrder(objNewOrder);
                        if (strRes != "")
                        {
                            TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Quotation", "Quote(" + qteObj.CRMQuotationID + ") has been converted to Order (" + strRes + ") successfully"));

                            return RedirectToAction("Index");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var fullError = ex.ToString();
                var logMessage = $"{DateTime.Now} - {System.Reflection.MethodBase.GetCurrentMethod().Name}  - {ex.Message} - {ex.InnerException} - {fullError}";
                GenericExceptionHandler.ErrorLogWrite(logMessage);
                TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Quotation", ex.Message +" _ " + ex.InnerException));
                

                _logger.LogError(ex, ex?.InnerException?.InnerException?.Message);
            }
            return RedirectToAction("ConvertToOrder", new { qteId = objInput.QuoteID });
            //return RedirectToAction("ConvertToOrder");
        }

        public async Task<IActionResult> OrderItems(string BusinessSegment)//string BusinesSeg,
        {
            var itemsObject = new QuoteConvertOrderModel();
            try
            {
                itemsObject.BusinessSegment = BusinessSegment;

                var objNewEnq = HttpContext.Session.GetObjectFromJson<QuoteConvertOrderModel>("ConvertOrderInfo");
                if (objNewEnq != null)
                {
                    itemsObject.QuoteID = objNewEnq.QuoteID;
                    itemsObject.BusinessSegment = objNewEnq.BusinessSegment;

                    itemsObject.OrderTypeList = objNewEnq.OrderTypeList;
                    itemsObject.CompNameList = objNewEnq.CompNameList;
                    itemsObject.ReasonsList = objNewEnq.ReasonsList;
                    itemsObject.WonLossList = objNewEnq.WonLossList;

                    itemsObject.OrderProducts = objNewEnq.OrderProducts;
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }



            return View(itemsObject);
        }

        public async Task<IActionResult> NavigateOrderItems(QuoteConvertOrderModel objInput)
        {
            try
            {
                var objNew = HttpContext.Session.GetObjectFromJson<QuoteConvertOrderModel>("ConvertOrderInfo");
                if (objNew != null)
                {
                    objInput.QuoteID = objNew.QuoteID;
                    objInput.QuoteDocumentCreatedDate = objNew.QuoteDocumentCreatedDate;
                    objInput.BusinessSegment = objNew.BusinessSegment;

                    objInput.OrderTypeList = objNew.OrderTypeList;
                    objInput.CompNameList = objNew.CompNameList;
                    objInput.ReasonsList = objNew.ReasonsList;
                    objInput.WonLossList = objNew.WonLossList;

                    objInput.OrderProducts = objNew.OrderProducts;
                }

                HttpContext.Session.SetObjectAsJson("ConvertOrderInfo", objInput);

                string status = "FAILED";
                if (objInput.BusinessSegment != null)
                {
                    status = "SUCCESS";
                }
                return Json(new { status = status, BusinessSegment = objInput.BusinessSegment });
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return Json(null);
        }

        public async Task<IActionResult> AddOrderProduct(string BusinesSeg, string wlstatus)
        {
            var newOrderProduct = new OrderProduct();
            try
            {
                newOrderProduct.BusinessSegment = BusinesSeg.Split('#')[0];
                newOrderProduct.WonLossValue = wlstatus;
                var lsProds = await _serProduct.GetProducts();
                if (lsProds != null)
                {
                    var lsSelect = lsProds.Select(x => new SelectListItem { Text = x.Text, Value = x.Text + "#" + x.Value }).ToList();
                    if (lsSelect.Count > 1)
                        lsSelect.Insert(0, new SelectListItem { Text = "SELECT", Value = "" });

                    newOrderProduct.ProductSegmentList = new SelectList(lsSelect, "Value", "Text");
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }




            return View(newOrderProduct);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrderProduct(OrderProduct objInput)
        {
            try
            {
                var objConOrder = HttpContext.Session.GetObjectFromJson<QuoteConvertOrderModel>("ConvertOrderInfo");
                if (objConOrder != null)
                {
                    if (objConOrder.OrderProducts == null)
                    {
                        objConOrder.OrderProducts = new List<OrderProduct>();
                    }

                    if (objInput != null)
                    {
                        if (objInput.ProductSeg != null)
                        {
                            objInput.ProductSegID = objInput.ProductSeg.Split('#')[1];
                            objInput.ProductSeg = objInput.ProductSeg.Split('#')[0];
                            objInput.BusinessSegment = objInput.BusinessSegment;

                            var idxObj = objConOrder.OrderProducts.Where(x => x.ProductSegID == objInput.ProductSegID).FirstOrDefault();
                            if (idxObj != null)
                            {
                                var idx = objConOrder.OrderProducts.IndexOf(idxObj);

                                if (objConOrder.OrderProducts[idx].EntityState == OrderProxy.EntityState.Deleted && !String.IsNullOrEmpty(objConOrder.OrderProducts[idx].OrderID))
                                {

                                    objConOrder.TotalTonnage += objInput.TotalTonnage;
                                    objConOrder.OrderProducts[idx].EntityState = OrderProxy.EntityState.Modified;
                                    objConOrder.OrderProducts[idx].TotalTonnage = objInput.TotalTonnage;
                                    objConOrder.OrderProducts[idx].TurnoverValue = objInput.TurnoverValue;
                                    objConOrder.OrderProducts[idx].MarginValue = objInput.MarginValue;
                                    objConOrder.OrderProducts[idx].ContractValue = objInput.ContractValue;

                                }
                                else
                                {
                                    objConOrder.OrderProducts[idx].Quantity += objInput.Quantity;

                                    objConOrder.OrderProducts[idx].TotalTonnage += objInput.TotalTonnage;
                                    objConOrder.OrderProducts[idx].TurnoverValue += objInput.TurnoverValue;
                                    objConOrder.OrderProducts[idx].MarginValue += objInput.MarginValue;
                                    objConOrder.OrderProducts[idx].ContractValue += objInput.ContractValue;

                                    if (!String.IsNullOrEmpty(objConOrder.OrderProducts[idx].OrderID))
                                    {
                                        objConOrder.OrderProducts[idx].EntityState = OrderProxy.EntityState.Modified;
                                    }


                                    objConOrder.TotalTonnage += objInput.TotalTonnage;
                                }
                            }
                            else
                            {

                                objConOrder.TotalTonnage += objInput.TotalTonnage;
                                objConOrder.TurnOver += objInput.TurnoverValue;
                                objConOrder.GrossMargin += objInput.MarginValue;

                                objInput.EntityState = OrderProxy.EntityState.Added;
                                objConOrder.OrderProducts.Add(objInput);
                            }

                        }
                    }

                    HttpContext.Session.SetObjectAsJson("ConvertOrderInfo", objConOrder);

                    return RedirectToAction("OrderItems", new { BusinessSegment = objConOrder.BusinessSegment });
                }

            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return View(objInput);
        }
        public async Task<IActionResult> EditOrderProduct(string productId, string wlstatus)
        {
            var editOrderProduct = new OrderProduct();
            try
            {
                var objConOrder = HttpContext.Session.GetObjectFromJson<QuoteConvertOrderModel>("ConvertOrderInfo");
                if (objConOrder != null)
                {
                    if (objConOrder.OrderProducts != null && objConOrder.OrderProducts.Count > 0)
                    {
                        editOrderProduct = objConOrder.OrderProducts.Where(x => x.ProductSegID == productId).FirstOrDefault();

                        var lsSelect = new List<SelectListItem> { new SelectListItem { Text = editOrderProduct.ProductSeg, Value = editOrderProduct.ProductSeg + "#" + editOrderProduct.ProductSegID } };
                        if (lsSelect.Count > 1)
                            lsSelect.Insert(0, new SelectListItem { Text = "SELECT", Value = "" });

                        editOrderProduct.ProductSegmentList = new SelectList(lsSelect, "Value", "Text");

                        editOrderProduct.ProductSeg = editOrderProduct.ProductSeg + "#" + editOrderProduct.ProductSegID;
                    }

                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }


            return View(editOrderProduct);
        }

        [HttpPost]
        public async Task<IActionResult> EditOrderProduct(OrderProduct objInput)
        {
            try
            {
                var objConOrder = HttpContext.Session.GetObjectFromJson<QuoteConvertOrderModel>("ConvertOrderInfo");
                if (objConOrder != null)
                {
                    if (objConOrder.OrderProducts == null)
                    {
                        objConOrder.OrderProducts = new List<OrderProduct>();
                    }

                    if (objInput != null)
                    {
                        if (objInput.ProductSeg != null)
                        {
                            objInput.ProductSegID = objInput.ProductSeg.Split('#')[1];
                            objInput.ProductSeg = objInput.ProductSeg.Split('#')[0];
                            objInput.BusinessSegment = objInput.BusinessSegment;

                            var idxObj = objConOrder.OrderProducts.Where(x => x.ProductSegID == objInput.ProductSegID).FirstOrDefault();
                            if (idxObj != null)
                            {
                                var idx = objConOrder.OrderProducts.IndexOf(idxObj);

                                double? totTonn = 0f;
                                double? totTurnOver = 0f;
                                double? totGrossMargin = 0f;
                                // double? totTonn = 0f;

                                totTonn = objInput.TotalTonnage - objConOrder.OrderProducts[idx].TotalTonnage;
                                totTurnOver = objInput.TurnoverValue - (objConOrder.OrderProducts[idx].TurnoverValue.HasValue ? objConOrder.OrderProducts[idx].TurnoverValue.Value : 0);
                                totGrossMargin = objInput.MarginValue - (objConOrder.OrderProducts[idx].MarginValue.HasValue ? objConOrder.OrderProducts[idx].MarginValue.Value : 0);

                                // objInput.ProductSegmentId = objInput.ProductSegment;
                                objConOrder.TotalTonnage = (objConOrder.TotalTonnage.HasValue ? objConOrder.TotalTonnage.Value : 0) + totTonn;
                                objConOrder.TurnOver = (objConOrder.TurnOver.HasValue ? objConOrder.TurnOver.Value : 0) + totTurnOver;
                                objConOrder.GrossMargin = (objConOrder.GrossMargin.HasValue ? objConOrder.GrossMargin.Value : 0) + totGrossMargin;

                                objConOrder.OrderProducts[idx].Quantity = objInput.Quantity;
                                objConOrder.OrderProducts[idx].TurnoverValue = objInput.TurnoverValue;
                                objConOrder.OrderProducts[idx].MarginValue = objInput.MarginValue;
                                objConOrder.OrderProducts[idx].TotalTonnage = objInput.TotalTonnage;
                                objConOrder.OrderProducts[idx].ContractValue = objInput.ContractValue;

                                if (!String.IsNullOrEmpty(objConOrder.OrderProducts[idx].OrderID))
                                {
                                    objConOrder.OrderProducts[idx].EntityState = OrderProxy.EntityState.Modified;
                                }
                                else
                                {
                                    objConOrder.OrderProducts[idx].EntityState = OrderProxy.EntityState.Added;
                                }
                            }

                        }
                    }

                    HttpContext.Session.SetObjectAsJson("ConvertOrderInfo", objConOrder);

                    return RedirectToAction("OrderItems", new { BusinessSegment = objConOrder.BusinessSegment });
                }

            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }


            return View(objInput);
        }

        public async Task<IActionResult> DeleteOrderProduct(string productId)
        {
            var editOrderProduct = new OrderProduct();
            try
            {
                var objNewQte = HttpContext.Session.GetObjectFromJson<QuoteConvertOrderModel>("ConvertOrderInfo");
                if (objNewQte != null)
                {
                    if (objNewQte.OrderProducts != null && objNewQte.OrderProducts.Count > 0)
                    {
                        editOrderProduct = objNewQte.OrderProducts.Where(x => x.ProductSegID == productId).FirstOrDefault();

                        var idx = objNewQte.OrderProducts.IndexOf(editOrderProduct);

                        objNewQte.TotalTonnage -= objNewQte.OrderProducts[idx].TotalTonnage;
                        objNewQte.TurnOver -= objNewQte.OrderProducts[idx].TurnoverValue;
                        objNewQte.GrossMargin -= objNewQte.OrderProducts[idx].MarginValue;

                        if (!String.IsNullOrEmpty(objNewQte.OrderProducts[idx].OrderID))
                        {
                            objNewQte.OrderProducts[idx].EntityState = OrderProxy.EntityState.Deleted;
                        }
                        else
                        {
                            objNewQte.OrderProducts.RemoveAt(idx);
                        }

                        HttpContext.Session.SetObjectAsJson("ConvertOrderInfo", objNewQte);

                        return RedirectToAction("ConvertToOrder");
                    }
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }



            return View();
        }

        //------------------------------------

        [HttpPost]
        public async Task<IActionResult> GetBranchs(string strOrgId, string strDivision)
        {
            try
            {
                var lsPlans = await _serLocation.RetreiveLocationDetailsByOrg(strOrgId, strDivision);
                if (lsPlans != null)
                {
                    var objPlantsInfo = new PlantSaleOfficeDetails();
                    objPlantsInfo.Plants = lsPlans.Branches.Select(x => new PlantSaleOfficeDetails.plantInfo { code = x.ID, name = x.Name }).ToList();
                    return Json(objPlantsInfo);
                }
            }
            catch (TimeoutException tex)
            {

            }
            catch (Exception ex)
            {

            }
            return Json(null);
        }

        [HttpPost]
        public async Task<IActionResult> GetSalesOfficePlants(string strOrgId, string strDivision)
        {
            try
            {
                var lsPlans = await _serLocation.RetreiveLocationDetailsByOrg(strOrgId, strDivision);
                if (lsPlans != null)
                {
                    var objPlantsInfo = new PlantSaleOfficeDetails();
                    objPlantsInfo.Plants = lsPlans.Plants.Select(x => new PlantSaleOfficeDetails.plantInfo { code = x.Code, name = x.Name }).ToList();
                    if (lsPlans.SalesOffices != null && lsPlans.SalesOffices.Count > 0)
                    {
                        objPlantsInfo.salesoffice = lsPlans.SalesOffices[0].Code;
                        objPlantsInfo.salesofficeName = lsPlans.SalesOffices[0].Name;
                    }
                    return Json(objPlantsInfo);
                }
            }
            catch (TimeoutException tex)
            {

            }
            catch (Exception ex)
            {

            }
            return Json(null);
        }

        [HttpPost]
        public async Task<IActionResult> CustomerAccount(string accId)
        {
            try
            {
                var lsAcc = await _serCustomer.GetCustomers(accId);

                //if (lsAcc != null && lsAcc.Count > 0)
                //{
                return Json(lsAcc);


                //  }
            }
            catch (TimeoutException tex)
            {

            }
            catch (Exception ex)
            {
            }

            return Json(null);
        }

        [HttpPost]
        public async Task<IActionResult> Customers(string strKey, string branch)
        {
            try
            {
                if (branch.Length > 0)
                {
                    var lsAcc = await _serCustomer.GetCustomersByBranch(strKey, branch.Split('#')[1]);

                    //if (lsAcc != null && lsAcc.Count > 0)
                    //{
                    return Json(lsAcc);
                    //  }
                }
            }
            catch (TimeoutException tex)
            {

            }
            catch (Exception ex)
            {
            }

            return Json(null);
        }

        [HttpPost]
        public async Task<IActionResult> SubSegment(string strKey,bool isDirect)
        {
            try
            {
                string Entity = "Lead";
                if (isDirect)
                {
                    Entity = "ENQUIRY";
                }

                var lsSubseg = await _serEntity.RetriveByParentId(Entity, "CUSTOMERSUBSEGMENT", strKey);
                if (lsSubseg != null && lsSubseg.Count > 0)
                {
                    return Json(lsSubseg.Select(y => new DropdownObject { Value = y.PropertyName + "#" + y.PropertyValue, Text = y.PropertyName }).ToList());
                }
            }
            catch (TimeoutException tex)
            {

            }
            catch (Exception ex)
            {
            }
            return Json(null);
        }

        [HttpPost]
        public async Task<IActionResult> AssignedToUsers(string strDivision, string strBranch)
        {
            try
            {
                var lsUsers = await _serUser.GetAssignedUsers(strDivision, strBranch);
                if (lsUsers != null && lsUsers.Count > 0)
                {
                    return Json(lsUsers);
                }
            }
            catch (TimeoutException tex)
            {

            }
            catch (Exception ex)
            {

            }
            return Json(null);
        }

        public async Task<IActionResult> RejectionHistory(string qteId, string status)
        {
            var objRHistory = new QuoteHistoryModel();
            try
            {
                objRHistory.QuoteID = qteId;
                objRHistory.Status = status;
                objRHistory.HistoryRecords = await _serQuote.QuotationHistory(qteId);

            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }


            return View(objRHistory);


        }

        #endregion

        /******************************************************************
     * ****************** Activities **********************************
     ******************************************************************/

        #region Activities
        public async Task<IActionResult> ActivityIndex(string qteId, string custname)
        {
            var objConPlan = new ContactPlanViewModel();
            try
            {

                objConPlan = await _serActivity.GetAllActivitiesByDocumentID(qteId);

                if (objConPlan != null)
                {
                    objConPlan.RefObjectID = qteId;
                    objConPlan.CustomerName = custname;
                    return View(objConPlan);
                }

            }
            catch (TimeoutException tex) { }
            catch (Exception)
            {


            }
            return View(objConPlan);
        }
        public async Task<IActionResult> ActivityCreate(string qteId, string custname)
        {
            var objContPlan = new ContactPlanNewModel();
            try
            {
                objContPlan = await _serActivity.ActvityCreateObject(qteId, custname);

            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return View(objContPlan);
        }

        [HttpPost]
        public async Task<IActionResult> ActivityCreate(ContactPlanNewModel objInput)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var strResult = await _serActivity.ActivityCreate(objInput);
                    if (strResult != "")
                    {
                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Quote Activity", "Activity (" + strResult + ") has been created successfully"));

                        return RedirectToAction("ActivityIndex", new { qteId = objInput.DocumentID, custname = objInput.Name });
                    }

                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }


            return View(objInput);
        }

        public async Task<IActionResult> ActivityEdit(string activityId, string qteId)
        {
            var objQteAct = new ContactPlanNewModel();
            try
            {
                objQteAct = await _serActivity.ActvityEditObject(activityId);
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }


            return View(objQteAct);
        }

        [HttpPost]
        public async Task<IActionResult> ActivityEdit(ContactPlanNewModel objInput)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var blRes = await _serActivity.ActivityEdit(objInput);
                    if (blRes)
                    {


                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Quote Activity", "Activity (" + objInput.ActivityId + ") has been updated successfully"));

                        return RedirectToAction("ActivityDetails", "Quote", new { activityId = objInput.ActivityId, qteId = objInput.DocumentID });
                    }
                }
                else
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Quote Activity", "Please provide all Values"));

                    // return RedirectToAction("ActivityDetails", "Lead", new { activityId = objInput.ActivityId });
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }



            return View(objInput);

        }

        public async Task<IActionResult> ActivityAttachments(string activityId, string qteId, string custname)
        {
            var objAtt = new AttachmentsModel();
            try
            {
                objAtt = await _serAttachment.GetAttachmentModel("ACTIVITY", activityId);
                objAtt.RefObjectId = qteId;
                objAtt.CustomerName = custname;

            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return View(objAtt);
        }
        [HttpPost]
        public async Task<IActionResult> ActivityAttachments(AttachmentsModel objInput)
        {
            try
            {
                var jsObject = Newtonsoft.Json.JsonConvert.DeserializeObject<UploadFile>(objInput.PendingUploadFiles);
                if (_serAttachment.VarifyJsonFileObject(jsObject))
                {
                    var lsFilesUploaded = await _serAttachment.UploadAttachments("ACTIVITY", objInput.ActivityId, jsObject);

                    if (lsFilesUploaded.Count > 0)
                    {


                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Quote Activity", "Attachments uploaded successfully"));

                        return RedirectToAction("ActivityIndex", new { qteId = objInput.RefObjectId, custname = objInput.CustomerName });
                    }
                }
                else
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Quote Activity", "Please add atleast one document"));

                    return RedirectToAction("ActivityAttachments", new { activityId = objInput.ActivityId, leadId = objInput.RefObjectId, custname = objInput.CustomerName });
                }



            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }


            return View(objInput);
        }
        public async Task<IActionResult> DeleteActivityAttachment(int Id, string activityId, string qteId, string custname)
        {
            try
            {
                var res = await _serAttachment.DeleteAttachment(Id);
            }
            catch (TimeoutException tex)
            {

            }
            catch (Exception ex)
            {

            }
            return RedirectToAction("ActivityAttachments", new { activityId = activityId, qteId = qteId, custname = custname });
        }
        public async Task<IActionResult> ActivityDetails(string activityId, string qteId)
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

        public async Task<IActionResult> RescheduleActivity(string activityId, string strNewVisitDate)
        {
            var blRes = false;
            try
            {
                blRes = await _serActivity.RescheduleActivity(activityId, strNewVisitDate);
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }


            return Json(new JsonResultObject { Status = blRes });
        }
        public async Task<IActionResult> CloseActivity(string activityId, string strOutcome)
        {
            var blRes = false;
            try
            {
                blRes = await _serActivity.CloseActivity(activityId, strOutcome);
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return Json(new JsonResultObject { Status = blRes });
        }

        #endregion

    }
}
