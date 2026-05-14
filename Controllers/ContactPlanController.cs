using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DPGSalesClient.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using ActivityProxy;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

using System.Net.Http;
using Microsoft.Extensions.Options;

namespace DPGSalesClient.Controllers
{
    public class ContactPlanController : Controller
    {
        #region Varaibles
       
        Models.BusinessLogic.ActivityBL _serActivity = null;
        Models.BusinessLogic.AttachmentBL _serAttachment = null;
        Models.BusinessLogic.CustomerBL _serCustomer = null;

        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;
        #endregion

        #region Constructure
        public ContactPlanController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor)
        {
            string strIp = SalesStaticMethods.GetRemoteIp(appSettings);
            _serActivity = new Models.BusinessLogic.ActivityBL(strIp, httpContextAccessor);
            _serAttachment = new Models.BusinessLogic.AttachmentBL(strIp, httpContextAccessor);
            _serCustomer = new Models.BusinessLogic.CustomerBL(strIp, httpContextAccessor);

            _logger = loggerFactory.CreateLogger<ContactPlanController>();
            _hostingEnvironment = hostingEnvironment;
        }

        #endregion

        /******************************************************************
         * ****************** Activity **********************************
         ******************************************************************/
        public async Task<IActionResult> Index()
        {
            HttpContext.Session.SetString("Controller", "ContactPlan");
            try
            {
                if (HttpContext.Session.CheckSession("NavigationList"))
                {
                    var lsData = await _serActivity.GetAllActivities();
                    if (lsData != null && lsData.ActivityList != null && lsData.ActivityList.Count > 0)
                    {
                        lsData.ActivityList = lsData.ActivityList.OrderByDescending(x => x.ActivityID).ToList();
                        return View(lsData);
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
            return View();
        }
        public async Task<IActionResult> Create()
        {
            var objContPlan = new ContactPlanNewModel();
            try
            {
                objContPlan = await _serActivity.ActvityCreateObject();
               
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return View(objContPlan);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ContactPlanNewModel objInput)
        {
            try
            {
                if (ModelState.IsValid)
                {                  
                    var strResult = await _serActivity.ActivityCreate(objInput);

                    if (strResult != "")
                    {
                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Activity", "Activity (" + strResult + ") has been created successfully"));

                        return RedirectToAction("Index");
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
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }
            return Json(null);
        }

        public async Task<IActionResult> Edit(string activityId)
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
        public async Task<IActionResult> Edit(ContactPlanNewModel objInput)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var blRes = await _serActivity.ActivityEdit(objInput);
                    if (blRes)
                    {

                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Activity", "Activity (" + objInput.ActivityId + ") has been updated successfully"));

                        return RedirectToAction("Details", "ContactPlan", new { activityId = objInput.ActivityId });
                    }
                }else
                {
                    //Validation Failed
                    //TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Activity", "Not Updated successfully"));
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

        public async Task<IActionResult> Attachments(string activityId)
        {
            var objAtt = new AttachmentsModel();
            try
            {
                objAtt= await _serAttachment.GetAttachmentModel("ACTIVITY", activityId);              
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
                var lsFilesUploaded = await _serAttachment.UploadAttachments("ACTIVITY", objInput.ActivityId, jsObject);

                if (lsFilesUploaded.Count > 0)
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Activity", "Attachments uploaded successfully"));

                    return RedirectToAction("Index");
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

        public async Task<IActionResult> Details(string activityId)
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
            try
            {             
                var blRes = await _serActivity.CloseActivity(activityId, strOutcome);
                return Json(new JsonResultObject { Status = blRes });
            }
            catch (TimeoutException tex)
            {

            }
            catch (Exception ex)
            {
            }
            return Json(null);
        }

        public async Task<IActionResult> Search(string strKey)
        {
            var objData = new ContactPlanViewModel();
            try
            {
                if (string.IsNullOrEmpty(strKey))
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Activity", "No Data Available"));
                    return RedirectToAction("Index");
                }
                objData = await _serActivity.Search(strKey);
                if (objData != null )
                {
                    objData.SearchName = strKey;
                    if (objData.IsNoData)
                       TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Activity", "No Data Available"));

                    if(objData.ActivityList!=null && objData.ActivityList.Count > 0)
                    {                        
                        objData.ActivityList = objData.ActivityList.OrderByDescending(x => x.ActivityID).ToList();
                    }
                }
                else                
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Activity", "No Data Available"));

                return View("Index",objData);

            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }
            return View("Index", objData);
        }

       

    }



}
