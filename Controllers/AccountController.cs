using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using DPGSalesClient.Models;
using Microsoft.Extensions.Options;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace DPGSalesClient.Controllers
{
    public class AccountController : Controller
    {
        #region Varaibles

        ServiceConnectors.LeadServiceConnector _serLead = null;
        ServiceConnectors.LocationServiceConnector _serLocation = null;
        ServiceConnectors.AccountServiceConnector _serAccount = null;
        ServiceConnectors.EntityMapServiceConnector _serEntity = null;
        ServiceConnectors.FileManagerServiceConnector _serFile = null;
        ServiceConnectors.UserServiceConnector _serUser = null;
        ServiceConnectors.ActivityServiceConnector _serActivity = null;
        ServiceConnectors.ProductServiceConnector _serProduct = null;
        ServiceConnectors.OpportunityServiceConnector _serEnquiry = null;

        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        #endregion

        #region Constructure
        public AccountController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor) 
        {
      
            _serAccount = new ServiceConnectors.AccountServiceConnector(SalesStaticMethods.GetRemoteIp(appSettings), httpContextAccessor);
            _serEntity = new ServiceConnectors.EntityMapServiceConnector(SalesStaticMethods.GetRemoteIp(appSettings), httpContextAccessor);
          
            _logger = loggerFactory.CreateLogger<AuthController>();
            _hostingEnvironment = hostingEnvironment;
        }
        #endregion

        /******************************************************************
       * ****************** ACCOUNT **********************************
       ******************************************************************/
        public async Task<IActionResult> Index()
        {
            HttpContext.Session.SetString("Controller", "Account");
            string strKey = null;
            var lstData = await _serAccount.SearchAccount(strKey);
            return View();
        }
        
        public async Task<IActionResult> Search(string strKey)
        {
            var lstData = await _serAccount.SearchAccount(strKey);
            if(lstData!=null&&lstData.Count > 0)
            {
                var lsAccounts = lstData.Select(y => new AccountModel { SAPAccountID = y.AccountID, Name = y.Name, Address1 = y.Address1, MobileNo = y.MobileNumber, CreatedOn = y.CreatedOn }).ToList();
                return View("Index", lsAccounts);
            }
            else
            {
             //  var lsAccounts= await _serAccount.ge
            }
            return View("Index");
        }
    }
}
