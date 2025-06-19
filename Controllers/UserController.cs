using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using DPGSalesClient.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using UserProxy;
using AuthorizationProxy;
using System.IO;
using OfficeOpenXml;
using System.Reflection;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace DPGSalesClient.Controllers
{
    public class UserController : Controller
    {

        #region Varaibles

        ServiceConnectors.UserServiceConnector _serUser = null;
        ServiceConnectors.RoleServiceConnector _serRole = null;
        ServiceConnectors.ClientServiceConnector _serClient = null;
        ServiceConnectors.LineOfBusinessServiceConnector _serLineOfBusiness = null;
        ServiceConnectors.OrganizationServiceConnector _serOrganization = null;

        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        #endregion

        #region Constructor
        public UserController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor)
        {
            string strIp = SalesStaticMethods.GetRemoteIp(appSettings);
            _serUser = new ServiceConnectors.UserServiceConnector(strIp, httpContextAccessor);
            _serRole = new ServiceConnectors.RoleServiceConnector(strIp, httpContextAccessor);
            _serClient = new ServiceConnectors.ClientServiceConnector(strIp, httpContextAccessor);
            _serLineOfBusiness = new ServiceConnectors.LineOfBusinessServiceConnector(strIp, httpContextAccessor);
            _serOrganization = new ServiceConnectors.OrganizationServiceConnector(strIp, httpContextAccessor);

            _logger = loggerFactory.CreateLogger<UserController>();
            _hostingEnvironment = hostingEnvironment;
        }
        #endregion

        #region Actions

        #region List

        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            HttpContext.Session.SetString("Controller", "User");
            var objData = new UserViewModel();
            try
            {
                if (HttpContext.Session.CheckSession("NavigationList"))
                {
                    if (HttpContext.Session.CheckSession("LoginUserInfo"))
                    {
                        var userInfo = HttpContext.Session.GetObjectFromJson<AGS_LoginUserInfo>("LoginUserInfo");
                        if (userInfo != null && userInfo.RoleInfo != null && userInfo.RoleInfo.Count > 0)
                            ViewBag.RoleID = userInfo.RoleInfo[0].RoleID;
                    }
                    var lsUsers = await _serUser.GetUsersAsync();
                    if (lsUsers != null)
                    {
                        objData.UserList = lsUsers.Select(y => new UserViewModel.UserViewItemModel
                        {
                            UserID = y.UserID,
                            Email = y.EmailID,
                            FirstName = y.Firstname,
                            LastName = y.LastName,
                            LogonName = y.LogonName,
                            Mobile = y.MobileNumber,
                            ValidFrom = y.ValidFrom,
                            ValidTo = y.ValidTo,
                            Status=y.Status
                        }).OrderByDescending(z => z.UserID).ToList();

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

        public async Task<IActionResult> Search(string strKey)
        {
            var objData = new UserViewModel();
            try
            {
                objData.SearchKey = strKey.Trim();
                var lsData = await _serUser.SearchUsers(strKey.Trim());
                if (lsData != null && lsData.Count > 0)
                {
                    objData.UserList = lsData.Select(y => new UserViewModel.UserViewItemModel
                    {
                        UserID = y.UserID,
                        Email = y.EmailID,
                        FirstName = y.Firstname,
                        LastName = y.LastName,
                        LogonName = y.LogonName,
                        Mobile = y.MobileNumber,
                        ValidFrom = y.ValidFrom,
                        ValidTo = y.ValidTo
                    }).OrderByDescending(z => z.UserID).ToList();

                    return View("Index", objData);
                }
                else
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("User", "No Data available"));
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

        #endregion

        #region Create

        public IActionResult Create()
        {
            try
            {
                UserNewModel objNew = new UserNewModel();
                return View(objNew);
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
        public async Task<IActionResult> Create(UserNewModel objInput)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    AGS_User objNewUser = new AGS_User();
                    objNewUser.LogonName = objInput.LogonName;
                    objNewUser.Firstname = objInput.FirstName;
                    objNewUser.LastName = objInput.LastName;
                    objNewUser.MobileNumber = objInput.Mobile;
                    objNewUser.EmailID = objInput.Email;
                                        
                    var strRes = await _serUser.CreateUser(objNewUser);
                    if (strRes != null && strRes != "")
                    {
                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("User", "User (" + strRes + ") has been created successfully"));
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("User", "User creation failed"));
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("User", "Please provide required fields data"));
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

        #endregion

        #region Edit
        public async Task<IActionResult> Edit(string id)
        {
            var editObject = new UserNewModel();
            try
            {
                var objUser = await _serUser.UserDetails(id);
                if (objUser == null)
                {
                    return RedirectToAction("Index", "User");
                }
                editObject.UserID = objUser.UserID;
                editObject.FirstName = objUser.Firstname;
                editObject.LastName = objUser.LastName;
                editObject.LogonName = objUser.LogonName;
                editObject.Mobile = objUser.MobileNumber;
                editObject.Email = objUser.EmailID;
                editObject.Status = objUser.Status;
            }
            catch (TimeoutException tex)
            {

            }
            catch (Exception ex)
            {

            }

            return View(editObject);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserNewModel objInput)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    AGS_User objEditUser = new AGS_User();
                    objEditUser.LogonName = objInput.LogonName;
                    objEditUser.Firstname = objInput.FirstName;
                    objEditUser.LastName = objInput.LastName;
                    objEditUser.MobileNumber = objInput.Mobile;
                    objEditUser.EmailID = objInput.Email;
                    objEditUser.Status = objInput.Status;
                    var strRes = await _serUser.UpdateUser(objInput.UserID,objEditUser);
                    if (strRes)
                    {
                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("User", "User (" + objInput.UserID + ") has been updated successfully"));
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("User", "User updation failed"));
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("User", "Please provide required fields data"));
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

        #endregion

        #region Details

            #region Get
            /// <summary>
            /// 
            /// </summary> 
            /// <param name="id"></param>
            /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var detailsObject = new UserDetailsViewModel();
            try
            {
                var objUser = await _serUser.UserDetails(id);
                if (objUser == null)
                {
                    return RedirectToAction("Index", "User");
                }
                var roles = await _serUser.GetUserRoles(id);
                var positions = await _serUser.GetUserPositions(id);
                var reportingUsers = await _serUser.GetUserReportings(id);

                detailsObject = new UserDetailsViewModel
                {
                    UserID = objUser.UserID,
                    Email = objUser.EmailID,
                    FirstName = objUser.Firstname,
                    LastName = objUser.LastName,
                    LogonName = objUser.LogonName,
                    Mobile = objUser.MobileNumber,
                    Status=objUser.Status,
                    Roles = roles.ToList().Select(x => new UserRoleViewModel()
                    {
                        UserID = objUser.UserID,
                        RoleID = x.RoleID,
                        RoleName = x.RoleName,
                        Description = x.RoleDescription
                    }).ToList(),
                    Positions = positions.ToList().Select(x => new UserPositionViewModel()
                    {
                        UserID = objUser.UserID,
                        RoleID = x.RoleID,
                        RoleName = x.RoleName,
                        ClientID = x.ClientID,
                        ClientName = x.ClientName,
                        LobID = x.LobID,
                        LobName = x.LobName,
                        LogonName = x.LogonName,
                        OrgID = x.OrgID,
                        OrgName = x.OrgName,
                        UserName = x.UserName
                    }).ToList(),
                    Reportings = reportingUsers.ToList().Select(x => new UserReportingViewModel()
                    {
                        UserID = x.UserID,
                        ReportingUserID = x.ReportingID,
                        ReportingEmail = x.ReportingEmailID,
                        ReportingLogonName = x.ReportingLogonName,
                        ReportingMobile = x.ReportingMobileNumber,
                        ReportingUserName = x.ReportingName
                    }).ToList()
                };
            }
            catch (TimeoutException tex)
            {

            }
            catch (Exception ex)
            {
            }
            return View(detailsObject);
        }

        #endregion
       

        #endregion

        #region Assign Role


            #region Get

        public async Task<IActionResult> AssignRole(string id)
        {
            HttpContext.Session.SetString("Controller", "User");
            ModifyUserRoleViewModel model = new ModifyUserRoleViewModel();
            try
            {
                if (id != null && id.Trim().Length > 0)
                {
                    model.UserID = id;
                    var roles = await _serRole.GetRolesAsync();
                    var userRoles = await _serUser.GetUserRoles(id);
                    if (roles != null && userRoles != null)
                    {
                        model.Roles = roles.Where(r => !userRoles.Any(ur => ur.RoleID == r.RoleID)).Select(x => new SelectListItemObject { Value = x.RoleName + "#" + x.RoleID, Text = x.RoleName }).ToList();
                        //if (model.Roles.Count > 1)
                        //{
                        //    model.Roles.Insert(0, new SelectListItemObject { Value = "", Text = "SELECT" });
                        //}
                    }
                    return View(model);
                }
                else
                {
                    return RedirectToAction("Index", "User");
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        #endregion

        #region Post
        [HttpPost]
        public async Task<IActionResult> AssignRole(ModifyUserRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    AGS_UserRole objUserRole = new AGS_UserRole();
                    objUserRole.EntityState = EntityState.Added;
                    objUserRole.UserID = model.UserID;
                    objUserRole.RoleID = model.RoleID.Split('#')[1];
                    var result = await _serUser.ModifyUserRoleAsync(objUserRole);
                    if (result)
                    {
                        #region PopUp

                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Role Assign", "Role (" + model.RoleID.Split('#')[0] + ") has been assigned to User (" + model.UserID + ")"));
                        return RedirectToAction("Details", new { id = model.UserID });
                        #endregion
                    }
                    else
                    {
                        ModelState.AddModelError("", "Something went wrong. Either Re-try the process by checking the entries or please contact support team.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Exception : " + ex.Message);
                }
                return View(model);
            }
            var roles = await _serRole.GetRolesAsync();
            var userRoles = await _serUser.GetUserRoles(model.UserID);
            model.Roles = roles.Where(r => !userRoles.Any(ur => ur.RoleID == r.RoleID)).Select(x => new SelectListItemObject { Value = x.RoleName + "#" + x.RoleID, Text = x.RoleName }).ToList();
            return View(model);
        }

        #endregion

        #endregion

        #region Remove Role

        [HttpPost]
        public async Task<IActionResult> RemoveRole(string strUserID, string strRoleID)
        {
            if (strUserID != null && strUserID.Trim().Length > 0 && strRoleID != null && strRoleID.Trim().Length > 0)
            {
                try
                {
                    AGS_UserRole objUserRole = new AGS_UserRole();
                    objUserRole.EntityState = EntityState.Deleted;
                    objUserRole.UserID = strUserID;
                    objUserRole.RoleID = strRoleID;
                    var result = await _serUser.ModifyUserRoleAsync(objUserRole);
                    if (result)
                    {
                        #region PopUp

                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Role Assign", "Role (" + strRoleID + ") has been assigned to User (" + strUserID + ")"));
                        return Json(true);
                        #endregion
                    }
                    else
                    {
                        ModelState.AddModelError("", "Something went wrong. Either Re-try the process by checking the entries or please contact support team.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Exception : " + ex.Message);
                }
            }
            return Json(null);
        }

        #endregion


        #region Assign Position


        #region Get

        public async Task<IActionResult> AssignPosition(string id)
        {
            HttpContext.Session.SetString("Controller", "User");
            ModifyUserPositionViewModel model = new ModifyUserPositionViewModel();
            try
            {
                if (id != null && id.Trim().Length > 0)
                {
                    model.UserID = id;
                    var userRoles = await _serUser.GetUserRoles(id);
                    if (userRoles != null)
                    {
                        model.Roles = userRoles.Select(x => new SelectListItemObject { Value = x.RoleName + "#" + x.RoleID, Text = x.RoleName }).ToList();
                        //if (model.Roles.Count > 1)
                        //{
                        //    model.Roles.Insert(0, new SelectListItemObject { Value = "", Text = "SELECT" });
                        //}
                    }
                    var clients = await _serClient.GetClients();
                    if (clients != null)
                    {
                        model.Clients = clients.Select(x => new SelectListItemObject { Value = x.Text + "#" + x.Value, Text = x.Text }).ToList();
                        //if (model.Roles.Count > 1)
                        //{
                        //    model.Roles.Insert(0, new SelectListItemObject { Value = "", Text = "SELECT" });
                        //}
                    }
                    var lineOfBusinessList = await _serLineOfBusiness.GetLineOfBusinessAsync();
                    if (lineOfBusinessList != null)
                    {
                        model.LOBs = lineOfBusinessList.Select(x => new SelectListItemObject { Value = x.Text + "#" + x.Value, Text = x.Text }).ToList();
                        //if (model.LOBs.Count > 1)
                        //{
                        //    model.LOBs.Insert(0, new SelectListItemObject { Value = "", Text = "SELECT" });
                        //}
                    }
                    var organizations = await _serOrganization.GetBranches();
                    if (organizations != null)
                    {
                        model.Organizations = organizations.Select(x => new SelectListItemObject { Value = x.Text + "#" + x.Value, Text = x.Text }).ToList();
                        //if (model.Organizations.Count > 1)
                        //{
                        //    model.Organizations.Insert(0, new SelectListItemObject { Value = "", Text = "SELECT" });
                        //}
                    }



                    return View(model);
                }
                else
                {
                    return RedirectToAction("Index", "User");
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        #endregion

        #region Post

        [HttpPost]
        public async Task<IActionResult> AssignPosition(ModifyUserPositionViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    AGS_UserHierarchy objUserHierarchy = new AGS_UserHierarchy();
                    objUserHierarchy.EntityState = EntityState.Added;
                    objUserHierarchy.UserID = model.UserID;
                    objUserHierarchy.RoleID = model.RoleID.Split('#')[1];
                    objUserHierarchy.ClientID = model.ClientID.Split('#')[1];
                    objUserHierarchy.LobID = model.LOBID.Split('#')[1];
                    objUserHierarchy.OrgID = model.OrgID.Split('#')[1];

                    var result = await _serUser.ModifyUserHierarchyAsync(objUserHierarchy);
                    if (result)
                    {
                        #region PopUp

                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Position Assign", "Role (" + model.RoleID.Split('#')[0] + ") has been assigned to User (" + model.UserID + ")"));
                        return RedirectToAction("Details", new { id = model.UserID });

                        #endregion
                    }
                    else
                    {
                        ModelState.AddModelError("", "Something went wrong. Either Re-try the process by checking the entries or please contact support team.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Exception : " + ex.Message);
                }
                return View(model);
            }
            var roles = await _serRole.GetRolesAsync();
            var userRoles = await _serUser.GetUserRoles(model.UserID);
            model.Roles = roles.Where(r => !userRoles.Any(ur => ur.RoleID == r.RoleID)).Select(x => new SelectListItemObject { Value = x.RoleName + "#" + x.RoleID, Text = x.RoleName }).ToList();
            return View(model);
        }

        #endregion

        #endregion

        #region Remove Position

        [HttpPost]
        public async Task<IActionResult> RemovePosition(string strUserID, string strIDs)
        {
            if (strUserID != null && strUserID.Trim().Length > 0 && strIDs != null && strIDs.Trim().Split('#').Length >= 4)
            {
                try
                {
                    AGS_UserHierarchy objUserHierarchy = new AGS_UserHierarchy();
                    objUserHierarchy.EntityState = EntityState.Deleted;
                    objUserHierarchy.UserID = strUserID;
                    objUserHierarchy.RoleID = strIDs.Trim().Split('#')[0];
                    objUserHierarchy.ClientID = strIDs.Trim().Split('#')[1];
                    objUserHierarchy.LobID = strIDs.Trim().Split('#')[2];
                    objUserHierarchy.OrgID = strIDs.Trim().Split('#')[3];

                    var result = await _serUser.ModifyUserHierarchyAsync(objUserHierarchy);
                    if (result)
                    {
                        #region PopUp

                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Position Assign", "Position has been assigned to User (" + strUserID + ")"));
                        return Json(true);
                        #endregion
                    }
                    else
                    {
                        ModelState.AddModelError("", "Something went wrong. Either Re-try the process by checking the entries or please contact support team.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Exception : " + ex.Message);
                }
            }
            return Json(null);
        }

        #endregion


        #region Assign Reporting


        #region Get

        public async Task<IActionResult> AssignReporting(string id)
        {
            HttpContext.Session.SetString("Controller", "User");
            ModifyUserReportingViewModel model = new ModifyUserReportingViewModel();
            try
            {
                if (id != null && id.Trim().Length > 0)
                {
                    model.UserID = id;
                    var lsReportingUsers = await _serUser.GetReportingUsers(id);
                    if (lsReportingUsers != null && lsReportingUsers.Count > 0)
                    {
                        model.ReportingUsers = lsReportingUsers.Select(x => new SelectListItemObject { Value = x.Text + "#" + x.Value, Text = x.Text }).ToList();
                    }
                    else
                    {
                        model.ReportingUsers = new List<SelectListItemObject>();
                    }

                    return View(model);
                }
                else
                {
                    return RedirectToAction("Index", "User");
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        #endregion

        #region Post

        [HttpPost]
        public async Task<IActionResult> AssignReporting(ModifyUserReportingViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    AGS_UserReporting objUserReporting = new AGS_UserReporting();
                    objUserReporting.EntityState = EntityState.Added;
                    objUserReporting.UserID = model.UserID;
                    objUserReporting.ReportingID = model.ReportingUserID.Split('#')[1];

                    var result = await _serUser.ModifyUserReportingAsync(objUserReporting);
                    if (result)
                    {
                        #region PopUp

                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Reporting Assign", "Role (" + model.ReportingUser + ") has been assigned to User (" + model.UserID + ")"));
                        return RedirectToAction("Details", new { id = model.UserID });

                        #endregion
                    }
                    else
                    {
                        ModelState.AddModelError("", "Something went wrong. Either Re-try the process by checking the entries or please contact support team.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Exception : " + ex.Message);
                }
                return View(model);
            }
            var lsReportingUsers = await _serUser.GetReportingUsers(model.UserID);
            if (lsReportingUsers != null && lsReportingUsers.Count > 0)
            {
                model.ReportingUsers = lsReportingUsers.Select(x => new SelectListItemObject { Value = x.Text + "#" + x.Value, Text = x.Text }).ToList();
            }
            return View(model);
        }

        #endregion

        #endregion

        #region Remove Reporting

        [HttpPost]
        public async Task<IActionResult> RemoveReporting(string strUserID, string strReportingID)
        {
            if (strUserID != null && strUserID.Trim().Length > 0 && strReportingID != null && strReportingID.Trim().Length > 0)
            {
                try
                {
                    AGS_UserReporting objUserReporting = new AGS_UserReporting();
                    objUserReporting.EntityState = EntityState.Deleted;
                    objUserReporting.UserID = strUserID;
                    objUserReporting.ReportingID = strReportingID;
                    var result = await _serUser.ModifyUserReportingAsync(objUserReporting);
                    if (result)
                    {
                        #region PopUp

                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Remove Reporting User", "Reporting User has been Removed to User (" + strUserID + ")"));
                        return Json(true);
                        #endregion
                    }
                    else
                    {
                        ModelState.AddModelError("", "Something went wrong. Either Re-try the process by checking the entries or please contact support team.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Exception : " + ex.Message);
                }
            }
            return Json(null);
        }

        #endregion

        #region Export
        [HttpGet]
        public async Task<IActionResult> Export()
        {
            try
            {
                var export = new List<UserExportModel>();
                var lsUserExport = await _serUser.GetUserExportData();
                if (lsUserExport != null)
                {
                    export = lsUserExport.Select(x => new UserExportModel()
                    {
                        UserID = x.UserId,
                        Username = x.Username,
                        EmployeeCode = x.LogonName,
                        Branch = (x.Positions != null && x.Positions.Count > 0) ? String.Join(",", x.Positions.Select(y => y.OrgName).ToArray()) : "",
                        RoleName = (x.Positions != null && x.Positions.Count > 0) ? String.Join(",", x.Positions.Select(y => y.RoleName).ToArray()) : "",
                        MobileNumber = x.MobileNumber,
                        EmailID = x.EmailID,
                        Status = x.Status,
                        ReportingName = (x.Reportings != null && x.Reportings.Count > 0) ? String.Join(",", x.Reportings.Select(y => y.ReportingName).ToArray()) : "",
                        Division = (x.Positions != null && x.Positions.Count > 0) ? String.Join(",", x.Positions.Select(y => y.ClientName).ToArray()) : "",
                    }).ToList();
                }
                PropertyInfo[] properties = typeof(UserExportModel).GetProperties();
                using (MemoryStream ms = new MemoryStream())
                {
                    using (ExcelPackage package = new ExcelPackage(ms))
                    {
                        int headerCol = 1;
                        int row = 1;


                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");
                        //First add the headers
                        var styleHeaderCell = worksheet.Workbook.Styles.CreateNamedStyle("HeaderCell");
                        styleHeaderCell.Style.Font.Bold = true;


                        foreach (var prop in properties)
                        {
                            worksheet.Cells[row, headerCol].Value = prop.Name;
                            worksheet.Cells[row, headerCol].StyleName = styleHeaderCell.Name;
                            headerCol++;
                        }
                        row++;
                        for (int i = 0; i < export.Count; i++)
                        {
                            int DataCol = 1;
                            worksheet.Cells[row, DataCol].Value = export[i].UserID;
                            worksheet.Cells[row, DataCol + 1].Value = export[i].EmployeeCode;
                            worksheet.Cells[row, DataCol + 2].Value = export[i].Username;
                            worksheet.Cells[row, DataCol + 3].Value = export[i].Branch;
                            worksheet.Cells[row, DataCol + 4].Value = export[i].RoleName;
                            worksheet.Cells[row, DataCol + 5].Value = export[i].MobileNumber;
                            worksheet.Cells[row, DataCol + 6].Value = export[i].EmailID;
                            worksheet.Cells[row, DataCol + 7].Value = export[i].Status;
                            worksheet.Cells[row, DataCol + 8].Value = export[i].ReportingName;
                            worksheet.Cells[row, DataCol + 9].Value = export[i].Division;                           

                            row++;
                        }

                        package.Save();
                    }
                    FileContentResult response = new FileContentResult(ms.ToArray(), "application/vnd.ms-excel") { FileDownloadName = "Users.xlsx" };
                    return response;

                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        #endregion

        #endregion

        #region Client Events

        [HttpPost]
        public async Task<IActionResult> CheckLogonName(string strKey)
        {
            try
            {
                var result = await _serUser.IsLogonNameAvailable(strKey);
                return Json(result);
            }
            catch (TimeoutException tex)
            {

            }
            catch (Exception ex)
            {
            }

            return Json(null);
        }

        #region Get Org By Role


        [HttpPost]
        public async Task<IActionResult> OrganizationByRole(string user, string role, string client, string lob)
        {
            try
            {
                var result = await _serOrganization.GetOrganizationByRole(role);
                var positions = await _serUser.GetUserPositions(user);
                var userOrgs = positions.Where(x => x.ClientID == client && x.LobID == lob && x.RoleID == role).Select(x => x.OrgID).ToList();
                result = result.Where(x => !userOrgs.Contains(x.Value)).ToList();
                if (result != null && result.Count > 0)
                {
                    return Json(result.Select(y => new DropdownObject { Value = y.Text + "#" + y.Value, Text = y.Text }).ToList());
                }

                return Json(result);
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

        #endregion
    }
}
