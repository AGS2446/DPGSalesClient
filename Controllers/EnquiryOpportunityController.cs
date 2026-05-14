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
using LeadProxy;
using System.IO;
using Microsoft.Extensions.Configuration;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace DPGSalesClient.Controllers
{
    public class EnquiryOpportunityController : Controller
    {
        #region Varaibles
        ServiceConnectors.OpportunityServiceConnector _serOpportunity = null;
        ServiceConnectors.LocationServiceConnector _serLocation = null;
        ServiceConnectors.EntityMapServiceConnector _serEntity = null;
        ServiceConnectors.LeadServiceConnector _serLead = null;
        ServiceConnectors.UserServiceConnector _serUser = null;
        ServiceConnectors.ProductServiceConnector _serProduct = null;
        ServiceConnectors.SAPEnquiryServiceConnector _serSAPEnquiry = null;
        ServiceConnectors.OpportunityServiceConnector _serEnquiry = null;

        Models.BusinessLogic.ActivityBL _serActivity = null;
        Models.BusinessLogic.AttachmentBL _serAttachment = null;
        Models.BusinessLogic.CustomerBL _serCustomer = null;

        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly HashSet<string> ExcludedDivisionIds;
        #endregion

        #region Constructor
        public EnquiryOpportunityController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            string strIp = SalesStaticMethods.GetRemoteIp(appSettings);

            _serOpportunity = new ServiceConnectors.OpportunityServiceConnector(strIp, httpContextAccessor);
            _serLocation = new ServiceConnectors.LocationServiceConnector(strIp, httpContextAccessor);
            _serEntity = new ServiceConnectors.EntityMapServiceConnector(strIp, httpContextAccessor);
            _serLead = new ServiceConnectors.LeadServiceConnector(strIp, httpContextAccessor);
            _serUser = new ServiceConnectors.UserServiceConnector(strIp, httpContextAccessor);
            _serProduct = new ServiceConnectors.ProductServiceConnector(strIp, httpContextAccessor);
            _serSAPEnquiry = new ServiceConnectors.SAPEnquiryServiceConnector(strIp, httpContextAccessor);
            _serEnquiry = new ServiceConnectors.OpportunityServiceConnector(strIp, httpContextAccessor);
            _serActivity = new Models.BusinessLogic.ActivityBL(strIp, httpContextAccessor);
            _serAttachment = new Models.BusinessLogic.AttachmentBL(strIp, httpContextAccessor);
            _serCustomer = new Models.BusinessLogic.CustomerBL(strIp, httpContextAccessor);

            _logger = loggerFactory.CreateLogger<EnquiryOpportunityController>();
            _hostingEnvironment = hostingEnvironment;
            ExcludedDivisionIds = configuration.GetSection("ExcludedDivisionIds").Get<HashSet<string>>();

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
                            SAPEnquiryID = y.SAPOpportunityID,
                            isDirect= !ExcludedDivisionIds.Contains(y.Division)

                        }).ToList();
                        var lsLocdetails = await _serLocation.RetreiveLocationDetails();
                       

                        objData.Addcount = lsLocdetails.Divisions.Count(x => ExcludedDivisionIds.Contains(x.ID));
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
            string Entity = "Lead";
            try
            {
                if (stBack == "BACK")
                {
                    var objNewEnq = HttpContext.Session.GetObjectFromJson<EnquiryNewModel>("EnquiryNew");
                    
                    if (objNewEnq.isDirect)
                    {
                        Entity = "ENQUIRY";
                    }
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
                            var lsSubseg = await _serEntity.RetriveByParentId(Entity, "CUSTOMERSUBSEGMENT", objNewEnq.CustomerSegment.Split('#')[0]);
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
                        objNew.isDirect = ExcludedDivisionIds.Contains(enqEdit.Division);
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

                            objNew.Classification1List = SalesStaticMethods.GetSelectlistItemsByName("CLASSIFICATION1", lsEntity, "P");
                            objNew.Classification2List = SalesStaticMethods.GetSelectlistItemsByName("CLASSIFICATION2", lsEntity, "P");
                            objNew.Classification3List = SalesStaticMethods.GetSelectlistItemsByName("CLASSIFICATION3", lsEntity, "B");
                            objNew.Classification4List = SalesStaticMethods.GetSelectlistItemsByName("CLASSIFICATION4", lsEntity, "P");
                            objNew.SourceTypeList = SalesStaticMethods.GetSelectlistItemsByName("SOURCETYPE", lsEntity, "P");

                        }

                        if (objNew.CustomerSubSegment != null)
                        {
                            var lsSubseg = await _serEntity.RetriveByParentId(Entity, "CUSTOMERSUBSEGMENT", enqEdit.CustomerSegment);
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
                       // objNewEnquiry.Classification1 = objInput.Classification1;
                        objNewEnquiry.Classification2 = objInput.Classification2;
                       // objNewEnquiry.Classification3 = objInput.Classification3.Split('#')[0];
                        //objNewEnquiry.Classification3ID = objInput.Classification3.Split('#')[1];
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
                        objNewEnquiry.EnquiryMaturityDate = string.IsNullOrWhiteSpace(objInput.EnquiryMaturityDate) ? (DateTime?)null : Convert.ToDateTime(objInput.EnquiryMaturityDate);
                    //    objNewEnquiry.EnquiryMaturityDate = SalesStaticMethods.ConvertDate(objInput.EnquiryMaturityDate);
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
            return RedirectToAction("Details", new { enqId = objInput.EnquiryID, status = "OPEN" });
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
                        CustomerDesignation = enqDetails.CustomerDesignation,
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
                        Files = lsFiles,
                        isDirect = !ExcludedDivisionIds.Contains(enqDetails.Division)
                    };
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }


            return View(objDetails);
        }
        public async Task<IActionResult> Attachments(string enqId, bool isDirect)
        {
            var objAtt = new AttachmentsModel();
           
            try
            {
                objAtt = await _serAttachment.GetAttachmentModel("OPPORTUNITY", enqId);
                objAtt.isDirect = isDirect;
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

                        return RedirectToAction("Attachments", new { enqId = objInput.ActivityId, isDirect = objInput.isDirect });
                    }
                }

                if (_serAttachment.VarifyJsonFileObject(jsObject))
                {
                    if (objInput.isDirect)
                    {
                        var lsFilesUploaded = await _serAttachment.UploadAttachments( "OPPORTUNITY", objInput.ActivityId, jsObject);

                        if (lsFilesUploaded.Count > 0)
                        {
                            TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Enquiry", "Attachments uploaded successfully"));

                            return RedirectToAction("Attachments", new { enqId = objInput.ActivityId, isDirect= objInput.isDirect});
                        }
                    }
                    else { 
                    var lsFilesUploaded = await _serAttachment.UploadEnquiryAttachments(objInput.DocumentType, "OPPORTUNITY", objInput.ActivityId, jsObject);

                    if (lsFilesUploaded.Count > 0)
                    {
                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Enquiry", "Attachments uploaded successfully"));

                            return RedirectToAction("Attachments", new { enqId = objInput.ActivityId });
                        }
                    }
                }
                else
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Enquiry", "Please add atleast one document"));

                    return RedirectToAction("Attachments", new { enqId = objInput.ActivityId, isDirect = objInput.isDirect });
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


            return RedirectToAction("Attachments", new { enqId = objInput.ActivityId, isDirect = objInput.isDirect });
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
        public async Task<IActionResult> UpdateEnquiryByConfirmation(string enqId,bool isDirect, string strAction)
        {
            try
            {
                string strStatus = "";

                switch (strAction)
                {
                    case "CONVERT":
                         strStatus = "PENDING FOR APPROVAL";

                        var objAttach = await _serAttachment.GetAttachments("OPPORTUNITY", enqId);
                        if (objAttach == null || objAttach.Count == 0)
                        {
                            return Json(new
                            {
                                status = "FAILED",
                                message = "At least one costsheet should be uploaded for Convert"
                            });
                        }

                        if (!isDirect)
                        {
                            bool hasBidNoBid = objAttach.Any(x => x.DocumentType == "BidNoBid");
                            bool hasContractsFinance = objAttach.Any(x => x.DocumentType == "ContractsFinance");
                            bool hasManagementApproval = objAttach.Any(x => x.DocumentType == "ManagementApproval");

                           // if (!(hasBidNoBid && hasContractsFinance && hasManagementApproval))
                            if (!(hasBidNoBid))
                            {
                                return Json(new
                                {
                                    status = "FAILED",
                                    message = "Please upload at least one file for  mandatory document type: Bid / No-Bid Analysis."
                                });
                            }
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



        // Dinesh
        [SessionTimeout]
        public async Task<IActionResult> CreateEnquiry(string strBack)
        {
            try
            {
                if (strBack == "BACK")
                {

                    #region Back

                    var objNewEnq = HttpContext.Session.GetObjectFromJson<LeadNewModel>("LeadNew");
                    if (objNewEnq != null)
                    {
                        if (objNewEnq.Branch != null)
                        {
                            var lsPlans = await _serLocation.RetreiveLocationDetailsByOrg(objNewEnq.Region.Split('#')[1], objNewEnq.Division.Split('#')[1]);
                            if (lsPlans != null)
                            {
                                objNewEnq.BranchList = lsPlans.Branches.Select(x => new SelectListItemObject { Value = x.Name + "#" + x.ID, Text = x.Name }).ToList();
                            }
                        }

                        if (objNewEnq.Plant != null)
                        {
                            var lsPlans = await _serLocation.RetreiveLocationDetailsByOrg(objNewEnq.Branch.Split('#')[1], objNewEnq.Division.Split('#')[1]);
                            if (lsPlans != null)
                            {
                                objNewEnq.PlantsList = lsPlans.Plants.Select(x => new SelectListItemObject { Value = x.Name + "#" + x.Code, Text = x.Name }).ToList();
                            }
                        }
                        if (objNewEnq.LeadAssignTo != null)
                        {
                            var lsUsers = await _serUser.GetAssignedUsers(objNewEnq.Division.Split('#')[1], objNewEnq.Branch.Split('#')[1]);
                            if (lsUsers != null)
                            {
                                objNewEnq.LeadAssignToList = lsUsers.Select(x => new SelectListItemObject { Value = x.Text + "#" + x.Value, Text = x.Text }).ToList();
                                if (objNewEnq.LeadAssignToList.Count > 1)
                                {
                                    objNewEnq.LeadAssignToList.Insert(0, new SelectListItemObject { Value = "", Text = "SELECT" });
                                }
                            }
                        }
                        if (objNewEnq.CustomerSubSegment != null)
                        {
                            var lsSubseg = await _serEntity.RetriveByParentId("ENQUIRY", "CUSTOMERSUBSEGMENT", objNewEnq.CustomerSegment.Split('#')[0]);
                            if (lsSubseg != null)
                            {
                                objNewEnq.CustomerSubSegmentList = lsSubseg.Select(x => new SelectListItemObject { Value = x.PropertyName + "#" + x.PropertyValue, Text = x.PropertyName }).ToList();
                                if (objNewEnq.CustomerSubSegmentList.Count > 1)
                                {
                                    objNewEnq.CustomerSubSegmentList.Insert(0, new SelectListItemObject { Value = "", Text = "SELECT" });
                                }
                            }
                        }

                        HttpContext.Session.SetObjectAsJson("LeadNew", objNewEnq);
                        return View(objNewEnq);
                    }

                    #endregion
                }
                else
                {
                    HttpContext.Session.ClearSession("LeadNew");

                    LeadNewModel objNew = new LeadNewModel();
                    #region Location details

                    var lsLocdetails = await _serLocation.RetreiveLocationDetails();

                    if (lsLocdetails != null)
                    {
                        var lsobjList = new List<SelectListItemObject>();
                        //Divisions
                        if (lsLocdetails.Divisions.Count > 0)
                        {
                            lsobjList = lsLocdetails.Divisions.Where(y => ExcludedDivisionIds.Contains(y.ID)).Select(x => new SelectListItemObject { Text = x.Name, Value = x.Name + "#" + x.ID }).ToList();
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
                                objNew.RegionList = lsobjList;

                                //Branch
                                lsobjList = new List<SelectListItemObject>();
                                lsobjList.Add(new SelectListItemObject { Text = "SELECT", Value = "" });
                                objNew.BranchList = lsobjList;

                                //Plant
                                objNew.PlantsList = lsobjList;

                            }
                            else if (lsLocdetails.Regions.Count == 1)
                            {
                                //Region
                                objNew.RegionList = lsobjList;

                                //Branch
                                var lsBranches = await _serLocation.RetreiveLocationDetailsByOrg(lsLocdetails.Regions[0].ID, lsLocdetails.Divisions[0].ID);
                                if (lsBranches != null && lsBranches.Branches.Count > 0)
                                {
                                    lsobjList = new List<SelectListItemObject>();
                                    lsobjList = lsBranches.Branches.Select(x => new SelectListItemObject { Text = x.Name, Value = x.Name + "#" + x.ID }).ToList();

                                    if (lsobjList.Count > 0)
                                    {
                                        lsobjList.Insert(0, new SelectListItemObject { Text = "SELECT", Value = "" });
                                        objNew.BranchList = lsobjList;

                                        //Plants
                                        lsobjList = new List<SelectListItemObject>();
                                        lsobjList.Add(new SelectListItemObject { Text = "SELECT", Value = "" });
                                        objNew.PlantsList = lsobjList;
                                    }
                                    else if (lsBranches.Branches.Count == 1)
                                    {
                                        //Branch
                                        objNew.BranchList = lsobjList;

                                        //Plants
                                        var lsPlants = await _serLocation.RetreiveLocationDetailsByOrg(lsBranches.Branches[0].ID, lsBranches.Branches[0].ID);
                                        if (lsPlants != null)
                                        {
                                            if (lsPlants.SalesOffices != null && lsPlants.SalesOffices.Count > 0)
                                                objNew.SalesOffice = lsPlants.SalesOffices[0].Code;

                                            lsobjList = new List<SelectListItemObject>();
                                            lsobjList = lsPlants.Plants.Select(x => new SelectListItemObject { Text = x.Name, Value = x.Name + "#" + x.Code }).ToList();
                                            if (lsobjList.Count > 1)
                                                lsobjList.Insert(0, new SelectListItemObject { Text = "SELECT", Value = "" });
                                            objNew.PlantsList = lsobjList;
                                        }

                                        if (objNew.DivisionList.Count == 1)
                                        {
                                            //Assigned To Users
                                            var lsUsers = await _serUser.GetAssignedUsers(objNew.DivisionList[0].Value.Split('#')[1], lsBranches.Branches[0].ID);
                                            if (lsUsers != null && lsUsers.Count > 0)
                                            {
                                                var lsUserSelect = lsUsers.Select(x => new SelectListItemObject { Text = x.Text, Value = x.Text + "#" + x.Value }).ToList();

                                                if (lsUserSelect.Count > 0)
                                                    lsUserSelect.Insert(0, new SelectListItemObject { Text = "SELECT", Value = "" });

                                                objNew.LeadAssignToList = lsUserSelect;
                                            }
                                        }



                                    }
                                }

                            }


                        }
                    }



                    #endregion

                    //EntityMap
                    var lsEntity = await _serEntity.RetriveByObjectName("ENQUIRY");
                    if (lsEntity != null)//&& lsEntity.Count>0
                    {
                        objNew.CustomerSegmentList = SalesStaticMethods.GetSelectlistItemsByName("CUSTOMERSEGMENT", lsEntity, "B");
                        objNew.CustomerClassificationList = SalesStaticMethods.GetSelectlistItemsByName("CUSTOMERCLASSIFICATION", lsEntity, "B");
                        objNew.CustomerTypeList = SalesStaticMethods.GetSelectlistItemsByName("CUSTOMERTYPE", lsEntity, "B");
                        objNew.BusinessSegmentList = SalesStaticMethods.GetSelectlistItemsByName("BUSINESSSEGMENT", lsEntity, "B");
                        objNew.ProdcutRequiredList = SalesStaticMethods.GetSelectlistItemsByName("PRODUCTREQUIRED", lsEntity, "P");
                        objNew.ProbabilityList = SalesStaticMethods.GetSelectlistItemsByName("PROBABILITY", lsEntity, "P");
                        objNew.PriorityList = SalesStaticMethods.GetSelectlistItemsByName("Priority", lsEntity, "P");
                        objNew.CurrencyList = SalesStaticMethods.GetSelectlistItemsByName("CURRENCY", lsEntity, "P");
                        objNew.Classification1List = SalesStaticMethods.GetSelectlistItemsByName("CLASSIFICATION1", lsEntity, "P");
                        objNew.Classification2List = SalesStaticMethods.GetSelectlistItemsByName("CLASSIFICATION2", lsEntity, "P");
                        objNew.Classification3List = SalesStaticMethods.GetSelectlistItemsByName("CLASSIFICATION3", lsEntity, "B");
                        objNew.Classification4List = SalesStaticMethods.GetSelectlistItemsByName("CLASSIFICATION4", lsEntity, "P");
                        objNew.SourceTypeList = SalesStaticMethods.GetSelectlistItemsByName("SOURCETYPE", lsEntity, "P");

                        objNew.CustomerSubSegmentList = new List<SelectListItemObject> { new SelectListItemObject { Text = "SELECT", Value = "" } };

                        if (objNew.LeadAssignToList == null)
                        {
                            objNew.LeadAssignToList = new List<SelectListItemObject> { new SelectListItemObject { Text = "SELECT", Value = "" } };
                        }
                    }

                    if (objNew.DivisionList == null)
                        objNew.DivisionList = new List<SelectListItemObject>();
                    if (objNew.RegionList == null)
                        objNew.RegionList = new List<SelectListItemObject>();
                    if (objNew.BranchList == null)
                        objNew.BranchList = new List<SelectListItemObject>();
                    if (objNew.PlantsList == null)
                        objNew.PlantsList = new List<SelectListItemObject>();




                    HttpContext.Session.SetObjectAsJson("LeadNew", objNew);
                    return View(objNew);
                }

            }
            catch (TimeoutException tex)
            {

            }
            catch (Exception ex)
            {
            }
            return View();
        }
        public async Task<IActionResult> EnquiryItemsN(string BusinessSegment)//string BusinesSeg,
        {
            var itemsObject = new LeadConvertEnquiryModel();
            try
            {
                itemsObject.BusinessSegment = BusinessSegment;

                var objNewEnq = HttpContext.Session.GetObjectFromJson<LeadConvertEnquiryModel>("leadEnquiryConvert");
                if (objNewEnq != null)
                {
                  //  itemsObject.LeadId = objNewEnq.LeadId;
                    itemsObject.BusinessSegment = objNewEnq.BusinessSegment;
                    itemsObject.Division = objNewEnq.Division;
                    itemsObject.EnquiryProducts = objNewEnq.EnquiryProducts;
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }



            return View(itemsObject);
        }
        public async Task<IActionResult> EnquiryAddProductN(string BusinesSeg)
        {
            var newEnquiryProduct = new EnquiryProduct();
            try
            {
                newEnquiryProduct.BusinessSegment = BusinesSeg;
                newEnquiryProduct.Division = HttpContext.Session.GetObjectFromJson<string>("division");
                // newEnquiryProduct.BusinessSegment = BusinesSeg;

                var lsProds = await _serProduct.GetProducts();
                if (lsProds != null)
                {
                    var lsSelect = lsProds.Select(x => new SelectListItem { Text = x.Text, Value = x.Text + "#" + x.Value }).ToList();
                    if (lsSelect.Count > 1)
                        lsSelect.Insert(0, new SelectListItem { Text = "SELECT", Value = "" });

                    newEnquiryProduct.ProductList = new SelectList(lsSelect, "Value", "Text");
                }

                // newEnquiryProduct.ProductSegmentList = new SelectList(new List<SelectListItem> { new SelectListItem {Text="PRD1",Value="PRD1" }, new SelectListItem { Text = "PRD2", Value = "PRD2" } }, "Value", "Text");
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {


            }
            return View(newEnquiryProduct);
        }

        [HttpPost]
        public async Task<IActionResult> EnquiryAddProductN(EnquiryProduct objInput)
        {
            try
            {
                var enqNewObj = HttpContext.Session.GetObjectFromJson<LeadConvertEnquiryModel>("leadEnquiryConvert");
                if (enqNewObj != null)
                {
                    if (enqNewObj.EnquiryProducts == null)
                        enqNewObj.EnquiryProducts = new List<EnquiryProduct>();

                    var idxObj = enqNewObj.EnquiryProducts.Where(x => x.ProductSegID == objInput.ProductSeg.Split('#')[1]).FirstOrDefault();
                    if (idxObj != null)
                    {
                        var idx = enqNewObj.EnquiryProducts.IndexOf(idxObj);
                        enqNewObj.EnquiryProducts[idx].Quantity += objInput.Quantity;
                        enqNewObj.EnquiryProducts[idx].TotalValue += objInput.TotalValue;
                        enqNewObj.EnquiryProducts[idx].TotalTonnage += objInput.TotalTonnage;

                        objInput.ProductSegID = objInput.ProductSeg.Split('#')[0];
                        enqNewObj.TotalValue += objInput.TotalValue;
                        enqNewObj.Tonnage += objInput.TotalTonnage;
                    }
                    else
                    {
                        objInput.ProductSegID = objInput.ProductSeg.Split('#')[1];
                        objInput.ProductSeg = objInput.ProductSeg.Split('#')[0];
                        objInput.BusinessSegment = objInput.BusinessSegment;

                        enqNewObj.TotalValue = (enqNewObj.TotalValue.HasValue ? enqNewObj.TotalValue.Value : 0f) + objInput.TotalValue;
                        enqNewObj.Tonnage = (enqNewObj.Tonnage.HasValue ? enqNewObj.Tonnage.Value : 0f) + objInput.TotalTonnage;

                        enqNewObj.EnquiryProducts.Add(objInput);
                    }


                    HttpContext.Session.SetObjectAsJson("leadEnquiryConvert", enqNewObj);
                    return RedirectToAction("EnquiryItemsN", new { BusinessSegment = enqNewObj.BusinessSegment });
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {


            }
            return View(objInput);
        }
        [HttpGet]
        public async Task<IActionResult> ConvertToEnquiry(LeadNewModel data)
        {
            var leadConvert = HttpContext.Session.GetObjectFromJson<LeadConvertEnquiryModel>("leadEnquiryConvert");
            var leadEnquiry = HttpContext.Session.GetObjectFromJson<LeadNewModel>("leadEnquiry");
            try
            {

                if (leadConvert != null)
                {
                    return View(leadConvert);
                }
                else
                {
                    leadConvert = new LeadConvertEnquiryModel();
                    leadConvert.BusinessSegment = data.BusinessSegment;
                    leadConvert.BusinessSegmentId = data.BusinessSegment;
                    leadConvert.EnquiryMaturityDate = DateTime.Now.ToString("dd/MM/yyyy");
                    leadConvert.LeadDocumentCreatedDate = data.DocumentCreatedDate;

                    HttpContext.Session.SetObjectAsJson("leadEnquiryConvert", leadConvert);
                    HttpContext.Session.SetObjectAsJson("leadEnquiry", data);

                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {


            }

            return View(leadConvert);
        }

        [HttpPost]
        public async Task<IActionResult> ConvertToEnquiry(LeadConvertEnquiryModel data)
        {
            var objInput = HttpContext.Session.GetObjectFromJson<LeadNewModel>("leadEnquiry");
            var enqNewObj = HttpContext.Session.GetObjectFromJson<LeadConvertEnquiryModel>("leadEnquiryConvert");
            try
            {
                if (objInput != null)
                {
                    AGS_Lead objNewLead = new AGS_Lead();
                    objNewLead.Division = objInput.Division.Split('#')[1];
                    objNewLead.DivisionName = objInput.Division.Split('#')[0];
                    objNewLead.Region = objInput.Region.Split('#')[1];
                    objNewLead.RegionName = objInput.Region.Split('#')[0];
                    objNewLead.Branch = objInput.Branch.Split('#')[1];
                    objNewLead.BranchName = objInput.Branch.Split('#')[0];
                    objNewLead.SalesOffice = objInput.SalesOffice;
                    objNewLead.PlantID = objInput.Plant.Split('#')[1];
                    objNewLead.PlantName = objInput.Plant.Split('#')[0];

                    objNewLead.AccountID = objInput.CustomerCode;
                    objNewLead.AccountName = objInput.CustomerName;

                    objNewLead.City = objInput.City;
                    objNewLead.State = objInput.State;
                    objNewLead.MobileNumber = objInput.MobileNumber;
                    objNewLead.Pincode = objInput.Pincode;
                    objNewLead.ContactName = objInput.ContactPerson;
                    objNewLead.Address1 = objInput.CustomerAddress;

                    objNewLead.CustomerSegmentID = objInput.CustomerSegment.Split('#')[1];
                    objNewLead.CustomerSegment = objInput.CustomerSegment.Split('#')[0];
                    objNewLead.SubSegmentID = objInput.CustomerSubSegment.Split('#')[1];
                    objNewLead.SubSegment = objInput.CustomerSubSegment.Split('#')[0];
                    objNewLead.CustomerType = objInput.CustomerType.Split('#')[0];
                    objNewLead.CustomerTypeID = objInput.CustomerType.Split('#')[1];
                    objNewLead.CustomerClassification = objInput.CustomerClassification.Split('#')[0];
                    objNewLead.CustomerClassificationId = objInput.CustomerClassification.Split('#')[1];

                    objNewLead.BusinessSegment = objInput.BusinessSegment.Split('#')[0];
                    objNewLead.BusinessSegmentID = objInput.BusinessSegment.Split('#')[1];
                    objNewLead.ProductRequired = objInput.ProdcutRequired;
                    objNewLead.Status = objInput.Status;
                    objNewLead.ProjectName = objInput.ProjectName;
                    //objNewLead.Priority = objInput.Priority;
                    objNewLead.Probability = objInput.Probability;
                    objNewLead.ContractValue = objInput.ContractValue_IN_LAKHS;
                    objNewLead.Currency = objInput.Currency;
                    objNewLead.CurrencyValue = objInput.CurrencyValue;
                    objNewLead.Description = objInput.Description;
                    //  objNewLead.DocumentCreatedDate = SalesStaticMethods.ConvertDate(objInput.DocumentCreatedDate);
                    objNewLead.LeadMaturityDate = SalesStaticMethods.ConvertDate(objInput.LeadMaturityDate);
                    //objNewLead.Classification1 = objInput.Classification1;
                    objNewLead.Classification2 = objInput.Classification2;
                    //objNewLead.Classification3 = objInput.Classification3.Split('#')[0];
                    //objNewLead.Classificaiton3id = objInput.Classification3.Split('#')[1];
                    objNewLead.Classification4 = objInput.Classification4;
                    objNewLead.Architect = objInput.Architect;
                    objNewLead.Consultant = objInput.Consultant;
                    objNewLead.CreatedOn = DateTime.Now;
                    objNewLead.SourceType = objInput.SourceType;
                    objNewLead.UserID = objInput.LeadAssignTo.Split('#')[1];
                    objNewLead.Username = objInput.LeadAssignTo.Split('#')[0];
                    var strRes = await _serLead.CreateLead(objNewLead);
                    if (strRes != "" && strRes != null)
                    {
                        OpportunityProxy.AGS_Opportunity objNewEnquiry = new OpportunityProxy.AGS_Opportunity();
                        var leadDetails = await _serLead.RetriveLead(strRes);
                        if (leadDetails != null)
                        {
                            objNewEnquiry.CRMLEADID = leadDetails.CRMLeadID;
                            objNewEnquiry.Division = leadDetails.Division;
                            objNewEnquiry.DivisionName = leadDetails.DivisionName;
                            objNewEnquiry.Region = leadDetails.Region;
                            objNewEnquiry.RegionName = leadDetails.RegionName;
                            objNewEnquiry.Branch = leadDetails.Branch;
                            objNewEnquiry.BranchName = leadDetails.BranchName;
                            objNewEnquiry.SalesOffice = leadDetails.SalesOffice;
                            objNewEnquiry.PlantID = leadDetails.PlantID;
                            objNewEnquiry.PlantName = leadDetails.PlantName;
                            objNewEnquiry.CustomerDesignation = objInput.CustomerDesignation;

                            objNewEnquiry.AccountID = leadDetails.AccountID;
                            objNewEnquiry.AccountName = leadDetails.AccountName;
                            objNewEnquiry.CustomerSegment = leadDetails.CustomerSegment;
                            objNewEnquiry.CustomerSegmentID = leadDetails.CustomerSegmentID;
                            objNewEnquiry.SubSegment = leadDetails.SubSegment;
                            objNewEnquiry.SubSegmentID = leadDetails.SubSegmentID;
                            objNewEnquiry.CustomerType = leadDetails.CustomerType;
                            objNewEnquiry.CustomerTypeID = leadDetails.CustomerTypeID;
                            objNewEnquiry.CustomerClassification = leadDetails.CustomerClassification;
                            objNewEnquiry.CustomerClassificationId = leadDetails.CustomerClassificationId;

                            objNewEnquiry.BusinessSegmentID = leadDetails.BusinessSegmentID;
                            objNewEnquiry.BusinessSegment = leadDetails.BusinessSegment;
                            objNewEnquiry.Classification1 = leadDetails.Classification1;
                            objNewEnquiry.Classification2 = leadDetails.Classification2;
                            objNewEnquiry.Classification3 = leadDetails.Classification3;
                            objNewEnquiry.Classification3ID = leadDetails.Classificaiton3id;
                            objNewEnquiry.Classification4 = leadDetails.Classification4;
                            objNewEnquiry.Currency = leadDetails.Currency;
                            objNewEnquiry.CurrencyValue = leadDetails.CurrencyValue;
                            objNewEnquiry.UserID = leadDetails.UserID;
                            objNewEnquiry.Username = leadDetails.Username;
                            objNewEnquiry.Architect = leadDetails.Architect;
                            objNewEnquiry.Consultant = leadDetails.Consultant;
                            objNewEnquiry.SourceType = leadDetails.SourceType;
                            objNewEnquiry.ProjectName = leadDetails.ProjectName;
                            objNewEnquiry.ProductRequired = leadDetails.ProductRequired;
                            objNewEnquiry.Description = leadDetails.Description;
                            objNewEnquiry.Probability = leadDetails.Probability;
                            objNewEnquiry.ContractValue = leadDetails.ContractValue;

                            //Enquiry details
                            objNewEnquiry.EnquiryMaturityDate = DateTime.Now;


                            objNewEnquiry.Tonnage = data.Tonnage;
                            objNewEnquiry.TotalValue = data.TotalValue;

                            objNewEnquiry.Status = "OPEN";

                            objNewEnquiry.OpportunityProducts = new List<OpportunityProxy.AGS_OpportunityProduct>();
                            if (enqNewObj.EnquiryProducts.Count > 0)
                            {
                                var lsProducts = enqNewObj.EnquiryProducts.Select(x => new OpportunityProxy.AGS_OpportunityProduct
                                {
                                    EntityState = OpportunityProxy.EntityState.Added,
                                    MaterialGroup = x.BusinessSegment,
                                    ProductID = x.ProductSegID,
                                    ProductName = x.ProductSeg,
                                    Quantity = x.Quantity,
                                    TotalValue = x.TotalValue,
                                    TotalTonnageQuantity = x.TotalTonnage
                                }).ToList();


                                objNewEnquiry.OpportunityProducts = lsProducts;
                            }
                            if (objNewEnquiry.OpportunityProducts.Count > 0)
                            {
                                var strEnq = await _serEnquiry.CreateOpportunity(objNewEnquiry);
                                if (strEnq != "")
                                {
                                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("ENQUIRY", "Lead (" + leadDetails.CRMLeadID + ") has been converted to enquiry (" + strEnq + ") successfully"));

                                    HttpContext.Session.ClearSession("leadEnquiryConvert");
                                    HttpContext.Session.ClearSession("division");
                                    return RedirectToAction("Index");
                                }
                            }
                            
                        }
                       
                        return RedirectToAction("Index");
                    }
                    else
                    {

                    }

                }
                else
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("ENQUIRY", "Please provide required fields data"));
                    var objNewEnq = HttpContext.Session.GetObjectFromJson<LeadNewModel>("LeadNew");
                    if (objNewEnq != null)
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
                        objInput.BusinessSegmentList = objNewEnq.BusinessSegmentList;
                        objInput.CurrencyList = objNewEnq.CurrencyList;

                        objInput.LeadAssignToList = objNewEnq.LeadAssignToList;
                        objInput.Classification1List = objNewEnq.Classification1List;
                        objInput.Classification2List = objNewEnq.Classification2List;
                        objInput.Classification3List = objNewEnq.Classification3List;
                        objInput.Classification4List = objNewEnq.Classification4List;
                        objInput.SourceTypeList = objNewEnq.SourceTypeList;
                    }

                    HttpContext.Session.SetObjectAsJson("LeadNew", objInput);
                    return View(objInput);

                }
            }
            catch (TimeoutException tex)
            {

            }
            catch (Exception ex)
            {
            }
            return View(objInput);
        }
    }
}
