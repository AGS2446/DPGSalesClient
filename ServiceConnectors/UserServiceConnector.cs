using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserProxy;

namespace DPGSalesClient.ServiceConnectors
{
    public class UserServiceConnector
    {
        private UserProxyServiceClient _serUser = new UserProxyServiceClient(UserProxyServiceClient.EndpointConfiguration.UserServiceSOAPEndPoint);
        public UserServiceConnector(string strRemoteIP, IHttpContextAccessor httpContextAccessor)
        {
            if (!String.IsNullOrEmpty(strRemoteIP))
            {
                if (strRemoteIP.Contains("https"))
                {
                   // _serUser = new UserProxyServiceClient(UserProxyServiceClient.EndpointConfiguration.UserServiceSOAPHttpsEndPoint, strRemoteIP + "/UserProxyService.svc/SOAP");
                }
                else
                {
                    _serUser = new UserProxyServiceClient(UserProxyServiceClient.EndpointConfiguration.UserServiceSOAPEndPoint, strRemoteIP + "/UserProxyService.svc/SOAP");
                   // _serUser.Endpoint.Binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None);
                }
               
            }
            _serUser.Endpoint.EndpointBehaviors.Add(new ServiceBehaviors.TokenEndpointBehavior(httpContextAccessor));
        }

        public async Task<List<AGS_User>> GetUsersAsync()
        {
            try
            {
                return await _serUser.RetrieveAllAsync("", "", 0, "", 0);
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public async Task<List<AGS_User>> SearchUsers(string key)
        {
            try
            {
                return await _serUser.SearchAsync(key);
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public async Task<bool> IsLogonNameAvailable(string key)
        {
            try
            {
                return await _serUser.IsLogonNameAvailableAsync(key);
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        public async Task<string> CreateUser(AGS_User model)
        {
            try
            {
                return await _serUser.CreateAsync(model);
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        public async Task<bool> UpdateUser(string id,AGS_User model)
        {
            try
            {
                return await _serUser.UpdateAsync(id, model);
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        public async Task<AGS_User> UserDetails(string id)
        {
            return await _serUser.RetrieveAsync(id);
        }

        public async Task<List<AGS_Role>> GetUserRoles(string id)
        {
            return await _serUser.UserRolesAsync(id);
        }
        public async Task<bool> ModifyUserRoleAsync(AGS_UserRole model)
        {
            try
            {
                return await _serUser.ModifyUserRoleAsync(model);
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        public async Task<List<AGS_UserHierarchy>> GetUserPositions(string id)
        {
            return await _serUser.UserHierarchiesAsync(id);
        }          

        public async Task<bool> ModifyUserHierarchyAsync(AGS_UserHierarchy model)
        {
            try
            {
                return await _serUser.ModifyUserHierarchyAsync(model);
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        public async Task<List<AGS_UserReporting>> GetUserReportings(string id)
        {
            return await _serUser.UserReportingsAsync(id);
        }

        public async Task<bool> ModifyUserReportingAsync(AGS_UserReporting model)
        {
            try
            {
                return await _serUser.ModifyUserReportingAsync(model);
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        public async Task<List<AGS_DropdownEntity>> GetReportingUsers(string userId)
        {
            return await _serUser.FillReportingUsersAsync(userId);
        }

        public async Task<List<AGS_DropdownEntity>> GetAssignedUsers(string division, string branch)
        {
            return await _serUser.FillAssigmentUsersAsync(division, branch);
        }

        public async Task<List<UserDetailReport>> GetUserExportData()
        {
            return await _serUser.RetrieveUserDetailReportAsync();
        }

    }
}
