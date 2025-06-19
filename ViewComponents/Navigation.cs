using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using DPGSalesClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DPGSalesClient.ViewComponents
{
    public class Navigation: ViewComponent
    {
        private IConfiguration _config;
        public Navigation(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                //var lsNav = new List<NavHeaderViewModel> { new NavHeaderViewModel {ScenarioName="Dashboard",Icon="fa-tachometer",OrderBy=1 },
                //                                           new NavHeaderViewModel {
                //                                               ScenarioName ="Sales Management",
                //                                               Icon ="fa-bar-chart",
                //                                               OrderBy =2,
                //                                               Items =new List<NavItemViewModel>{
                //                                                   new NavItemViewModel {
                //                                                       ProcessName ="Leads",
                //                                                       Action ="Index",
                //                                                       Controller ="Lead",
                //                                                       Icon ="",
                //                                                       OrderBy =1
                //                                                   },
                //                                                     new NavItemViewModel {
                //                                                       ProcessName ="Home",
                //                                                       Action ="Index",
                //                                                       Controller ="Home",
                //                                                       Icon ="",
                //                                                       OrderBy =1
                //                                                   }
                //                                               }
                //                                           }
                //};

                //return View(lsNav);
                //username
               @ViewBag.loggedUser= HttpContext.Session.GetString("loggedInUserName");


                var navigationList = HttpContext.Session.GetObjectFromJson<List<NavHeaderViewModel>>("NavigationList");
                if (navigationList != null && navigationList.Count > 0)
                {
                    string ControllerName = HttpContext.Session.GetString("Controller");
                    @ViewBag.ControllerName = ControllerName;
                    var selectedBsnr = navigationList.Where(x => x.Items.Any(c => c.Controller == ControllerName)).FirstOrDefault();
                    if (selectedBsnr != null)
                    {
                        @ViewBag.BusinessScenarioID = selectedBsnr.ScenarioID;                        
                    }
                    return View(navigationList);
                }
                else
                {                 
                    return View(new List<NavHeaderViewModel>());
                }

            }
            catch (Exception ex)
            {   
            }
            return View();
        }
    }
}
