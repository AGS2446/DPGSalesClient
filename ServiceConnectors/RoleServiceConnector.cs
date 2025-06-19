using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoleProxy;
using Microsoft.AspNetCore.Http;

namespace DPGSalesClient.ServiceConnectors
{
    public class RoleServiceConnector
    {
        private RoleProxyServiceClient _serRole = new RoleProxyServiceClient(RoleProxyServiceClient.EndpointConfiguration.RoleServiceSOAPHttpsEndPoint);
        public RoleServiceConnector(string strRemoteIP, IHttpContextAccessor httpContextAccessor)
        {
            if (!String.IsNullOrEmpty(strRemoteIP))
            {
                if (strRemoteIP.Contains("https"))
                {
                    _serRole = new RoleProxyServiceClient(RoleProxyServiceClient.EndpointConfiguration.RoleServiceSOAPHttpsEndPoint, strRemoteIP + "/RoleProxyService.svc/SOAP");
                }
                else
                {
                    _serRole = new RoleProxyServiceClient(RoleProxyServiceClient.EndpointConfiguration.RoleServiceSOAPEndPoint, strRemoteIP + "/RoleProxyService.svc/SOAP");
                   // _serRole.Endpoint.Binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None);
                }
               
            }

            _serRole.Endpoint.EndpointBehaviors.Add(new ServiceBehaviors.TokenEndpointBehavior(httpContextAccessor));            
        }

        public async Task<bool> ValidateApprovalAmount(string actionName,double contractVal)
        {
            return await _serRole.ValidateAmountAsync("Opportunity", actionName,contractVal);
        }

        public async Task<List<AGS_Role>> GetRolesAsync()
        {
            try
            {
                return await _serRole.RetrieveAllAsync("", "", 0, "", 0);
            }
            catch (Exception ex)
            {

            }
            return null;
        }

    }
}
