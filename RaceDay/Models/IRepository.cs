using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceDay.Models
{
	public interface IRepository
	{
		void SaveChanges();
		void DiscardAndReset();

		MFUser GetUserById(string userId);
		MFUser CreateUser(Facebook.FacebookUser user);

		void DefaultGroup(MFUser user, Group group, GroupRoleEnum role);
		GroupRoleEnum IsUserInGroup(MFUser user, Group group);
		List<GroupMember> UserMembership(MFUser user);

		Group FindGroupByCode(string code);
		Group FindGroupByCodeAndKey(string code, string key);

		Event GetEventById(Int32 eventId);
		Event AddEvent(Int32 groupId, String name, DateTime date, String url, String location, String description, String creator);
        Event AddEvent(Event eventInfo);
        bool DeleteEvent(int eventId);
		void AddUserToEvent(MFUser user, Event newEvent, AttendingEnum isAttending);
		void RemoveUserFromEvent(MFUser user, Event eEvent);
		List<Event> GetUserEvents(MFUser user);
		List<MFUser> GetUsersForEvent(Event eEvent);

		List<viewEventUserAttendance> GetUpcomingEventsForUser(DateTime dtStart, string idUser);
	}
}
