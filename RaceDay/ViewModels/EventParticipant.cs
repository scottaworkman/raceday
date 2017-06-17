using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RaceDay.Models;

namespace RaceDay.ViewModels
{
	public class EventParticipant
	{
		public String UserId { get; set; }
		public String FirstName { get; set; }
		public String LastName { get; set; }
		public String FullName { get; set; }
		public String Image { get; set; }

		public static EventParticipant FromUser(MFUser user, string access_code)
		{
			EventParticipant p = new EventParticipant()
			{
				UserId = user.UserId,
				FirstName = user.FirstName,
				LastName = user.LastName,
				FullName = user.Name,
				Image = Facebook.FacebookUser.UserImage(user.UserId)
			};

			return p;
		}
	}
}