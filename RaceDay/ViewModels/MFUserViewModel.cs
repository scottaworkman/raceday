using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RaceDay.ViewModels
{
	public class MFUserViewModel
	{
		[Required(ErrorMessage="Facebook UserId is required")]
		[Display(Name="User ID")]
		public String UserId { get; set; }

		[Required(ErrorMessage="Name is required")]
		[StringLength(100, ErrorMessage = "Maximum length of 100 characters")]
		[Display(Name="Name")]
		public string Name { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        [StringLength(50, ErrorMessage = "Maximum length of 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(50, ErrorMessage = "Maximum length of 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [StringLength(100, ErrorMessage = "Maximum length of 100 characters")]
        [EmailAddress(ErrorMessage = "Invalid Email address")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}