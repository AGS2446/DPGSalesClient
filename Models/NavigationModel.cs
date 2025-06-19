using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.Models
{
    public class NavHeaderViewModel
    {
        public string ScenarioID { get; set; }
        [Required]
        [Display(Name = "Scenario Name")]
        public string ScenarioName { get; set; }
        [Required]
        public string Description { get; set; }
        [Display(Name = "Scenario Icon")]
        public string Icon { get; set; }
        [Display(Name = "BG Color Class")]
        public string Class { get; set; }

        public int? OrderBy { get; set; }

        public virtual ICollection<NavItemViewModel> Items { get; set; }
    }

    public class NavItemViewModel
    {
        [Display(Name = "Process ID")]
        public string ProcessID { get; set; }
        [Display(Name = "Process Name")]
        [Required]
        public string ProcessName { get; set; }
        [Required]
        public string Description { get; set; }
        [Display(Name = "Controller Name")]
        [Required]
        public string Controller { get; set; }
        [Display(Name = "Action Method")]
        [Required]
        public string Action { get; set; }
        [Display(Name = "Bussiness Scenario")]
        [Required]
        public string ScenarioID { get; set; }
        [Display(Name = "Bussiness Scenario")]
        public string ScenarioName { get; set; }
        [Display(Name = "Icon Class")]
        public string Icon { get; set; }
        [Display(Name = "Css Class")]
        public string Class { get; set; }

        public int? OrderBy { get; set; }
    }
}
