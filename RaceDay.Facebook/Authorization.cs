using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

using Newtonsoft.Json.Linq;
using RaceDay.Utilities;

namespace RaceDay.Facebook
{
	/// <summary>
	/// Interface IAuthorizationService
	/// 
	/// Interface definition for authorization service provider
	/// </summary>
	/// 
	public interface IAuthorizationService
	{
		bool Authorize(HttpContextBase httpContext);
	}

	/// <summary>
	/// Class FacebookAuthorizeAttribute
	/// 
	/// Custom AuthorizeAttribute that uses a Facebook authorization provider to handle
	/// the Facebook OAuth 2.0 process.  Logon view defined in forms authentication section
	/// of the web.config
	/// </summary>
	/// 
	public class FacebookAuthorizeAttribute : AuthorizeAttribute
	{
		/// <summary>
		/// IAuthorizationService property allowing a custom interface class to be used
		/// </summary>
		/// 
		public IAuthorizationService AuthorizationService { get; set; }

		/// <summary>
		/// Default constructor uses the FacebookAuthorizationService defined in this class file
		/// </summary>
		/// 
		public FacebookAuthorizeAttribute()
		{
			AuthorizationService = new FacebookAuthorizationService();
		}

		/// <summary>
		/// override AuthorizeCore
		/// 
		/// AuthorizeAttribute override to return if the user has been authorized by Facebook
		/// </summary>
		/// <param name="httpContext"></param>
		/// <returns></returns>
		/// 
		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
			return AuthorizationService.Authorize(httpContext);
		}
	}

	/// <summary>
	/// Class FacebookAuthorizationService
	/// 
	/// Service provider for determining if Facebook has authorized the user
	/// </summary>
	/// 
	public class FacebookAuthorizationService : IAuthorizationService
	{
		/// <summary>
		/// Authorize
		/// 
		/// Required member of the IAuthorizationService provider returns true/false to indicate
		/// if user has been authorized
		/// </summary>
		/// <param name="httpContect"></param>
		/// <returns></returns>
		/// 
		public bool Authorize(HttpContextBase httpContext)
		{
			if (!String.IsNullOrEmpty(RaceDayConfiguration.Instance.DebugUser))
			{
				FormsAuthenticationTicket ticket = CreateFormsTicket(RaceDayConfiguration.Instance.DebugUser, "", Int32.MaxValue);
				System.Web.Security.FormsIdentity id = new System.Web.Security.FormsIdentity(ticket);

				FacebookUser fbUser = FacebookUser.Create(id, null);
				fbUser.id = ticket.Name;
				fbUser.first_name = "Johnny";
				fbUser.last_name = "Test";
				fbUser.email = "test@me.com";
				httpContext.User = fbUser;

				return true;
			}

			if (!String.IsNullOrEmpty(httpContext.Request.QueryString["code"]))
			{
				String redirectUrl = String.Concat(httpContext.Request.Url.Scheme, "://", httpContext.Request.Url.Host, (!httpContext.Request.Url.IsDefaultPort ? ":" + httpContext.Request.Url.Port : ""), httpContext.Request.Path);

				FacebookConnection fbObject = new FacebookConnection();
				fbObject.GetFacebookAccessToken(redirectUrl, httpContext.Request.QueryString["code"]);
				fbObject.GetFacebookUserId();

				FormsAuthenticationTicket ticket = CreateFormsTicket( fbObject.user_id, fbObject.access_token, fbObject.token_expires);
				System.Web.Security.FormsIdentity id = new System.Web.Security.FormsIdentity(ticket);

				FacebookConnection fb = new FacebookConnection(id);
				httpContext.User = fb.GetFacebookUser(ticket.Name);

				httpContext.Response.Redirect(httpContext.Request.Path);
				return true;
			}

			return IsFacebookAuthorized(httpContext);
		}

		/// <summary>
		/// IsFacebookAuthorized
		/// 
		/// Checks the Facebook request objects to determine if Facebook has passed in
		/// a valid authorization request and the user has allowed app permissions.
		/// 
		/// The signed_request POST object has a 2 part Base64Url token separated by
		/// a '.'. The first part contains the hash for the payload using the application 
		/// secret to validate the request.  The second part contains the payload data.
		/// 
		/// While the payload contains the hash algorithm, the HMACSHA256 is assumed to be
		/// the hash algorithm.
		/// </summary>
		/// <param name="httpContext"></param>
		/// <returns></returns>
		/// 
		protected bool IsFacebookAuthorized(HttpContextBase httpContext)
		{
			String signedRequestUrl = GetSignedRequest(httpContext);
			if (!String.IsNullOrEmpty(signedRequestUrl))
			{
				JObject fbAuthorization = ValidateAndGetAuthorizationPayload(signedRequestUrl);
				String fbUserId = (String)fbAuthorization.SelectToken("user_id");
				if (!String.IsNullOrEmpty(fbUserId))
				{
					String oAuthToken = (String)fbAuthorization.SelectToken("oauth_token");
					Int32 expires = (Int32)fbAuthorization.SelectToken("expires");
					Int32 issued_at = (Int32)fbAuthorization.SelectToken("issued_at");
					FormsAuthenticationTicket ticket = CreateFormsTicket(fbUserId, oAuthToken, expires - issued_at);

					FacebookConnection fb = new FacebookConnection(new System.Web.Security.FormsIdentity(ticket));
					httpContext.User = fb.GetFacebookUser(ticket.Name);

					return true;
				}
			}

			// This must occur after the check for signedRequestUrl so different users can be checked first
			//
			if ((httpContext.User != null) && (httpContext.User.Identity.IsAuthenticated))
				return true;

			return false;
		}

		/// <summary>
		/// CreateFormsTicket
		/// 
		/// Generates a Forms Authentication ticket using the identity of the user and the access token sent from Oauth
		/// </summary>
		/// <param name="currentUrl"></param>
		/// <param name="userId"></param>
		/// <param name="requestCode"></param>
		/// <returns></returns>
		/// 
		public FormsAuthenticationTicket CreateFormsTicket(String userId, String oauthToken, Int32 expires)
		{
			FormsAuthentication.Initialize();
			FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
				1,
				userId,
				DateTime.Now,
				DateTime.Now.AddSeconds(expires),
				false,
				oauthToken,
				FormsAuthentication.FormsCookiePath);
			String hash = FormsAuthentication.Encrypt(ticket);
			HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, hash);
			HttpContext.Current.Response.Cookies.Add(cookie);
			return ticket;
		}



		/// <summary>
		/// GetSignedRequest
		/// 
		/// Retrieves the signed_request object from the POST data
		/// </summary>
		/// <param name="httpContext"></param>
		/// <returns></returns>
		/// 
		protected String GetSignedRequest(HttpContextBase httpContext)
		{
			String signedRequestUrl = String.Empty;
			if (!String.IsNullOrEmpty(httpContext.Request.Form["signed_request"]))
				signedRequestUrl = httpContext.Request.Form["signed_request"];
			return signedRequestUrl;
		}

		/// <summary>
		/// ValidateAndGetAuthorizationPayload
		/// 
		/// Retrieves the Base64Url payload as well as the hash key and validates
		/// the Hash key against the payload using the application secret.
		/// 
		/// Returns the JSON object which is the payload for signed_request
		/// </summary>
		/// <param name="signedRequestUrl"></param>
		/// <returns></returns>
		/// 
		protected JObject ValidateAndGetAuthorizationPayload(String signedRequestUrl)
		{
			String[] signedRequest = signedRequestUrl.Split('.');
			String expectedSignature = signedRequest[0];
			String base64Payload = signedRequest[1];

			JObject jsonAuthorization = null;
			if (IsValidAuthorizationRequest(expectedSignature, base64Payload))
				jsonAuthorization = GetAuthorizationTicketFromRequest(base64Payload);

			return jsonAuthorization;
		}

		/// <summary>
		/// IsValidAuthorizationRequest
		/// 
		/// Validates the hash signature of signed_request
		/// </summary>
		/// <param name="expectedSignature"></param>
		/// <param name="encryptedPayload"></param>
		/// <returns></returns>
		/// 
		protected bool IsValidAuthorizationRequest(String expectedSignature, String encryptedPayload)
		{
			String applicationSecret = RaceDayConfiguration.Instance.ApplicationSecret;
			var Hmac = SignWithHmac(UTF8Encoding.UTF8.GetBytes(encryptedPayload), UTF8Encoding.UTF8.GetBytes(applicationSecret));
			var HmacBase64 = ToUrlBase64String(Hmac);

			return (HmacBase64 == expectedSignature);
		}

		/// <summary>
		/// GetAuthorizationTicketFromRequest
		/// 
		/// Converts the payload data from Base64Url to the JSON object
		/// represented in the payload
		/// </summary>
		/// <param name="base64Payload"></param>
		/// <returns></returns>
		/// 
		protected JObject GetAuthorizationTicketFromRequest(String base64Payload)
		{
			Byte[] binaryPayload = Convert.FromBase64String(base64Payload.WebBase64());
			String authorizationPayload = UTF8Encoding.UTF8.GetString(binaryPayload);

			JObject jsonTicket = JObject.Parse(@"{""Error"": ""Invalid Payload""}");
			try
			{
				jsonTicket = JObject.Parse(authorizationPayload);
			}
			catch (Exception)
			{
				jsonTicket = JObject.Parse(@"{""Error"": ""Invalid Payload""}");
			}

			return jsonTicket;
		}

		/// <summary>
		/// SignWithHmac
		/// 
		/// Uses payload data and application secret to calculate the HMACSHA256 hash
		/// </summary>
		/// <param name="dataToSign"></param>
		/// <param name="keyBody"></param>
		/// <returns></returns>
		/// 
		protected Byte[] SignWithHmac(Byte[] dataToSign, Byte[] keyBody)
		{
			using (var hmacAlgorithm = new HMACSHA256(keyBody))
			{
				hmacAlgorithm.ComputeHash(dataToSign);
				return hmacAlgorithm.Hash;
			}
		}

		/// <summary>
		/// ToUrlBase64String
		/// 
		/// Converts byte data to Base64Url
		/// </summary>
		/// <param name="inputData"></param>
		/// <returns></returns>
		/// 
		protected String ToUrlBase64String(Byte[] inputData)
		{
			return Convert.ToBase64String(inputData).Replace("=", String.Empty)
													.Replace('+', '-')
													.Replace('/', '_');
		}
	}
}
