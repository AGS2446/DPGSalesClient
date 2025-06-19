using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using DPGSalesClient.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using PurchaseOrderProxy;
using Microsoft.Extensions.Options;
using OrderProxy;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace DPGSalesClient.Controllers
{
    public class SalesOrderController : Controller
    {
        #region Varaibles  
        ServiceConnectors.OrderServiceConnector _serOrder = null;

        ServiceConnectors.AccountServiceConnector _serAccount = null;
        ServiceConnectors.EntityMapServiceConnector _serEntity = null;
        ServiceConnectors.SalesAreaServiceConnector _serSalesArea = null;

        ServiceConnectors.EquipmentServiceConnector _serEquipment = null;
        ServiceConnectors.PurchaseOrderServiceConnector _serPurchaseOrder = null;

        Models.BusinessLogic.AttachmentBL _serAttachment = null;
        Models.BusinessLogic.CustomerBL _serCustomer = null;


        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        #endregion

        #region Constructor
        public SalesOrderController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor)
        {
            string strIp = SalesStaticMethods.GetRemoteIp(appSettings);

            _serOrder = new ServiceConnectors.OrderServiceConnector(strIp, httpContextAccessor);            
            _serAccount = new ServiceConnectors.AccountServiceConnector(strIp, httpContextAccessor);
            _serEntity = new ServiceConnectors.EntityMapServiceConnector(strIp, httpContextAccessor);
            _serSalesArea = new ServiceConnectors.SalesAreaServiceConnector(strIp, httpContextAccessor);
            _serEquipment = new ServiceConnectors.EquipmentServiceConnector(strIp, httpContextAccessor);
            _serPurchaseOrder = new ServiceConnectors.PurchaseOrderServiceConnector(strIp, httpContextAccessor);


            _serAttachment = new Models.BusinessLogic.AttachmentBL(strIp, httpContextAccessor);
            _serCustomer = new Models.BusinessLogic.CustomerBL(strIp, httpContextAccessor);


            _logger = loggerFactory.CreateLogger<SalesOrderController>();
            _hostingEnvironment = hostingEnvironment;
        }

        #endregion        

        #region Order
        public async Task<IActionResult> Index()
        {
            HttpContext.Session.SetString("Controller", "SalesOrder");
            try
            {
                var lsOrder = await _serOrder.RetriveOrders("", "", 0, "", 0);
                if (lsOrder != null)
                {
                       return View("Index", lsOrder.Where(x=>x.DivisionName== "PRODUCT SALES" && x.Status=="Open" && x.Wonlose=="Win" && string.IsNullOrEmpty(x.SAPOrderID)).Select(y => new SalesOrderViewModel { OrderID = y.CRMOrderID, EnquiryID = y.CRMOPPORTUNITYID,QuoteID=y.CRMQUOTATIONID, CustomerName = y.AccountName, TotalValue=y.TotalValue, Status = y.Status, CreatedOn = y.CreatedOn, LeadID = y.CRMLeadID, Division = y.DivisionName, Branch = y.BranchName }).ToList());
                   // return View("Index", lsOrder.Select(y => new SalesOrderViewModel { OrderID = y.CRMOrderID, EnquiryID = y.CRMOPPORTUNITYID, QuoteID = y.CRMQUOTATIONID, CustomerName = y.AccountName, TotalValue = y.TotalValue, Status = y.Status, CreatedOn = y.CreatedOn, LeadID = y.CRMLeadID, Division = y.DivisionName, Branch = y.BranchName }).ToList());
                }
            }
            catch (Exception ex)
            {

            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(string strKey)
        {
            try
            {
                var lsData = await _serOrder.SearchOrders(strKey);
                if (lsData != null && lsData.Count > 0)
                {
                    var lsleads = lsData.Select(y => new SalesOrderViewModel { OrderID = y.CRMOrderID, EnquiryID = y.CRMOPPORTUNITYID, QuoteID = y.CRMQUOTATIONID, CustomerName = y.AccountName, TotalValue = y.TotalValue, Status = y.Status, CreatedOn = y.CreatedOn, LeadID = y.CRMLeadID, Division = y.DivisionName, Branch = y.BranchName }).ToList();
                    return View("Index", lsleads);
                }
                else
                {
                    var lsOrder = await _serOrder.RetriveOrders("", "", 0, "", 0);
                    if (lsOrder != null)
                    {
                        return View("Index", lsOrder.Select(y => new SalesOrderViewModel { OrderID = y.CRMOrderID, EnquiryID = y.CRMOPPORTUNITYID, QuoteID = y.CRMQUOTATIONID, CustomerName = y.AccountName, TotalValue = y.TotalValue, Status = y.Status, CreatedOn = y.CreatedOn, LeadID = y.CRMLeadID, Division = y.DivisionName, Branch = y.BranchName }).ToList());
                    }
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return View("Index");
        }

        public async Task<IActionResult> Details(string ordId, string status)
        {
            var objDetails = new SalesOrderDetailsModel();
            try
            {
                var lsFiles = await _serAttachment.GetAttachments("ORDERS", ordId);

                var orderDetails = await _serOrder.RetriveOrder(ordId);
                if (orderDetails != null)
                {
                    objDetails = new SalesOrderDetailsModel
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


            HttpContext.Session.ClearSession("SalesOrderNew");

            return View(objDetails);
        }
        #endregion

        #region SAP Order
        public async Task<IActionResult> Create(string ordId,string stBack)
        {
            try
            {

                if (stBack == "BACK")
                {
                    var objNewSalesOrder = HttpContext.Session.GetObjectFromJson<SalesOrderNewModel>("SalesOrderNew");
                    if (objNewSalesOrder != null)
                    {
                        if (objNewSalesOrder.OrderType != null)
                        {
                            var lsCondType = await _serEntity.RetriveByParentId("SalesOrder", "ConditionType", objNewSalesOrder.OrderType);
                            if (lsCondType != null && lsCondType.Count > 0)
                            {
                                objNewSalesOrder.ConditionTypeList=lsCondType.Select(y => new SelectListItemObject { Value = y.PropertyName, Text = y.PropertyName }).ToList();
                            }
                        }
                        if (objNewSalesOrder.SalesOrg != null)
                        {
                            var lsDistChannel = await _serSalesArea.GetDistChannel(objNewSalesOrder.SalesOrg);
                            if (lsDistChannel != null && lsDistChannel.Count > 0)
                            {
                               objNewSalesOrder.DistChannelList =lsDistChannel.Select(y => new SelectListItemObject { Value = y.Value, Text = y.Text }).ToList();
                            }
                        }
                        if (objNewSalesOrder.SalesOrg!=null && objNewSalesOrder.DistChannel != null)
                        {
                            var lsDivision = await _serSalesArea.GetDivision(objNewSalesOrder.SalesOrg, objNewSalesOrder.DistChannel);
                            if (lsDivision != null && lsDivision.Count > 0)
                            {
                               objNewSalesOrder.DivisionList=lsDivision.Select(y => new SelectListItemObject { Value = y.Value, Text = y.Text }).ToList();
                            }
                        }

                        HttpContext.Session.SetObjectAsJson("SalesOrderNew", objNewSalesOrder);
                        return View(objNewSalesOrder);
                    }
                }
                else
                {
                    SalesOrderNewModel objNewSalesOrder = new SalesOrderNewModel();

                    var objList = new List<SelectListItemObject>();
                    var lsEntity = await _serEntity.RetriveByObjectName("SalesOrder");
                    if (lsEntity != null)
                    {
                        objNewSalesOrder.OrderTypeList = SalesStaticMethods.GetSelectlistItemsByName("OrderType", lsEntity, "P");
                        if (objNewSalesOrder.OrderTypeList == null)
                        {
                            objNewSalesOrder.OrderTypeList = new List<SelectListItemObject> { new SelectListItemObject { Text = "SELECT", Value = "" } };
                        }
                        var lstDropdownEntity = await _serSalesArea.GetSalesOrg();
                        if (lstDropdownEntity != null && lstDropdownEntity.Count > 0)
                        {
                            objList= lstDropdownEntity.Select(x => new SelectListItemObject { Text = x.Text, Value = x.Value }).ToList();
                            objList.Insert(0, new SelectListItemObject { Text = "SELECT", Value = "" });
                            objNewSalesOrder.SalesOrgList = objList;
                        }
                        objNewSalesOrder.DistChannelList = new List<SelectListItemObject> { new SelectListItemObject { Text = "SELECT", Value = "" } };
                        objNewSalesOrder.DivisionList = new List<SelectListItemObject> { new SelectListItemObject { Text = "SELECT", Value = "" } };
                        objNewSalesOrder.ConditionTypeList = new List<SelectListItemObject> { new SelectListItemObject { Text = "SELECT", Value = "" } };                        

                    }
                    objNewSalesOrder.CRMOrderID = ordId;
                    HttpContext.Session.SetObjectAsJson("SalesOrderNew", objNewSalesOrder);
                    return View(objNewSalesOrder);
                }

            }
            catch (Exception ex)
            {

            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(SalesOrderNewModel model)
        {
            try
            {
                var objNewSalesOrder = HttpContext.Session.GetObjectFromJson<SalesOrderNewModel>("SalesOrderNew");
                if (objNewSalesOrder != null)
                {
                    if (objNewSalesOrder.salesOrderItems != null && objNewSalesOrder.salesOrderItems.Count > 0)
                    {
                        AGS_PurchaseOrder purchaseOrder = new AGS_PurchaseOrder();

                        purchaseOrder.OrderType = model.OrderType;
                        purchaseOrder.SalesOrg = model.SalesOrg;
                        purchaseOrder.DistChannel = model.DistChannel;
                        purchaseOrder.Division = model.Division;
                        purchaseOrder.PONO = model.PONumber;
                        purchaseOrder.PODate = SalesStaticMethods.ConvertDate(model.PODate);

                        purchaseOrder.ConditionType = model.ConditionType;

                        purchaseOrder.AccountID = model.CustomerCode;
                        purchaseOrder.AccountName = model.CustomerName;
                        purchaseOrder.ShiptoPartyID = model.ShipToPartyCode;
                        purchaseOrder.ShiptoPartyName = model.ShipToPartyName;
                        purchaseOrder.CRMOrderNo = model.CRMOrderID;
                        var order = await _serOrder.RetriveOrder(model.CRMOrderID);
                        if (order != null)
                        {
                            purchaseOrder.DivisionID = order.Division;
                            purchaseOrder.DivisionName = order.DivisionName;
                            purchaseOrder.BranchID = order.Branch;
                            purchaseOrder.BranchName = order.BranchName;
                        }
                        purchaseOrder.PurchaseOrderItems = new List<PurchaseOrderProxy.AGS_PurchaseOrderItem>();
                        if (objNewSalesOrder.salesOrderItems.Count > 0)
                        {
                            var lsItems = objNewSalesOrder.salesOrderItems.Select(x => new PurchaseOrderProxy.AGS_PurchaseOrderItem
                            {
                                
                                ProductID = x.MaterialNo,
                                ProductName = x.MaterialDesc,
                                PlantID=x.Plant,
                                PlantName=x.Plant,
                                Quantity = x.Quantity,
                                TotalValue = x.TotalValue,
                                EntityState = x.EntityState
                                
                            }).ToList();

                            purchaseOrder.PurchaseOrderItems = lsItems;
                        }

                        if (purchaseOrder.PurchaseOrderItems.Count > 0)
                        {
                            var strPO = await _serPurchaseOrder.Create(purchaseOrder);
                            if (strPO != "")
                            {
                                AGS_Order crmOrder = new AGS_Order();
                                crmOrder.Status = "Converted";
                                var orderUpdate = await _serOrder.UpdateOrder(model.CRMOrderID, crmOrder);
                                if (orderUpdate)
                                {
                                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("SalesOrder", "Sales Order created successfully"));
                                }
                                
                                return RedirectToAction("SAPOrders");
                            }
                        }
                        HttpContext.Session.SetString("SalesOrderNew", "");
                    }
                    else
                    {
                        model.OrderTypeList = objNewSalesOrder.OrderTypeList;
                        model.SalesOrgList = objNewSalesOrder.SalesOrgList;
                        model.DistChannelList = objNewSalesOrder.DistChannelList;
                        model.DivisionList = objNewSalesOrder.DivisionList;
                        model.ConditionTypeList = objNewSalesOrder.ConditionTypeList;

                        HttpContext.Session.SetObjectAsJson("SalesOrderNew", model);

                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("SalesOrder", "You should provide at least one Item"));

                        return RedirectToAction("Create", new { stBack = "BACK" });
                    }
                }                        
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {

            }
            return View();
        }

        public async Task<IActionResult> SAPOrders()
        {
            try
            {
                var lsSAPOrder = await _serPurchaseOrder.GetSAPOrders();
                if (lsSAPOrder != null)
                {                  
                    return View("SAPOrders", lsSAPOrder.Select(y => new SAPOrdersViewModel { CRMSalesOrderID = y.PurchaseOrderID, SAPOrderNO = y.SAPOrderID, OrderID = y.CRMOrderNo, OrderType = y.OrderType, Customer = y.AccountName, SAPRemarks = y.Remarks, CreatedOn = y.CreatedOn }).ToList());
                }
            }
            catch (Exception ex)
            {

            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SAPSearch(string strKey)
        {
            try
            {
                var lsData = await _serPurchaseOrder.Search(strKey);
                if (lsData != null && lsData.Count > 0)
                {
                    var lsleads = lsData.Select(y => new SAPOrdersViewModel { CRMSalesOrderID = y.PurchaseOrderID, SAPOrderNO = y.SAPOrderID, OrderID = y.CRMOrderNo, OrderType = y.OrderType, Customer = y.AccountName, SAPRemarks = y.Remarks, CreatedOn = y.CreatedOn }).ToList();
                    return View("SAPOrders", lsleads);
                }
                else
                {
                    var lsOrder = await _serPurchaseOrder.GetSAPOrders();
                    if (lsOrder != null)
                    {
                        return View("SAPOrders", lsOrder.Select(y => new SAPOrdersViewModel { CRMSalesOrderID = y.PurchaseOrderID, SAPOrderNO = y.SAPOrderID, OrderID = y.CRMOrderNo, OrderType = y.OrderType, Customer = y.AccountName, SAPRemarks = y.Remarks, CreatedOn = y.CreatedOn }).ToList());
                    }
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return View("SAPOrders");
        }

        #endregion

        #region Local Methods

        [HttpPost]
        public async Task<IActionResult> Customers(string strKey)
        {
            try
            {
                var lsAcc = await _serCustomer.GetCustomers(strKey);
                if (lsAcc != null && lsAcc.Count > 0)
                {
                    return Json(lsAcc);
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
        public async Task<IActionResult> Materials(string strKey)
        {
            try
            {
                var lsEquip = await _serEquipment.GetEquipments(strKey);
                if (lsEquip != null && lsEquip.Count > 0)
                {
                    return Json(lsEquip);
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
        public async Task<IActionResult> GetDistChannel(string strKey)
        {
            try
            {
                var lsDistChannel = await _serSalesArea.GetDistChannel(strKey);
                if (lsDistChannel != null && lsDistChannel.Count > 0)
                {
                    return Json(lsDistChannel.Select(y => new DropdownObject { Value = y.Value, Text = y.Text }).ToList());
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
        public async Task<IActionResult> GetDivision(string strSalesOrg,string strDistChannel)
        {
            try
            {
                var lsDivision = await _serSalesArea.GetDivision(strSalesOrg, strDistChannel);
                if (lsDivision != null && lsDivision.Count > 0)
                {
                    return Json(lsDivision.Select(y => new DropdownObject { Value = y.Value, Text = y.Text }).ToList());
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
        public async Task<IActionResult> GetPlant(string strMaterial)
        {
            try
            {
                var lsPlant = await _serEquipment.GetPlants(strMaterial);
                if (lsPlant != null && lsPlant.Count > 0)
                {
                    return Json(lsPlant.Select(y => new DropdownObject { Value = y.Value, Text = y.Text }).ToList());
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
        public async Task<IActionResult> GetConditionType(string strOrderType)
        {
            try
            {
                var lsCondType = await _serEntity.RetriveByParentId("SalesOrder", "ConditionType", strOrderType);
                if (lsCondType != null && lsCondType.Count > 0)
                {
                    return Json(lsCondType.Select(y => new DropdownObject { Value = y.PropertyName, Text = y.PropertyName }).ToList());
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

        public async Task<IActionResult> NavigateSOItems(SalesOrderNewModel objInput)
        {
            try
            {
                var objNew = HttpContext.Session.GetObjectFromJson<SalesOrderNewModel>("SalesOrderNew");
                if (objNew != null)
                {
                    objInput.SalesOrderID = objNew.SalesOrderID;
                    objInput.OrderTypeList = objNew.OrderTypeList;
                    objInput.SalesOrgList = objNew.SalesOrgList;
                    objInput.DistChannelList = objNew.DistChannelList;
                    objInput.DivisionList = objNew.DivisionList;
                    objInput.ConditionTypeList = objNew.ConditionTypeList;                  
                }

                HttpContext.Session.SetObjectAsJson("SalesOrderNew", objInput);

                string status = "SUCCESS";               
                return Json(new { status = status, ORderID = objInput.CRMOrderID });
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return Json(null);
        }


        public async Task<IActionResult> SalesOrderItems(string ordId)
        {
            var itemsObject = new SalesOrderItemModel();
            try
            {
                    itemsObject.CRMOrderID= ordId;                
                var objEditOrder = HttpContext.Session.GetObjectFromJson<SalesOrderNewModel>("SalesOrderNew");
                if (objEditOrder != null)
                {
                    itemsObject.SalesOrderID = objEditOrder.SalesOrderID;                    
                    itemsObject.salesOrderItems = objEditOrder.salesOrderItems;
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return View(itemsObject);
        }


        public async Task<IActionResult> AddItem()
        {
            var newOrderItem = new SalesOrderItem();
            try
            {

                newOrderItem.PlantList = new List<SelectListItemObject> { new SelectListItemObject { Text = "SELECT", Value = "" } };

            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return View(newOrderItem);
        }

        [HttpPost]
        public async Task<IActionResult> AddItem(SalesOrderItem objInput)
        {
            try
            {
                var objSalesOrder = HttpContext.Session.GetObjectFromJson<SalesOrderNewModel>("SalesOrderNew");
                if (objSalesOrder != null)
                {
                    if (objSalesOrder.salesOrderItems == null)
                    {
                        objSalesOrder.salesOrderItems = new List<SalesOrderItem>();
                    }

                    if (objInput != null)
                    {
                        if (objInput.MaterialNo != null)
                        {
                            
                            objInput.MaterialNo = objInput.MaterialNo;
                            objInput.MaterialDesc = objInput.MaterialDesc;
                            objInput.Plant = objInput.Plant;                            
                            var idxObj = objSalesOrder.salesOrderItems.Where(x => x.MaterialNo == objInput.MaterialNo && x.Plant==objInput.Plant).FirstOrDefault();
                            if (idxObj != null)
                            {
                                var idx = objSalesOrder.salesOrderItems.IndexOf(idxObj);

                                if (objSalesOrder.salesOrderItems[idx].EntityState == PurchaseOrderProxy.EntityState.Deleted && !String.IsNullOrEmpty(objSalesOrder.salesOrderItems[idx].SalesOrderID))
                                {

                                    objSalesOrder.TotalValue += objInput.TotalValue;
                                    objSalesOrder.salesOrderItems[idx].EntityState = PurchaseOrderProxy.EntityState.Modified;
                                    objSalesOrder.salesOrderItems[idx].TotalValue = objInput.TotalValue;
                                    objSalesOrder.salesOrderItems[idx].Quantity = objInput.Quantity;
                                    

                                }
                                else
                                {
                                    objSalesOrder.salesOrderItems[idx].Quantity += objInput.Quantity;
                                    objSalesOrder.salesOrderItems[idx].TotalValue += objInput.TotalValue;

                                    if (!String.IsNullOrEmpty(objSalesOrder.salesOrderItems[idx].SalesOrderID))
                                    {
                                        objSalesOrder.salesOrderItems[idx].EntityState = PurchaseOrderProxy.EntityState.Modified;
                                    }


                                    objSalesOrder.TotalValue += objInput.TotalValue;
                                }
                            }
                            else
                            {

                                objSalesOrder.TotalValue += objInput.TotalValue;
                                objSalesOrder.Quantity += objInput.Quantity;
                                

                                objInput.EntityState = PurchaseOrderProxy.EntityState.Added;
                                objSalesOrder.salesOrderItems.Add(objInput);
                                
                            }

                        }
                    }

                    HttpContext.Session.SetObjectAsJson("SalesOrderNew", objSalesOrder);

                    return RedirectToAction("SalesOrderItems",new { ordId= objSalesOrder.CRMOrderID });
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return View(objInput);
        }


        public async Task<IActionResult> EditItem(string materialNo,string plant)
        {
            var editOrderItem = new SalesOrderItem();
            try
            {
                var objSalesOrder = HttpContext.Session.GetObjectFromJson<SalesOrderNewModel>("SalesOrderNew");
                if (objSalesOrder != null)
                {
                    if (objSalesOrder.salesOrderItems != null && objSalesOrder.salesOrderItems.Count > 0)
                    {
                        editOrderItem = objSalesOrder.salesOrderItems.Where(x => x.MaterialNo == materialNo && x.Plant==plant).FirstOrDefault();

                        editOrderItem.PlantList = new List<SelectListItemObject> { new SelectListItemObject { Text = "SELECT", Value = "" } };                        
                    }
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }


            return View(editOrderItem);
        }

        [HttpPost]
        public async Task<IActionResult> EditItem(SalesOrderItem objInput)
        {
            try
            {
                var objSalesOrder = HttpContext.Session.GetObjectFromJson<SalesOrderNewModel>("SalesOrderNew");
                if (objSalesOrder != null)
                {
                    if (objSalesOrder.salesOrderItems == null)
                    {
                        objSalesOrder.salesOrderItems = new List<SalesOrderItem>();
                    }

                    if (objInput != null)
                    {
                        if (objInput.MaterialNo != null)
                        {
                            objInput.MaterialNo = objInput.MaterialNo;
                            objInput.MaterialDesc = objInput.MaterialDesc;
                            objInput.Plant = objInput.Plant;

                            var idxObj = objSalesOrder.salesOrderItems.Where(x => x.MaterialNo == objInput.MaterialNo && x.Plant==objInput.Plant).FirstOrDefault();
                            if (idxObj != null)
                            {
                                var idx = objSalesOrder.salesOrderItems.IndexOf(idxObj);

                                double? totValue = 0f;



                                objSalesOrder.salesOrderItems[idx].Quantity = objInput.Quantity;
                                objSalesOrder.salesOrderItems[idx].TotalValue = objInput.TotalValue;
                                

                                if (!String.IsNullOrEmpty(objSalesOrder.salesOrderItems[idx].SalesOrderID))
                                {
                                    objSalesOrder.salesOrderItems[idx].EntityState = PurchaseOrderProxy.EntityState.Modified;
                                }
                                else
                                {
                                    objSalesOrder.salesOrderItems[idx].EntityState = PurchaseOrderProxy.EntityState.Added;
                                }
                            }

                        }
                    }

                    HttpContext.Session.SetObjectAsJson("SalesOrderNew", objSalesOrder);

                    return RedirectToAction("SalesOrderItems",new { ordId = objSalesOrder.CRMOrderID });
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return View(objInput);
        }


        public async Task<IActionResult> DeleteItem(string materialNo, string plant)
        {
            var editOrderItem = new SalesOrderItem();
            try
            {
                var objNewOrder = HttpContext.Session.GetObjectFromJson<SalesOrderNewModel>("SalesOrderNew");
                if (objNewOrder != null)
                {
                    if (objNewOrder.salesOrderItems != null && objNewOrder.salesOrderItems.Count > 0)
                    {
                        editOrderItem = objNewOrder.salesOrderItems.Where(x => x.MaterialNo == materialNo && x.Plant==plant).FirstOrDefault();

                        var idx = objNewOrder.salesOrderItems.IndexOf(editOrderItem);                        

                        if (!String.IsNullOrEmpty(objNewOrder.salesOrderItems[idx].SalesOrderID))
                        {
                            objNewOrder.salesOrderItems[idx].EntityState = PurchaseOrderProxy.EntityState.Deleted;
                        }
                        else
                        {
                            objNewOrder.salesOrderItems.RemoveAt(idx);
                        }

                        HttpContext.Session.SetObjectAsJson("SalesOrderNew", objNewOrder);

                        return RedirectToAction("SalesOrderItems",new { ordId = objNewOrder.CRMOrderID });
                    }
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
