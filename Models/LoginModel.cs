using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.Models
{
    public class LoginViewModel
    {
        [Required]
        public string UserID { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class LoginUserInfo
    {
        public string UserID { get; set; }
        public string Username { get; set; }
        public string LogonName { get; set; }
        public string RoleID { get; set; }
        public string RoleName { get; set; }

    }


    public class ForgotPassword {
        public string UserID { get; set; }
    }
    public class ResetPassword {
        public string UserID { get; set; }
        public string Password { get; set; }
        public string Code { get; set; }
        public string ConfirmPassword { get; set; }
    }

   

}
