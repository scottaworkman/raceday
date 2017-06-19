using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RaceDay.Models;

namespace RaceDay.Controllers
{
	public class LoginController : ApiController
	{
		// POST api/<controller>
		public HttpResponseMessage Post([FromBody]LoginAuth auth)
		{
			if ((auth == null) || (string.IsNullOrEmpty(auth.groupid)) || (string.IsNullOrEmpty(auth.userid)))
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Missing authentication information");
			}
			else
			{
				Repository repository = new Repository();
				var fbGroup = repository.FindGroupByCodeAndKey(auth.groupid, auth.apikey);
				var mfUser = repository.GetUserById(auth.userid);
				var groupMember = repository.UserGroupMembership(mfUser ?? new MFUser(), fbGroup ?? new Group());

				if (groupMember == null)
				{
					if (mfUser == null)
						return Request.CreateErrorResponse(HttpStatusCode.NotFound, "User not found");

					if (fbGroup == null)
						return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid credentials");

					repository.DefaultGroup(mfUser, fbGroup, GroupRoleEnum.member);
					repository.SaveChanges();

					groupMember = repository.UserGroupMembership(mfUser, fbGroup);
					if (groupMember == null)
						return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid credentials");
				}

				if ((string.IsNullOrEmpty(groupMember.AccessToken)) ||
					((groupMember.AccessExpiration.HasValue == false) || (groupMember.AccessExpiration.Value < DateTime.Now.ToUniversalTime())))
				{
					groupMember.AccessToken = Guid.NewGuid().ToString();
					groupMember.AccessExpiration = DateTime.Now.AddDays(1).ToUniversalTime();
					repository.SaveChanges();
				}
				else
				{
					groupMember.AccessExpiration = DateTime.Now.AddDays(1).ToUniversalTime();
					repository.SaveChanges();
				}

				var result = new AuthResult
				{
					token = groupMember.AccessToken,
					expiration = groupMember.AccessExpiration.Value,
                    role = groupMember.Role,
                    name = mfUser.Name
				};

				return Request.CreateResponse(HttpStatusCode.OK, result);
			}
		}

		public class LoginAuth
		{
			public string groupid { get; set; }
			public string userid { get; set; }
			public string apikey { get; set; }
		}

		public class AuthResult
		{
			public string token { get; set; }
			public DateTime expiration { get; set; }
            public int role { get; set; }
            public string name { get; set; }
        }
    }
}