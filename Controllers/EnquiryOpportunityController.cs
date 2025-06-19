using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using DPGSalesClient.Models;
using ActivityProxy;
using Microsoft.AspNetCore.Mvc.Rendering;
using OpportunityProxy;
using SAPEnquiryProxy;
using AuthorizationProxy;
using Microsoft.Extensions.Options;


// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace DPGSalesClient.Controllers
{
    public class EnquiryOpportunityController : Controller
    {
        #region Varaibles
        ServiceConnectors.OpportunityServiceConnector _serOpportunity = null;
        ServiceConnectors.LocationServiceConnector _serLocation = null;
        ServiceConnectors.EntityMapServiceConnector _serEntity = null;
        ServiceConnectors.UserServiceConnector _serUser = null;
        ServiceConnectors.ProductServiceConnector _serProduct = null;
        ServiceConnectors.SAPEnquiryServiceConnector _serSAPEnquiry = null;

        Models.BusinessLogic.ActivityBL _serActivity = null;
        Models.BusinessLogic.AttachmentBL _serAttachment = null;
        Models.BusinessLogic.CustomerBL _serCustomer = null;

        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;
        #endregion

        #region Constructor
        public EnquiryOpportunityController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor)
        {
            string strIp = SalesStaticMethods.GetRemoteIp(appSettings);

            _serOpportunity = new ServiceConnectors.OpportunityServiceConnector(strIp, httpContextAccessor);
            _serLocation = new ServiceConnectors.LocationServiceConnector(strIp, httpContextAccessor);
            _serEntity = new ServiceConnectors.EntityMapServiceConnector(strIp, httpContextAccessor);
            _serUser = new ServiceConnectors.UserServiceConnector(strIp, httpContextAccessor);
            _serProduct = new ServiceConnectors.ProductServiceConnector(strIp, httpContextAccessor);
            _serSAPEnquiry = new ServiceConnectors.SAPEnquiryServiceConnector(strIp, httpContextAccessor);

            _serActivity = new Models.BusinessLogic.ActivityBL(strIp, httpContextAccessor);
            _serAttachment = new Models.BusinessLogic.AttachmentBL(strIp, httpContextAccessor);
            _serCustomer = new Models.BusinessLogic.CustomerBL(strIp, httpContextAccessor);

            _logger = loggerFactory.CreateLogger<EnquiryOpportunityController>();
            _hostingEnvironment = hostingEnvironment;
        }

        #endregion

        /******************************************************************
         * ****************** ENQUIRY **********************************
         ******************************************************************/

        #region Enquiry

        public async Task<IActionResult> Index()
        {
            HttpContext.Session.SetString("Controller", "EnquiryOpportunity");
            var objData = new EnquiryViewModel();
            try
            {
                if (HttpContext.Session.CheckSession("NavigationList"))
                {
                    AGS_LoginUserInfo logInfo = HttpContext.Session.GetObjectFromJson<AGS_LoginUserInfo>( "LoginUserInfo" );
                    HttpContext.Session.SetString( "UserRoleId", logInfo.RoleInfo[ 0 ].RoleID );
                    ViewBag.RoleId = logInfo.RoleInfo[ 0 ].RoleID;
                    var lsEnquiry = await _serOpportunity.RetriveOpportunities("", "", 0, "", 0);

                    if (lsEnquiry != null)
                    {
                        objData.EnquiryList = lsEnquiry.Select(y => new EnquiryViewModel.EnquiryViewItemModel
                        {
                            EnquiryID = y.CRMOpportunityID,
                            BusineeSegment = y.BusinessSegment,
                            CustomerName = y.AccountName,
                            ProjectName = y.ProjectName,
                            Status = y.Status,
                            CreatedOn = y.CreatedOn,
                            LeadID = y.CRMLEADID,
                            Division = y.DivisionName,
                            Branch = y.BranchName,
                            Probablity = y.Probability,
                            ContractValue = y.ContractValue,
                            SAPEnquiryID = y.SAPOpportunityID

                        }).ToList();

                        return View("Index", objData);
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Auth");
                }

            }
            catch (Exception ex)
            {


            }

            return View("Index", objData);
        }

        public async Task<IActionResult> Edit(string enqId, string stBack)
        {
            try
            {
                if (stBack == "BACK")
                {
                    var objNewEnq = HttpContext.Session.GetObjectFromJson<EnquiryNewModel>("EnquiryNew");
                    if (objNewEnq != null)
                    {
                        if (objNewEnq.Branch != null)
                        {
                            var lsPlans = await _serLocation.RetreiveLocationDetailsByOrg(objNewEnq.Region.Split('#')[1], objNewEnq.Division.Split('#')[1]);
                            if (lsPlans != null && lsPlans.Branches != null && lsPlans.Branches.Count > 0)
                            {
                                objNewEnq.BranchList = lsPlans.Branches.Select(x => new SelectListItemObject { Value = x.Name + "#" + x.ID, Text = x.Name }).ToList();
                            }
                        }
                        if (objNewEnq.Plant != null)
                        {
                            var lsPlans = await _serLocation.RetreiveLocationDetailsByOrg(objNewEnq.Branch.Split('#')[1], objNewEnq.Division.Split('#')[1]);
                            if (lsPlans != null && lsPlans.Plants != null && lsPlans.Plants.Count > 0)
                            {
                                objNewEnq.PlantsList = lsPlans.Plants.Select(x => new SelectListItemObject { Value = x.Name + "#" + x.Code, Text = x.Name }).ToList();
                            }
                        }
                        if (objNewEnq.AssignedToUser != null)
                        {
                            var lsUsers = await _serUser.GetAssignedUsers(objNewEnq.Division.Split('#')[1], objNewEnq.Branch.Split('#')[1]);
                            if (lsUsers != null)
                            {
                                objNewEnq.EnquiryAssignToList = lsUsers.Select(x => new SelectListItemObject { Value = x.Text + "#" + x.Value, Text = x.Text }).ToList();
                                if (objNewEnq.EnquiryAssignToList.Count > 1)
                                {
                                    objNewEnq.EnquiryAssignToList.Insert(0, new SelectListItemObject { Value = "", Text = "SELECT" });
                                }
                            }
                        }
                        if (objNewEnq.CustomerSubSegment != null)
                        {
                            var lsSubseg = await _serEntity.RetriveByParentId("Lead", "CUSTOMERSUBSEGMENT", objNewEnq.CustomerSegment.Split('#')[0]);
                            if (lsSubseg != null)
                            {
                                objNewEnq.CustomerSubSegmentList = lsSubseg.Select(x => new SelectListItemObject { Value = x.PropertyName + "#" + x.PropertyValue, Text = x.PropertyName }).ToList();
                                if (objNewEnq.CustomerSubSegmentList.Count > 1)
                                {
                                    objNewEnq.CustomerSubSegmentList.Insert(0, new SelectListItemObject { Value = "", Text = "SELECT" });
                                }
                            }
                        }

                        HttpContext.Session.SetObjectAsJson("EnquiryNew", objNewEnq);
                        return View(objNewEnq);
                    }
                }
                else
                {

                    var objNew = new EnquiryNewModel();
                    var enqEdit = await _serOpportunity.RetriveOpportunity(enqId);
                    if (enqId != null)
                    {
                        objNew.EnquiryID = enqEdit.CRMOpportunityID;
                        objNew.Division = enqEdit.DivisionName + "#" + enqEdit.Division;
                        HttpContext.Session.SetObjectAsJson("enquiryDivision", objNew.Division);

                        objNew.Region = enqEdit.RegionName + "#" + enqEdit.Region;
                        objNew.Branch = enqEdit.BranchName + "#" + enqEdit.Branch;
                        objNew.SalesOffice = enqEdit.SalesOffice;
                        objNew.Plant = enqEdit.PlantName + "#" + enqEdit.PlantID;

                        objNew.CustomerCode = enqEdit.AccountID;
                        objNew.CustomerName = enqEdit.AccountName;

                        objNew.CustomerSegment = enqEdit.CustomerSegment + "#" + enqEdit.CustomerSegmentID;
                        objNew.CustomerSubSegment = enqEdit.SubSegment + "#" + enqEdit.SubSegmentID;
                        objNew.CustomerType = enqEdit.CustomerType + "#" + enqEdit.CustomerTypeID;
                        objNew.CustomerClassification = enqEdit.CustomerClassification + "#" + enqEdit.CustomerClassificationId;

                        objNew.Probability = enqEdit.Probability;
                        objNew.ContractValue_IN_LAKHS = enqEdit.ContractValue;
                        objNew.Status = enqEdit.Status;
                        objNew.Tonnage = enqEdit.Tonnage;
                        objNew.TotalValue = enqEdit.TotalValue;
                        objNew.ProjectName = enqEdit.ProjectName;
                        objNew.Architect = enqEdit.Architect;
                        objNew.Consultant = enqEdit.Consultant;
                        objNew.EnquiryDescription = enqEdit.Description;
                        //  objNew.DocumentCreatedDate = enqEdit.DocumentCreatedDate.HasValue ? enqEdit.DocumentCreatedDate.Value.ToString("dd/MM/yyyy") : "";
                        objNew.EnquiryMaturityDate = enqEdit.EnquiryMaturityDate.HasValue ? enqEdit.EnquiryMaturityDate.Value.ToString("dd/MM/yyyy") : "";
                        //  objNew.EnquiryValidityDate = enqEdit.EnquiryValidityDate.HasValue ? enqEdit.EnquiryValidityDate.Value.ToString("dd/MM/yyyy") : "";
                        objNew.Probability = enqEdit.Probability;
                        objNew.ContractValue_IN_LAKHS = enqEdit.ContractValue;
                        objNew.BusinessSegment = enqEdit.BusinessSegment + "#" + enqEdit.BusinessSegmentID;
                        objNew.ProdcutRequired = enqEdit.ProductRequired;
                        objNew.SourceType = enqEdit.SourceType;
                        objNew.Classification1 = enqEdit.Classification1;
                        objNew.Classification2 = enqEdit.Classification2;
                        objNew.Classification3 = enqEdit.Classification3 + "#" + enqEdit.Classification3ID;
                        objNew.Classification4 = enqEdit.Classification4;
                        objNew.AssignedToUser = enqEdit.Username + "#" + enqEdit.UserID;
                        objNew.Currency = enqEdit.Currency;
                        objNew.CurrencyValue = enqEdit.CurrencyValue;

                        objNew.City = enqEdit.City;
                        objNew.State = enqEdit.State;
                        objNew.MobileNumber = enqEdit.MobileNumber;
                        objNew.Pincode = enqEdit.Pincode;
                        objNew.ContactPerson = enqEdit.ContactName;
                        objNew.CustomerAddress = enqEdit.Address1;
                        #region Account details
                        //var objAcc = await _serCustomer.GetCustomers(enqEdit.AccountName);
                        //if (objAcc != null && objAcc.Count>0)
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
                            var lsBranches = await _serLocation.RetreiveLocationDetailsByOrg(enqEdit.Region, enqEdit.Division);
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
                                var lsPlans = await _serLocation.RetreiveLocationDetailsByOrg(enqEdit.Branch, enqEdit.Division);
                                if (lsPlans != null)
                                {
                                    objNew.PlantsList = lsPlans.Plants.Select(x => new SelectListItemObject { Value = x.Name + "#" + x.Code, Text = x.Name }).ToList();
                                }
                            }
                            if (objNew.AssignedToUser != null)
                            {
                                var lsUsers = await _serUser.GetAssignedUsers(enqEdit.Division, enqEdit.Branch);
                                if (lsUsers != null)
                                {
                                    objNew.EnquiryAssignToList = lsUsers.Select(x => new SelectListItemObject { Value = x.Text + "#" + x.Value, Text = x.Text }).ToList();
                                    if (objNew.EnquiryAssignToList.Count > 1)
                                    {
                                        objNew.EnquiryAssignToList.Insert(0, new SelectListItemObject { Value = "", Text = "SELECT" });
                                    }
                                }
                            }


                        }
                        #endregion

                        #region EntityMap Details


                        var lsEntity = await _serEntity.RetriveByObjectName("LEAD");
                        if (lsEntity != null && lsEntity.Count > 0)
                        {
                            objNew.CustomerSegmentList = SalesStaticMethods.GetSelectlistItemsByName("CUSTOMERSEGMENT", lsEntity, "B");
                            objNew.CustomerClassificationList = SalesStaticMethods.GetSelectlistItemsByName("CUSTOMERCLASSIFICATION", lsEntity, "B");
                            objNew.CustomerTypeList = SalesStaticMethods.GetSelectlistItemsByName("CUSTOMERTYPE", lsEntity, "B");
                            objNew.BusinessSegmentList = SalesStaticMethods.GetSelectlistItemsByName("BUSINESSSEGMENT", lsEntity, "B");
                            objNew.ProdcutRequiredList = SalesStaticMethods.GetSelectlistItemsByName("PRODUCTREQUIRED", lsEntity, "P");
                            objNew.ProbabilityList = SalesStaticMethods.GetSelectlistItemsByName("PROBABILITY", lsEntity, "P");
                            objNew.CurrencyList = SalesStaticMethods.GetSelectlistItemsByName("CURRENCY", lsEntity, "P");

                            objNew.Classification1List = SalesStaticMethods.GetSelectlistItemsByName("CLASSIFICATION1", lsEntity, "P");
                            objNew.Classification2List = SalesStaticMethods.GetSelectlistItemsByName("CLASSIFICATION2", lsEntity, "P");
                            objNew.Classification3List = SalesStaticMethods.GetSelectlistItemsByName("CLASSIFICATION3", lsEntity, "B");
                            objNew.Classification4List = SalesStaticMethods.GetSelectlistItemsByName("CLASSIFICATION4", lsEntity, "P");
                            objNew.SourceTypeList = SalesStaticMethods.GetSelectlistItemsByName("SOURCETYPE", lsEntity, "P");

                        }

                        if (objNew.CustomerSubSegment != null)
                        {
                            var lsSubseg = await _serEntity.RetriveByParentId("Lead", "CUSTOMERSUBSEGMENT", enqEdit.CustomerSegment);
                            if (lsSubseg != null)
                            {
                                objNew.CustomerSubSegmentList = lsSubseg.Select(x => new SelectListItemObject { Value = x.PropertyName + "#" + x.PropertyValue, Text = x.PropertyName }).ToList();
                                if (objNew.CustomerSubSegmentList.Count > 1)

                                    objNew.CustomerSubSegmentList.Insert(0, new SelectListItemObject { Value = "", Text = "SELECT" });
                            }
                        }
                        #endregion


                        if (enqEdit.OpportunityProducts != null && enqEdit.OpportunityProducts.Count > 0)
                        {
                            objNew.OpportunityProducts = enqEdit.OpportunityProducts.Select(x => new EnquiryProduct
                            {
                                BusinessSegment = enqEdit.BusinessSegment,
                                EnquiryID = enqEdit.CRMOpportunityID,
                                ProductSegID = x.ProductID,
                                ProductSeg = x.ProductName,
                                Quantity = x.Quantity,
                                TotalTonnage = x.TotalTonnageQuantity,
                                TotalValue = x.TotalValue
                            }).ToList();
                        }


                        HttpContext.Session.SetObjectAsJson("EnquiryNew", objNew);
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
        public async Task<IActionResult> Edit(EnquiryNewModel objInput)
        {
            try
            {
                var objNewEnq = HttpContext.Session.GetObjectFromJson<EnquiryNewModel>("EnquiryNew");
                if (objNewEnq != null)
                {
                    if (objNewEnq.OpportunityProducts != null && objNewEnq.OpportunityProducts.Count > 0)
                    {
                        AGS_Opportunity objNewEnquiry = new AGS_Opportunity();

                        objNewEnquiry.Division = objInput.Division.Split('#')[1];
                        objNewEnquiry.DivisionName = objInput.Division.Split('#')[0];
                        objNewEnquiry.Region = objInput.Region.Split('#')[1];
                        objNewEnquiry.RegionName = objInput.Region.Split('#')[0];
                        objNewEnquiry.Branch = objInput.Branch.Split('#')[1];
                        objNewEnquiry.BranchName = objInput.Branch.Split('#')[0];
                        objNewEnquiry.SalesOffice = objInput.SalesOffice;
                        objNewEnquiry.PlantID = objInput.Plant.Split('#')[1];
                        objNewEnquiry.PlantName = objInput.Plant.Split('#')[0];


                        objNewEnquiry.AccountID = objInput.CustomerCode;
                        objNewEnquiry.AccountName = objInput.CustomerName;
                        objNewEnquiry.City = objInput.City;
                        objNewEnquiry.State = objInput.State;
                        objNewEnquiry.MobileNumber = objInput.MobileNumber;
                        objNewEnquiry.Pincode = objInput.Pincode;
                        objNewEnquiry.ContactName = objInput.ContactPerson;
                        objNewEnquiry.Address1 = objInput.CustomerAddress;

                        objNewEnquiry.CustomerSegment = objInput.CustomerSegment.Split('#')[0];
                        objNewEnquiry.CustomerSegmentID = objInput.CustomerSegment.Split('#')[1];
                        objNewEnquiry.SubSegment = objInput.CustomerSubSegment.Split('#')[0];
                        objNewEnquiry.SubSegmentID = objInput.CustomerSubSegment.Split('#')[1];
                        objNewEnquiry.CustomerTypeID = objInput.CustomerType.Split('#')[1];
                        objNewEnquiry.CustomerType = objInput.CustomerType.Split('#')[0];
                        objNewEnquiry.CustomerClassification = objInput.CustomerClassification.Split('#')[0];
                        objNewEnquiry.CustomerClassificationId = objInput.CustomerClassification.Split('#')[1];

                        objNewEnquiry.BusinessSegmentID = objInput.BusinessSegment.Split('#')[1];
                        objNewEnquiry.BusinessSegment = objInput.BusinessSegment.Split('#')[0];
                        objNewEnquiry.Classification1 = objInput.Classification1;
                        objNewEnquiry.Classification2 = objInput.Classification2;
                        objNewEnquiry.Classification3 = objInput.Classification3.Split('#')[0];
                        objNewEnquiry.Classification3ID = objInput.Classification3.Split('#')[1];
                        objNewEnquiry.Classification4 = objInput.Classification4;
                        objNewEnquiry.UserID = objInput.AssignedToUser.Split('#')[1];
                        objNewEnquiry.Username = objInput.AssignedToUser.Split('#')[0];
                        objNewEnquiry.Architect = objInput.Architect;
                        objNewEnquiry.Consultant = objInput.Consultant;
                        objNewEnquiry.SourceType = objInput.SourceType;
                        objNewEnquiry.ProjectName = objInput.ProjectName;
                        objNewEnquiry.ProductRequired = objInput.ProdcutRequired;
                        objNewEnquiry.ContractValue = objInput.ContractValue_IN_LAKHS;
                        objNewEnquiry.Probability = objInput.Probability;
                        objNewEnquiry.Currency = objInput.Currency;
                        objNewEnquiry.CurrencyValue = objInput.CurrencyValue;

                        //Enquiry details
                        objNewEnquiry.EnquiryMaturityDate = SalesStaticMethods.ConvertDate(objInput.EnquiryMaturityDate);
                        // objNewEnquiry.EnquiryValidityDate = SalesStaticMethods.ConvertDate(objInput.EnquiryValidityDate);
                        // objNewEnquiry.DocumentCreatedDate = SalesStaticMethods.ConvertDate(objInput.DocumentCreatedDate);
                        objNewEnquiry.Tonnage = objInput.Tonnage;
                        objNewEnquiry.TotalValue = objInput.TotalValue;
                        objNewEnquiry.Description = objInput.EnquiryDescription;
                        objNewEnquiry.Status = objInput.Status;

                        objNewEnquiry.OpportunityProducts = new List<OpportunityProxy.AGS_OpportunityProduct>();
                        if (objNewEnq.OpportunityProducts.Count > 0)
                        {
                            var lsProducts = objNewEnq.OpportunityProducts.Select(x => new OpportunityProxy.AGS_OpportunityProduct
                            {
                                MaterialGroup = x.BusinessSegment,
                                ProductID = x.ProductSegID,
                                ProductName = x.ProductSeg,
                                Quantity = x.Quantity,
                                TotalValue = x.TotalValue,
                                EntityState = x.EntityState,
                                TotalTonnageQuantity = x.TotalTonnage
                            }).ToList();


                            objNewEnquiry.OpportunityProducts = lsProducts;
                        }

                        if (objNewEnquiry.OpportunityProducts.Count > 0)
                        {
                            var blRes = await _serOpportunity.UpdateOpportunity(objNewEnq.EnquiryID, objNewEnquiry);
                            if (blRes)
                            {
                                TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Enquiry", "Enquiry(" + objNewEnq.EnquiryID + ") has been update successfully"));

                                return RedirectToAction("Details", new { enqId = objNewEnq.EnquiryID, status = "OPEN" });
                            }
                        }
                        HttpContext.Session.SetString("enquiryDivision", "");
                        HttpContext.Session.SetString("EnquiryNew", "");                        
                    }
                    else
                    {

                        objInput.DivisionList = objNewEnq.DivisionList;
                        objInput.RegionList = objNewEnq.RegionList;
                        objInput.BranchList = objNewEnq.BranchList;
                        objInput.PlantsList = objNewEnq.PlantsList;
                        objInput.CustomerSegmentList = objNewEnq.CustomerSegmentList;
                        objInput.CustomerSubSegmentList = objNewEnq.CustomerSubSegmentList;
                        objInput.CustomerTypeList = objNewEnq.CustomerTypeList;
                        objInput.CustomerClassificationList = objNewEnq.CustomerClassificationList;
                        objInput.ProbabilityList = objNewEnq.ProbabilityList;
                        objInput.ProdcutRequiredList = objNewEnq.ProdcutRequiredList;
                        objInput.SourceTypeList = objNewEnq.SourceTypeList;
                        objInput.EnquiryAssignToList = objNewEnq.EnquiryAssignToList;
                        objInput.Classification1List = objNewEnq.Classification1List;
                        objInput.Classification2List = objNewEnq.Classification2List;
                        objInput.Classification3List = objNewEnq.Classification3List;
                        objInput.Classification4List = objNewEnq.Classification4List;
                        objInput.BusinessSegmentList = objNewEnq.BusinessSegmentList;
                        objInput.OpportunityProducts = objNewEnq.OpportunityProducts;
                        objInput.CurrencyList = objNewEnq.CurrencyList;

                        HttpContext.Session.SetObjectAsJson("EnquiryNew", objInput);


                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Enquiry", "You should provide at least one product"));

                        return RedirectToAction("Edit", new { enqId = objNewEnq.EnquiryID, stBack = "BACK" });
                    }
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }
            return View(objInput);
        }

        public async Task<IActionResult> Details(string enqId, string status)
        {
            var objDetails = new EnquiryDetailsModel();
            try
            {
                string strRoleID = "";
                if (HttpContext.Session.CheckSession("LoginUserInfo"))
                {
                    var userInfo = HttpContext.Session.GetObjectFromJson<AGS_LoginUserInfo>("LoginUserInfo");
                    if (userInfo != null && userInfo.RoleInfo != null && userInfo.RoleInfo.Count > 0)
                        strRoleID = userInfo.RoleInfo[0].RoleID;
                }
                var lsFiles = await _serAttachment.GetAttachments("OPPORTUNITY", enqId);


                var enqDetails = await _serOpportunity.RetriveOpportunity(enqId);
                if (enqDetails != null)
                {
                    objDetails = new EnquiryDetailsModel
                    {
                        EnquiryID = enqDetails.CRMOpportunityID,
                        SAPEnquiryID = enqDetails.SAPOpportunityID,
                        Status = enqDetails.Status,
                        Division = enqDetails.DivisionName,
                        Region = enqDetails.RegionName,
                        Branch = enqDetails.BranchName,
                        SalesOffice = enqDetails.SalesOffice,
                        Plant = enqDetails.PlantName,
                        CustomerCode = enqDetails.AccountID,
                        CustomerName = enqDetails.AccountName,
                        BusinessSegment = enqDetails.BusinessSegment,
                        ProdcutRequired = enqDetails.ProductRequired,
                        ProjectName = enqDetails.ProjectName,
                        Architect = enqDetails.Architect,
                        Consultant = enqDetails.Consultant,
                        ContractValue_IN_LAKHS = enqDetails.ContractValue,
                        Probability = enqDetails.Probability,
                        LeadID = enqDetails.CRMLEADID,
                        Tonnage = enqDetails.Tonnage,
                        TotalValue = enqDetails.TotalValue,
                        CustomerClassification = enqDetails.CustomerClassification,
                        CustomerSegment = enqDetails.CustomerSegment,
                        CustomerSubSegment = enqDetails.SubSegment,
                        CustomerType = enqDetails.CustomerType,
                        AssignedToUser = enqDetails.Username,
                        EnquiryMaturityDate = enqDetails.EnquiryMaturityDate.HasValue ? enqDetails.EnquiryMaturityDate.Value.ToString("dd/MM/yyyy") : "",
                        //EnquiryValidityDate = enqDetails.EnquiryValidityDate.HasValue ? enqDetails.EnquiryValidityDate.Value.ToString("dd/MM/yyyy") : "",
                        // DocumentCreatedDate = enqDetails.DocumentCreatedDate.HasValue ? enqDetails.DocumentCreatedDate.Value.ToString("dd/MM/yyyy") : "",
                        EnquiryDescription = enqDetails.Description,
                        SourceType = enqDetails.SourceType,
                        Classification1 = enqDetails.Classification1,
                        Classification2 = enqDetails.Classification2,
                        Classification3 = enqDetails.Classification3,
                        Classification4 = enqDetails.Classification4,
                        Currency = enqDetails.Currency,
                        UserRoleID = strRoleID,
                        Files = lsFiles
                    };
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }


            return View(objDetails);
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

                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Enquiry", "Please add atleast one document"));

                    return RedirectToAction("Attachments", new { enqId = objInput.ActivityId });
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
            return RedirectToAction("Attachments", new { enqId = activityID });
        }
        public async Task<IActionResult> RejectionHistory(string enqId, string status)
        {
            var objRHistory = new EnquiryHistoryModel();
            try
            {
                objRHistory.EnquryId = enqId;
                objRHistory.Status = status;
                objRHistory.HistoryRecords = await _serOpportunity.OpportunityHistory(enqId);

            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }


            return View(objRHistory);


        }

        public async Task<IActionResult> SAPEnquiry(string enqId)
        {
            AGS_SAPEnquiry sapEnquiry = new AGS_SAPEnquiry();
            try
            {
                string strSAPAccountID = null;
                var enqDetails = await _serOpportunity.RetriveOpportunity(enqId);
                var custDetails = await _serCustomer.GetCustomers(enqDetails.AccountID);
                if (custDetails != null)
                {
                    //switch (enqDetails.AccountID)
                    //{
                    //    case "ACC022467":
                    //        strSAPAccountID = "0026133530";
                    //        break;
                    //    case "ACC022468":
                    //        strSAPAccountID = "0026140598";
                    //        break;
                    //    case "ACC049997":
                    //        strSAPAccountID = "0028013209";
                    //        break;
                    //    default:
                    //        strSAPAccountID = custDetails[0].sapCustomerID;
                    //        break;
                    //}
                    strSAPAccountID = custDetails[0].sapCustomerID;
                }
                if (enqDetails != null)
                {
                    sapEnquiry = new AGS_SAPEnquiry
                    {
                        ClientID = enqDetails.Division,
                        ORD_REASON = enqDetails.DivisionName == "CC - VOLTAS " ? enqDetails.Reason : null,
                        SalesOffice = enqDetails.SalesOffice,
                        BusinessSegmentID = enqDetails.BusinessSegmentID,
                        CustomerGroupID = enqDetails.CustomerTypeID,
                        ValidFromDate = enqDetails.CreatedOn,
                        MaturityDate = enqDetails.EnquiryMaturityDate,
                        CustomerClassificationID = enqDetails.CustomerClassificationId,
                        CustomerSubSegmentId = enqDetails.SubSegmentID,
                        OpportunityID = enqDetails.CRMOpportunityID,
                        DocumentCreatedDate = enqDetails.CreatedOn,
                        Plant = enqDetails.PlantID,
                        SAPAccountNumber = strSAPAccountID,
                        SAPEnquiryProducts = enqDetails.OpportunityProducts.Select(x => new AGS_SAPEnquiry.AGS_SAPEnquiryProduct
                        {
                            Quantity = x.Quantity,
                            ProductName = x.ProductName,
                            TotalTonnageQuantity = x.TotalTonnageQuantity,
                            Value = x.Value
                        }).ToList()
                    };
                    var response = await _serSAPEnquiry.Create(sapEnquiry);
                    if (response.Status == "SUCCESS")
                    {
                        AGS_Opportunity enquiryUpdate = new AGS_Opportunity();

                        enquiryUpdate.SAPOpportunityID = response.SAPEnquiryNumber;
                        enquiryUpdate.CRMOpportunityID = enqDetails.CRMOpportunityID;
                        enquiryUpdate.Status = enqDetails.Status;

                        var blRes = await _serOpportunity.UpdateOpportunity(enqId, enquiryUpdate);
                        if (blRes)
                        {
                            return Json(new { status = "SUCCESS", sapNumber = response.SAPEnquiryNumber });
                        }
                    }
                    else
                    {
                        return Json(new { status = "FAILED", message = response.Error.Message });
                    }

                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }
            return Json(null);
        }

        public async Task<IActionResult> EnquiryItems(string BusinessSegment, string enqId)//string BusinesSeg,
        {
            var itemsObject = new EnquiryItemModel();
            try
            {
                itemsObject.BusinessSegment = BusinessSegment;
                itemsObject.EnquiryID = enqId;
                var objNewEnq = HttpContext.Session.GetObjectFromJson<EnquiryNewModel>("EnquiryNew");
                if (objNewEnq != null)
                {
                    itemsObject.EnquiryID = objNewEnq.EnquiryID;
                    itemsObject.BusinessSegment = objNewEnq.BusinessSegment;
                    itemsObject.Products = objNewEnq.OpportunityProducts;
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }



            return View(itemsObject);
        }

        public async Task<IActionResult> NavigateEnquiryItems(EnquiryNewModel objInput)
        {
            try
            {
                var objNew = HttpContext.Session.GetObjectFromJson<EnquiryNewModel>("EnquiryNew");
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
                    objInput.EnquiryAssignToList = objNew.EnquiryAssignToList;
                    objInput.Classification1List = objNew.Classification1List;
                    objInput.Classification2List = objNew.Classification2List;
                    objInput.Classification3List = objNew.Classification3List;
                    objInput.Classification4List = objNew.Classification4List;
                    objInput.BusinessSegmentList = objNew.BusinessSegmentList;
                    objInput.OpportunityProducts = objNew.OpportunityProducts;
                    objInput.CurrencyList = objNew.CurrencyList;
                }

                HttpContext.Session.SetObjectAsJson("EnquiryNew", objInput);

                string status = "FAILED";
                if (objInput.BusinessSegment != null)
                {
                    status = "SUCCESS";
                }
                return Json(new { status = status, BusinessSegment = objInput.BusinessSegment, enquiryId = objInput.EnquiryID });
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return Json(null);
        }

        public async Task<IActionResult> AddProduct(string BusinesSeg)
        {
            var newEnquiryProduct = new EnquiryProduct();
            try
            {
                newEnquiryProduct.BusinessSegment = BusinesSeg.Split('#')[0];
                newEnquiryProduct.Division = HttpContext.Session.GetObjectFromJson<string>("enquiryDivision");
                var lsProds = await _serProduct.GetProducts();
                if (lsProds != null)
                {
                    var lsSelect = lsProds.Select(x => new SelectListItem { Text = x.Text, Value = x.Text + "#" + x.Value }).ToList();
                    if (lsSelect.Count > 1)
                        lsSelect.Insert(0, new SelectListItem { Text = "SELECT", Value = "" });

                    newEnquiryProduct.ProductList = new SelectList(lsSelect, "Value", "Text");

                }

            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return View(newEnquiryProduct);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(EnquiryProduct objInput)
        {
            try
            {
                var objNewEnq = HttpContext.Session.GetObjectFromJson<EnquiryNewModel>("EnquiryNew");
                if (objNewEnq != null)
                {
                    if (objNewEnq.OpportunityProducts == null)
                    {
                        objNewEnq.OpportunityProducts = new List<EnquiryProduct>();
                    }

                    if (objInput != null)
                    {
                        if (objInput.ProductSeg != null)
                        {
                            objInput.ProductSegID = objInput.ProductSeg.Split('#')[1];
                            objInput.ProductSeg = objInput.ProductSeg.Split('#')[0];
                            objInput.BusinessSegment = objInput.BusinessSegment;

                            var idxObj = objNewEnq.OpportunityProducts.Where(x => x.ProductSegID == objInput.ProductSegID).FirstOrDefault();
                            if (idxObj != null)
                            {
                                var idx = objNewEnq.OpportunityProducts.IndexOf(idxObj);

                                if (objNewEnq.OpportunityProducts[idx].EntityState == EntityState.Deleted && !String.IsNullOrEmpty(objNewEnq.OpportunityProducts[idx].EnquiryID))
                                {
                                    objNewEnq.TotalValue += objInput.TotalValue;
                                    objNewEnq.Tonnage += objInput.TotalTonnage;
                                    objInput.EntityState = EntityState.Modified;
                                    objNewEnq.OpportunityProducts[idx].EntityState = EntityState.Modified;
                                    objNewEnq.OpportunityProducts[idx].TotalTonnage = objInput.TotalTonnage;
                                    objNewEnq.OpportunityProducts[idx].TotalValue = objInput.TotalValue;

                                }
                                else
                                {
                                    objNewEnq.OpportunityProducts[idx].Quantity += objInput.Quantity;
                                    objNewEnq.OpportunityProducts[idx].TotalValue += objInput.TotalValue;
                                    objNewEnq.OpportunityProducts[idx].TotalTonnage += objInput.TotalTonnage;

                                    if (!String.IsNullOrEmpty(objNewEnq.OpportunityProducts[idx].EnquiryID))
                                    {
                                        objInput.EntityState = EntityState.Modified;
                                    }

                                    objNewEnq.TotalValue += objInput.TotalValue;
                                    objNewEnq.Tonnage += objInput.TotalTonnage;
                                }
                            }
                            else
                            {

                                objNewEnq.TotalValue += objInput.TotalValue;
                                objNewEnq.Tonnage += objInput.TotalTonnage;
                                objInput.EntityState = EntityState.Added;
                                objNewEnq.OpportunityProducts.Add(objInput);
                            }

                        }
                    }


                    objNewEnq.strProducts = Newtonsoft.Json.JsonConvert.SerializeObject(objNewEnq.OpportunityProducts);

                    HttpContext.Session.SetObjectAsJson("EnquiryNew", objNewEnq);

                    return RedirectToAction("EnquiryItems");
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
            var editEnquiryProduct = new EnquiryProduct();
            try
            {
                var objNewEnq = HttpContext.Session.GetObjectFromJson<EnquiryNewModel>("EnquiryNew");
                if (objNewEnq != null)
                {
                    if (objNewEnq.OpportunityProducts != null && objNewEnq.OpportunityProducts.Count > 0)
                    {
                        editEnquiryProduct = objNewEnq.OpportunityProducts.Where(x => x.ProductSegID == productId).FirstOrDefault();

                        var lsSelect = new List<SelectListItem> { new SelectListItem { Text = editEnquiryProduct.ProductSeg, Value = editEnquiryProduct.ProductSeg + "#" + editEnquiryProduct.ProductSegID } };
                        if (lsSelect.Count > 1)
                            lsSelect.Insert(0, new SelectListItem { Text = "SELECT", Value = "" });

                        editEnquiryProduct.ProductList = new SelectList(lsSelect, "Value", "Text");

                        editEnquiryProduct.ProductSeg = editEnquiryProduct.ProductSeg + "#" + editEnquiryProduct.ProductSegID;
                    }

                    editEnquiryProduct.Division = HttpContext.Session.GetObjectFromJson<string>("enquiryDivision");
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return View(editEnquiryProduct);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(EnquiryProduct objInput)
        {
            try
            {
                var objNewEnq = HttpContext.Session.GetObjectFromJson<EnquiryNewModel>("EnquiryNew");
                if (objNewEnq != null)
                {
                    if (objNewEnq.OpportunityProducts == null)
                    {
                        objNewEnq.OpportunityProducts = new List<EnquiryProduct>();
                    }

                    if (objInput != null)
                    {
                        if (objInput.ProductSeg != null)
                        {
                            objInput.ProductSegID = objInput.ProductSeg.Split('#')[1];
                            objInput.ProductSeg = objInput.ProductSeg.Split('#')[0];
                            objInput.BusinessSegment = objInput.BusinessSegment;

                            var idxObj = objNewEnq.OpportunityProducts.Where(x => x.ProductSegID == objInput.ProductSegID).FirstOrDefault();
                            if (idxObj != null)
                            {
                                var idx = objNewEnq.OpportunityProducts.IndexOf(idxObj);

                                double? totVal = 0f;
                                double? totTonn = 0;

                                totVal = objInput.TotalValue - objNewEnq.OpportunityProducts[idx].TotalValue;
                                totTonn = objInput.TotalTonnage - objNewEnq.OpportunityProducts[idx].TotalTonnage;

                                // objInput.ProductSegmentId = objInput.ProductSegment;
                                objNewEnq.TotalValue += totVal;
                                objNewEnq.Tonnage += totTonn;

                                objNewEnq.OpportunityProducts[idx].Quantity = objInput.Quantity;
                                objNewEnq.OpportunityProducts[idx].TotalValue = objInput.TotalValue;
                                objNewEnq.OpportunityProducts[idx].TotalTonnage = objInput.TotalTonnage;

                                if (!String.IsNullOrEmpty(objNewEnq.OpportunityProducts[idx].EnquiryID))
                                {
                                    objNewEnq.OpportunityProducts[idx].EntityState = EntityState.Modified;
                                }
                                else
                                {
                                    objNewEnq.OpportunityProducts[idx].EntityState = EntityState.Added;
                                }
                            }

                        }
                    }


                    // objNewEnq.strProducts = Newtonsoft.Json.JsonConvert.SerializeObject(objNewEnq.OpportunityProducts);

                    HttpContext.Session.SetObjectAsJson("EnquiryNew", objNewEnq);

                    return RedirectToAction("EnquiryItems");
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
            try
            {
                var editEnquiryProduct = new EnquiryProduct();
                var objNewEnq = HttpContext.Session.GetObjectFromJson<EnquiryNewModel>("EnquiryNew");
                if (objNewEnq != null)
                {
                    if (objNewEnq.OpportunityProducts != null && objNewEnq.OpportunityProducts.Count > 0)
                    {
                        editEnquiryProduct = objNewEnq.OpportunityProducts.Where(x => x.ProductSegID == productId).FirstOrDefault();

                        var idx = objNewEnq.OpportunityProducts.IndexOf(editEnquiryProduct);

                        objNewEnq.TotalValue -= objNewEnq.OpportunityProducts[idx].TotalValue;
                        objNewEnq.Tonnage -= objNewEnq.OpportunityProducts[idx].TotalTonnage;

                        if (!String.IsNullOrEmpty(objNewEnq.OpportunityProducts[idx].EnquiryID))
                        {
                            objNewEnq.OpportunityProducts[idx].EntityState = EntityState.Deleted;
                        }
                        else
                        {
                            objNewEnq.OpportunityProducts.RemoveAt(idx);
                        }


                        // objNewEnq.strProducts = Newtonsoft.Json.JsonConvert.SerializeObject(objNewEnq.OpportunityProducts);

                        HttpContext.Session.SetObjectAsJson("EnquiryNew", objNewEnq);

                        return RedirectToAction("EnquiryItems");
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
        public async Task<IActionResult> UpdateEnquiryByConfirmation(string enqId, string strAction)
        {
            try
            {
                string strStatus = "";

                switch (strAction)
                {
                    case "CONVERT":
                        // strStatus = "PENDING FOR APPROVAL";

                        var objAttach = await _serAttachment.GetAttachments("OPPORTUNITY", enqId);
                        if (objAttach != null && objAttach.Count > 0)
                        {
                            strStatus = "PENDING FOR RECOMMEND";

                        }
                        else
                        {
                            return Json(new { status = "FAILED", message = "Atleast one costsheet should be upload for Convert" });
                        }

                        break;
                    case "CANCELLED":
                        strStatus = "CANCELLED";
                        break;
                    case "DEFERRED":
                        strStatus = "DEFERRED";
                        break;

                }

                var blRes = await _serOpportunity.UpdateOpportunity(enqId, new AGS_Opportunity { Status = strStatus });
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
        public async Task<IActionResult> UpdateEnquiryByStatusRemarks(string enqId, string strAction,string strRemarks,string strDate)
        {
            try
            {
                string strStatus = "";

                switch (strAction)
                {
                    case "CANCELLED":
                        strStatus = "CANCELLED";
                        break;
                    case "DEFERRED":
                        strStatus = "DEFERRED";
                        break;

                }

                var blRes = await _serOpportunity.UpdateOpportunity(enqId, new AGS_Opportunity { Status = strStatus, Remarks = strRemarks, EnquiryValidityDate = SalesStaticMethods.ConvertDate(strDate) });                     
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
        public async Task<IActionResult> UpdateContractValue(string enqId, double contractValue)
        {
            try
            {
                AGS_UploadContractValue request = new AGS_UploadContractValue();
                request.DocumentId = enqId;
                request.ContractValue = contractValue;

                var blRes = await _serOpportunity.UploadContractValue(request);
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
        public async Task<IActionResult> Search(string strKey)
        {
            var objData = new EnquiryViewModel();
            try
            {
                objData.SearchKey = strKey.Trim();
                var lsData = await _serOpportunity.SearchOpportunities(strKey.Trim());
                if (lsData != null && lsData.Count > 0)
                {
                    objData.EnquiryList = lsData.Select(y => new EnquiryViewModel.EnquiryViewItemModel
                    {
                        EnquiryID = y.CRMOpportunityID,
                        SAPEnquiryID = y.SAPOpportunityID,
                        BusineeSegment = y.BusinessSegment,
                        CustomerName = y.AccountName,
                        ProjectName = y.ProjectName,
                        Status = y.Status,
                        CreatedOn = y.CreatedOn,
                        LeadID = y.CRMLEADID,
                        Division = y.DivisionName,
                        Branch = y.BranchName,
                        Probablity = y.Probability,
                        ContractValue = y.ContractValue
                    }).ToList();
                    return View("Index", objData);
                }
                else
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Enquiry", "No Data available"));
                    //var lsEnquiry = await _serOpportunity.RetriveOpportunities("", "", 0, "", 0);
                    //if (lsEnquiry != null)
                    //{
                    //    return View("Index", lsEnquiry.Select(y => new EnquiryViewModel { EnquiryID = y.CRMOpportunityID, BusineeSegment = y.BusinessSegment, CustomerName = y.AccountName, Status = y.Status, CreatedOn = y.CreatedOn, LeadID = y.CRMLEADID, Division = y.DivisionName, Branch = y.BranchName, Probablity = y.Probability, ContractValue = y.ContractValue }).ToList());
                    //}

                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return View("Index", objData);
        }

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
        public async Task<IActionResult> SubSegment(string strKey)
        {
            try
            {
                var lsSubseg = await _serEntity.RetriveByParentId("Lead", "CUSTOMERSUBSEGMENT", strKey);
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

        #endregion
        /******************************************************************
       * ****************** Activities **********************************
       ******************************************************************/

        #region Activities
        public async Task<IActionResult> ActivityIndex(string enqId, string custname)
        {
            var objConPlan = new ContactPlanViewModel();
            try
            {
                objConPlan = await _serActivity.GetAllActivitiesByDocumentID(enqId);

                if (objConPlan != null)
                {
                    objConPlan.RefObjectID = enqId;
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
        public async Task<IActionResult> ActivityCreate(string enqId, string custname)
        {
            var objContPlan = new ContactPlanNewModel();
            try
            {
                objContPlan = await _serActivity.ActvityCreateObject(enqId, custname);

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

                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Enquiry Activity", "Activity (" + strResult + ") has been created successfully"));

                        return RedirectToAction("ActivityIndex", new { enqId = objInput.DocumentID, custname = objInput.Name });
                    }
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }




            return View(objInput);
        }

        public async Task<IActionResult> ActivityEdit(string activityId, string enqId)
        {
            var objEnqAct = new ContactPlanNewModel();
            try
            {
                objEnqAct = await _serActivity.ActvityEditObject(activityId);
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }




            return View(objEnqAct);
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


                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Enquiry Activity", "Activity(" + objInput.ActivityId + ") has been updated successfully"));

                        return RedirectToAction("ActivityDetails", "EnquiryOpportunity", new { activityId = objInput.ActivityId, enqId = objInput.DocumentID });
                    }
                }
                else
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Enquiry Activity", "Please provide all Values"));

                    // return RedirectToAction("ActivityDetails", "Lead", new { activityId = objInput.ActivityId });
                }

            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }


            return View(objInput);

        }

        public async Task<IActionResult> ActivityAttachments(string activityId, string enqId, string custname)
        {
            var objAtt = new AttachmentsModel();
            try
            {
                objAtt = await _serAttachment.GetAttachmentModel("ACTIVITY", activityId);
                objAtt.RefObjectId = enqId;
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
                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Enquiry Activity", "Attachments uploaded successfully"));

                        return RedirectToAction("ActivityIndex", new { enqId = objInput.RefObjectId, custname = objInput.CustomerName });
                    }
                }
                else
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Enquiry Activity", "Please add atleast one document"));

                    return RedirectToAction("ActivityAttachments", new { activityId = objInput.ActivityId, leadId = objInput.RefObjectId, custname = objInput.CustomerName });
                }



            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }


            return View(objInput);
        }

        public async Task<IActionResult> DeleteActivityAttachment(int Id, string activityId, string enqId, string custname)
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
            return RedirectToAction("ActivityAttachments", new { activityId = activityId, enqId = enqId, custname = custname });
        }
        public async Task<IActionResult> ActivityDetails(string activityId, string enqId)
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
