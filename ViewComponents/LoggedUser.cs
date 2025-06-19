using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.ViewComponents
{
    public class LoggedUser : ViewComponent
    {
        private IConfiguration _config;
        public LoggedUser(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {  
                var loginUserObj = HttpContext.Session.GetObjectFromJson<AuthorizationProxy.AGS_LoginUserInfo>("LoginUserInfo");
                if (loginUserObj != null)
                {                  
                   return View(loginUserObj);
                }
                else
                {
                    return View(new AuthorizationProxy.AGS_LoginUserInfo());
                }

            }
            catch (Exception ex)
            {
            }
            return View();
        }
    }
}
