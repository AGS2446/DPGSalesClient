using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using OrderProxy;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using DPGSalesClient.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using ActivityProxy;
using Microsoft.Extensions.Options;
using AuthorizationProxy;

namespace DPGSalesClient.Controllers
{
    public class OrderController : Controller
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
        #endregion

        #region Constructor
        public OrderController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor)
        {
            string strIp = SalesStaticMethods.GetRemoteIp(appSettings);

            _serOrder = new ServiceConnectors.OrderServiceConnector(strIp, httpContextAccessor);
            _serCompititor = new ServiceConnectors.CompititorServiceConnector(strIp, httpContextAccessor);
            _serLocation = new ServiceConnectors.LocationServiceConnector(strIp, httpContextAccessor);
            _serEntity = new ServiceConnectors.EntityMapServiceConnector(strIp, httpContextAccessor);
            _serUser = new ServiceConnectors.UserServiceConnector(strIp, httpContextAccessor);
            _serProduct = new ServiceConnectors.ProductServiceConnector(strIp, httpContextAccessor);

            _serActivity = new Models.BusinessLogic.ActivityBL(strIp, httpContextAccessor);
            _serAttachment = new Models.BusinessLogic.AttachmentBL(strIp, httpContextAccessor);
            _serCustomer = new Models.BusinessLogic.CustomerBL(strIp, httpContextAccessor);

            _logger = loggerFactory.CreateLogger<OrderController>();
            _hostingEnvironment = hostingEnvironment;
        }

        #endregion

        /******************************************************************
         * ****************** Order **********************************
         ******************************************************************/

        #region Order
        public async Task<IActionResult> Index()
        {
            HttpContext.Session.SetString("Controller", "Order");
            var objData = new OrderViewModel();
            try
            {
                if (HttpContext.Session.CheckSession("NavigationList"))
                {
                    AGS_LoginUserInfo logInfo = HttpContext.Session.GetObjectFromJson<AGS_LoginUserInfo>( "LoginUserInfo" );
                    HttpContext.Session.SetString( "UserRoleId", logInfo.RoleInfo[ 0 ].RoleID );
                    ViewBag.RoleId = logInfo.RoleInfo[ 0 ].RoleID;
                    var lsOrder = await _serOrder.RetriveOrders("", "", 0, "", 0);
                    if (lsOrder != null)
                    {
                        objData.OrderList = lsOrder.Select(y => new OrderViewModel.OrderViewItemModel { OrderID = y.CRMOrderID, EnquiryID = y.CRMOPPORTUNITYID, BusineeSegment = y.BusinessSegment, CustomerName = y.AccountName, Status = y.Wonlose, CreatedOn = y.CreatedOn, LeadID = y.CRMLeadID, Division = y.DivisionName, Branch = y.BranchName, ContractValue = y.ContractValue }).ToList();
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
            var objData = new OrderViewModel();
            try
            {
                ViewBag.RoleId = HttpContext.Session.GetString( "UserRoleId" );
                objData.SearchKey = strKey.Trim();
                var lsData = await _serOrder.SearchOrders(strKey.Trim());
                if (lsData != null && lsData.Count > 0)
                {
                    objData.OrderList = lsData.Select(y => new OrderViewModel.OrderViewItemModel { OrderID = y.CRMOrderID, EnquiryID = y.CRMOPPORTUNITYID, BusineeSegment = y.BusinessSegment, CustomerName = y.AccountName, Status = y.Wonlose, CreatedOn = y.CreatedOn, LeadID = y.CRMLeadID, Division = y.DivisionName, Branch = y.BranchName, ContractValue = y.ContractValue }).ToList();
                    return View("Index", objData);
                }
                else
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Order", "No Data available"));
                    //var lsOrder = await _serOrder.RetriveOrders("", "", 0, "", 0);
                    //if (lsOrder != null)
                    //{
                    //    return View("Index", lsOrder.Select(y => new OrderViewModel { OrderID = y.CRMOrderID, EnquiryID = y.CRMOPPORTUNITYID, BusineeSegment = y.BusinessSegment, CustomerName = y.AccountName, Status = y.Status, CreatedOn = y.CreatedOn, LeadID = y.CRMLeadID, Division = y.DivisionName, Branch = y.BranchName, ContractValue = y.ContractValue }).ToList());
                    //}

                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return View("Index", objData);
        }

        public async Task<IActionResult> Attachments(string ordId)
        {
            var objAtt = new AttachmentsModel();
            try
            {
                objAtt = await _serAttachment.GetAttachmentModel("ORDER", ordId);
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
                    var lsFilesUploaded = await _serAttachment.UploadAttachments("ORDER", objInput.ActivityId, jsObject);

                    if (lsFilesUploaded.Count > 0)
                    {
                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("ORDER", "Documents uploaded successfully"));

                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("ORDER", "Please add atleast one document"));

                    return RedirectToAction("Attachments", new { ordId = objInput.ActivityId });
                }



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
            return RedirectToAction("Attachments", new { ordId = activityID });
        }

        public async Task<IActionResult> Details(string ordId, string status)
        {
            var objDetails = new OrderDetailsModel();
            try
            {
                var lsFiles = await _serAttachment.GetAttachments("ORDER", ordId);
                ViewBag.RoleId = HttpContext.Session.GetString( "UserRoleId" );

                var orderDetails = await _serOrder.RetriveOrder(ordId);
                if (orderDetails != null)
                {
                    objDetails = new OrderDetailsModel
                    {
                        OrderID = orderDetails.CRMOrderID,
                        QuoteID = orderDetails.CRMQUOTATIONID,
                        EnquiryID = orderDetails.CRMOPPORTUNITYID,
                        LeadID = orderDetails.CRMLeadID,
                        Status = orderDetails.Status,
                        Division = orderDetails.DivisionName,
                        Region = orderDetails.RegionName,
                        Branch = orderDetails.BranchName,
                        SalesOffice = orderDetails.SalesOffice,
                        Plant = orderDetails.PlantName,
                        CustomerCode = orderDetails.AccountID,
                        CustomerName = orderDetails.AccountName,
                        CustomerClassification = orderDetails.CustomerClassification,
                        CustomerSegment = orderDetails.CustomerSegment,
                        CustomerSubSegment = orderDetails.SubSegment,
                        CustomerType = orderDetails.CustomerType,
                        TotalTonnage = orderDetails.Tonnage,
                        TotalItemValue = orderDetails.TotalValue,
                        BusinessSegment = orderDetails.BusinessSegment,
                        ProjectName = orderDetails.ProjectName,
                        Architect = orderDetails.Architect,
                        Consultant = orderDetails.Consultant,
                        ContractValue = orderDetails.ContractValue,
                        Classification1 = orderDetails.Classification1,
                        Classification2 = orderDetails.Classification2,
                        Classification3 = orderDetails.Classification3,
                        Classification4 = orderDetails.Classification4,
                        AssignTo = orderDetails.Username,
                        CompititorPrice = orderDetails.CompetitorPrice,
                        DocumentCreatedDate = orderDetails.DocumentCreatedDate.HasValue ? orderDetails.DocumentCreatedDate.Value.ToString("dd/MM/yyyy") : "",
                        GrossMargin = orderDetails.GrossMargin,
                        OrderType = orderDetails.OrderType,
                        PONo = orderDetails.PoNo,
                        PODate = orderDetails.PoDate.HasValue ? orderDetails.PoDate.Value.ToString("dd/MM/yyyy") : "",
                        CompName = orderDetails.CompetitorName,
                        Reasons = orderDetails.Reason,
                        TotalCost = orderDetails.TotalCost,
                        TurnOver = orderDetails.TurnOverValue,
                        WonLossValue = orderDetails.Wonlose,
                        Files = lsFiles
                    };
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }


            HttpContext.Session.ClearSession("OrderEdit");

            return View(objDetails);
        }


        public async Task<IActionResult> Edit(string ordId, string strBack)
        {
            try
            {
                if (strBack == "BACK")
                {
                    var objNewOrder = HttpContext.Session.GetObjectFromJson<OrderNewModel>("OrderEdit");
                    if (objNewOrder != null)
                    {
                        if (objNewOrder.Branch != null)
                        {
                            var lsPlans = await _serLocation.RetreiveLocationDetailsByOrg(objNewOrder.Region.Split('#')[1], objNewOrder.Division.Split('#')[1]);
                            if (lsPlans != null)
                            {
                                objNewOrder.BranchList = lsPlans.Branches.Select(x => new SelectListItemObject { Value = x.Name + "#" + x.ID, Text = x.Name }).ToList();
                            }
                        }
                        if (objNewOrder.Plant != null)
                        {
                            var lsPlans = await _serLocation.RetreiveLocationDetailsByOrg(objNewOrder.Branch.Split('#')[1], objNewOrder.Division.Split('#')[1]);
                            if (lsPlans != null)
                            {
                                objNewOrder.PlantsList = lsPlans.Plants.Select(x => new SelectListItemObject { Value = x.Name + "#" + x.Code, Text = x.Name }).ToList();
                            }
                        }

                        if (objNewOrder.CustomerSubSegment != null)
                        {
                            var lsSubseg = await _serEntity.RetriveByParentId("Lead", "CUSTOMERSUBSEGMENT", objNewOrder.CustomerSegment.Split('#')[0]);
                            if (lsSubseg != null)
                            {
                                objNewOrder.CustomerSubSegmentList = lsSubseg.Select(x => new SelectListItemObject { Value = x.PropertyName + "#" + x.PropertyValue, Text = x.PropertyName }).ToList();
                                if (objNewOrder.CustomerSubSegmentList.Count > 1)
                                {
                                    objNewOrder.CustomerSubSegmentList.Insert(0, new SelectListItemObject { Value = "", Text = "SELECT" });
                                }
                            }
                        }

                        HttpContext.Session.SetObjectAsJson("OrderEdit", objNewOrder);
                        return View(objNewOrder);
                    }
                }
                else
                {

                    var objNew = new OrderNewModel();
                    var ordEdit = await _serOrder.RetriveOrder(ordId);
                    if (ordEdit != null)
                    {
                        objNew.OrderID = ordEdit.CRMOrderID;
                        objNew.QuoteID = ordEdit.CRMQUOTATIONID;
                        objNew.EnquiryID = ordEdit.CRMOPPORTUNITYID;
                        objNew.LeadID = ordEdit.CRMLeadID;
                        objNew.Division = ordEdit.DivisionName;// + "#" + ordEdit.Division;
                        objNew.Region = ordEdit.RegionName + "#" + ordEdit.Region;
                        objNew.Branch = ordEdit.BranchName + "#" + ordEdit.Branch;
                        objNew.SalesOffice = ordEdit.SalesOffice;
                        objNew.Plant = ordEdit.PlantName + "#" + ordEdit.PlantID;

                        objNew.CustomerCode = ordEdit.AccountID;
                        objNew.CustomerName = ordEdit.AccountName;
                        objNew.CustomerSegment = ordEdit.CustomerSegment + "#" + ordEdit.CustomerSegmentID;
                        objNew.CustomerSubSegment = ordEdit.SubSegment + "#" + ordEdit.SubSegmentID;
                        objNew.CustomerType = ordEdit.CustomerType + "#" + ordEdit.CustomerTypeID;
                        objNew.CustomerClassification = ordEdit.CustomerClassification + "#" + ordEdit.CustomerClassificationId;


                        objNew.Status = ordEdit.Status;
                        objNew.TotalTonnage = ordEdit.Tonnage;
                        objNew.TotalItemValue = ordEdit.TotalValue;
                        objNew.ProjectName = ordEdit.ProjectName;

                        objNew.OrderType = ordEdit.OrderType;
                        objNew.WonLossValue = ordEdit.Wonlose;
                        objNew.Reasons = ordEdit.Reason;
                        objNew.GrossMargin = ordEdit.GrossMargin;
                        objNew.TurnOver = ordEdit.TurnOverValue;
                        objNew.Reasons = ordEdit.Reason;
                        objNew.ContractValue = ordEdit.ContractValue;
                        objNew.TotalCost = ordEdit.TotalCost;
                        objNew.CompititorPrice = ordEdit.CompetitorPrice;
                        objNew.CompName = ordEdit.CompetitorName;
                        objNew.PONo = ordEdit.PoNo;
                        objNew.PODate = ordEdit.PoDate.HasValue ? ordEdit.PoDate.Value.ToString("dd/MM/yyyy") : "";


                        objNew.Architect = ordEdit.Architect;
                        objNew.Consultant = ordEdit.Consultant;

                        objNew.DocumentCreatedDate = ordEdit.DocumentCreatedDate.HasValue ? ordEdit.DocumentCreatedDate.Value.ToString("dd/MM/yyyy") : "";

                        objNew.BusinessSegment = ordEdit.BusinessSegment + "#" + ordEdit.BusinessSegmentID;
                        objNew.Classification1 = ordEdit.Classification1;
                        objNew.Classification2 = ordEdit.Classification2;
                        objNew.Classification3 = ordEdit.Classification3 + "#" + ordEdit.Classification3ID;
                        objNew.Classification4 = ordEdit.Classification4;
                        objNew.AssignTo = ordEdit.Username;


                        objNew.City = ordEdit.City;
                        objNew.State = ordEdit.State;
                        objNew.MobileNumber = ordEdit.MobileNumber;
                        objNew.Pincode = ordEdit.Pincode;
                        objNew.ContactPerson = ordEdit.ContactName;
                        objNew.CustomerAddress = ordEdit.Address1;
                        #region Account details
                        //var objAcc = await _serCustomer.GetCustomers(ordEdit.AccountName);
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
                            var lsBranches = await _serLocation.RetreiveLocationDetailsByOrg(ordEdit.Region, ordEdit.Division);
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
                                var lsPlans = await _serLocation.RetreiveLocationDetailsByOrg(ordEdit.Branch, ordEdit.Division);
                                if (lsPlans != null)
                                {
                                    objNew.PlantsList = lsPlans.Plants.Select(x => new SelectListItemObject { Value = x.Name + "#" + x.Code, Text = x.Name }).ToList();
                                }
                            }

                        }
                        #endregion

                        #region EntityMap Details


                        var lsEntity = await _serEntity.RetriveByObjectName("LEAD");
                        if (lsEntity != null)//&& lsEntity.Count > 0
                        {
                            objNew.CustomerSegmentList = SalesStaticMethods.GetSelectlistItemsByName("CUSTOMERSEGMENT", lsEntity, "B");
                            objNew.CustomerClassificationList = SalesStaticMethods.GetSelectlistItemsByName("CUSTOMERCLASSIFICATION", lsEntity, "B");
                            objNew.CustomerTypeList = SalesStaticMethods.GetSelectlistItemsByName("CUSTOMERTYPE", lsEntity, "B");
                            objNew.BusinessSegmentList = SalesStaticMethods.GetSelectlistItemsByName("BUSINESSSEGMENT", lsEntity, "B");

                            objNew.Classification1List = SalesStaticMethods.GetSelectlistItemsByName("CLASSIFICATION1", lsEntity, "P");
                            objNew.Classification2List = SalesStaticMethods.GetSelectlistItemsByName("CLASSIFICATION2", lsEntity, "P");
                            objNew.Classification3List = SalesStaticMethods.GetSelectlistItemsByName("CLASSIFICATION3", lsEntity, "B");
                            objNew.Classification4List = SalesStaticMethods.GetSelectlistItemsByName("CLASSIFICATION4", lsEntity, "P");

                        }

                        var lsOrderEntity = await _serEntity.RetriveByObjectName("ORDER");
                        if (lsOrderEntity != null)
                        {
                            objNew.OrderTypeList = SalesStaticMethods.GetSelectlistItemsByName("OrderType", lsOrderEntity, "P");
                            objNew.WonLossValueList = SalesStaticMethods.GetSelectlistItemsByName("Status", lsOrderEntity, "P");
                            objNew.ReasonsList = SalesStaticMethods.GetSelectlistItemsByName("Reason", lsOrderEntity, "P");

                        }
                        var lsCompNames = await _serCompititor.FillCompititors();
                        if (lsCompNames != null)
                        {
                            objNew.CompNameList = lsCompNames.Select(x => new SelectListItemObject { Text = x.Text, Value = x.Value }).ToList();
                        }

                        if (objNew.CustomerSubSegment != null)
                        {
                            var lsSubseg = await _serEntity.RetriveByParentId("Lead", "CUSTOMERSUBSEGMENT", ordEdit.CustomerSegment);
                            if (lsSubseg != null)
                            {
                                objNew.CustomerSubSegmentList = lsSubseg.Select(x => new SelectListItemObject { Value = x.PropertyName + "#" + x.PropertyValue, Text = x.PropertyName }).ToList();
                                if (objNew.CustomerSubSegmentList.Count > 1)

                                    objNew.CustomerSubSegmentList.Insert(0, new SelectListItemObject { Value = "", Text = "SELECT" });
                            }
                        }
                        #endregion


                        if (ordEdit.OrderProducts != null && ordEdit.OrderProducts.Count > 0)
                        {
                            objNew.OrderProducts = ordEdit.OrderProducts.Select(x => new OrderProduct
                            {
                                BusinessSegment = x.MaterialGroup,
                                OrderID = x.CrmOrderID,
                                ProductSegID = x.ProductID,
                                ProductSeg = x.ProductName,
                                Quantity = x.Quantity,
                                TotalTonnage = x.TotalTonnageQuantity,
                                ContractValue = x.ContractValue,
                                TurnoverValue = x.TurnoverValue,
                                MarginValue = x.GrossMargin
                            }).ToList();
                        }


                        if (objNew.DivisionList == null)
                            objNew.DivisionList = new List<SelectListItemObject>();
                        if (objNew.RegionList == null)
                            objNew.RegionList = new List<SelectListItemObject>();
                        if (objNew.BranchList == null)
                            objNew.BranchList = new List<SelectListItemObject>();
                        if (objNew.PlantsList == null)
                            objNew.PlantsList = new List<SelectListItemObject>();

                        HttpContext.Session.SetObjectAsJson("OrderEdit", objNew);
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
        public async Task<IActionResult> Edit(OrderNewModel objInput)
        {

            try
            {

                if (ModelState.IsValid)
                {
                    var objNewOrder = HttpContext.Session.GetObjectFromJson<OrderNewModel>("OrderEdit");
                    if (objNewOrder != null)
                    {
                        if (objNewOrder.OrderProducts != null && objNewOrder.OrderProducts.Count > 0)
                        {
                            try
                            {
                                var objEditOrder = new OrderProxy.AGS_Order();
                                objEditOrder.CRMQUOTATIONID = objInput.QuoteID;
                                objEditOrder.CRMOPPORTUNITYID = objInput.EnquiryID;
                                objEditOrder.CRMLeadID = objInput.LeadID;

                                //   objEditOrder.Division = objInput.Division.Split('#')[1];
                                //   objEditOrder.DivisionName = objInput.Division.Split('#')[0];
                                objEditOrder.Region = objInput.Region.Split('#')[1];
                                objEditOrder.RegionName = objInput.Region.Split('#')[0];
                                objEditOrder.Branch = objInput.Branch.Split('#')[1];
                                objEditOrder.BranchName = objInput.Branch.Split('#')[0];
                                objEditOrder.SalesOffice = objInput.SalesOffice;
                                objEditOrder.PlantID = objInput.Plant.Split('#')[1];
                                objEditOrder.PlantName = objInput.Plant.Split('#')[0];

                                objEditOrder.AccountID = objInput.CustomerCode;
                                objEditOrder.AccountName = objInput.CustomerName;
                                objEditOrder.City = objInput.City;
                                objEditOrder.State = objInput.State;
                                objEditOrder.MobileNumber = objInput.MobileNumber;
                                objEditOrder.Pincode = objInput.Pincode;
                                objEditOrder.ContactName = objInput.ContactPerson;
                                objEditOrder.Address1 = objInput.CustomerAddress;

                                objEditOrder.CustomerClassification = objInput.CustomerClassification.Split('#')[0];
                                objEditOrder.CustomerClassificationId = objInput.CustomerClassification.Split('#')[1];
                                objEditOrder.CustomerSegment = objInput.CustomerSegment.Split('#')[0];
                                objEditOrder.CustomerSegmentID = objInput.CustomerSegment.Split('#')[1];
                                objEditOrder.SubSegment = objInput.CustomerSubSegment.Split('#')[0];
                                objEditOrder.SubSegmentID = objInput.CustomerSubSegment.Split('#')[1];
                                objEditOrder.CustomerType = objInput.CustomerType;


                                objEditOrder.BusinessSegmentID = objInput.BusinessSegment.Split('#')[1];
                                objEditOrder.BusinessSegment = objInput.BusinessSegment.Split('#')[0];
                                objEditOrder.Classification1 = objInput.Classification1;
                                objEditOrder.Classification2 = objInput.Classification2;
                                objEditOrder.Classification3 = objInput.Classification3.Split('#')[0];
                                objEditOrder.Classification3ID = objInput.Classification3.Split('#')[1];
                                objEditOrder.Classification4 = objInput.Classification4;
                                objEditOrder.Consultant = objInput.Consultant;
                                objEditOrder.Architect = objInput.Architect;
                                objEditOrder.ProjectName = objInput.ProjectName;

                                objEditOrder.DocumentCreatedDate = SalesStaticMethods.ConvertDate(objInput.DocumentCreatedDate);


                                // objEditOrder.Status = "OPEN";
                                // objEditOrder.UserID = objInput.AssignTo;
                                // objEditOrder.Username = objInput.AssignTo;


                                objEditOrder.CRMLeadID = objInput.LeadID;
                                objEditOrder.CRMOPPORTUNITYID = objInput.EnquiryID;
                                objEditOrder.CRMQUOTATIONID = objInput.QuoteID;
                                objEditOrder.CRMOrderID = objInput.OrderID;

                                // objEditOrder.ProductRequired = objInput.ProductRequired;
                                objEditOrder.TotalCost = objInput.TotalCost;
                                objEditOrder.TotalValue = objInput.TotalItemValue;
                                objEditOrder.ContractValue = objInput.ContractValue;


                                objEditOrder.PoNo = objInput.PONo;
                                objEditOrder.PoDate = SalesStaticMethods.ConvertDate(objInput.PODate);
                                objEditOrder.OrderType = objInput.OrderType;
                                objEditOrder.Wonlose = objInput.WonLossValue;


                                if (objInput.WonLossValue.ToUpper() == "WIN")
                                {
                                    objEditOrder.GrossMargin = objInput.GrossMargin;
                                    objEditOrder.TurnOverValue = objInput.TurnOver;
                                    objEditOrder.Tonnage = objInput.TotalTonnage;

                                    objEditOrder.CompetitorPrice = null;
                                    objEditOrder.CompetitorName = "";
                                    objEditOrder.Reason = "";
                                }
                                else if (objInput.WonLossValue.ToUpper() == "LOST")
                                {
                                    objEditOrder.CompetitorPrice = objInput.CompititorPrice;
                                    objEditOrder.CompetitorName = objInput.CompName;
                                    objEditOrder.Reason = objInput.Reasons;

                                    objEditOrder.GrossMargin = null;
                                    objEditOrder.TurnOverValue = null;
                                    objEditOrder.Tonnage = null;
                                }

                                var objConOrder = HttpContext.Session.GetObjectFromJson<OrderNewModel>("OrderEdit");
                                if (objConOrder != null && objConOrder.OrderProducts != null && objConOrder.OrderProducts.Count > 0)
                                {
                                    objEditOrder.OrderProducts = objConOrder.OrderProducts.Select(x => new OrderProxy.AGS_OrderProduct
                                    {
                                        EntityState = x.EntityState,
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

                                if (objEditOrder.OrderProducts != null && objEditOrder.OrderProducts.Count > 0)
                                {
                                    var blRes = await _serOrder.UpdateOrder(objEditOrder.CRMOrderID, objEditOrder);
                                    if (blRes)
                                    {

                                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Order", "Order(" + objEditOrder.CRMOrderID + ") has been updated successfully"));

                                        return RedirectToAction("Details", new { ordId = objInput.OrderID, status = "OPEN" });
                                    }
                                }
                                HttpContext.Session.SetString("OrderEdit", "");
                            }
                            catch (Exception ex)
                            {
                            }


                        }
                        else
                        {

                            objInput.DivisionList = objNewOrder.DivisionList;
                            objInput.RegionList = objNewOrder.RegionList;
                            objInput.BranchList = objNewOrder.BranchList;
                            objInput.PlantsList = objNewOrder.PlantsList;
                            objInput.CustomerSegmentList = objNewOrder.CustomerSegmentList;
                            objInput.CustomerSubSegmentList = objNewOrder.CustomerSubSegmentList;
                            objInput.CustomerTypeList = objNewOrder.CustomerTypeList;
                            objInput.CustomerClassificationList = objNewOrder.CustomerClassificationList;
                            objInput.Classification1List = objNewOrder.Classification1List;
                            objInput.Classification2List = objNewOrder.Classification2List;
                            objInput.Classification3List = objNewOrder.Classification3List;
                            objInput.Classification4List = objNewOrder.Classification4List;
                            objInput.BusinessSegmentList = objNewOrder.BusinessSegmentList;
                            objInput.OrderProducts = objNewOrder.OrderProducts;
                            objInput.CompNameList = objNewOrder.CompNameList;
                            objInput.ReasonsList = objNewOrder.ReasonsList;
                            objInput.OrderTypeList = objNewOrder.OrderTypeList;
                            objInput.WonLossValueList = objNewOrder.WonLossValueList;

                            HttpContext.Session.SetObjectAsJson("OrderEdit", objInput);


                            TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Order", "You should provide at least one product"));

                            return RedirectToAction("Edit", new { ordId = objNewOrder.OrderID, stBack = "BACK" });
                        }
                    }
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return View(objInput);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateContractValue(string ordId, double contractValue)
        {
            try
            {
                AGS_UploadContractValue request = new AGS_UploadContractValue();
                request.DocumentId = ordId;
                request.ContractValue = contractValue;

                var blRes = await _serOrder.UploadContractValue(request);
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

        public async Task<IActionResult> NavigateOrderItems(OrderNewModel objInput)
        {
            try
            {
                var objNew = HttpContext.Session.GetObjectFromJson<OrderNewModel>("OrderEdit");
                if (objNew != null)
                {
                    objInput.OrderID = objNew.OrderID;
                    objInput.DivisionList = objNew.DivisionList;
                    objInput.RegionList = objNew.RegionList;
                    objInput.BranchList = objNew.BranchList;
                    objInput.PlantsList = objNew.PlantsList;
                    objInput.CustomerSegmentList = objNew.CustomerSegmentList;
                    objInput.CustomerSubSegmentList = objNew.CustomerSubSegmentList;
                    objInput.CustomerTypeList = objNew.CustomerTypeList;
                    objInput.CustomerClassificationList = objNew.CustomerClassificationList;
                    objInput.Classification1List = objNew.Classification1List;
                    objInput.Classification2List = objNew.Classification2List;
                    objInput.Classification3List = objNew.Classification3List;
                    objInput.Classification4List = objNew.Classification4List;
                    objInput.BusinessSegmentList = objNew.BusinessSegmentList;
                    objInput.OrderTypeList = objNew.OrderTypeList;
                    objInput.WonLossValueList = objNew.WonLossValueList;
                    objInput.CompNameList = objNew.CompNameList;
                    objInput.ReasonsList = objNew.ReasonsList;
                    objInput.OrderProducts = objNew.OrderProducts;
                }

                HttpContext.Session.SetObjectAsJson("OrderEdit", objInput);

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
        public async Task<IActionResult> OrderItems(string BusinessSegment, string ordId)//string BusinesSeg,
        {
            var itemsObject = new OrderItemModel();
            try
            {
                itemsObject.BusinessSegment = BusinessSegment;
                itemsObject.OrderID = ordId;
                var objEditOrder = HttpContext.Session.GetObjectFromJson<OrderNewModel>("OrderEdit");
                if (objEditOrder != null)
                {
                    itemsObject.OrderID = objEditOrder.OrderID;
                    itemsObject.QuoteID = objEditOrder.QuoteID;
                    itemsObject.BusinessSegment = objEditOrder.BusinessSegment;
                    itemsObject.Products = objEditOrder.OrderProducts;
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
            var newOrderProduct = new OrderProduct();
            try
            {
                newOrderProduct.BusinessSegment = BusinesSeg.Split('#')[0];
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
        public async Task<IActionResult> AddProduct(OrderProduct objInput)
        {
            try
            {
                var objConOrder = HttpContext.Session.GetObjectFromJson<OrderNewModel>("OrderEdit");
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

                    HttpContext.Session.SetObjectAsJson("OrderEdit", objConOrder);

                    return RedirectToAction("OrderItems");
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
            var editOrderProduct = new OrderProduct();
            try
            {
                var objConOrder = HttpContext.Session.GetObjectFromJson<OrderNewModel>("OrderEdit");
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
        public async Task<IActionResult> EditProduct(OrderProduct objInput)
        {
            try
            {
                var objConOrder = HttpContext.Session.GetObjectFromJson<OrderNewModel>("OrderEdit");
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

                    HttpContext.Session.SetObjectAsJson("OrderEdit", objConOrder);

                    return RedirectToAction("OrderItems");
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
            var editOrderProduct = new OrderProduct();
            try
            {
                var objNewQte = HttpContext.Session.GetObjectFromJson<OrderNewModel>("OrderEdit");
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

                        HttpContext.Session.SetObjectAsJson("OrderEdit", objNewQte);

                        return RedirectToAction("OrderItems");
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
        public async Task<IActionResult> Customers(string strKey)
        {
            try
            {
                var lsAcc = await _serCustomer.GetCustomers(strKey);

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




        #endregion


        /******************************************************************
     * ****************** Activities **********************************
     ******************************************************************/

        #region Activities
        public async Task<IActionResult> ActivityIndex(string ordId, string custname)
        {
            var objConPlan = new ContactPlanViewModel();
            try
            {

                objConPlan = await _serActivity.GetAllActivitiesByDocumentID(ordId);

                if (objConPlan != null)
                {
                    objConPlan.RefObjectID = ordId;
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
        public async Task<IActionResult> ActivityCreate(string ordId, string custname)
        {
            var objContPlan = new ContactPlanNewModel();
            try
            {
                objContPlan = await _serActivity.ActvityCreateObject(ordId, custname);

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

                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Order Activity", "Activity (" + strResult + ") has been created successfully"));

                        return RedirectToAction("ActivityIndex", new { ordId = objInput.DocumentID, custname = objInput.Name });
                    }

                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return View(objInput);
        }

        public async Task<IActionResult> ActivityEdit(string activityId, string ordId)
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

                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Order Activity", "Activity (" + objInput.ActivityId + ") has been updated successfully"));

                        return RedirectToAction("ActivityDetails", "Order", new { activityId = objInput.ActivityId, ordId = objInput.DocumentID, custname = objInput.Name });
                    }
                }
                else
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Order Activity", "Please provide all Values"));

                    // return RedirectToAction("ActivityDetails", "Lead", new { activityId = objInput.ActivityId });
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }



            return View(objInput);

        }

        public async Task<IActionResult> ActivityAttachments(string activityId, string ordId, string custname)
        {
            var objAtt = new AttachmentsModel();

            try
            {
                objAtt = await _serAttachment.GetAttachmentModel("ACTIVITY", activityId);
                objAtt.RefObjectId = ordId;
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
                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Order Activity", "Attachments uploaded successfully"));

                        return RedirectToAction("ActivityIndex", new { ordId = objInput.RefObjectId, custname = objInput.CustomerName });
                    }
                }
                else
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Order Activity", "Please add atleast one document"));

                    return RedirectToAction("ActivityAttachments", new { activityId = objInput.ActivityId, ordId = objInput.RefObjectId, custname = objInput.CustomerName });
                }



            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }


            return View(objInput);
        }
        public async Task<IActionResult> DeleteActivityAttachment(int Id, string activityId, string ordId, string custname)
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
            return RedirectToAction("ActivityAttachments", new { activityId = activityId, ordId = ordId, custname = custname });
        }
        public async Task<IActionResult> ActivityDetails(string activityId, string ordId)
        {
            var detailsObject = new ContactPlanDetailsModel();
            try
            {
                detailsObject = await _serActivity.ActvityDetailsObject("ACTIVITY", activityId);
                detailsObject.DocumentID = ordId;
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
