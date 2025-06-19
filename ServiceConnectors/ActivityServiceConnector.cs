using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActivityProxy;
using Microsoft.AspNetCore.Http;

namespace DPGSalesClient.ServiceConnectors
{
    public class ActivityServiceConnector
    {
        private ActivityProxyServiceClient _serActivity = new ActivityProxyServiceClient(ActivityProxyServiceClient.EndpointConfiguration.ActivityProxySOAPHttpsEndPoint);
        public ActivityServiceConnector(string strRemoteIP, IHttpContextAccessor httpContextAccessor)
        {
            if (!String.IsNullOrEmpty(strRemoteIP))
            {
                if (strRemoteIP.Contains("https"))
                {
                    _serActivity = new ActivityProxyServiceClient(ActivityProxyServiceClient.EndpointConfiguration.ActivityProxySOAPHttpsEndPoint, strRemoteIP + "/ActivityProxyService.svc/SOAP");
                }
                else
                {
                    _serActivity = new ActivityProxyServiceClient(ActivityProxyServiceClient.EndpointConfiguration.ActivityProxySOAPEndPoint, strRemoteIP + "/ActivityProxyService.svc/SOAP");
                   // _serActivity.Endpoint.Binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None);
                }

                
            }
            _serActivity.Endpoint.EndpointBehaviors.Add(new ServiceBehaviors.TokenEndpointBehavior(httpContextAccessor));
            
        }

        public async Task<string> CreateActivity(AGS_Activity_New newObj)
        {
           return await _serActivity.CreateAsync(newObj);
        }

        public async Task<bool> CloseActivity(string strId,AGS_Activity_Close model)
        {
            return await _serActivity.CloseAsync(strId, model);
        }

        public async Task<bool> DeleteActivity(string strId)
        {
            return await _serActivity.DeleteAsync(strId);

        }

        public async Task<bool> UpdateActivity(string strId, AGS_Activity_Modify model)
        {
            return await _serActivity.UpdateAsync(strId, model);
        }
        public async Task<AGS_Activity_Retrieve> RetriveActivity(string strId)
        {
            return await _serActivity.RetrieveAsync(strId);
        }
        public async Task<List<AGS_Activity_Retrieve>> RetriveActivities(string sortBy, string sortOrder, int pageSize, string marker, int currentIndex)
        {
            return await _serActivity.RetrieveAllAsync(sortBy,sortOrder,pageSize,marker,currentIndex);
        }

        public async Task<bool> ReScheduleActivity(string strId, AGS_Activity_ReSchedule model)
        {
            return await _serActivity.ReScheduleAsync(strId, model);
        }
        public async Task<List<AGS_Activity_Retrieve>> SearchActivities(string strKey)
        {
            return await _serActivity.SearchAsync(strKey);
        }
        public async Task<List<AGS_Activity_Retrieve>> RetriveByDocumentID(string documentId)
        {
            return await _serActivity.RetrieveByDocumentIdAsync(documentId);
        }

    }
}
