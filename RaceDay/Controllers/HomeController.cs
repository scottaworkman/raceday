using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RaceDay.Facebook;
using RaceDay.Models;
using RaceDay.Utilities;
using RaceDay.ViewModels;

namespace RaceDay.Controllers
{
	[Facebook.FacebookAuthorize]
	[HandleError(View = "Error")]
	public partial class HomeController : BaseController
    {
        // GET: /Home/Index
		//
		// Main page of the application.  Displays the list of events along with participation and also initializes the
		// event form for adding events.
		//
		public virtual ActionResult Index()
        {
			IndexViewModel model = new IndexViewModel();

			// Current user information
			//
			Repository db = new Repository();
			MFUser currentUser = db.GetUserById(FacebookUser.CurrentUser.id);
			if (currentUser == null)
				return RedirectToAction(MVC.Home.Pending());

			// Determine if the user has access to the app through an approved group.
			// TODO:  For now, everyone will get access to JYMF
			//
			List<GroupMember> membership = db.UserMembership(currentUser);
			if ((membership == null) || (membership.Count == 0))
			{
				Group defaultGroup = db.FindGroupByCode("JYMF");
				db.DefaultGroup(currentUser, defaultGroup, GroupRoleEnum.member);
				db.SaveChanges();
			
				membership = db.UserMembership(currentUser);

				AppUserNotification(currentUser.UserId, defaultGroup.Name);
			}

			model.EventForm.GroupId = membership[0].GroupId;

			// Get all events for the current user
			//
			List<Event> events = db.GetUserEvents(currentUser);
			model.Events = EventInfo.CopyFromModel(events, currentUser.UserId);

			// Return the view
			//
			model.PageMessage = GetTempDataPageMessage();
            return View(model);
        }

		// GET: /Home/Pending
		//
		public virtual ActionResult Pending()
		{
			return View();
		}

		// POST: /Home/Event
		//
		// Event form has posted new event information to be added to the event list
		// ValidateInput - Razor view engine provides automatic HTML Encoding so allow all input values
		//   as they will be safely displayed in the view.
		//
		[HttpPost]
		[ValidateInput(false)]
		public virtual ActionResult Event(IndexViewModel model)
		{
			if (ModelState.IsValid)
			{
				Repository db = new Repository();
				Event newEvent = db.AddEvent(
					model.EventForm.GroupId,
					model.EventForm.EventName.Trim(),
					model.EventForm.EventDate,
					(model.EventForm.EventUrl != null ? model.EventForm.EventUrl.Trim() : null),
					(model.EventForm.EventLocation != null ? model.EventForm.EventLocation.Trim() : null),
					(model.EventForm.EventDescription != null ? model.EventForm.EventDescription.Trim() : null),
					FacebookUser.CurrentUser.id
				);
				db.AddUserToEvent(db.GetUserById(FacebookUser.CurrentUser.id), newEvent, AttendingEnum.Attending);
				db.SaveChanges();

				// notify admin to keep track.  TODO: Notify group admin / or other users
				//
				AppEventNotification(newEvent.Name, newEvent.Date);

				// redisplay new list positioning on the added event
				//
				return Redirect(VirtualPathUtility.ToAppRelative("~/") + "#e" + newEvent.EventId.ToString());
			}
			else
				return View(MVC.Home.Views.Index, model);
		}

		// POST: /Home/Attending
		//
		// Change attendance on a single event
		//
		[HttpPost]
		public virtual ActionResult Attending(String EventId, String ClassName)
		{
			Repository repository = new Repository();

			// Current user must be logged in and found
			//
			if (FacebookUser.CurrentUser == null)
				return new HttpStatusCodeResult(400, "User authorization not found");

			MFUser currentUser = repository.GetUserById(FacebookUser.CurrentUser.id);
			if (currentUser == null)
				return new HttpStatusCodeResult(400, "User record not found");

			// Event Id must be an integer and found
			//
			Int32 eventId = 0;
			if (!Int32.TryParse(EventId, out eventId))
				eventId = 0;
			if (eventId == 0)
				return new HttpStatusCodeResult(400, "Invalid event Id");

			Event currentEvent = repository.GetEventById(eventId);
			if (currentEvent == null)
				return new HttpStatusCodeResult(400, "Event record not found");

			// ClassName must be recognized
			//
			if ((ClassName != "glyphicon-check") && (ClassName != "glyphicon-unchecked"))
				return new HttpStatusCodeResult(400, "Unrecognized class name");

			// Switch the attendance
			//
			AttendanceResult result = new AttendanceResult();

			if (ClassName == "glyphicon-check")
			{
				repository.RemoveUserFromEvent(currentUser, currentEvent);
				repository.SaveChanges();

				result.Button = RenderPartialViewToString(MVC.Shared.Views.Partials._NotAttendingButton, EventInfo.CopyFromEvent(false, currentEvent));
			}
			else
			{
				repository.AddUserToEvent(currentUser, currentEvent, AttendingEnum.Attending);
				repository.SaveChanges();

				result.Button = RenderPartialViewToString(MVC.Shared.Views.Partials._AttendingButton, EventInfo.CopyFromEvent(true, currentEvent));
			}

			// Rebind the participant list with the change
			//
			result.Attendees = String.Empty;
			return Json(result);
		}

		// POST:  /Home/Participants
		//
		// Return participants for the given event.  Returns HTML from the partial view that renders participant information
		//
		[HttpPost]
		public virtual ActionResult Participants(string EventId)
		{
			Repository repository = new Repository();

			// Current user must be logged in
			//
			if (FacebookUser.CurrentUser == null)
				return new HttpStatusCodeResult(400, "User authorization not found");

			// Event Id must be an integer and found
			//
			Int32 eventId = 0;
			if (!Int32.TryParse(EventId, out eventId))
				eventId = 0;
			if (eventId == 0)
				return new HttpStatusCodeResult(400, "Invalid event Id");

			Event currentEvent = repository.GetEventById(eventId);
			if (currentEvent == null)
				return new HttpStatusCodeResult(400, "Event record not found");

			// Retrieve participants and format into appropriate view model
			//
			var attendings = repository.GetUsersForEvent(currentEvent.EventId);
			List<EventParticipant> participants = new List<EventParticipant>();
			foreach (MFUser user in attendings)
			{
				participants.Add(EventParticipant.FromUser(user, FacebookUser.CurrentUser.access_token));
			}

			// return the rendered view with participants in it
			//
			AttendanceResult result = new AttendanceResult();
			result.Attendees = RenderPartialViewToString(MVC.Shared.Views.Partials._ParticipantList, participants);

			return Json(result);
		}
    }
}
