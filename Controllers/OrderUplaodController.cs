using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using DPGSalesClient.Models;
using System.IO;
using OfficeOpenXml;
using OrderProxy;


// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace DPGSalesClient.Controllers
{
    public class OrderUplaodController : Controller
    {
        #region Varaibles

        ServiceConnectors.OrderServiceConnector _serOrder = null;
       
        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        #endregion

        #region Constructor
        public OrderUplaodController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor)
        {
            string strIp = SalesStaticMethods.GetRemoteIp(appSettings);

            _serOrder = new ServiceConnectors.OrderServiceConnector(strIp, httpContextAccessor);
            
            _logger = loggerFactory.CreateLogger<AccountController>();
            _hostingEnvironment = hostingEnvironment;
        }

        #endregion
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        #region Upload

        #region Get

        public IActionResult Upload()
        {
            HttpContext.Session.SetString("Controller", "Customer");
            try
            {
                if (HttpContext.Session.CheckSession("NavigationList"))
                {
                    return View();
                }
                else
                {
                    return RedirectToAction("Login", "Auth");
                }
            }
            catch (Exception ex)
            {
            }
            return View("Index");
        }

        #endregion

        #region Post

        [HttpPost]
        public IActionResult Upload(ICollection<IFormFile> files)
        {
            try
            {
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            using (var ms = new MemoryStream())
                            {
                                file.CopyTo(ms);

                                try
                                {
                                    using (ExcelPackage package = new ExcelPackage(ms))
                                    {
                                        ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                                        List<AGS_UploadOrder> uploadOrders = new List<AGS_UploadOrder>();
                                        for (int rowIndex = 2; rowIndex <= worksheet.Dimension.Rows; rowIndex++)
                                        {
                                            if (worksheet.Cells[rowIndex, 1].Value != null)
                                            {
                                                uploadOrders.Add(new AGS_UploadOrder()
                                                {
                                                    SAPOrderID = worksheet.Cells[rowIndex, 1].Value?.ToString(),
                                                    CRMQUOTATIONID = worksheet.Cells[rowIndex, 2].Value?.ToString(),
                                                    CRMOPPORTUNITYID = worksheet.Cells[rowIndex, 3].Value?.ToString(),
                                                    CRMLeadID = worksheet.Cells[rowIndex, 4].Value?.ToString(),
                                                    SalesOffice = worksheet.Cells[rowIndex, 5].Value?.ToString(),
                                                    ProjectName = worksheet.Cells[rowIndex, 6].Value?.ToString(),
                                                    PoNo = worksheet.Cells[rowIndex, 7].Value?.ToString(),
                                                    PoDate =Convert.ToDateTime(worksheet.Cells[rowIndex, 8].Value),
                                                    OrderType = worksheet.Cells[rowIndex, 9].Value?.ToString(),
                                                    TotalValue = (double)worksheet.Cells[rowIndex, 10].Value,
                                                    Wonlose = worksheet.Cells[rowIndex, 11].Value?.ToString(),
                                                    OrderReasons = worksheet.Cells[rowIndex, 12].Value?.ToString(),
                                                    CompetitorName = worksheet.Cells[rowIndex, 13].Value?.ToString(),
                                                    Username = worksheet.Cells[rowIndex, 14].Value?.ToString(),
                                                    DocumentCreatedDate = Convert.ToDateTime(worksheet.Cells[rowIndex, 16].Value),                                                                                                       
                                                    CustomerSegmentID = worksheet.Cells[rowIndex, 17].Value?.ToString(),
                                                    CustomerSegment = worksheet.Cells[rowIndex, 18].Value?.ToString(),
                                                    CustomerType = worksheet.Cells[rowIndex, 19].Value?.ToString(),
                                                    DivisionName = worksheet.Cells[rowIndex, 20].Value?.ToString(),
                                                    BranchName = worksheet.Cells[rowIndex, 21].Value?.ToString(),
                                                    RegionName = worksheet.Cells[rowIndex, 22].Value?.ToString(),
                                                    AccountName = worksheet.Cells[rowIndex, 23].Value?.ToString(),
                                                    CustomerClassification = worksheet.Cells[rowIndex, 24].Value?.ToString(),
                                                    ProductRequired = worksheet.Cells[rowIndex, 25].Value?.ToString(),
                                                    ContractValue = (double)worksheet.Cells[rowIndex, 26].Value,
                                                    CompetitorPrice = (double)worksheet.Cells[rowIndex, 27].Value,
                                                    TotalCost = (double)worksheet.Cells[rowIndex, 28].Value,
                                                    PlantID = worksheet.Cells[rowIndex, 29].Value?.ToString(),
                                                    PlantName = worksheet.Cells[rowIndex, 30].Value?.ToString(),
                                                    CurrencyValue =(double)worksheet.Cells[rowIndex, 31].Value,

                                                });

                                            }
                                        }
                                        if (uploadOrders.Count > 0)
                                        {
                                            var result = _serOrder.OrdersUplaod(uploadOrders);
                                        }
                                        else
                                        {
                                            ViewBag.Result = "Upload was Failed";
                                            return RedirectToAction("Index");
                                        }

                                    }
                                }
                                catch (Exception ex)
                                {
                                    //Error                            
                                    var a = "Some error occured while importing." + ex.Message;
                                }
                            }
                        }
                        else
                        {
                            // In valid 
                            return View();
                        }
                    }

                }
                ViewBag.Result = "Upload was Successful";
            }
            catch (Exception ec)
            {

            }
            return RedirectToAction("Index");
        }

        #endregion

        #endregion
    }
}
