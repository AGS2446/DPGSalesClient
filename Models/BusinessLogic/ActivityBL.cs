using ActivityProxy;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.Models.BusinessLogic
{
    public class ActivityBL
    {
        ServiceConnectors.ActivityServiceConnector _serActivity = null;
        ServiceConnectors.EntityMapServiceConnector _serEntity = null;
        AttachmentBL _serFile = null;
        CustomerBL _serCust = null;
        public ActivityBL(string strRemoteIp,IHttpContextAccessor httpContextAccessor)
        {
            _serActivity = new ServiceConnectors.ActivityServiceConnector(strRemoteIp, httpContextAccessor);
            _serEntity = new ServiceConnectors.EntityMapServiceConnector(strRemoteIp, httpContextAccessor);
            _serFile = new AttachmentBL(strRemoteIp, httpContextAccessor);
            _serCust = new CustomerBL(strRemoteIp, httpContextAccessor);
        }

        public async Task<ContactPlanViewModel> GetAllActivities()
        {
            ContactPlanViewModel objResult = new ContactPlanViewModel();
            try
            {
                objResult.IsNoData = false;

                var lsData = await _serActivity.RetriveActivities("", "", 0, "", 0);
                if (lsData != null && lsData.Count > 0)
                {
                    objResult.ActivityList = lsData.Select(x => new ContactPlanViewModel.ActivityListItem { ActivityID = x.ActivityID, VisitOn = x.FromDate, ObjectOfVisit = x.ObjectiveoftheVisit, Status = x.Status, CreatedOn = x.CreatedOn, CustomerName = x.CustomerName, DocumentID = x.DocumentID }).ToList();
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }
            return objResult;
        }
        public async Task<ContactPlanViewModel> GetAllActivitiesByDocumentID(string strRefId)
        {
            ContactPlanViewModel objResult = new ContactPlanViewModel();
            try
            {
                objResult.IsNoData = false;

                var lsData = await _serActivity.RetriveByDocumentID(strRefId);
                if (lsData != null && lsData.Count > 0)
                {
                    objResult.ActivityList = lsData.Select(x => new ContactPlanViewModel.ActivityListItem { ActivityID = x.ActivityID, VisitOn = x.FromDate, ObjectOfVisit = x.ObjectiveoftheVisit, Status = x.Status, CreatedOn = x.CreatedOn, CustomerName = x.CustomerName,DocumentID=x.DocumentID }).ToList();
                }else
                {
                    objResult.IsNoData = true;
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }
            return objResult;
        }
        public async Task<ContactPlanNewModel> ActvityCreateObject(string refId=null,string strCust="")
        {
            var objContPlan = new ContactPlanNewModel();
            try
            {
                objContPlan.VisitOn = DateTime.Now.ToString("dd/MM/yyyy");
                objContPlan.DocumentID = refId;
                var lsEntities = await _serEntity.RetriveByObjectName("Activity");
               
                if (lsEntities != null && lsEntities.Count > 0)
                {
                    objContPlan.SupportRequiredList = SalesStaticMethods.GetSelectlistItemsByName("SupportRequired", lsEntities, "P");
                    objContPlan.ObjectiveOfVisitList = SalesStaticMethods.GetSelectlistItemsByName("ObjectiveOftheVisit", lsEntities, "P");
                }

                if (strCust != "")
                {
                    objContPlan.Name = strCust;

                    var custInfo=await _serCust.GetCustomers(strCust);
                    if (custInfo != null && custInfo.Count>0)
                    {
                        objContPlan.CustomerID = custInfo[0].customerId;
                        
                        objContPlan.MobileNumber = custInfo[0].mobilenumber;
                        objContPlan.EmailID = custInfo[0].emailid;
                        objContPlan.ContactName = custInfo[0].contactperson;
                    }
                }

            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return objContPlan;
        }

        public async Task<ContactPlanNewModel> ActvityEditObject(string activityId)
        {
            var objAct = new ContactPlanNewModel();
            try
            {
                var objActivity = await _serActivity.RetriveActivity(activityId);
                if (objActivity != null)
                {
                    objAct = new ContactPlanNewModel
                    {
                        ActivityId = objActivity.ActivityID,
                        VisitOn = objActivity.FromDate.HasValue ? objActivity.FromDate.Value.ToString("dd/MM/yyyy") : "",
                        CustomerID = objActivity.CustomerID,
                        Name = objActivity.CustomerName,
                        DocumentID = objActivity.DocumentID,
                        ListofDicsussion = objActivity.ListofDisussion,
                        ObjectiveofVisit = objActivity.ObjectiveoftheVisit,
                        OutcomeofMeeting = objActivity.OutcomeoftheMeeting,
                        OtherTeamMembers = objActivity.OtherTeamMembers,
                        Remarks = objActivity.Remarks,
                        SupportRequired = objActivity.SupportRequired,
                        ContactName = objActivity.ContactName,
                        Designation = objActivity.Designation,
                        EmailID = objActivity.EmailID,
                        MobileNumber = objActivity.MobileNumber
                    };
                }

                var lsEntities = await _serEntity.RetriveByObjectName("Activity");
                if (lsEntities != null && lsEntities.Count > 0)
                {
                    objAct.SupportRequiredList = SalesStaticMethods.GetSelectlistItemsByName("SupportRequired", lsEntities, "P");
                    objAct.ObjectiveOfVisitList = SalesStaticMethods.GetSelectlistItemsByName("ObjectiveOftheVisit", lsEntities, "P");
                }


            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }

            return objAct;
        }

        public async Task<ContactPlanDetailsModel> ActvityDetailsObject(string processName, string activityId)
        {
            var detailsObject = new ContactPlanDetailsModel();
            try
            {

                var objActivity = await _serActivity.RetriveActivity(activityId);

                var lsFiles = await _serFile.GetAttachments(processName.ToUpper(), activityId);
              
                detailsObject = new ContactPlanDetailsModel
                {
                    ActivityId = objActivity.ActivityID,
                    VisitOn = objActivity.FromDate,
                    CustomerID = objActivity.CustomerID,
                    Status = objActivity.Status,
                    CustomerName = objActivity.CustomerName,
                    DocumentID = objActivity.DocumentID,
                    ListofDicsussion = objActivity.ListofDisussion,
                    ObjectiveofVisit = objActivity.ObjectiveoftheVisit,
                    OutcomeofMeeting = objActivity.OutcomeoftheMeeting,
                    OtherTeamMembers = objActivity.OtherTeamMembers,
                    ContactName = objActivity.ContactName,
                    Designation = objActivity.Designation,
                    EmailID = objActivity.EmailID,
                    MobileNumber = objActivity.MobileNumber,
                    Remarks = objActivity.Remarks,
                    SupportRequired = objActivity.SupportRequired,
                    Files = lsFiles
                };
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }
            return detailsObject;
        }

        public async Task<string> ActivityCreate(ContactPlanNewModel objInput)
        {
            var strResult = "";
            try
            {
                AGS_Activity_New objNewActivity = new AGS_Activity_New();
                objNewActivity.FromDate = SalesStaticMethods.ConvertDate(objInput.VisitOn);
                objNewActivity.ListofDisussion = objInput.ListofDicsussion;
                objNewActivity.ObjectiveoftheVisit = objInput.ObjectiveofVisit;
                objNewActivity.OtherTeamMembers = objInput.OtherTeamMembers;
                objNewActivity.Remarks = objInput.Remarks;
                objNewActivity.SupportRequired = objInput.SupportRequired;
                objNewActivity.DocumentID = objInput.DocumentID;
                objNewActivity.MobileNumber = objInput.MobileNumber;
                objNewActivity.ContactName = objInput.ContactName;
                objNewActivity.Designation = objInput.Designation;
                objNewActivity.EmailID = objInput.EmailID;
                objNewActivity.CustomerID = objInput.CustomerID;
                objNewActivity.CustomerName = objInput.Name;

                 strResult = await _serActivity.CreateActivity(objNewActivity);
            }
            catch (Exception ex)
            {

                
            }

            return strResult;
        }

        public async Task<bool> ActivityEdit(ContactPlanNewModel objInput)
        {
            var blRes = false;
            try
            {
                AGS_Activity_Modify objEdit = new AGS_Activity_Modify();
                objEdit.ActivityID = objInput.ActivityId;
                objEdit.CustomerID = objInput.CustomerID;
                objEdit.CustomerName = objInput.Name;
                objEdit.ListofDisussion = objInput.ListofDicsussion;
                objEdit.ModifiedOn = DateTime.Now;
                objEdit.ObjectiveoftheVisit = objInput.ObjectiveofVisit;
                objEdit.OtherTeamMembers = objInput.OtherTeamMembers;
                objEdit.Remarks = objInput.Remarks;
                objEdit.SupportRequired = objInput.SupportRequired;
                objEdit.MobileNumber = objInput.MobileNumber;
                objEdit.ContactName = objInput.ContactName;
                objEdit.Designation = objInput.Designation;
                objEdit.EmailID = objInput.EmailID;

                blRes = await _serActivity.UpdateActivity(objInput.ActivityId, objEdit);
               

            }
            catch (TimeoutException tex)
            {

            }
            catch (Exception ex)
            {
            }
            return blRes;

        }

        public async Task<bool> RescheduleActivity(string activityId, string strNewVisitDate)
        {
            var blRes = false;

            try
            {
                AGS_Activity_ReSchedule objResch = new AGS_Activity_ReSchedule();
                objResch.ActivityID = activityId;
                objResch.FromDate = SalesStaticMethods.ConvertDate(strNewVisitDate);

                blRes = await _serActivity.ReScheduleActivity(activityId, objResch);
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }
            return blRes;
        }

        public async Task<bool> CloseActivity(string activityId, string strOutcome)
        {
            var blRes = false;
            try
            {
                AGS_Activity_Close objClose = new AGS_Activity_Close();
                objClose.ActivityID = activityId;
                objClose.OutcomeoftheMeeting = strOutcome;

                blRes = await _serActivity.CloseActivity(activityId, objClose);
               
            }
            catch (TimeoutException tex)
            {

            }
            catch (Exception ex)
            {
            }
            return blRes;
        }

        public async Task<ContactPlanViewModel> Search(string strKey)
        {
            ContactPlanViewModel objResult = new ContactPlanViewModel();            
            try
            {
                objResult.IsNoData = false;

                var lsData = await _serActivity.SearchActivities(strKey);
                if (lsData != null && lsData.Count > 0)
                {
                    objResult.ActivityList = lsData.Select(x => new ContactPlanViewModel.ActivityListItem { ActivityID = x.ActivityID, VisitOn = x.FromDate, ObjectOfVisit = x.ObjectiveoftheVisit, Status = x.Status, CreatedOn = x.CreatedOn, CustomerName = x.CustomerName, DocumentID = x.DocumentID }).ToList();
                }
                else
                {
                    // TempData.SetObjectAsJson("PopupViewModel", SalesStaticMethods.CreatePopupModel("Activity", "No Data Available"));
                    objResult.IsNoData = true;
                    //lsData = await _serActivity.RetriveActivities("", "", 0, "", 0);
                    //if (lsData != null && lsData.Count > 0)
                    //{
                    //    objResult.ActivityList = lsData.Select(x => new ContactPlanViewModel.ActivityListItem { ActivityID = x.ActivityID, VisitOn = x.FromDate, ObjectOfVisit = x.ObjectiveoftheVisit, Status = x.Status, CreatedOn = x.CreatedOn, CustomerName = x.CustomerName, DocumentID = x.DocumentID }).ToList();
                    //}
                }
            }
            catch (TimeoutException tex) { }
            catch (Exception ex)
            {
            }
            return objResult;
        }
    }
}
