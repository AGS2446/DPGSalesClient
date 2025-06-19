using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountProxy;
using Microsoft.AspNetCore.Http;

namespace DPGSalesClient.ServiceConnectors
{
    public class AccountServiceConnector
    {
        private AccountProxyServiceClient _serAccount = new AccountProxyServiceClient(AccountProxyServiceClient.EndpointConfiguration.AccountProxySOAPHttpsEndPoint);
        //private AccountProxyServiceClient _serAccount = new AccountProxyServiceClient();
        
        public AccountServiceConnector(string strRemoteIP, IHttpContextAccessor httpContextAccessor)
        {
            if (!String.IsNullOrEmpty(strRemoteIP))
            {
                if (strRemoteIP.Contains("https"))
                {
                 _serAccount = new AccountProxyServiceClient(AccountProxyServiceClient.EndpointConfiguration.AccountProxySOAPHttpsEndPoint, strRemoteIP + "/AccountProxyService.svc/SOAP");
                }
                else
                {
                    _serAccount = new AccountProxyServiceClient(AccountProxyServiceClient.EndpointConfiguration.AccountProxySOAPEndPoint, strRemoteIP + "/AccountProxyService.svc/SOAP");
                   // _serAccount.Endpoint.Binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None);
                }
            }

            _serAccount.Endpoint.EndpointBehaviors.Add(new ServiceBehaviors.TokenEndpointBehavior(httpContextAccessor));            
        }

        public async Task<List<AGS_Account>> SearchAccount(string key)
        {
           return await _serAccount.SearchAsync(key);
        }
        public async Task<List<AGS_Account>> SearchAccountByBranch(string key,string branchId)
        {
            return await _serAccount.SearchByBranchAsync(branchId,key);
        }

    }
}
