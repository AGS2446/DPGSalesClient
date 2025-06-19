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

namespace DPGSalesClient.Controllers
{
    public class OQDashboardController : Controller
    {
        #region Variables

        ServiceConnectors.ReportServiceConnector _serReport = null;
        ServiceConnectors.OrderServiceConnector _serOrder = null;
        ServiceConnectors.QuotationServiceConnector _serQuotation = null;

        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        #endregion

        public OQDashboardController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor)
        {
            _serReport = new ServiceConnectors.ReportServiceConnector(SalesStaticMethods.GetRemoteIp(appSettings), httpContextAccessor);
            _serOrder=new ServiceConnectors.OrderServiceConnector(SalesStaticMethods.GetRemoteIp(appSettings), httpContextAccessor);
            _serQuotation = new ServiceConnectors.QuotationServiceConnector(SalesStaticMethods.GetRemoteIp(appSettings), httpContextAccessor);

            _logger = loggerFactory.CreateLogger<OQDashboardController>(); 
            _hostingEnvironment = hostingEnvironment;


        }

        public async Task<IActionResult> Report()
        {
            HttpContext.Session.SetString("Controller", "OQDashboard");

            try
            {
                if (VerifySession())
                {
                    string strDateFrom = "";
                    if (DateTime.Now.Month < 4)
                    {
                        strDateFrom = "01/04/" + (DateTime.Now.Year-1);
                    }
                    else
                    {
                        strDateFrom = "01/04/" + DateTime.Now.Year;
                    }
                    var objInput = new ReportProxy.OrderDashboardRequest {DateFrom = SalesStaticMethods.ConvertDate(strDateFrom), DateTo =DateTime.Now };//DateFrom = SalesStaticMethods.ConvertDate("01/04/2018"), DateTo = SalesStaticMethods.ConvertDate("16/05/2018")
                    
                    var objResp = await _serReport.OrderDashboard(objInput);

                    var res = GetReponseModel(objResp);

                    return View(new Models.OQDashReportModel { FromDate =strDateFrom, ToDate = DateTime.Now.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture), Response1 = objResp,Response= res });

                }
                else
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Authorization Error", "Session has expired"));
                    return RedirectToAction("Login", "Auth");
                }
            }
            catch (Exception ex)
            {

            }          
         
            return View();
        }
        public async Task<IActionResult> Search(string FromDate,string ToDate)
        {
            try
            {
                if (VerifySession())
                {
                    var objInput = new ReportProxy.OrderDashboardRequest { DateFrom = SalesStaticMethods.ConvertDate(FromDate), DateTo = SalesStaticMethods.ConvertDate(ToDate) };//DateFrom = SalesStaticMethods.ConvertDate("01/04/2018"), DateTo = SalesStaticMethods.ConvertDate("16/05/2018")


                    var objResp = await _serReport.OrderDashboard(objInput);
                    var res = GetReponseModel(objResp);

                    return View("Report", new Models.OQDashReportModel { FromDate = FromDate, ToDate = ToDate, Response = res,Response1=objResp });
                }else
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Authorization Error", "Session has expired"));

                    return RedirectToAction("Login", "Auth");
                }
            }
            catch (Exception ex)
            {
            }

            return RedirectToAction("Report");
        }

        [HttpPost]
        public async Task<IActionResult> ReportViewNavigator(OQReportNavModel objInput)
        {
            //RedirectToActionResult res = null;

            //if (objInput.Type.StartsWith("O"))
            //{
            //    res = RedirectToAction("OrderReport", objInput);
            //}
            //else if (objInput.Type.StartsWith("Q"))
            //{
            //    res = RedirectToAction("QuoteReport", objInput);
            //}
            //else if (objInput.Type.StartsWith("M"))
            //{
            //    res = RedirectToAction("MonthlyQuoteReport", objInput);
            //}

            //return res;

           

            if (objInput.Type.StartsWith("O"))
            {
                return Json(OrderReportService(objInput));
            }
            else if (objInput.Type.StartsWith("Q"))
            {
                return Json(QuoteReportService(objInput));
            }
            else if (objInput.Type.StartsWith("M"))
            {
                return Json(MonthlyQuoteReport(objInput));
            }

            return Json(null);
        }     

        public async Task<IActionResult> OrderReportService(OQReportNavModel objInput)
        {
            var objData = new OQOrderDataModel();
            try
            {
                objData.FromDate = objInput.FromDate;
                objData.ToDate = objInput.ToDate;

                var strStatus = "";
                if (objInput.Type.StartsWith("OW"))
                    strStatus = "Win";
                else if (objInput.Type.StartsWith("OL"))
                    strStatus = "Lost";

                var lsData = await _serOrder.OrderReport(new OrderProxy.CrmObjectReportRequest { DivisionName = objInput.Division, RegionName = objInput.Region, CustomerSubsegment = objInput.Segment, DateFrom = SalesStaticMethods.ConvertDate(objInput.FromDate), DateTo = SalesStaticMethods.ConvertDate(objInput.ToDate), Status = strStatus });
                if (lsData != null && lsData.Count > 0)
                {
                    var lsPreData = new List<OQOrderDataModel.Item>();
                    foreach (var od in lsData)
                    {
                        //if (od.OrderProducts.Count > 0)
                        //{
                            
                            //lsPreData.AddRange(od.OrderProducts.Select(x => new OQOrderDataModel.Item
                            //{
                            //    AccountName = od.AccountName,
                            //    BusinessSegment = od.BusinessSegment,
                            //    Competitor = od.CompetitorName,
                            //    ContractValue = od.ContractValue,
                            //    OrderType = od.OrderType,
                            //    Price = od.CompetitorPrice,
                            //    Reason = od.Reason,
                            //    Status = od.Status,
                            //    SubSegment = od.SubSegment,
                            //    Tonnage = od.Tonnage,
                            //    ProductName = x.ProductName,
                            //    Quantity = x.Quantity
                            //}));
                        //}
                        //else
                        //{
                            lsPreData.Add(new OQOrderDataModel.Item
                            {
                                Branch=od.BranchName,
                                AccountName = od.AccountName,
                                BusinessSegment = od.BusinessSegment,
                                Competitor = od.CompetitorName,
                                ContractValue = od.ContractValue,
                                OrderType = od.OrderType,
                                Price = od.CompetitorPrice,
                                Reason = od.Reason,
                                Status = od.Status,
                                SubSegment = od.SubSegment,
                                Tonnage = od.Tonnage
                            });
                        //}
                    }
                    objData.OrderItemList = lsPreData;


                }

            }
            catch (Exception ex)
            {
            }


            return Json(objData);

        }
        public async Task<IActionResult> QuoteReportService(OQReportNavModel objInput)
        {

            var objData = new OQQuoteDataModel();
            try
            {
                objData.FromDate = objInput.FromDate;
                objData.ToDate = objInput.ToDate;

                var lsData = await _serQuotation.QuotationReport(new QuotationProxy.CrmObjectReportRequest { DivisionName = objInput.Division, RegionName = objInput.Region, CustomerSubsegment = objInput.Segment, DateFrom = SalesStaticMethods.ConvertDate(objInput.FromDate), DateTo = SalesStaticMethods.ConvertDate(objInput.ToDate) });
                if (lsData != null && lsData.Count > 0)
                {
                    objData.QuoteItemList = lsData.Select(x => new OQQuoteDataModel.Item
                    {
                        Division = x.DivisionName,
                        Branch=x.BranchName,
                        Plant = x.PlantID + "-" + x.PlantName,
                        ContractValue = x.ContractValue,
                        ProductRequired = x.ProductRequired,
                        Tonnage = x.Tonnage,
                        SalesEngg = x.Username
                    }).ToList();
                }

            }
            catch (Exception ex)
            {
            }


            return Json(objData);

        }

        public async Task<IActionResult>MonthlyQuoteReport(OQReportNavModel objInput)
        {

            var objData = new OQQuoteDataModel();
            try
            {
                objData.FromDate = objInput.FromDate;
                objData.ToDate = objInput.ToDate;

                string startDate = "", endDate = "";

                startDate = GetDate(objInput.Type, true);
                endDate = GetDate(objInput.Type, false);
                
                var lsData = await _serQuotation.QuotationReport(new QuotationProxy.CrmObjectReportRequest { DivisionName = objInput.Division, RegionName = objInput.Region, CustomerSubsegment = objInput.Segment, MaturityDateFrom = SalesStaticMethods.ConvertDate(startDate), MaturityDateTo = SalesStaticMethods.ConvertDate(endDate) });
                if (lsData != null && lsData.Count > 0)
                {
                    objData.QuoteItemList = lsData.Select(x => new OQQuoteDataModel.Item
                    {
                        Division = x.DivisionName,
                        Branch=x.BranchName,
                        Plant = x.PlantID + "-" + x.PlantName,
                        ContractValue = x.ContractValue,
                        ProductRequired = x.ProductRequired,
                        Tonnage = x.Tonnage,
                        SalesEngg = x.Username
                    }).ToList();
                }

            }
            catch (Exception ex)
            {
            }


            return Json( objData);

        }

        private bool VerifySession()
        {
            bool blRes = false;
            try
            {
                if (HttpContext.Session.CheckSession("NavigationList"))
                {
                    blRes = true;
                }
                
            }
            catch (Exception ex)
            {
            }
            return blRes;
         }

        private string GetDate(string type,bool IsStart)
        {
            var strRes = "";
            
            try
            {
                switch (type)
                {
                    case "M1":
                        strRes = (IsStart ? "01" : DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month).ToString())+"/" + DateTime.Now.Month + "/" + DateTime.Now.Year;
                        break;
                    case "M2":
                        {
                            var dt = DateTime.Now.AddMonths(1);
                            strRes = (IsStart ? "01" : DateTime.DaysInMonth(dt.Year, dt.Month).ToString()) + "/" + dt.Month + "/" + dt.Year;
                        }                      
                        break;
                    case "M3":
                        {
                            var dt = DateTime.Now.AddMonths(2);
                            strRes = (IsStart ? "01" : DateTime.DaysInMonth(dt.Year, dt.Month).ToString()) + "/" + dt.Month + "/" + dt.Year;
                        }
                        break;
                    case "MOF3S":
                        {
                            var dt = DateTime.Now.AddMonths(2);
                            strRes = (IsStart ? "01" + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year : DateTime.DaysInMonth(dt.Year, dt.Month).ToString() + "/" + dt.Month + "/" + dt.Year);
                        }
                     
                        break;
                }
            }
            catch (Exception ex)
            {
            }


            return strRes;
        }

        private OrderDashboardResponseModel GetReponseModel(ReportProxy.OrderDashboardResponse objInput)
        {
            var objResp = new OrderDashboardResponseModel();
            try
            {
                objResp.QuotesCount = objInput.QuotesCount;
                objResp.QuotesValue = objInput.QuotesValue;
                objResp.QuotesDivisionWise = DivisiontoString(objInput.QuotesDivisionWise,"QT");

                objResp.OrderWinCount = objInput.OrderWinCount;
                objResp.OrderWinValue = objInput.OrderWinValue;
                objResp.OrderWinDivisionWise = DivisiontoString(objInput.OrderWinDivisionWise,"OW");

                objResp.OrderLostCount = objInput.OrderLostCount;
                objResp.OrderLostValue = objInput.OrderLostValue;
                objResp.OrderLostDivisionWise = DivisiontoString(objInput.OrderLostDivisionWise,"OL");


                objResp.OrderForcast3MonthsCount = objInput.OrderForcast3MonthsCount;
                objResp.OrderForcast3MonthsValue = objInput.OrderForcast3MonthsValue;
                objResp.OrderForcast3MonthsDivisionWise = DivisiontoString(objInput.OrderForcast3MonthsDivisionWise, "MOF3S");

                objResp.OrderForcastMonth1Name = objInput.OrderForcastMonth1Name;
                objResp.OrderForcastMonth1Count = objInput.OrderForcastMonth1Count;
                objResp.OrderForcastMonth1Value = objInput.OrderForcastMonth1Value;
                objResp.OrderForcastMonth1DivisionWise = DivisiontoString(objInput.OrderForcastMonth1DivisionWise,"M1");

                objResp.OrderForcastMonth2Name = objInput.OrderForcastMonth2Name;
                objResp.OrderForcastMonth2Count = objInput.OrderForcastMonth2Count;
                objResp.OrderForcastMonth2Value = objInput.OrderForcastMonth2Value;
                objResp.OrderForcastMonth2DivisionWise = DivisiontoString(objInput.OrderForcastMonth2DivisionWise, "M2");

                objResp.OrderForcastMonth3Name = objInput.OrderForcastMonth3Name;
                objResp.OrderForcastMonth3Count = objInput.OrderForcastMonth3Count;
                objResp.OrderForcastMonth3Value = objInput.OrderForcastMonth3Value;
                objResp.OrderForcastMonth3DivisionWise = DivisiontoString(objInput.OrderForcastMonth3DivisionWise, "M3");



            }
            catch (Exception ex)
            {

            }
            return objResp;
        }

        private string DivisiontoString(List<ReportProxy.OrderDashboard_DivisionWise> lsDivisions,string parent)
        {
            var strResult = "";
            try
            {
                // strResult = "{\"Divisions:\"" + Newtonsoft.Json.JsonConvert.SerializeObject(lsDivisions.Select(div => new OrderDashboardResponseModel.ItemModel { Type = "D", Name = div.DivisionName, Value = div.Value.ToString("#.##"), ChildsInString = "{\"Regions:\"" + Newtonsoft.Json.JsonConvert.SerializeObject( div.Regions.Select(reg=>new OrderDashboardResponseModel.ItemModel { Type="R",Name=reg.RegionName,Value=reg.Value.ToString("#.##"),ChildsInString="{\"Segments:\""+ Newtonsoft.Json.JsonConvert.SerializeObject(reg.Segments.Select(seg=> new OrderDashboardResponseModel.ItemModel { Type="S",Name=seg.SegmentName,Value=seg.Value.ToString("#.##")}) )+"}"  }) )+"}" }))+"}";

                var divs= lsDivisions.Select(div => new OrderDashboardResponseModel.ItemModel { Type = "D", Name = div.DivisionName, Value = div.Value.ToString("#.##"),Parent=parent, Childs = div.Regions.Select(reg => new OrderDashboardResponseModel.ItemModel { Type = "R", Name = reg.RegionName, Value = reg.Value.ToString("#.##"),Parent= parent+"#"+div.DivisionName, Childs =reg.Segments.Select(seg => new OrderDashboardResponseModel.ItemModel { Type = "S", Name = seg.SegmentName, Value = seg.Value.ToString("#.##"),Parent= parent + "#" + div.DivisionName+"#"+reg.RegionName }).ToList() }).ToList() });

                strResult =  Newtonsoft.Json.JsonConvert.SerializeObject(new { divisions= divs });
            }
            catch (Exception ex)
            {
            }
            return strResult;
        }
       
    }
}
