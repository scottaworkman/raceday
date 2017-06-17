using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RaceDay;
using RaceDay.Utilities;

namespace RaceDay.Facebook
{
	public class FacebookConnection
	{
		public const string ACCESS_TOKEN_URL = "https://graph.facebook.com/oauth/access_token";
		public const string USERID_URL = "https://graph.facebook.com/me";
		public const string OBJECT_URL = "https://graph.facebook.com/{0}?access_token={1}";
		public const string RELATION_URL = "https://graph.facebook.com/{0}/{1}?access_token={2}";
		public const string RELATION_URL_PUBLIC = "https://graph.facebook.com/{0}/{1}";
		public const string QUERY_URL = "https://api.facebook.com/method/fql.query?query={0}&format=JSON&access_token={1}";
		public const string NOTIFICATION_URL = "https://graph.facebook.com/{0}/notifications?ref={1}&href={2}&template={3}&access_token={4}";

		public FormsIdentity identity { get; set; }
		public String user_id { get; set; }
		public String access_token { get; set; }
		public Int32 token_expires { get; set; }

		/// <summary>
		/// Facebook
		/// 
		/// Constructor setting the user authorization parameters
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="accessToken"></param>
		/// 
		public FacebookConnection(IIdentity id)
		{
			if (id is FormsIdentity)
			{
				this.identity = (FormsIdentity)id;
				user_id = identity.Name;
				FormsAuthenticationTicket ticket = identity.Ticket;
				access_token = ticket.UserData;
			}
		}

		public FacebookConnection()
		{
		}

		/// <summary>
		/// FacebookAccessToken
		/// 
		/// Retrieves Graph API access token based upon the authorization code passed after user allows app permissions
		/// </summary>
		/// <param name="redirectUrl"></param>
		/// <param name="requestCode"></param>
		/// 
		public void GetFacebookAccessToken(String redirectUrl, String requestCode)
		{
			String accessTokenUrl = String.Format("{0}?client_id={1}&redirect_uri={2}&client_secret={3}&code={4}",
														ACCESS_TOKEN_URL,
														RaceDayConfiguration.Instance.ApplicationId,
														redirectUrl,
														RaceDayConfiguration.Instance.ApplicationSecret,
														requestCode);

			// As of V2.3, Facebook responds with Json
			String authResponse = GetHttpRequest(accessTokenUrl);
			dynamic accessToken = JObject.Parse(authResponse);

			if ((accessToken == null) || (accessToken.access_token == null))
				throw new InvalidOperationException("No Access token for Redirect URL: " + redirectUrl);

			access_token = accessToken.access_token;
			token_expires = Convert.ToInt32(accessToken.expires_in.Value);
		}

		/// <summary>
		/// GetFacebookUserId
		/// 
		/// Requests facebook user object for the current user in order to retrieve the user id only
		/// </summary>
		/// 
		public void GetFacebookUserId()
		{
			if (String.IsNullOrEmpty(access_token))
				throw new InvalidOperationException("Graph API access token not set");

			String userIdUrl = String.Format("{0}?fields=id&access_token={1}", USERID_URL, access_token);
			String userResponse = GetHttpRequest(userIdUrl);
			if (!String.IsNullOrEmpty(userResponse))
			{
				JObject jsonUser = JObject.Parse(userResponse);
				user_id = (String)jsonUser.SelectToken("id");
			}
		}

		/// <summary>
		/// GetFacebookUser
		/// 
		/// Retrieves the specified user's information from the Facebook Graph API
		/// </summary>
		/// <returns></returns>
		/// 
		public FacebookUser GetFacebookUser(String fbUserId)
		{
			if (String.IsNullOrEmpty(access_token))
				throw new InvalidOperationException("Graph API access token not set");

			String userUrl = String.Format(OBJECT_URL, fbUserId, access_token);
			userUrl += "&fields=id,name,first_name,last_name,email";
			String userResponse = GetHttpRequest(userUrl);
			if (!String.IsNullOrEmpty(userResponse))
			{
				String picUrl = String.Format(RELATION_URL, fbUserId, "picture", access_token);

				JObject jsonUser = JObject.Parse(userResponse);
				jsonUser.Add("picture", picUrl);

				return FacebookUser.Create(identity, jsonUser);
			}

			return null;
		}

		public Boolean IsMemberOfGroup(String fbUserId, String fbGroupId)
		{
			if (String.IsNullOrEmpty(access_token))
				throw new InvalidOperationException("Graph API access token not set");

			String groupUrl = String.Format(RELATION_URL, fbGroupId, "members", access_token ) + "&limit=200";
			String groupResponse = GetHttpRequest(groupUrl);

			if (!String.IsNullOrEmpty(groupResponse))
			{
				var groupMembers = JsonConvert.DeserializeObject<FacebookGroupMemberList>(groupResponse);
				foreach (FacebookGroupMember member in groupMembers.data)
				{
					if (member.id == fbUserId)
						return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Retrieves a list of your Facebook friends using this application
		/// </summary>
		/// <param name="fbApplicationId"></param>
		/// <returns></returns>
		/// 
		public List<FacebookFriend> GetApplicationFriends(String fbApplicationId)
		{
			if (String.IsNullOrEmpty(access_token))
				throw new InvalidOperationException("Graph API access token not set");

			List<FacebookFriend> friendsList = new List<FacebookFriend>();

			String applicationUrl = String.Format(QUERY_URL, "SELECT uid, name FROM user WHERE uid IN (SELECT uid2 FROM friend WHERE uid1 = me()) AND is_app_user = 1", access_token);
			String applicationResponse = GetHttpRequest(applicationUrl);
			if (!String.IsNullOrEmpty(applicationResponse))
			{
				List<JObject> appFriends = JsonConvert.DeserializeObject<List<JObject>>(applicationResponse);
				foreach (JObject friendId in appFriends)
				{
					friendsList.Add(FacebookFriend.Create(friendId));
				}
			}

			return friendsList;
		}

		/// <summary>
		/// GetApplicationPermissions
		/// 
		/// Retrieves the application permissions that the user approved.  Can be used to determine
		/// allowable commands in the application.
		/// </summary>
		/// <returns></returns>
		/// 
		public List<String> GetApplicationPermissions()
		{
			if (String.IsNullOrEmpty(access_token))
				throw new InvalidOperationException("Graph API access token not set");

			List<String> permissions = new List<String>();
			
			String permissionsUrl = String.Format(OBJECT_URL, "me/permissions", access_token);
			String permissionsResponse = GetHttpRequest(permissionsUrl);
			if (!String.IsNullOrEmpty(permissionsResponse))
			{
				JObject permissionsData = JObject.Parse(permissionsResponse);
				JToken permissionsValue;
				if (permissionsData.TryGetValue("data", out permissionsValue))
				{
					foreach (JToken token in permissionsValue[0])
					{
						String jsonProperty = token.ToString();
						String[] jsonValues = jsonProperty.Split(':');
						if (jsonValues.Length == 2)
							permissions.Add(jsonValues[0].Replace("\"", String.Empty));
					}
				}
			}

			return permissions;
		}

		/// <summary>
		/// Sends notification message to a user
		/// </summary>
		/// <param name="UserId"></param>
		/// <param name="reference"></param>
		/// <param name="message"></param>
		/// 
		public Boolean UserNotification(string app_token, string UserId, string reference, string href, string message)
		{
			string postUrl = String.Format(NOTIFICATION_URL, UserId, reference, href, HttpUtility.UrlEncode(message), app_token);

			WebRequest request = WebRequest.Create(postUrl);
			request.Timeout = 10000;
			request.Method = "POST";
			request.ContentLength = 0;
			request.ContentType = "application/x-www-form-urlencoded";

			StreamReader responseReader = null;
			String response = String.Empty;

			try
			{
				responseReader = new StreamReader(request.GetResponse().GetResponseStream());
				response = responseReader.ReadToEnd();
			}
			catch (Exception e)
			{
				return false;
			}
			finally
			{
				if (responseReader != null)
				{
					responseReader.Close();
					responseReader.Dispose();
				}
				responseReader = null;
			}

			JObject postId = JObject.Parse(response);
			JToken jresult = null;
			if (postId != null)
				jresult = postId.SelectToken("success");

			return (jresult != null);
		}

		/// <summary>
		/// Returns a user replacable token for the template of a notification given the user id.
		/// Facebook fills in the user token with the name of the user
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		/// 
		public static String UserTemplateToken(String userId)
		{
			return String.Format("@[{0}]", userId);
		}

		public static String AppToken(String appId, String appSecret)
		{
			return appId + "|" + appSecret;
		}

		/// <summary>
		/// PostFeedMessage
		/// 
		/// Submits a post to the logged in user's news feed and returns the Id of the new post
		/// </summary>
		/// <param name="post"></param>
		/// 
		public String PostFeedMessage(FacebookPost post)
		{
			string postUrl = String.Format(OBJECT_URL, "me/feed", access_token);
			var body = Encoding.UTF8.GetBytes(post.UrlString().Substring(1));

			WebRequest request = WebRequest.Create(postUrl);
			request.Timeout = 10000;
			request.Method = "POST";
			request.ContentLength = body.Length;
			request.ContentType = "application/x-www-form-urlencoded";

			Stream postStream = request.GetRequestStream();
			postStream.Write(body, 0, body.Length);
			postStream.Close();

			StreamReader responseReader = null;
			String response = String.Empty;

			try
			{
				responseReader = new StreamReader(request.GetResponse().GetResponseStream());
				response = responseReader.ReadToEnd();
			}
			catch( Exception e )
			{
				return String.Empty;
			}
			finally
			{
				if (responseReader != null)
				{
					responseReader.Close();
					responseReader.Dispose();
				}
				responseReader = null;
			}

			JObject postId = JObject.Parse(response);
			return postId.SelectToken("id").ToString().Replace("\"", "");
		}

		/// <summary>
		/// GetHttpRequest
		/// 
		/// Makes HTTP request to the specified URL and returns the response
		/// </summary>
		/// <param name="Url"></param>
		/// <returns></returns>
		/// 
		public static String GetHttpRequest(String Url)
		{
			WebRequest request = WebRequest.Create(Url);
			StreamReader responseReader = null;
			String response = String.Empty;

			try
			{
				responseReader = new StreamReader(request.GetResponse().GetResponseStream());
				response = responseReader.ReadToEnd();
			}
			catch(Exception e)
			{
				return String.Concat( "Error: ", e.Message );
			}
			finally
			{
				if (responseReader != null)
				{
					responseReader.Close();
					responseReader.Dispose();
				}
				responseReader = null;
			}

			return response;
		}

		/// <summary>
		/// GetHttpRedirect
		/// 
		/// Retrieves the URL of a HTTP redirect
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		/// 
		public static String GetHttpRedirect(String url)
		{
			WebRequest request = WebRequest.Create(url);
			WebResponse response = null;
			try
			{
				response = request.GetResponse();
			}
			catch (Exception)
			{
			}

			if ((response != null) && (response.ResponseUri != null))
			{
				return response.ResponseUri.ToString();
			}

			return String.Empty;
		}
	}

	/// <summary>
	/// class FacebookPost
	/// 
	/// Encapsulates a post object for a new feed
	/// </summary>
	/// 
	public class FacebookPost
	{
		String Id { get; set; }
		String Message { get; set; }
		String Link { get; set; }
		String LinkName { get; set; }
		String Caption { get; set; }
		String Description { get; set; }

		public FacebookPost()
		{
		}

		public FacebookPost(String message, String link, String linkName, String caption, String description)
		{
			Id = String.Empty;
			Message = message;
			Link = link;
			LinkName = linkName;
			Caption = caption;
			Description = description;
		}

		public String UrlString()
		{
			String url = String.Empty;

			if (!String.IsNullOrEmpty(Message))
				url += String.Format("&message={0}", HttpUtility.UrlEncode(Message));

			if (!String.IsNullOrEmpty(Link))
				url += String.Format("&link={0}", HttpUtility.UrlEncode(Link));

			if (!String.IsNullOrEmpty(LinkName))
				url += String.Format("&name={0}", HttpUtility.UrlEncode(LinkName));

			if (!String.IsNullOrEmpty(Caption))
				url += String.Format("&caption={0}", HttpUtility.UrlEncode(Caption));

			if (!String.IsNullOrEmpty(Description))
				url += String.Format("&description={0}", HttpUtility.UrlEncode(Description));

			return url;
		}
	}

	public class FacebookGroupMemberList
	{
		public List<FacebookGroupMember> data { get; set; }
	}

	public class FacebookGroupMember
	{
		public string id { get; set; }
		public string name { get; set; }
		public Boolean administrator { get; set; }
	}
}
