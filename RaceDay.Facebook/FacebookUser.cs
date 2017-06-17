using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;

namespace RaceDay.Facebook
{
	/// <summary>
	/// FacebookUser
	/// </summary>
	/// 
	public class FacebookUser : System.Security.Principal.IPrincipal
	{
		protected System.Security.Principal.IIdentity identity = null;

		public String id { get; set; }
		public String name { get; set; }
		public String first_name { get; set; }
		public String last_name { get; set; }
		public String gender { get; set; }
		public String link { get; set; }
		public String about { get; set; }
		public String bio { get; set; }
		public String birthday { get; set; }
		public String email { get; set; }
		public String hometown { get; set; }
		public String location { get; set; }
		public String quotes { get; set; }
		public String picture { get; set; }
		public FacebookFriend significant_other { get; set; }
		public String access_token
		{
			get
			{
				return ((System.Web.Security.FormsIdentity)identity).Ticket.UserData;
			}
		}

		/// <summary>
		/// Create
		/// 
		/// Parses the standard Facebook user JSON object for the properties.  Note that picture
		/// has been added manually to be included in the user object
		/// </summary>
		/// <param name="jsonUser"></param>
		/// <returns></returns>
		/// 
		public static FacebookUser Create(System.Web.Security.FormsIdentity identity, JObject jsonUser)
		{
			FacebookUser user = new FacebookUser();
			user.identity = identity;

			if (jsonUser != null)
			{
				JToken value = null;
				if (jsonUser.TryGetValue("id", out value))
					user.id = jsonUser.SelectToken("id").Value<String>();
				if (jsonUser.TryGetValue("name", out value))
					user.name = jsonUser.SelectToken("name").Value<String>();
				if (jsonUser.TryGetValue("first_name", out value))
					user.first_name = jsonUser.SelectToken("first_name").Value<String>();
				if (jsonUser.TryGetValue("last_name", out value))
					user.last_name = jsonUser.SelectToken("last_name").Value<String>();
				if (jsonUser.TryGetValue("gender", out value))
					user.gender = jsonUser.SelectToken("gender").Value<String>();
				if (jsonUser.TryGetValue("link", out value))
					user.link = jsonUser.SelectToken("link").Value<String>();
				if (jsonUser.TryGetValue("about", out value))
					user.about = jsonUser.SelectToken("about").Value<String>();
				if (jsonUser.TryGetValue("bio", out value))
					user.bio = jsonUser.SelectToken("bio").Value<String>();
				if (jsonUser.TryGetValue("birthday", out value))
					user.birthday = jsonUser.SelectToken("birthday").Value<String>();
				if (jsonUser.TryGetValue("email", out value))
					user.email = jsonUser.SelectToken("email").Value<String>();
				if (jsonUser.TryGetValue("hometown", out value))
					user.hometown = jsonUser.SelectToken("hometown").Value<String>();
				if (jsonUser.TryGetValue("location", out value))
					user.location = jsonUser.SelectToken("location").Value<String>();
				if (jsonUser.TryGetValue("quotes", out value))
					user.quotes = jsonUser.SelectToken("quotes").Value<String>();
				if (jsonUser.TryGetValue("picture", out value))
					user.picture = jsonUser.SelectToken("picture").Value<String>();
				if (jsonUser.TryGetValue("significant_other", out value))
					user.significant_other = FacebookFriend.Create((JObject)jsonUser.SelectToken("significant_other"));
			}

			return user;
		}

		public static FacebookUser CurrentUser
		{
			get
			{
				return HttpContext.Current.User as FacebookUser;
			}
		}

		public static String UserImage(string userId)
		{
			return String.Format(FacebookConnection.RELATION_URL_PUBLIC, userId, "picture") + "?type=square";
		}

		#region IPrincipal Members

		public System.Security.Principal.IIdentity Identity
		{
			get { return identity; }
		}

		public bool IsInRole(string role)
		{
			return true;
		}

		#endregion
	}

	/// <summary>
	/// FacebookFriend
	/// </summary>
	/// 
	public class FacebookFriend
	{
		public String id { get; set; }
		public String name { get; set; }
		public String pictureUrl { get; set; }

		public static FacebookFriend Create(JObject node)
		{
			FacebookFriend friend = new FacebookFriend();
			if (node != null)
			{
				JToken value = null;
				if (node.TryGetValue("id", out value))
					friend.id = node.SelectToken("id").Value<String>();
				else if (node.TryGetValue("uid", out value))
					friend.id = node.SelectToken("uid").Value<String>();
				if (node.TryGetValue("name", out value))
					friend.name = node.SelectToken("name").Value<String>();
			}
			friend.pictureUrl = String.Format(FacebookConnection.RELATION_URL_PUBLIC, friend.id, "picture") + "?type=square";

			return friend;
		}
	}
}
