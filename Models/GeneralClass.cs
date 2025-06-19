using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.Models
{
    public  class SalesStaticMethods
    {
        public static List<SelectListItemObject> GetSelectlistItemsByName(string strName, List<EntityMapProxy.AGS_EntityMap> lsSource, string appendCase)
        {
            if (lsSource != null)//&& lsSource.Count > 0
            {
                

                var lsData = lsSource.Where(x => x.AttributeName == strName).Select(y => new SelectListItemObject { Text = y.PropertyName, Value = (appendCase == "B" ? y.PropertyName + "#" + y.PropertyValue : ((appendCase == "P") ? y.PropertyName : y.PropertyValue)) }).ToList();
                if (lsData.Count > 1)
                    lsData.Insert(0, new SelectListItemObject { Text = "SELECT", Value = "" });

                if (lsData == null)
                    lsData = new List<SelectListItemObject> { new SelectListItemObject { Text = "SELECT", Value = "" } };

                return lsData;
            }
            return null;
        }

        public static DateTime? ConvertDate(string strDate)
        {
            if (strDate == null)
                return null;

            return new DateTime(Convert.ToInt32(strDate.Split('/')[2]), Convert.ToInt32(strDate.Split('/')[1]), Convert.ToInt32(strDate.Split('/')[0]));
        }

        public static PopupViewModel CreatePopupModel(string hdr,string msg)
        {
            PopupViewModel objPopup = new PopupViewModel();
            objPopup.Header = hdr;
            objPopup.Message = msg;

            return objPopup;
        }

        public static string GetRemoteIp(Microsoft.Extensions.Options.IOptions<AppSettings> appSettings)
        {
            string strServiceIP = "";
            if (appSettings != null && appSettings.Value != null && appSettings.Value.Data != null && appSettings.Value.Data.Count > 0)
            {
                strServiceIP = appSettings.Value.Data.Where(x => x.Key == "IP").FirstOrDefault().Value;
            }

            return strServiceIP;
        }

        public static string Encrypt(string strInputString)
        {
            var strRes = "";
            try
            {
                strRes=Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(strInputString));
            }
            catch (Exception ex)
            {
            }
            return strRes;
        }
        public static string Decrypt(string strInputString)
        {
            var strRes = "";
            try
            {
                strRes = System.Text.Encoding.ASCII.GetString(Convert.FromBase64String(strInputString));
            }
            catch (Exception ex)
            {
            }
            return strRes;
        }

    }
}
