using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using DPGSalesClient.Models;
using AuthenticationProxy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace DPGSalesClient.Controllers
{
    public class AuthController : Controller
    {
        ServiceConnectors.AuthServiceConnector _serAuth = null;
        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        public AuthController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor)
        {
            
            _serAuth = new ServiceConnectors.AuthServiceConnector(SalesStaticMethods.GetRemoteIp(appSettings), httpContextAccessor);
            _logger = loggerFactory.CreateLogger<AuthController>();
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var userInfo = HttpContext.Session.GetObjectFromJson<LoginUserInfo>("LoginUserInfo");
            if (userInfo != null && returnUrl != null)
            {  
                  return RedirectToLocal(returnUrl);
            }
            else
            {
                string userid="", password = "";
                bool blRemMe = false;
                if (Request.Cookies["USER"] != null)
                {
                    userid = Request.Cookies["USER"].ToString();
                    blRemMe = true;
                }
                return View(new LoginViewModel {UserID=userid,RememberMe= blRemMe });
            }
          
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                //model.UserID; // "USER000001";
                //model.Password; // "12345";

                try
                {
                    //PopupViewModel popupViewModel = new PopupViewModel();
                    //popupViewModel.Header = "Reset Password";
                    //popupViewModel.CssClassHeader = "bg-info dker text-white";
                    //popupViewModel.Message = "password has been reset successful.";
                    //TempData.SetObjectAsJson("PopupViewModel", popupViewModel);

                    //return RedirectToAction("Index", "ContactPlan");

                    //    TempData["PopupViewModel"] = "Test";
                    var token = "";
                    try
                    {
                        token = await _serAuth.LoginAsync(new AGS_Credentials { User = model.UserID, Password = DPGSalesClient.Models.SalesStaticMethods.Encrypt(model.Password) });
                    }
                    catch (Exception ex)
                    {
                    }                  

                    if (token != null )
                    {
                        if (token.Trim().Length > 0)
                        {                            
                            _logger.LogInformation(1, "User logged in.");
                            HttpContext.Session.SetString("Token", token);
                            ServiceBehaviors.Token.Value = token;

                            if (model.RememberMe)
                            {
                                Response.Cookies.Append("USER", model.UserID);
                            }
                            else
                            {
                                Response.Cookies.Delete("USER");
                            }


                            var userInfo = await _serAuth.GetLoginUserInfoAsync();
                            HttpContext.Session.SetObjectAsJson("LoginUserInfo", userInfo);

                            var navigationListMain = await _serAuth.GetBsnrAsync();
                            var navigationList = DataBusinessLogic.Navigation.PrepareNavigationList(navigationListMain);
                            HttpContext.Session.SetObjectAsJson("NavigationList", navigationList);


                            if (navigationList.Count > 0)
                            {
                                if (returnUrl != null && returnUrl.Length > 0)
                                {
                                    return RedirectToLocal(returnUrl);
                                }
                                else
                                {
                                    return RedirectToAction(navigationList[0].Items.ToList()[0].Action, navigationList[0].Items.ToList()[0].Controller);
                                    //  return RedirectToAction("Index", "Lead");
                                }
                            }
                            else
                            {
                                // ModelState.AddModelError(string.Empty, "User was not assigned any process");

                                TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Authentication Error", "Invalid Credentials"));
                            }
                        }
                        else
                        {
                            _logger.LogWarning(2, "Invalid Credentials");
                            //   ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                            // TempData["PopupViewModel"] = "Invalid login attempt.";


                            TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Authentication Error", "Invalid Credentials"));
                            return View(model);
                        }
                    }
                    else
                    {
                        _logger.LogWarning(2, "Unable Connect Server");
                        //   ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        // TempData["PopupViewModel"] = "Invalid login attempt.";


                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Authentication Error", "Unable Connect Server"));
                        return View(model);
                    }


                }
                catch (TimeoutException tex)
                {

                }
                catch (Exception ex) when (ex.Message.Equals("An error occurred while sending the request"))
                {
                    TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Server Error", "Server connecting error occured."));
                }
                catch (Exception ex) when (ex.InnerException.Message.Equals("12002"))
                {
                }
                catch (Exception ex)
                {
                    //  ModelState.AddModelError(string.Empty, "Invalid Credentials");

                    if (ex == null)
                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Server Error", "Server connecting error occured."));
                    else
                        TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Authentication Error", ex.Message));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
            //  return View("../Lead/Index", new { });
        }

        public IActionResult ForgotPassword()
        {

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string UserID)
        {

            var blResult = await _serAuth.ForgotAsync(UserID);
            if (blResult)
            {
                return RedirectToAction("ResetPassword", new { UserID = UserID });
            }
            else
            {
                TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Login", "User not found"));

                return RedirectToAction("Login","Auth");
            }
           

        }

        public IActionResult ResetPassword(string UserID)
        {
            return View(new ResetPassword { UserID=UserID});
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPassword objReset)
        {
             AGS_ResetPassword objNew = new AGS_ResetPassword {User=objReset.UserID,NewPassword=Models.SalesStaticMethods.Encrypt(objReset.Password),TempCode=objReset.Code };           
             var lsResult= await _serAuth.ResetPasswordAsync(objNew);

            if (lsResult)
            {          
                TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Reset Password", "password has been reset successful"));

                return RedirectToAction("Login");
            }
            else
            {
                return View(new ResetPassword {UserID=objReset.UserID });
            }
         
           
        }

        [HttpPost]      
        public IActionResult ChangeRole(string role)
        {
            return View();
        }

        #region Private Methods              

        #region Redirect To Local

        private IActionResult RedirectToLocal(string returnUrl)
        {
            return Redirect(returnUrl);
        }

        #endregion


        #endregion


      
    }
}
