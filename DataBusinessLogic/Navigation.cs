using DPGSalesClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.DataBusinessLogic
{
    public class Navigation
    {
        public static List<NavHeaderViewModel> PrepareNavigationList(List<AuthorizationProxy.AGS_NavigationHeader> objInNHMain)
        {
            List<NavHeaderViewModel> scenarios = new List<NavHeaderViewModel>();
            try
            {
                var objInNH = objInNHMain.OrderBy(x => x.OrderType).ToList();

                foreach (var bsnr in objInNH)
                {
                    List<NavItemViewModel> processes = new List<NavItemViewModel>();
                    foreach (var proc in bsnr.Processes)
                    {
                        processes.Add(new NavItemViewModel
                        {
                            ProcessID = proc.ID,
                            ProcessName = proc.Name,
                            Description = proc.Description,
                            OrderBy=proc.OrderType
                        });
                    }

                    processes = processes.OrderBy(x => x.OrderBy).ToList();

                    processes = ApplyMVCIndexNames(processes);
                    scenarios.Add(new NavHeaderViewModel()
                    {
                        ScenarioID = bsnr.ID,
                        ScenarioName = bsnr.Name,
                        Description = bsnr.Decription,
                        OrderBy = bsnr.OrderType,
                        Items = processes
                    });
                    scenarios = ApplyClientBSNRClass(scenarios);
                }
            }
            catch (Exception ex)
            {

            }
            return scenarios;
        }

        private static List<NavItemViewModel> ApplyMVCIndexNames(List<NavItemViewModel> businessProcesses)
        {
            try
            {

                foreach (var proc in businessProcesses)
                {
                    switch (proc.ProcessName.ToUpper())
                    {
                        #region IMAM

                        case "ROLE":
                            proc.Controller = "Role";
                            proc.Action = "Index";
                            break;
                        case "USER":
                            proc.Controller = "User";
                            proc.Action = "Index";
                            break;
                        case "CLIENT":
                            proc.Controller = "Client";
                            proc.Action = "Index";
                            break;

                        #endregion

                        #region Sales Objects

                        case "LEAD":
                            proc.Controller = "Lead";
                            proc.Action = "Index";
                            break;
                        case "OPPORTUNITY":
                            proc.Controller = "EnquiryOpportunity";
                            proc.Action = "Index";
                            break;                      
                        case "QUOTATION":
                            proc.Controller = "Quote";
                            proc.Action = "Index";
                            break;
                        case "ORDER":
                            proc.Controller = "Order";
                            proc.Action = "Index";
                            break;
                        case "ACCOUNT":
                            proc.Controller = "Account";
                            proc.Action = "Index";
                            break;
                        case "PRODUCT":
                            proc.Controller = "Product";
                            proc.Action = "Index";
                            break;

                        #endregion

                        #region Activity

                        case "ACTIVITY":
                            proc.Controller = "ContactPlan";
                            proc.Action = "Index";
                            break;

                        #endregion

                        #region Reports

                        case "HOME":
                            proc.Controller = "Home";
                            proc.Action = "Index";
                            break;

                        case "DPG DASHBOARD":
                            proc.Controller = "OQDashboard";
                            proc.Action = "Report";
                            break;

                        case "QUOTE APPROVALS": 
                            proc.Controller = "EnquiryApprovalReport";
                            proc.Action = "Index";
                            break;
                        case "EXECUTIVE SUMMARY REPORT":
                            proc.Controller = "ExecutiveSummaryReport";
                            proc.Action = "Index";
                            break;

                        #endregion

                        #region Dashboard
                        case "MAJOR ORDER WIN AND LOSS":
                            proc.Controller = "SalesReport";
                            proc.Action = "Index";
                            break;
                        case "ORDER BOOKING":
                            proc.Controller = "ProjectOrderBookedReport";
                            proc.Action = "Index";
                            break;
                        case "GPS":
                            proc.Controller = "Gps";
                            proc.Action = "Index";
                            break;
                        //Lakshmi
                        case "LEAD REPORT":
                            proc.Controller = "LeadReport";
                            proc.Action = "Report";
                            break;
                        case "ENQUIRY REPORT":
                            proc.Controller = "OpportunityReport";
                            proc.Action = "Report";
                            break;
                        case "QUOTE REPORT":
                            proc.Controller = "QuotationReport";
                            proc.Action = "Report";
                            break;
                        case "ORDER REPORT":
                            proc.Controller = "OrderReport";
                            proc.Action = "Report";
                            break;
                        case "SALESORDER":
                            proc.Controller = "SalesOrder";
                            proc.Action = "Index";
                            break;

                        //Harsha
                        case "ENQUIRY FORECAST REPORT":
                            proc.Controller = "EnquiryForecastReport";
                            proc.Action = "Index";
                            break;

                        //Gangadhar
                        case "QUOTE FORECAST REPORT":
                            proc.Controller = "QuotationForecastReport";
                            proc.Action = "QuoteForecastReport";
                            break;

                        //Sri Ram
                        case "LEAD CONVERTION REPORT":
                            proc.Controller = "StrikeLeadReport";
                            proc.Action = "LeadReport";
                            break;
                        case "ENQUIRY CONVERTION REPORT":
                            proc.Controller = "StrikeEnquiryreport";
                            proc.Action = "EnquiryReport";
                            break;
                        case "QUOTE CONVERTION REPORT":
                            proc.Controller = "StrikeQuoteReport";
                            proc.Action = "QuoteReport";
                            break;

                        case "SALES FUNNEL REPORT":
                            proc.Controller = "FunnelReport";
                            proc.Action = "AmFunnelChart";
                            break;
                        case "ORDER UPLOAD":
                            proc.Controller = "OrderUpload";
                            proc.Action = "Index";
                            break;
                            #endregion

                    }
                }
            }
            catch (Exception ex)
            {

            }
            return businessProcesses;
        }

        private static List<NavHeaderViewModel> ApplyClientBSNRClass(List<NavHeaderViewModel> scenarioViewModelList)
        {
            try
            {

                foreach (var scenario in scenarioViewModelList)
                {
                    switch (scenario.ScenarioName.ToUpper())
                    {

                        case "IDENTITY":
                            scenario.Icon = "fa-user-circle"; //md-accounts-list-alt md-account-child
                            break;  
                        case "GPS":
                            scenario.Icon = "fa-map";
                            break;
                        case "ORGANIZATION":
                            scenario.Icon = "fa-users";
                            break;
                        case "CONFIGURATION MANAGEMENT":
                            scenario.Icon = "fa-cogs";
                            break;
                        case "CUSTOMER CONTACT PLAN":
                            scenario.Icon = "fa-calendar";
                            break;
                        case "SALES MANAGEMENT":
                            scenario.Icon = "fa-line-chart";// "fa-bar-chart";
                            break;
                        case "SALES MASTER MANAGEMENT":
                            scenario.Icon = "fa-area-chart";// "fa-bar-chart";
                            break;                            
                        case "DASHBOARD":
                            scenario.Icon = "fa-tachometer";
                            break;
                        case "REPORTS":
                            scenario.Icon = "fa-bar-chart";
                            break;
                        case "APPROVALS":
                            scenario.Icon = "fa-check-square-o";
                            break; 
                        case "HOME":
                            scenario.Icon = "fa-home";
                            break;
                            
                        default:
                            scenario.Icon = "fa-check-circle";
                            break;
                    }
                }
                scenarioViewModelList.RemoveAll(x => x.Items == null || x.Items.Count == 0);
            }
            catch (Exception ex)
            {

            }
            return scenarioViewModelList;
        }
    }
}
