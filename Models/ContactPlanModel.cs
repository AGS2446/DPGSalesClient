using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.Models
{
    public class ContactPlanViewModel
    {
        public string RefObjectID { get; set; }
        public string CustomerName { get; set; }
        public string SearchName { get; set; }
        public bool IsNoData { get; set; }
        public List<ActivityListItem> ActivityList { get; set; }
        public class ActivityListItem
        {
            public string ActivityID { get; set; }
            public string DocumentID { get; set; }
            public DateTime? VisitOn { get; set; }
            public string ObjectOfVisit { get; set; }
            public string Status { get; set; }
            public DateTime? CreatedOn { get; set; }
            public string CustomerName { get; set; }
        }
    }

    public class ContactPlanNewModel
    {

        public string ActivityId { get; set; }
        [Required]
        public string VisitOn { get; set; }

       
        [Required]
        public string ObjectiveofVisit { get; set; }
        [Required]
        public string ListofDicsussion { get; set; }
        public string OutcomeofMeeting { get; set; }
        [Required]
        public string SupportRequired { get; set; }
        public string OtherTeamMembers { get; set; }
        public DateTime? NextPlanedVisitOn { get; set; }
        [Required]
        public string ContactName { get; set; }
        [Required]
        public string Designation { get; set; }
        [Required]
        public string MobileNumber { get; set; }
        [Required]
        public string EmailID { get; set; }

        public string Remarks { get; set; }
        public string DocumentID { get; set; }
     
        public string CustomerID { get; set; }
        [Required]
        public string Name { get; set; }   
        public string DataFiles { get; set; }
        public List<SelectListItemObject> ObjectiveOfVisitList { get; set; }
        public List<SelectListItemObject> SupportRequiredList { get; set; }

    }

    public class ContactPlanDetailsModel
    {
        public string ActivityId { get; set; }
        public string Status { get; set; }
        public DateTime? VisitOn { get; set; }      
        public string ObjectiveofVisit { get; set; }       
        public string ListofDicsussion { get; set; }
        public string OutcomeofMeeting { get; set; }      
        public string SupportRequired { get; set; }
        public string OtherTeamMembers { get; set; }
        public DateTime? NextPlanedVisitOn { get; set; }
        public string Remarks { get; set; }
        public string DocumentID { get; set; }
        public string CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string ContactName { get; set; }
        public string Designation { get; set; }
        public string MobileNumber { get; set; }
        public string EmailID { get; set; }
        
        public List<DownloadFileObject> Files { get; set; }

    }
      



    public class JsonResultObject
    {
        public bool Status { get; set; }
        public string Message { get; set; }
    }

  
   


}
