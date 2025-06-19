using System;
using System.Runtime;
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
    public class OrderUploadController : Controller
    {
        #region Varaibles

        ServiceConnectors.OrderServiceConnector _serOrder = null;

        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        #endregion

        #region Constructor
        public OrderUploadController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor)
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
            HttpContext.Session.SetString("Controller", "OrderUpload");
            return View();
        }

        #region Upload

        #region Get

        public IActionResult Upload()
        {
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
        public async Task<IActionResult> Upload(ICollection<IFormFile> files)
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
                                                    PoDate = worksheet.Cells[rowIndex, 8].Value != null ? FromOADate(worksheet.Cells[rowIndex, 8].Value) : (DateTime?)null, 
                                                    OrderType = worksheet.Cells[rowIndex, 9].Value?.ToString(),
                                                    TotalValue = (double?)worksheet.Cells[rowIndex, 10].Value,
                                                    Wonlose = worksheet.Cells[rowIndex, 11].Value?.ToString(),
                                                    OrderReasons = worksheet.Cells[rowIndex, 12].Value?.ToString(),
                                                    CompetitorName = worksheet.Cells[rowIndex, 13].Value?.ToString(),
                                                    Username = worksheet.Cells[rowIndex, 14].Value?.ToString(),
                                                    DocumentCreatedDate = worksheet.Cells[rowIndex, 15].Value != null ? FromOADate(worksheet.Cells[rowIndex, 15].Value) : (DateTime?)null,
                                                    CustomerSegmentID = worksheet.Cells[rowIndex, 16].Value?.ToString(),
                                                    CustomerSegment = worksheet.Cells[rowIndex, 17].Value?.ToString(),
                                                    CustomerType = worksheet.Cells[rowIndex, 18].Value?.ToString(),
                                                    DivisionName = worksheet.Cells[rowIndex, 19].Value?.ToString(),
                                                    BranchName = worksheet.Cells[rowIndex, 20].Value?.ToString(),
                                                    RegionName = worksheet.Cells[rowIndex, 21].Value?.ToString(),
                                                    AccountName = worksheet.Cells[rowIndex, 22].Value?.ToString(),
                                                    CustomerClassification = worksheet.Cells[rowIndex, 23].Value?.ToString(),
                                                    ProductRequired = worksheet.Cells[rowIndex, 24].Value?.ToString(),
                                                    ContractValue = (double?)worksheet.Cells[rowIndex, 25].Value,
                                                    CompetitorPrice = (double?)worksheet.Cells[rowIndex, 26].Value,
                                                    TotalCost = (double?)worksheet.Cells[rowIndex, 27].Value,
                                                    PlantID = worksheet.Cells[rowIndex, 28].Value?.ToString(),
                                                    PlantName = worksheet.Cells[rowIndex, 29].Value?.ToString()                                                   

                                                });

                                            }
                                        }
                                        if (uploadOrders.Count > 0)
                                        {
                                            var result = await _serOrder.OrdersUplaod(uploadOrders);
                                            TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Order Upload", "Uploaded successfully"));
                                        }
                                        else
                                        {
                                            TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Order Upload", "No data available to upload"));
                                            return RedirectToAction("Index");
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Order Upload", "Some error occured while importing:" + ex.Message));
                                    return RedirectToAction("Index");
                                    // var a = "Some error occured while importing." + ex.Message;
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

        #region Local Methods

        public  DateTime FromOADate(object date)
        {            
            return new DateTime(DoubleDateToTicks((double)date), DateTimeKind.Unspecified);
        }

        private long DoubleDateToTicks(double value)
        {
            if (value >= 2958466.0 || value <= -657435.0)
                throw new ArgumentException("Not a valid value");
            long num1 = (long)(value * 86400000.0 + (value >= 0.0 ? 0.5 : -0.5));
            if (num1 < 0L)
                num1 -= num1 % 86400000L * 2L;
            long num2 = num1 + 59926435200000L;
            if (num2 < 0L || num2 >= 315537897600000L)
                throw new ArgumentException("Not a valid value");
            return num2 * 10000L;
        }

        #endregion
    }


}
