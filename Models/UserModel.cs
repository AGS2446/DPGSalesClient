using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.Models
{
    public class UserViewModel
    {
        public string SearchKey { get; set; }
        public List<UserViewItemModel> UserList { get; set; }
        public class UserViewItemModel
        {
            public string UserID { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime? ValidFrom { get; set; }
            public DateTime? ValidTo { get; set; }
            public string Mobile { get; set; }
            public string LogonName { get; set; }
            public string Email { get; set; }
            public string Status { get; set; }
        }
    }

    public class UserNewModel
    {
        public string UserID { get; set; }

        [Required]
        public string LogonName { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Mobile { get; set; }
        [Required]
        public string Email { get; set; }
        public string Status { get; set; }
    }


    public class UserDetailsViewModel
    {
        public string UserID { get; set; }

        [Required]
        public string LogonName { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Mobile { get; set; }
        [Required]
        public string Email { get; set; }
        public string Status { get; set; }
        public List<UserRoleViewModel> Roles { get; set; }
        public List<UserPositionViewModel> Positions { get; set; }
        public List<UserReportingViewModel> Reportings { get; set; }
    }
    public class UserRoleViewModel
    {
        public string UserID { get; set; }
        public string RoleID { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }

    }

    public class UserPositionViewModel
    {
        public string UserID { get; set; }
        public string LogonName { get; set; }
        public string UserName { get; set; }
        public string RoleID { get; set; }
        public string RoleName { get; set; }
        public string OrgID { get; set; }
        public string OrgName { get; set; }
        public string LobID { get; set; }
        public string LobName { get; set; }
        public string ClientID { get; set; }
        public string ClientName { get; set; }
    }

    public class UserReportingViewModel
    {
        public string UserID { get; set; }
        public string ReportingUserID { get; set; }
        public string ReportingLogonName { get; set; }
        public string ReportingUserName { get; set; }      
        public string ReportingMobile { get; set; }
        public string ReportingEmail { get; set; }
    }


    public class ModifyUserRoleViewModel
    {
        [Required]
        public string UserID { get; set; }
        [Required]
        [Display(Name = "Role")]
        public string RoleID { get; set; }

        [Display(Name = "Role")]
        public string RoleName { get; set; }

        public List<SelectListItemObject> Roles { get; set; }

    }
    public class ModifyUserPositionViewModel
    {
        [Required]
        public string UserID { get; set; }
        [Required]
        [Display(Name = "Role Name")]
        public string RoleID { get; set; }

        [Required]
        [Display(Name = "Client")]
        public string ClientID { get; set; }

        [Required]
        [Display(Name = "Line of Business")]
        public string LOBID { get; set; }

        [Required]
        [Display(Name = "Organization")]
        public string OrgID { get; set; }

        public List<SelectListItemObject> Clients { get; set; }
        public List<SelectListItemObject> LOBs { get; set; }
        public List<SelectListItemObject> Organizations { get; set; }
        public List<SelectListItemObject> Roles { get; set; }
    }

    public class ModifyUserReportingViewModel
    {
        [Required]
        public string UserID { get; set; }
        [Required]

        [Display(Name = "Reporting User")]
        public string ReportingUserID { get; set; }

        [Display(Name = "Reporting User")]
        public string ReportingUser { get; set; }
        public List<SelectListItemObject> ReportingUsers { get; set; }
    }

    public class UserExportModel
    {
        public string UserID { get; set; }
        public string EmployeeCode { get; set; }
        public string Username { get; set; }        
        public string Branch { get; set; }
        public string RoleName { get; set; }
        public string MobileNumber { get; set; }
        public string EmailID { get; set; }
        public string Status { get; set; }
        public string ReportingName { get; set; }
        public string Division { get; set; }
    }

}
