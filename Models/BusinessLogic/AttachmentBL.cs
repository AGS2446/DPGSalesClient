using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.Models.BusinessLogic
{
    public class AttachmentBL
    {
        ServiceConnectors.FileManagerServiceConnector _serFile = null;
        public AttachmentBL(string strRemoteIp, IHttpContextAccessor httpContextAccessor)
        {
            _serFile = new ServiceConnectors.FileManagerServiceConnector(strRemoteIp, httpContextAccessor);
        }

        public async Task<AttachmentsModel> GetAttachmentModel(string processName,string activityId)
        {
            var objAtt = new AttachmentsModel();
            try
            {
                var lsFilesUpload = new UploadFile();
                lsFilesUpload.Files = new List<UploadFileobject>();

                var inCnt = 0;

                //Uploaded files
                var objFiles = await _serFile.GetFiles(processName.ToUpper(), activityId);
                var lsFiles = new List<DownloadFileObject>();
                if (objFiles != null && objFiles.Count > 0)
                {
                    lsFiles = objFiles.Select(x => new DownloadFileObject
                    {
                        AttachmentId=x.AttachmentID,
                        Name = x.Name,
                        FileUrl = x.URL,
                        CreatedOn=x.CreatedOn,
                        UserID=x.UserID,
                        UserName=x.Username,
                        DocumentType=x.DocumentType
                        
                    }).ToList();

                    inCnt = lsFiles.Count;
                }

                //Pending upload files
                for (int intFile = 0; intFile < (6 - inCnt); intFile++)
                {
                    lsFilesUpload.Files.Add(new UploadFileobject { Name = "", File = "" });
                }

                var newObj = Newtonsoft.Json.JsonConvert.SerializeObject(lsFilesUpload);

                objAtt = new AttachmentsModel { ActivityId = activityId, PendingUploadFiles = newObj, UploadedFiles = lsFiles };
            }
            catch (TimeoutException tex)
            {

            }
            catch (Exception ex)
            {
            }
            return objAtt;
        }

        public async Task<List<string>> UploadAttachments(string procesName,string documentId, UploadFile objFU)
        {
            var lsFilesUploaded = new List<string>();
            try
            {
               
                if (objFU.Files.Count > 0)
                {
                    for (int intF = 0; intF < objFU.Files.Count; intF++)
                    {
                        if (objFU.Files[intF].Name != "")
                        {
                            var blRes = await _serFile.UploadFile(new FileManagerProxy.AGS_Attachment_Upload
                            {
                                Name = objFU.Files[intF].Name,
                                DocumentID = documentId,
                                ObjectName = procesName.ToUpper(),
                                Base = objFU.Files[intF].File,
                                DocumentType=""
                            });

                            if (blRes)
                            {
                                lsFilesUploaded.Add(objFU.Files[intF].Name + " - Uploaded");
                            }
                        }

                    }
                }
            }
            catch(TimeoutException tex)
            {

            }
            catch (Exception ex)
            {
            }

            return lsFilesUploaded;
        }

        public async Task<List<string>> UploadAttachment(string procesName, string documentId, UploadFileobject objFU)
        {
            var lsFilesUploaded = new List<string>();
            try
            {

                if (objFU.File!="")
                {
                    
                        if (objFU.Name != "")
                        {
                            var blRes = await _serFile.UploadFile(new FileManagerProxy.AGS_Attachment_Upload
                            {
                                
                                Name = objFU.Name,
                                DocumentID = documentId,
                                ObjectName = procesName.ToUpper(),
                                Base = objFU.File
                            });

                            if (blRes)
                            {
                                lsFilesUploaded.Add(objFU.Name + " - Uploaded");
                            }
                        }

                   
                }
            }
            catch (TimeoutException tex)
            {

            }
            catch (Exception ex)
            {
            }

            return lsFilesUploaded;
        }

        public async Task<List<DownloadFileObject>> GetAttachments(string procesName, string documentId)
        {
            var lsResFiles = new List<DownloadFileObject>();
            try
            {

                var objFiles = await _serFile.GetFiles(procesName.ToUpper(), documentId);

              
                if (objFiles != null && objFiles.Count > 0)
                {
                    lsResFiles = objFiles.Select(x => new DownloadFileObject
                    {
                        AttachmentId=x.AttachmentID,
                        Name = x.Name,                        
                        FileUrl = x.URL,
                        CreatedOn = x.CreatedOn,
                        UserID=x.UserID,
                        UserName=x.Username,
                        DocumentType=x.DocumentType
                    }).ToList();
                }
            }
            catch (TimeoutException tex)
            {

            }
            catch (Exception ex)
            {
            }

            return lsResFiles;
        }

        public bool VarifyJsonFileObject(UploadFile objFU)
        {
            var blRes = false;
            try
            {
                if (objFU.Files.Where(x => x.Name == "").Count() != objFU.Files.Count)
                    blRes = true;

            }
            catch (Exception ex)
            {
            }

            return blRes;
        }

        public async Task<bool> DeleteAttachment(int Id)
        {
            var blRes = false;
            try
            {
                blRes = await _serFile.DeleteFile(Id.ToString());
            }
            catch (TimeoutException tex)
            {

            }
            catch (Exception ex)
            {
            }
            return blRes;
        }
        public async Task<List<string>> UploadEnquiryAttachments(string Type,string procesName, string documentId, UploadFile objFU)
        {
            var lsFilesUploaded = new List<string>();
            try
            {

                if (objFU.Files.Count > 0)
                {
                    for (int intF = 0; intF < objFU.Files.Count; intF++)
                    {
                        if (objFU.Files[intF].Name != "")
                        {
                            var blRes = await _serFile.UploadFile(new FileManagerProxy.AGS_Attachment_Upload
                            {
                                Name = objFU.Files[intF].Name,
                                DocumentID = documentId,
                                ObjectName = procesName.ToUpper(),
                                Base = objFU.Files[intF].File,
                                DocumentType = Type
                            });

                            if (blRes)
                            {
                                lsFilesUploaded.Add(objFU.Files[intF].Name + " - Uploaded");
                            }
                        }

                    }
                }
            }
            catch (TimeoutException tex)
            {

            }
            catch (Exception ex)
            {
            }

            return lsFilesUploaded;
        }
    }
}
