using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using DPGSalesClient.Models;
using System.Globalization;
using Microsoft.Extensions.Options;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace DPGSalesClient.Controllers
{
    public class GpsController : Controller
    {

        #region Varaibles

        ServiceConnectors.GpsServiceConnector _serGps = null;
        ServiceConnectors.OrganizationServiceConnector _serOrg = null;
        ServiceConnectors.ClientServiceConnector _serClnt = null;


        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        #endregion

        #region Constructure
        public GpsController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor)
        {
            string strIp = SalesStaticMethods.GetRemoteIp(appSettings);
            _serGps = new ServiceConnectors.GpsServiceConnector(strIp, httpContextAccessor);
            _serOrg = new ServiceConnectors.OrganizationServiceConnector(strIp, httpContextAccessor);
            _serClnt = new ServiceConnectors.ClientServiceConnector(strIp, httpContextAccessor);
            _logger = loggerFactory.CreateLogger<GpsController>();
            _hostingEnvironment = hostingEnvironment;
        }
        #endregion

        /******************************************************************
       * ****************** GpsTrack **********************************
       ******************************************************************/

        #region GpsTrack

        #region List View
        public async Task<IActionResult> Index(string[] division, string[] branch)
        {
            HttpContext.Session.SetString("Controller", "Gps");
            var clientResult = await _serClnt.GetClients();
            ViewBag.Divisions = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(clientResult.Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Text = x.Text, Value = x.Value }).ToList(), "Value", "Text");
            var bracnchResult = await _serOrg.GetBranches();
            ViewBag.Branches = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(bracnchResult.Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Text = x.Text, Value = x.Value }).ToList(), "Value", "Text");

            var filter = new GpsProxy.GpsFilter();
            if (division.Count() > 0)
                filter.Division = string.Join(",", division);
            if (branch.Count() > 0)
                filter.Branch = string.Join(",", branch);
            var lsGps = await _serGps.GetGpsAsync(filter);
            var list = lsGps.Select(x => new MapViewModel()
            {
                UserID = x.UserID,
                Name = x.Username,
                EmailID = x.EmailID,
                LogonName = x.LogonName,
                MobileNumber = x.MobileNumber,
                Latitude = x.Latitude,
                Longitude = x.Langitude,
                Address = x.Address,
                UpdatedOn = x.GpsDate.Value
            }).ToList();
            return View(list);
        }

        #endregion      

        #region Map View

        public IActionResult MapView()
        {
            HttpContext.Session.SetString("Controller", "Gps");
            return View();
        }

        #endregion


        #region Details

        #region Details View

        public async Task<IActionResult> Details(string UserId, string Search)
        {
            HttpContext.Session.SetString("Controller", "Gps");
            if (UserId != null && UserId.Trim().Length > 0)
            {
                DateTime dtGpsDate = DateTime.Now.Date;
                if (Search != null && Search.Trim().Length > 0)
                {
                    dtGpsDate = DateTime.ParseExact(Search.Trim(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                }
                ViewBag.UserId = UserId;
                ViewBag.Search = dtGpsDate.ToString("dd/MM/yyyy");
                var gpsList = await _serGps.GetGpsDetailsAsync(UserId, dtGpsDate.ToString("yyyy-MM-dd"));
                return View(gpsList.Select(x => new GpsNavigationViewModel() { GpsDate = x.GpsDate, Address = x.Address, Createdon = x.Createdon }).ToList());
            }
            else
            {
                return View();
            }
        }

        #endregion

        #region Map View

        public IActionResult mapviewDetails(string UserId, string Search)
        {

            HttpContext.Session.SetString("Controller", "Gps");

            if (UserId != null && UserId.Trim().Length > 0)
            {
                DateTime dtGpsDate = DateTime.Now.Date;
                if (Search != null && Search.Trim().Length > 0)
                {
                    dtGpsDate = DateTime.ParseExact(Search.Trim(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                }
                ViewBag.UserId = UserId;
                ViewBag.Search = dtGpsDate.ToString("dd/MM/yyyy");
                HttpContext.Session.SetString("GpsSearchDate", dtGpsDate.ToString("dd/MM/yyyy"));
            }
            return View();
        }

        #endregion

        #endregion

        #endregion

        #region Client Methods

        #region Json User Gps

        [HttpPost]
        public async Task<JsonResult> GetJsonUserGps(string division, string branch)
        {
            try
            {
                var lsGps = await _serGps.GetGpsAsync(new GpsProxy.GpsFilter() { Branch = branch, Division = division });
                var list = lsGps.Select(x => new MapViewModel()
                {
                    UserID = x.UserID,
                    Name = x.Username,
                    EmailID = x.EmailID,
                    LogonName = x.LogonName,
                    MobileNumber = x.MobileNumber,
                    Latitude = x.Latitude,
                    Longitude = x.Langitude,
                    Address = x.Address,
                    UpdatedOn = x.GpsDate.Value
                }).ToList();
                return Json(list);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion

        #region Json User Gps

        public async Task<JsonResult> GetJsonUserNavGps(string UserId, string Search)
        {
            try
            {
                HttpContext.Session.SetString("Controller", "Gps");
                if (UserId != null && UserId.Trim().Length > 0)
                {
                    DateTime dtGpsDate = DateTime.Now.Date;
                    if (Search != null && Search.Trim().Length > 0)
                    {
                        dtGpsDate = DateTime.ParseExact(Search.Trim(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    }
                    ViewBag.UserId = UserId;
                    ViewBag.Search = dtGpsDate.ToString("dd/MM/yyyy");
                    var gpsList = await _serGps.GetGpsDetailsAsync(UserId, dtGpsDate.ToString("yyyy-MM-dd"));
                    return Json(gpsList.Select(x => new GpsNavigationViewModel() { GpsDate = x.GpsDate, Latitude = x.Latitude, Longitude =x.Langitude, Address = x.Address, Createdon = x.Createdon }));
                }
                else
                {
                    return Json(null);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion

        #endregion
    }
}
