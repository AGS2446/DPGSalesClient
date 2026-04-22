using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.Models
{
    public class AttachmentsModel
    {
        public string ActivityId { get; set; }
        public string RefObjectId { get; set; }
        public string CustomerName { get; set; }
        public string PendingUploadFiles { get; set; }
        public string DocType { get; set; }
        public List<DownloadFileObject> UploadedFiles { get; set; }
    }
    public class UploadFileobject
    {
        public string Name { get; set; }
        public string File { get; set; }
    }
    public class UploadFile
    {
        public List<UploadFileobject> Files { get; set; }
    }

    public class DownloadFileObject
    {
        public int AttachmentId { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string FileUrl { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
    }
}
