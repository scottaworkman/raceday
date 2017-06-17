using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RaceDay.ViewModels
{
	public class IndexViewModel : BaseViewModel
	{
		public EventFormViewModel EventForm { get; set; }
		public List<EventInfo> Events { get; set; }

		public IndexViewModel()
		{
			EventForm = new EventFormViewModel();
			Events = new List<EventInfo>();
		}
	}
}