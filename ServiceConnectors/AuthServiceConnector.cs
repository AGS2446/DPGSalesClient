using AuthenticationProxy;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthorizationProxy;
using Microsoft.AspNetCore.Http;

namespace DPGSalesClient.ServiceConnectors
{
    public class AuthServiceConnector
    {
        #region Variables

        private AuthenticationProxyServiceClient _authproxy = new AuthenticationProxyServiceClient(AuthenticationProxyServiceClient.EndpointConfiguration.AuthenticationServiceSOAPHttpsEndPoint);      
        private AuthorizationProxyServiceClient _authorizePxy = new AuthorizationProxyServiceClient(AuthorizationProxyServiceClient.EndpointConfiguration.AuthorizationServiceSOAPHttpsEndPoint);      

        #endregion

        public AuthServiceConnector(string strRemoteIP, IHttpContextAccessor httpContextAccessor)
        {

            if (!String.IsNullOrEmpty(strRemoteIP))
            {
                // _authproxy = new AuthenticationProxyServiceClient(AuthenticationProxyServiceClient.EndpointConfiguration.AuthenticationServiceSOAPEndPoint, strRemoteIP + "/GateKeeper/Proxy/AuthenticationProxyService.svc/SOAP");
                //_authorizePxy = new AuthorizationProxyServiceClient(AuthorizationProxyServiceClient.EndpointConfiguration.AuthorizationServiceSOAPEndPoint, strRemoteIP + "/GateKeeper/Proxy/AuthorizationProxyService.svc/SOAP");

                if (strRemoteIP.Contains("https"))
                {
                    _authproxy = new AuthenticationProxyServiceClient(AuthenticationProxyServiceClient.EndpointConfiguration.AuthenticationServiceSOAPHttpsEndPoint, strRemoteIP + "/AuthenticationProxyService.svc/SOAP");
                    _authorizePxy = new AuthorizationProxyServiceClient(AuthorizationProxyServiceClient.EndpointConfiguration.AuthorizationServiceSOAPHttpsEndPoint, strRemoteIP + "/AuthorizationProxyService.svc/SOAP");
                }
                else
                {
                    _authproxy = new AuthenticationProxyServiceClient(AuthenticationProxyServiceClient.EndpointConfiguration.AuthenticationServiceSOAPEndPoint, strRemoteIP + "/AuthenticationProxyService.svc/SOAP");
                   // _authproxy.Endpoint.Binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None);

                    _authorizePxy = new AuthorizationProxyServiceClient(AuthorizationProxyServiceClient.EndpointConfiguration.AuthorizationServiceSOAPEndPoint, strRemoteIP + "/AuthorizationProxyService.svc/SOAP");
                    //_authorizePxy.Endpoint.Binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None);
                }               
            }

            _authorizePxy.Endpoint.EndpointBehaviors.Add(new ServiceBehaviors.TokenEndpointBehavior(httpContextAccessor));         
        }

        public async Task<string> LoginAsync(AGS_Credentials creds)
        {
            try
            {               
                return await _authproxy.AuthenticateAsync(creds);
            }
            catch (Exception ex)
            {
              
            }
            return "";
        }

        public async Task<bool> ForgotAsync(string user)
        {
            try
            {
                return await _authproxy.ForgotPasswordAsync(user);
            }
            catch (Exception ex)
            {
              
            }
            return false;
        }

        public async Task<bool> ResetPasswordAsync(AGS_ResetPassword model)
        {
            try
            {
                return await _authproxy.ResetPasswordAsync(model);
            }
            catch (Exception ex)
            {
               
            }
            return false;
        }

        public async Task<AGS_LoginUserInfo> GetLoginUserInfoAsync()
        {
            try
            {
                return await _authorizePxy.LoginUserInfoAsync();
            }
            catch (Exception ex)
            {
               
            }
            return null;
        }

        public async Task<List<AGS_NavigationHeader>> GetBsnrAsync()
        {
            try
            {
                return await _authorizePxy.BusinessScenariosAsync();
            }
            catch (Exception ex)
            {
               
            }
            return null;
        }


        //public async Task<string[]> CreateAsync(BusinessEntityCollection objBEC)
        //{
        //    var result = await _proxy.CreateAsync(objBEC);
        //    return result;
        //}

        //public async Task<bool> UpdateAsync(BusinessEntityCollection objBEC)
        //{
        //    var result = await _proxy.UpdateAsync(objBEC);
        //    return result;
        //}

        //public async Task<BusinessEntityCollection> RetrieveAsync(RetriveEntity objRetriveEntity)
        //{
        //    var result = await _proxy.RetrieveAsync(objRetriveEntity);
        //    return result;
        //}

        //public async Task<bool> DeleteAsync(DeleteEntity objDeleteEntity)
        //{
        //    var result = await _proxy.DeleteAsync(objDeleteEntity);
        //    return result;
        //}


        #region Back up 

        //public async Task<AGS_LoginValidationResponse> LoginAsync(AGS_LoginValidationRequest objLoginReq)
        //{
        //    var result = await _proxy.LoginAsync(new LoginRequest() { objRequest = objLoginReq });
        //    return result.LoginResult;
        //}

        //public async Task<string[]> CreateAsync(BusinessEntityCollection objBEC)
        //{
        //    var result = await _proxy.CreateAsync(new CreateRequest() { objBEC = objBEC });
        //    return result.CreateResult;
        //}

        //public async Task<bool> UpdateAsync(BusinessEntityCollection objBEC)
        //{
        //    var result = await _proxy.UpdateAsync(new UpdateRequest() { objBEC = objBEC });
        //    return result.UpdateResult;
        //}

        //public async Task<BusinessEntityCollection> RetrieveAsync(RetriveEntity retriveEntity)
        //{
        //    var result = await _proxy.RetrieveAsync(new RetrieveRequest() { objRetriveEntity = retriveEntity });
        //    return result.RetrieveResult;
        //}

        //public async Task<bool> DeleteAsync(DeleteEntity objDeleteEntity)
        //{
        //    var result = await _proxy.DeleteAsync(new DeleteRequest() { objDeleteEntity = objDeleteEntity });
        //    return result.DeleteResult;
        //}


        #endregion


    }
}
