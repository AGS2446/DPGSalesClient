using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient
{
    public class SessionTimeout : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Session != null && !context.HttpContext.Session.CheckSession("NavigationList"))
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Auth", action = "Login" }));
            }

            base.OnActionExecuting(context);
        }
    }
}
