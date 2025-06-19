using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrganizationProxy;
using Microsoft.AspNetCore.Http;

namespace DPGSalesClient.ServiceConnectors
{
    public class OrganizationServiceConnector
    {
        private OrganizationProxyServiceClient _serOrganization = new OrganizationProxyServiceClient(OrganizationProxyServiceClient.EndpointConfiguration.OrganizationServiceSOAPHttpsEndPoint);

        public OrganizationServiceConnector(string strRemoteIP, IHttpContextAccessor httpContextAccessor)
        {
            if (!String.IsNullOrEmpty(strRemoteIP))
            {
                if (strRemoteIP.Contains("https"))
                {
                    _serOrganization = new OrganizationProxyServiceClient(OrganizationProxyServiceClient.EndpointConfiguration.OrganizationServiceSOAPHttpsEndPoint, strRemoteIP + "/OrganizationProxyService.svc/SOAP");
                }
                else
                {
                    _serOrganization = new OrganizationProxyServiceClient(OrganizationProxyServiceClient.EndpointConfiguration.OrganizationServiceSOAPEndPoint, strRemoteIP + "/OrganizationProxyService.svc/SOAP");
                   // _serOrganization.Endpoint.Binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None);
                }
               
            }
            _serOrganization.Endpoint.EndpointBehaviors.Add(new ServiceBehaviors.TokenEndpointBehavior(httpContextAccessor));
        }

        public async Task<List<AGS_DropdownEntity>> GetOrganizationByRole(string role)
        {
            return await _serOrganization.FillOrganizationByRoleAsync(role);
        }

        public async Task<List<AGS_DropdownEntity>> GetBranches()
        {
          return await _serOrganization.FillBranchesAsync();
        }
    }
}
