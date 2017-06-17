using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RaceDay.ViewModels
{
	public class LogonViewModel : BaseViewModel
	{
		public string ApplicationId { get; set; }
		public string RedirectUri { get; set; }
		public string Permissions { get; set; }
	}
}