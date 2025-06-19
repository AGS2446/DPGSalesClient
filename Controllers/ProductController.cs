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
    public class ProductController : Controller
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
        public ProductController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor)
        {
           
            _serProduct = new ServiceConnectors.ProductServiceConnector(SalesStaticMethods.GetRemoteIp(appSettings), httpContextAccessor);          

            _logger = loggerFactory.CreateLogger<ProductController>();
            _hostingEnvironment = hostingEnvironment;
        }
        #endregion

        /******************************************************************
      * ****************** PRODUCT **********************************
      ******************************************************************/

        #region Product
        public async Task<IActionResult> Index()
        {
            HttpContext.Session.SetString("Controller", "Product");
            var lsProducts = await _serProduct.GetProductsList();
            if (lsProducts != null)
            {
                return View(lsProducts.Select(x => new ProductViewModel { ProductName = x.ProductName, Description = x.Description, Category = x.Category, Status = x.Status }).ToList());
            }
            return View();
        }

        public async Task<IActionResult> Search(string strKey)
        {
            var lsData = await _serProduct.SearchProducts(strKey);
            if (lsData != null && lsData.Count > 0)
            {
                var lsproducts = lsData.Select(y => new ProductViewModel { ProductName = y.ProductName, Description = y.Description, Category = y.Category, Status = y.Status }).ToList();
                return View("Index", lsproducts); 
            }
            else
            {
                var lsproducts = await _serProduct.GetProductsList();
                if (lsproducts != null)
                {
                    return View("Index", lsproducts.Select(y => new ProductViewModel { ProductName = y.ProductName, Description = y.Description, Category = y.Category, Status = y.Status }).ToList());
                }
            }
            return View("Index");
        }
        #endregion
    }
}
