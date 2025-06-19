using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace VoltasSalesClient.Models
{

    public  class GenObjectClass
    {
        public static string GetAllFiledsPropsString(object pObject, string strAppend)
        {
            System.Text.StringBuilder objStr = new System.Text.StringBuilder();
            var lsProps = GetPropertiesNameOfClass(pObject);
            foreach (var item in lsProps)
            {
                objStr.AppendLine("public string " + item + " {get; set;}");
            }

            return objStr.ToString();
        }
        public static string GetAllFiledsString(object pObject,string strAppend)
        {
            System.Text.StringBuilder objStr = new System.Text.StringBuilder();
            var lsProps = GetPropertiesNameOfClass(pObject);
            foreach (var item in lsProps)
            {
                objStr.AppendLine(strAppend + "." + item + " =\"\";\r\n");
            }

            return objStr.ToString();
        }
        public static List<string> GetPropertiesNameOfClass(object pObject)
        {
            List<string> propertyList = new List<string>();
            if (pObject != null)
            {
                foreach (var prop in pObject.GetType().GetProperties())
                {
                    propertyList.Add(prop.Name);
                }
            }
            return propertyList;
        }
       
    }

    public class Payment
    {
        IPayment objP = null;

        public bool DoPaymnet(PaymentObject objPay)
        {
            objP = GetObject(objPay.Type);
            objP.PayAmount(objPay);

            return true;
        }

        private IPayment GetObject(string strType)
        {
            return GetList()[strType];
        }

        private Dictionary<string, IPayment> GetList()
        {
            return new Dictionary<string, IPayment>() { { "BILL", new BillPayment() }, { "EXPENSE", new ExpensePayment() } };
        }
    }
    public interface IPayment
    {
        bool Validate(PaymentObject objPayment);
        bool PayAmount(PaymentObject objPayment);
    }
    public abstract class PaymentProcess : IPayment
    {
        public abstract bool Validate(PaymentObject objPayment);
       
        public bool PayAmount(PaymentObject objPayment)
        {
            return true;
        }
    }   
    public class BillPayment : PaymentProcess
    {
        public override bool Validate(PaymentObject objPayment)
        {
           
            return true;
        }
    }
    public class ExpensePayment : PaymentProcess
    {
        public override bool Validate(PaymentObject objPayment)
        {
            throw new NotImplementedException();
        }
    }
    public class PaymentObject
    {
        public string Type { get; set; }
    }
}
