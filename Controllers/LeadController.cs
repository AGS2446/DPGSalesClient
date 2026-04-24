using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using DPGSalesClient.Models;
using LeadProxy;
using Microsoft.AspNetCore.Mvc.Rendering;
using ActivityProxy;
using Microsoft.Extensions.Options;
using AuthorizationProxy;

namespace DPGSalesClient.Controllers
{
    public class LeadController : Controller
    {
        #region Varaibles

        ServiceConnectors.LeadServiceConnector _serLead = null;
        ServiceConnectors.LocationServiceConnector _serLocation = null;
        ServiceConnectors.EntityMapServiceConnector _serEntity = null;
        ServiceConnectors.UserServiceConnector _serUser = null;
        ServiceConnectors.ProductServiceConnector _serProduct = null;
        ServiceConnectors.OpportunityServiceConnector _serEnquiry = null;

        Models.BusinessLogic.ActivityBL _serActivity = null;
        Models.BusinessLogic.AttachmentBL _serAttachment = null;
        Models.BusinessLogic.CustomerBL _serCustomer = null;

        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;
        private static readonly HashSet<string> ExcludedDivisionIds = new HashSet<string>
{
    "CLNT000002",
    "CLNT000007",
    "CLNT000008",
    "CLNT000009"
};
        #endregion

        #region Constructure
        public LeadController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor)
        {
            string strIp = SalesStaticMethods.GetRemoteIp(appSettings);
            _serLead = new ServiceConnectors.LeadServiceConnector(strIp, httpContextAccessor);
            _serLocation = new ServiceConnectors.LocationServiceConnector(strIp, httpContextAccessor);
            _serEntity = new ServiceConnectors.EntityMapServiceConnector(strIp, httpContextAccessor);
            _serUser = new ServiceConnectors.UserServiceConnector(strIp, httpContextAccessor);
            _serProduct = new ServiceConnectors.ProductServiceConnector(strIp, httpContextAccessor);
            _serEnquiry = new ServiceConnectors.OpportunityServiceConnector(strIp, httpContextAccessor);

            _serActivity = new Models.BusinessLogic.ActivityBL(strIp, httpContextAccessor);
            _serAttachment = new Models.BusinessLogic.AttachmentBL(strIp, httpContextAccessor);
            _serCustomer = new Models.BusinessLogic.CustomerBL(strIp, httpContextAccessor);

            _logger = loggerFactory.CreateLogger<LeadController>();
            _hostingEnvironment = hostingEnvironment;
        }

        #endregion

        /******************************************************************
       * ****************** LEAD **********************************
       ******************************************************************/

        #region Lead

        public async Task<IActionResult> Index()
        {
            HttpContext.Session.SetString("Controller", "Lead");
            var objData = new LeadViewModel();
            try
            {
                if (HttpContext.Session.CheckSession("NavigationList"))
                {
                    AGS_LoginUserInfo logInfo = HttpContext.Session.GetObjectFromJson<AGS_LoginUserInfo>("LoginUserInfo");
                    HttpContext.Session.SetString("UserRoleId", logInfo.RoleInfo[0].RoleID);
                    ViewBag.RoleId = logInfo.RoleInfo[0].RoleID;
                    var lsLeads = await _serLead.GetLeadsAsync();
                    var lsLocdetails = await _serLocation.RetreiveLocationDetails();
                    var ids = new List<string>
                                {
                                    "CLNT000002",
                                    "CLNT000007",
                                    "CLNT000008",
                                    "CLNT000009"
                                };

                    objData.Addcount = lsLocdetails.Divisions.Count(x => ids.Contains(x.ID));
                    if (lsLeads != null)
                    {
                        objData.LeadList = lsLeads
     .Where(y => !ids.Contains(y.Division))          // ❌ exclude these IDs
    .Select(y => new LeadViewModel.LeadViewItemModel
    {
        LeadID = y.CRMLeadID,
        BusineeSegment = y.BusinessSegment,
        CustomerName = y.AccountName,
        ProjectName = y.ProjectName,
        Status = y.Status,
        CreatedOn = y.CreatedOn,
        Division = y.DivisionName,
        Branch = y.BranchName,
        Probablity = y.Probability,
        ContractValue = y.ContractValue
    })
    .OrderByDescending(z => z.LeadID)
    .ToList();
                        return View(objData);
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Auth");
                }
            }
            catch (TimeoutException tex)
            {

            }
            catch (Exception ex) when (ex.Message.Equals("400"))
            {

            }
            return View(objData);
        }
        [SessionTimeout]
        public async Task<IActionResult> Create(string strBack)
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
                            lsobjList = lsLocdetails.Divisions.Where(y => !ExcludedDivisionIds.Contains(y.ID)).Select(x => new SelectListItemObject { Text = x.Name, Value = x.Name + "#" + x.ID }).ToList();
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
                    var lsEntity = await _serEntity.RetriveByObjectName("LEAD");
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

        [HttpPost]
        public async Task<IActionResult> Create(LeadNewModel objInput)
        {
            try
            {
                if (ModelState.IsValid)
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
                    objNewLead.Priority = objInput.Priority;
                    objNewLead.Probability = objInput.Probability;
                    objNewLead.ContractValue = objInput.ContractValue_IN_LAKHS;
                    objNewLead.Currency = objInput.Currency;
                    objNewLead.CurrencyValue = objInput.CurrencyValue;
                    objNewLead.Description = objInput.Description;
                    //  objNewLead.DocumentCreatedDate = SalesStaticMethods.ConvertDate(objInput.DocumentCreatedDate);
                    objNewLead.LeadMaturityDate = SalesStaticMethods.ConvertDate(objInput.LeadMaturityDate);
                    objNewLead.Classification1 = objInput.Classification1;
                    objNewLead.Classification2 = objInput.Classification2;
                    objNewLead.Classification3 = objInput.Classification3.Split('#')[0];
                    objNewLead.Classificaiton3id = objInput.Classification3.Split('#')[1];
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
                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Lead", "Lead (" + strRes + ") has been created successfully"));

                        return RedirectToAction("Index");
                    }
                    else
                    {

                    }

                }
                else
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Lead", "Please provide required fields data"));
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
        public async Task<IActionResult> Edit(string leadId)
        {
            try
            {

                var objGetLead = await _serLead.RetriveLead(leadId);
                if (objGetLead != null)
                {

                    LeadNewModel objNew = new LeadNewModel
                    {
                        LeadID = objGetLead.CRMLeadID,
                        Division = objGetLead.DivisionName + "#" + objGetLead.Division,
                        Region = objGetLead.RegionName + "#" + objGetLead.Region,
                        Branch = objGetLead.BranchName + "#" + objGetLead.Branch,
                        SalesOffice = objGetLead.SalesOffice,
                        Plant = objGetLead.PlantName + "#" + objGetLead.PlantID,
                        CustomerCode = objGetLead.AccountID,
                        CustomerName = objGetLead.AccountName,
                        BusinessSegment = objGetLead.BusinessSegment + "#" + objGetLead.BusinessSegmentID,
                        ProdcutRequired = objGetLead.ProductRequired,
                        ProjectName = objGetLead.ProjectName,
                        Architect = objGetLead.Architect,
                        Consultant = objGetLead.Consultant,
                        ContractValue_IN_LAKHS = objGetLead.ContractValue,
                        Probability = objGetLead.Probability,
                        CustomerClassification = objGetLead.CustomerClassification + "#" + objGetLead.CustomerClassificationId,
                        CustomerType = objGetLead.CustomerType + "#" + objGetLead.CustomerTypeID,
                        CustomerSegment = objGetLead.CustomerSegment + "#" + objGetLead.CustomerSegmentID,
                        CustomerSubSegment = objGetLead.SubSegment + "#" + objGetLead.SubSegmentID,
                        Status = objGetLead.Status,
                        LeadAssignTo = objGetLead.Username + "#" + objGetLead.UserID,
                        LeadMaturityDate = objGetLead.LeadMaturityDate.HasValue ? objGetLead.LeadMaturityDate.Value.ToString("dd/MM/yyyy") : "",
                        //  DocumentCreatedDate = objGetLead.DocumentCreatedDate.HasValue ? objGetLead.DocumentCreatedDate.Value.ToString("dd/MM/yyyy") : "",
                        Description = objGetLead.Description,
                        Priority = objGetLead.Priority,
                        SourceType = objGetLead.SourceType,
                        Classification1 = objGetLead.Classification1,
                        Classification2 = objGetLead.Classification2,
                        Classification3 = objGetLead.Classification3 + "#" + objGetLead.Classificaiton3id,
                        Classification4 = objGetLead.Classification4,
                        Currency = objGetLead.Currency,
                        CurrencyValue = objGetLead.CurrencyValue,
                        City = objGetLead.City,
                        State = objGetLead.State,
                        MobileNumber = objGetLead.MobileNumber,
                        Pincode = objGetLead.Pincode,
                        ContactPerson = objGetLead.ContactName,
                        CustomerAddress = objGetLead.Address1
                    };

                    #region Account details
                    //var objAcc = await _serCustomer.GetCustomers(objGetLead.AccountName);
                    //if (objAcc != null && objAcc.Count > 0)
                    //{
                    //    objNew.CustomerCode = objAcc[0].customerId;
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
                        var lsBranches = await _serLocation.RetreiveLocationDetailsByOrg(objGetLead.Region, objGetLead.Division);
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
                            var lsPlants = await _serLocation.RetreiveLocationDetailsByOrg(objGetLead.Branch, objGetLead.Division);
                            if (lsPlants != null)
                            {
                                objNew.PlantsList = lsPlants.Plants.Select(x => new SelectListItemObject { Value = x.Name + "#" + x.Code, Text = x.Name }).ToList();
                            }
                        }
                        if (objNew.LeadAssignTo != null)
                        {
                            var lsUsers1 = await _serUser.GetAssignedUsers(objGetLead.Division, objGetLead.Branch);
                            if (lsUsers1 != null)
                            {
                                objNew.LeadAssignToList = lsUsers1.Select(x => new SelectListItemObject { Value = x.Text + "#" + x.Value, Text = x.Text }).ToList();
                                if (objNew.LeadAssignToList.Count > 1)
                                {
                                    objNew.LeadAssignToList.Insert(0, new SelectListItemObject { Value = "", Text = "SELECT" });
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
                        objNew.PriorityList = SalesStaticMethods.GetSelectlistItemsByName("Priority", lsEntity, "P");
                        objNew.CurrencyList = SalesStaticMethods.GetSelectlistItemsByName("CURRENCY", lsEntity, "P");
                        objNew.Classification1List = SalesStaticMethods.GetSelectlistItemsByName("CLASSIFICATION1", lsEntity, "P");
                        objNew.Classification2List = SalesStaticMethods.GetSelectlistItemsByName("CLASSIFICATION2", lsEntity, "P");
                        objNew.Classification3List = SalesStaticMethods.GetSelectlistItemsByName("CLASSIFICATION3", lsEntity, "B");
                        objNew.Classification4List = SalesStaticMethods.GetSelectlistItemsByName("CLASSIFICATION4", lsEntity, "P");
                        objNew.SourceTypeList = SalesStaticMethods.GetSelectlistItemsByName("SOURCETYPE", lsEntity, "P");

                    }

                    if (objNew.CustomerSubSegment != null)
                    {
                        var lsSubseg1 = await _serEntity.RetriveByParentId("Lead", "CUSTOMERSUBSEGMENT", objGetLead.CustomerSegment);
                        if (lsSubseg1 != null)
                        {
                            objNew.CustomerSubSegmentList = lsSubseg1.Select(x => new SelectListItemObject { Value = x.PropertyName + "#" + x.PropertyValue, Text = x.PropertyName }).ToList();
                            if (objNew.CustomerSubSegmentList.Count > 1)

                                objNew.CustomerSubSegmentList.Insert(0, new SelectListItemObject { Value = "", Text = "SELECT" });
                        }
                    }
                    #endregion

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
        [HttpPost]
        public async Task<IActionResult> Edit(LeadNewModel objInput)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    AGS_Lead objNewLead = new AGS_Lead();
                    objNewLead.CRMLeadID = objInput.LeadID;
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
                    objNewLead.CustomerTypeID = objInput.CustomerType.Split('#')[1];
                    objNewLead.CustomerType = objInput.CustomerType.Split('#')[0];
                    objNewLead.CustomerClassification = objInput.CustomerClassification.Split('#')[0];
                    objNewLead.CustomerClassificationId = objInput.CustomerClassification.Split('#')[1];

                    objNewLead.BusinessSegment = objInput.BusinessSegment.Split('#')[0];
                    objNewLead.BusinessSegmentID = objInput.BusinessSegment.Split('#')[1];
                    objNewLead.ProductRequired = objInput.ProdcutRequired;
                    objNewLead.Status = objInput.Status;
                    objNewLead.ProjectName = objInput.ProjectName;
                    objNewLead.Priority = objInput.Priority;
                    objNewLead.Probability = objInput.Probability;
                    objNewLead.ContractValue = objInput.ContractValue_IN_LAKHS;
                    objNewLead.Currency = objInput.Currency;
                    objNewLead.CurrencyValue = objInput.CurrencyValue;
                    objNewLead.Description = objInput.Description;
                    //  objNewLead.DocumentCreatedDate = SalesStaticMethods.ConvertDate(objInput.DocumentCreatedDate);
                    objNewLead.LeadMaturityDate = SalesStaticMethods.ConvertDate(objInput.LeadMaturityDate);
                    objNewLead.Classification1 = objInput.Classification1;
                    objNewLead.Classification2 = objInput.Classification2;
                    objNewLead.Classification3 = objInput.Classification3.Split('#')[0];
                    objNewLead.Classificaiton3id = objInput.Classification3.Split('#')[1];
                    objNewLead.Classification4 = objInput.Classification4;
                    objNewLead.Architect = objInput.Architect;
                    objNewLead.Consultant = objInput.Consultant;
                    objNewLead.CreatedOn = DateTime.Now;
                    objNewLead.SourceType = objInput.SourceType;
                    objNewLead.UserID = objInput.LeadAssignTo.Split('#')[1];
                    objNewLead.Username = objInput.LeadAssignTo.Split('#')[0];
                    var blRes = await _serLead.UpdateLead(objInput.LeadID, objNewLead);
                    if (blRes)
                    {
                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Lead", "Lead (" + objInput.LeadID + ") has been updated successfully"));

                        return RedirectToAction("Details", new { leadId = objInput.LeadID, status = "OPEN" });
                    }

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

        public async Task<IActionResult> Attachments(string leadId)
        {

            var objAttachModel = new AttachmentsModel();
            try
            {
                objAttachModel = await _serAttachment.GetAttachmentModel("LEAD", leadId);

            }
            catch (TimeoutException tex)
            {

            }
            catch (Exception ex)
            {

            }
            return View(objAttachModel);

        }
        [HttpPost]
        public async Task<IActionResult> Attachments(AttachmentsModel objInput)
        {
            try
            {
                var jsObject = Newtonsoft.Json.JsonConvert.DeserializeObject<UploadFile>(objInput.PendingUploadFiles);

                if (_serAttachment.VarifyJsonFileObject(jsObject))
                {
                    var lsFilesUploaded = await _serAttachment.UploadAttachments("LEAD", objInput.ActivityId, jsObject);

                    if (lsFilesUploaded.Count > 0)
                    {
                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Lead", "Attachments uploaded successfully"));

                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Lead", "Please add atleast one document"));

                    return RedirectToAction("Attachments", new { leadId = objInput.ActivityId });
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
            return RedirectToAction("Attachments", new { leadId = activityID });
        }

        public async Task<IActionResult> Details(string leadId, string status)
        {
            var detailsObject = new LeadDetailsModel();
            ViewBag.RoleId = HttpContext.Session.GetString("UserRoleId");
            try
            {
                var objLead = await _serLead.RetriveLead(leadId);
                HttpContext.Session.SetObjectAsJson("division", objLead.DivisionName + "#" + objLead.Division);
                var lsFiles = await _serAttachment.GetAttachments("LEAD", leadId);


                detailsObject = new LeadDetailsModel
                {
                    LeadID = objLead.CRMLeadID,
                    Division = objLead.DivisionName,


                    Region = objLead.RegionName,
                    Branch = objLead.BranchName,
                    SalesOffice = objLead.SalesOffice,
                    Plant = objLead.PlantName,
                    CustomerCode = objLead.AccountID,
                    CustomerName = objLead.AccountName,
                    CustomerClassification = objLead.CustomerClassification,
                    CustomerType = objLead.CustomerType,
                    CustomerSegment = objLead.CustomerSegment,
                    CustomerSubSegment = objLead.SubSegment,
                    BusinessSegment = objLead.BusinessSegment,
                    ProdcutRequired = objLead.ProductRequired,
                    ProjectName = objLead.ProjectName,
                    Architect = objLead.Architect,
                    Consultant = objLead.Consultant,
                    ContractValue_IN_LAKHS = objLead.ContractValue,
                    Probability = objLead.Probability,
                    Status = objLead.Status,
                    LeadAssignTo = objLead.Username,
                    LeadMaturityDate = objLead.LeadMaturityDate,
                    // DocumentCreatedDate = objLead.DocumentCreatedDate,
                    Description = objLead.Description,
                    Priority = objLead.Priority,
                    SourceType = objLead.SourceType,
                    Classification1 = objLead.Classification1,
                    Classification2 = objLead.Classification2,
                    Classification3 = objLead.Classification3,
                    Classification4 = objLead.Classification4,
                    Currency = objLead.Currency,
                    Files = lsFiles
                };

                HttpContext.Session.ClearSession("leadEnquiryConvert");
            }
            catch (TimeoutException tex)
            {

            }
            catch (Exception ex)
            {
            }
            return View(detailsObject);
        }

        public async Task<IActionResult> RejectionHistory(string leadId, string status)
        {
            var objRHistory = new LeadHistoryModel();
            try
            {
                objRHistory.LeadId = leadId;
                objRHistory.Status = status;
                objRHistory.HistoryRecords = await _serLead.LeadHistory(leadId);

            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }


            return View(objRHistory);


        }

        [HttpPost]
        public async Task<IActionResult> Cancel(string leadId)
        {
            try
            {
                var blRes = await _serLead.UpdateLead(leadId, new AGS_Lead { Status = "CANCELLED" });
                if (blRes)
                {
                    return Json(new { status = "SUCCESS" });
                }
                else
                {
                    return Json(new { status = "FAILED" });
                }
            }
            catch (TimeoutException tex)
            {
                return Json(new { status = "FAILED", message = "Timeout operation" });
            }
            catch (Exception ex)
            {
                return Json(null);
            }

        }

        public async Task<IActionResult> Search(string strKey)
        {
            var objData = new LeadViewModel();
            try
            {
                objData.SearchKey = strKey.Trim();
                var lsData = await _serLead.SearchLeads(strKey.Trim());
                ViewBag.RoleId = HttpContext.Session.GetString("UserRoleId");

                if (lsData != null && lsData.Count > 0)
                {
                    objData.LeadList = lsData.Select(y => new LeadViewModel.LeadViewItemModel { LeadID = y.CRMLeadID, BusineeSegment = y.BusinessSegment, CustomerName = y.AccountName, ProjectName = y.ProjectName, Status = y.Status, CreatedOn = y.CreatedOn, Division = y.DivisionName, Branch = y.BranchName, Probablity = y.Probability, ContractValue = y.ContractValue }).OrderByDescending(z => z.LeadID).ToList();

                    return View("Index", objData);
                }
                else
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Lead", "No Data available"));

                    //var lsLeads = await _serLead.GetLeadsAsync();
                    //if (lsLeads != null)
                    //{
                    //    objData.LeadList = lsLeads.Select(y => new LeadViewModel.LeadViewItemModel { LeadID = y.CRMLeadID, BusineeSegment = y.BusinessSegment, CustomerName = y.AccountName, Status = y.Status, CreatedOn = y.CreatedOn, Division = y.DivisionName, Branch = y.BranchName, Probablity = y.Probability, ContractValue = y.ContractValue }).OrderByDescending(z => z.LeadID).ToList();

                    //    return View("Index", objData);
                    //}

                }
            }
            catch (TimeoutException tex)
            {

            }
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
        public async Task<IActionResult> ActivityIndex(string leadId, string custname)
        {
            var objConPlan = new ContactPlanViewModel();
            try
            {

                objConPlan = await _serActivity.GetAllActivitiesByDocumentID(leadId);

                if (objConPlan != null)
                {
                    objConPlan.RefObjectID = leadId;
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
        public async Task<IActionResult> ActivityCreate(string leadId, string custname)
        {
            var objContPlan = new ContactPlanNewModel();
            try
            {
                objContPlan = await _serActivity.ActvityCreateObject(leadId, custname);

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
                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Lead Activity", "Activity (" + strResult + ") has been created successfully"));

                        return RedirectToAction("ActivityIndex", new { leadId = objInput.DocumentID, custname = objInput.Name });
                    }

                }

            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {

            }
            return View(objInput);
        }

        public async Task<IActionResult> ActivityEdit(string activityId, string leadId)
        {
            var objAct = new ContactPlanNewModel();
            try
            {
                objAct = await _serActivity.ActvityEditObject(activityId);
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }


            return View(objAct);
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
                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Lead Activity", "Activity (" + objInput.ActivityId + ") has been  updated successfully"));

                        return RedirectToAction("ActivityDetails", "Lead", new { activityId = objInput.ActivityId, leadId = objInput.DocumentID });
                    }
                }
                else
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Lead Activity", "Please provide all Values"));

                    // return RedirectToAction("ActivityDetails", "Lead", new { activityId = objInput.ActivityId });
                }

            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {

            }

            return View(objInput);

        }

        public async Task<IActionResult> ActivityAttachments(string activityId, string leadId, string custname)
        {
            var objAtt = new AttachmentsModel();
            try
            {
                objAtt = await _serAttachment.GetAttachmentModel("ACTIVITY", activityId);
                objAtt.RefObjectId = leadId;
                objAtt.CustomerName = custname;
            }
            catch (TimeoutException tex)
            {

            }
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
                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Documents", "Documents uploaded successfully"));

                        return RedirectToAction("ActivityIndex", new { leadId = objInput.RefObjectId, custname = objInput.CustomerName });
                    }
                }
                else
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Lead Activity", "Please add atleast one document"));

                    return RedirectToAction("ActivityAttachments", new { activityId = objInput.ActivityId, leadId = objInput.RefObjectId, custname = objInput.CustomerName });
                }

            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {


            }
            return View(objInput);
        }
        public async Task<IActionResult> DeleteActivityAttachment(int Id, string activityId, string leadId, string custname)
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
            return RedirectToAction("ActivityAttachments", new { activityId = activityId, leadId = leadId, custname = custname });
        }
        public async Task<IActionResult> ActivityDetails(string activityId, string leadId)
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
            catch (TimeoutException tex)
            {

            }
            catch (Exception ex)
            {
            }
            return Json(new JsonResultObject { Status = blRes });
        }

        #endregion

        /******************************************************************
        * ****************** Convert To Enquiry **********************************
        ******************************************************************/

        #region Convert To Enquiry

        public async Task<IActionResult> ConvertToEnquiry(string leadId, string BusinesSeg, string DocDate)
        {
            var leadConvert = HttpContext.Session.GetObjectFromJson<LeadConvertEnquiryModel>("leadEnquiryConvert");
            try
            {

                if (leadConvert != null)
                {
                    return View(leadConvert);
                }
                else
                {
                    leadConvert = new LeadConvertEnquiryModel();
                    leadConvert.LeadId = leadId;
                    leadConvert.BusinessSegment = BusinesSeg;
                    leadConvert.BusinessSegmentId = BusinesSeg;
                    leadConvert.EnquiryMaturityDate = DateTime.Now.ToString("dd/MM/yyyy");
                    // leadConvert.EnquiryValidDate = DateTime.Now.ToString("dd/MM/yyyy");
                    leadConvert.LeadDocumentCreatedDate = DocDate;

                    HttpContext.Session.SetObjectAsJson("leadEnquiryConvert", leadConvert);

                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {


            }

            return View(leadConvert);
        }
        [HttpPost]
        public async Task<IActionResult> ConvertToEnquiry(LeadConvertEnquiryModel objInput)
        {

            OpportunityProxy.AGS_Opportunity objNewEnquiry = new OpportunityProxy.AGS_Opportunity();
            try
            {
                var enqNewObj = HttpContext.Session.GetObjectFromJson<LeadConvertEnquiryModel>("leadEnquiryConvert");
                if (enqNewObj != null)
                {
                    if (enqNewObj.EnquiryProducts != null && enqNewObj.EnquiryProducts.Count > 0)
                    {
                        var leadDetails = await _serLead.RetriveLead(enqNewObj.LeadId);
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
                            objNewEnquiry.EnquiryMaturityDate = SalesStaticMethods.ConvertDate(objInput.EnquiryMaturityDate);
                            //  objNewEnquiry.EnquiryValidityDate = SalesStaticMethods.ConvertDate(objInput.EnquiryValidDate);
                            //objNewEnquiry.DocumentCreatedDate = SalesStaticMethods.ConvertDate(objInput.EnquiryDocumentCreatedDate);
                            objNewEnquiry.Tonnage = objInput.Tonnage;
                            objNewEnquiry.TotalValue = objInput.TotalValue;

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
                                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Lead", "Lead (" + leadDetails.CRMLeadID + ") has been converted to enquiry (" + strEnq + ") successfully"));

                                    HttpContext.Session.ClearSession("leadEnquiryConvert");
                                    HttpContext.Session.ClearSession("division");
                                    return RedirectToAction("Index");
                                }
                            }
                        }
                    }
                    else
                    {
                        enqNewObj.BusinessSegment = objInput.BusinessSegment;
                        enqNewObj.EnquiryDocumentCreatedDate = objInput.EnquiryDocumentCreatedDate;
                        enqNewObj.EnquiryMaturityDate = objInput.EnquiryMaturityDate;
                        enqNewObj.EnquiryValidDate = objInput.EnquiryValidDate;
                        enqNewObj.LeadDocumentCreatedDate = objInput.LeadDocumentCreatedDate;
                        enqNewObj.LeadId = objInput.LeadId;

                        HttpContext.Session.SetObjectAsJson("leadEnquiryConvert", enqNewObj);

                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Lead", "For enquiry conversion should have atleast one product"));


                        return RedirectToAction("ConvertToEnquiry", new { leadId = objInput.LeadId, BusinesSeg = objInput.BusinessSegment });
                    }
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }
            return View(objInput);
        }

        public async Task<IActionResult> EnquiryItems(string BusinessSegment)//string BusinesSeg,
        {
            var itemsObject = new LeadConvertEnquiryModel();
            try
            {
                itemsObject.BusinessSegment = BusinessSegment;

                var objNewEnq = HttpContext.Session.GetObjectFromJson<LeadConvertEnquiryModel>("leadEnquiryConvert");
                if (objNewEnq != null)
                {
                    itemsObject.LeadId = objNewEnq.LeadId;
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

        public async Task<IActionResult> NavigateEnquiryItems(LeadConvertEnquiryModel objInput)
        {
            try
            {
                var objNew = HttpContext.Session.GetObjectFromJson<LeadConvertEnquiryModel>("leadEnquiryConvert");
                if (objNew != null)
                {
                    objInput.LeadId = objNew.LeadId;
                    objInput.LeadDocumentCreatedDate = objNew.LeadDocumentCreatedDate;
                    objInput.BusinessSegment = objNew.BusinessSegment;

                    objInput.EnquiryProducts = objNew.EnquiryProducts;
                }

                HttpContext.Session.SetObjectAsJson("leadEnquiryConvert", objInput);

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

        public async Task<IActionResult> EnquiryAddProduct(string BusinesSeg)
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
        public async Task<IActionResult> EnquiryAddProduct(EnquiryProduct objInput)
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
                    return RedirectToAction("EnquiryItems", new { BusinessSegment = enqNewObj.BusinessSegment });
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {


            }
            return View(objInput);
        }
        public async Task<IActionResult> EnquiryEditProduct(string productName)
        {
            var editEnquiryProduct = new EnquiryProduct();
            try
            {
                var enqNewObj = HttpContext.Session.GetObjectFromJson<LeadConvertEnquiryModel>("leadEnquiryConvert");
                if (enqNewObj != null)
                {
                    if (enqNewObj.EnquiryProducts != null && enqNewObj.EnquiryProducts.Count > 0)
                    {
                        editEnquiryProduct = enqNewObj.EnquiryProducts.Where(x => x.ProductSeg == productName).FirstOrDefault();

                    }

                    var lsProds = await _serProduct.GetProducts();
                    if (lsProds != null)
                    {
                        var lsSelect = lsProds.Select(x => new SelectListItem { Text = x.Text, Value = x.Text + "#" + x.Value }).ToList();
                        if (lsSelect.Count > 1)
                            lsSelect.Insert(0, new SelectListItem { Text = "SELECT", Value = "" });

                        editEnquiryProduct.ProductList = new SelectList(lsSelect, "Value", "Text");
                        editEnquiryProduct.ProductSeg = editEnquiryProduct.ProductSeg + "#" + editEnquiryProduct.ProductSegID;
                    }
                    editEnquiryProduct.Division = HttpContext.Session.GetObjectFromJson<string>("division");
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {


            }
            return View(editEnquiryProduct);
        }

        [HttpPost]
        public async Task<IActionResult> EnquiryEditProduct(EnquiryProduct objInput)
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
                        double? totVal = 0f;
                        double? totTonn = 0;

                        totVal = objInput.TotalValue - enqNewObj.EnquiryProducts[idx].TotalValue;
                        totTonn = objInput.TotalTonnage - enqNewObj.EnquiryProducts[idx].TotalTonnage;

                        // objInput.ProductSegmentId = objInput.ProductSegment;
                        enqNewObj.TotalValue += totVal;
                        enqNewObj.Tonnage += totTonn;

                        enqNewObj.EnquiryProducts[idx].Quantity = objInput.Quantity;
                        enqNewObj.EnquiryProducts[idx].TotalValue = objInput.TotalValue;
                        enqNewObj.EnquiryProducts[idx].TotalTonnage = objInput.TotalTonnage;
                    }

                    HttpContext.Session.SetObjectAsJson("leadEnquiryConvert", enqNewObj);
                    return RedirectToAction("EnquiryItems", new { BusinessSegment = enqNewObj.BusinessSegment });
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

        public async Task<IActionResult> EnquiryDeleteProduct(string productName)
        {
            try
            {


                var enqNewObj = HttpContext.Session.GetObjectFromJson<LeadConvertEnquiryModel>("leadEnquiryConvert");
                if (enqNewObj != null)
                {
                    if (enqNewObj.EnquiryProducts == null)
                        enqNewObj.EnquiryProducts = new List<EnquiryProduct>();

                    var idxObj = enqNewObj.EnquiryProducts.Where(x => x.ProductSeg == productName).FirstOrDefault();
                    if (idxObj != null)
                    {
                        var idx = enqNewObj.EnquiryProducts.IndexOf(idxObj);

                        enqNewObj.TotalValue -= enqNewObj.EnquiryProducts[idx].TotalValue;
                        enqNewObj.Tonnage -= enqNewObj.EnquiryProducts[idx].TotalTonnage;

                        enqNewObj.EnquiryProducts.RemoveAt(idx);
                    }

                    HttpContext.Session.SetObjectAsJson("leadEnquiryConvert", enqNewObj);
                    return RedirectToAction("ConvertToEnquiry", new { leadId = enqNewObj.LeadId, BusinesSeg = enqNewObj.BusinessSegment });
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {


            }
            return View();
        }


        #endregion


    }
}
