using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileManagerProxy;
using Microsoft.AspNetCore.Http;

namespace DPGSalesClient.ServiceConnectors
{
    public class FileManagerServiceConnector
    {
        private FileManagerServiceClient _serFM = new FileManagerServiceClient(FileManagerServiceClient.EndpointConfiguration.FileManagerProxySOAPEndPoint);
        public FileManagerServiceConnector(string strRemoteIP, IHttpContextAccessor httpContextAccessor)
        {
            if (!String.IsNullOrEmpty(strRemoteIP))
            {
                if (strRemoteIP.Contains("https"))
                {
                    _serFM = new FileManagerServiceClient(FileManagerServiceClient.EndpointConfiguration.FileManagerProxySOAPEndPoint, strRemoteIP + "/FileManagerService.svc/SOAP");
                }
                else
                {
                    _serFM = new FileManagerServiceClient(FileManagerServiceClient.EndpointConfiguration.FileManagerProxySOAPEndPoint, strRemoteIP + "/FileManagerService.svc/SOAP");
                   // _serFM.Endpoint.Binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None);
                }
                
            }
            _serFM.Endpoint.EndpointBehaviors.Add(new ServiceBehaviors.TokenEndpointBehavior(httpContextAccessor));            
        }

        public async Task<bool> UploadFile(AGS_Attachment_Upload model)
        {
           return await _serFM.UploadFileAsync(model);
        }

        public async Task<List<AGS_Attachment_Download>> GetFiles(string objectName,string docId)
        {
            return (await _serFM.GetFilesAsync(objectName, docId)).ToList();
        }
        public async Task<bool> DeleteFile(string Id)
        {
            return await _serFM.DeleteFileAsync(Id);
        }


    }
}
