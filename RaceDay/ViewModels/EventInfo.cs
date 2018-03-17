using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RaceDay.ViewModels
{
	public class EventInfo
	{
		public Int32 EventId { get; set; }
		public String Name { get; set; }
		public DateTime Date { get; set; }
		public String Url { get; set; }
		public String Location { get; set; }
		public String Description { get; set; }
		public String CreatorId { get; set; }
		public Boolean Attending { get; set; }
        public Int32 AttendanceCount { get; set; }

		public EventInfo()
		{
		}

		public EventInfo(Int32 eventId, String eventName, DateTime eventDate, String eventUrl, String eventLocation, String eventDescription, String creatorId, Boolean bAttending, Int32 nAttendanceCount)
		{
			this.EventId = eventId;
			this.Name = eventName;
			this.Date = eventDate;
			this.Url = eventUrl;
			this.Location = eventLocation;
			this.Description = eventDescription;
			this.CreatorId = creatorId;
			this.Attending = bAttending;
            this.AttendanceCount = nAttendanceCount;
		}

        public static EventInfo CopyFromEvent(Boolean attending, RaceDay.Models.Event source)
        {

            return new EventInfo(source.EventId, source.Name, source.Date, source.Url, source.Location, source.Description, source.CreatorId, attending, source.Attendings.Count);
        }

		/// <summary>
		/// Create list of events the user can see by copying from DB query result
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		/// 
		public static List<EventInfo> CopyFromModel(List<RaceDay.Models.Event> source, String currentUserId)
		{
			List<EventInfo> dest = new List<EventInfo>();

			foreach (RaceDay.Models.Event s in source)
			{
				EventInfo d = new EventInfo()
				{
					EventId = s.EventId,
					Name = s.Name,
					Date = s.Date,
					Url = s.Url,
					Location = s.Location,
					Description = s.Description,
					CreatorId = s.CreatorId,
					Attending = (s.Attendings.Count(r => (r.UserId == currentUserId) && (r.Attending1 == (int)AttendingEnum.Attending)) > 0),
                    AttendanceCount = s.Attendings.Count
				};
				dest.Add(d);
			}

			return dest;
		}
	}

	public class AttendanceResult
	{
		public static String AttendingMessage = "I'm Attending";
		public static String NotAttendingMessage = "Not Attending";

		public String Button { get; set; }
		public String Attendees { get; set; }
	}
}