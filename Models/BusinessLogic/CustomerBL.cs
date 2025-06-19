using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.Models.BusinessLogic
{
  
    public class CustomerBL
    {
        ServiceConnectors.AccountServiceConnector _serAccount = null;
        public CustomerBL(string strRemoteIp, IHttpContextAccessor httpContextAccessor)
        {
            _serAccount = new ServiceConnectors.AccountServiceConnector(strRemoteIp, httpContextAccessor);
        }

        public async Task<List<CustomerObject>> GetCustomers(string strKey)
        {
            var lsCustomers = new List<CustomerObject>();
            try
            {
                var lsAcc = await _serAccount.SearchAccount(strKey);
                if (lsAcc != null && lsAcc.Count > 0)
                {
                    lsCustomers = lsAcc.Select(x => new CustomerObject {
                        customerId = x.AccountID,
                        sapCustomerID=x.TempAccountID, 
                        customerName = x.Name,
                        contactperson = x.ContactName,
                        mobilenumber = x.MobileNumber,
                        customeraddress = x.Address1,
                        emailid =x.EmailID,                        
                        city=x.City,
                        pincode=x.Pincode,
                        state=x.State
                    }).ToList();
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }
            return lsCustomers;
        }
        public async Task<List<CustomerObject>> GetCustomersByBranch(string strKey, string branchId)
        {
            var lsCustomers = new List<CustomerObject>();
            try
            {
                var lsAcc = await _serAccount.SearchAccountByBranch(strKey,branchId);
                if (lsAcc != null && lsAcc.Count > 0)
                {
                    lsCustomers = lsAcc.Select(x => new CustomerObject
                    {
                        customerId = x.AccountID,
                        customerName = x.Name,
                        contactperson = x.ContactName,
                        mobilenumber = x.MobileNumber,
                        customeraddress = x.Address1,                        
                        emailid = x.EmailID,
                        city = x.City,
                        pincode = x.Pincode,
                        state = x.State
                    }).ToList();
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }
            return lsCustomers;
        }

    }
}
