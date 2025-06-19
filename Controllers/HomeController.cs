using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DPGSalesClient.Models;
using Microsoft.AspNetCore.Http;

namespace DPGSalesClient.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            HttpContext.Session.SetString("Controller", "Home");

            //var lsHomeItms = new List<HomeListItem> {
            //    new HomeListItem {Name="ACCOUNTS",ImageUrl="hicon1.png",Action="Index",Controller="Lead" },                   
            //            new HomeListItem {Name="PRODUCTS",ImageUrl="hicon3.png",Action="Index",Controller="Lead" },
            //                new HomeListItem {Name="ENQUIRIES",ImageUrl="hicon4.png",Action="Index",Controller="Lead" },
            //                    new HomeListItem {Name="QUOTES",ImageUrl="hicon5.png",Action="Index",Controller="Lead" },
            //                      new HomeListItem {Name="ORDERS",ImageUrl="hicon6.png",Action="Index",Controller="Lead" },                  
            //            new HomeListItem {Name="Customer Visit Plan",ImageUrl="hicon8.png",Action="Index",Controller="Lead" },
            //              new HomeListItem {Name="COMPITETORS",ImageUrl="hicon7.png",Action="Index",Controller="Lead" },
            //              new HomeListItem {Name="DASHBOARD",ImageUrl="hicon2.png",Action="Index",Controller="Lead" }

            //};
            return View();
           
        }

      
    }
}
