using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RaceDay.ViewModels
{
	public class EventFormViewModel
	{
		[Required(ErrorMessage="Group selection is required")]
		[Display(Name="Group")]
		public Int32 GroupId { get; set; }

		[Required(ErrorMessage="Name is required")]
		[StringLength(50, ErrorMessage = "Maximum length of 50 characters")]
		[Display(Name="Name")]
		public string EventName { get; set; }

		[Required(ErrorMessage="Date is required")]
		[Display(Name="Date")]
		[DataType(DataType.Date, ErrorMessage="Invalid date format (mm/dd/yyyy)")]
		public DateTime EventDate { get; set; }

		[Display(Name="Web Site")]
		[StringLength(200, ErrorMessage = "Maximum length of 200 characters")]
		[DataType(DataType.Url, ErrorMessage="Invalid web site format")]
		public string EventUrl { get; set; }

		[Display(Name="Location")]
		[StringLength(50, ErrorMessage="Maximum length of 50 characters")]
		public string EventLocation { get; set; }

		[Display(Name="Description")]
		[StringLength(500, ErrorMessage="Maximum length of 500 characters")]
		public string EventDescription { get; set; }
	}
}