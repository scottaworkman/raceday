using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using RaceDay.Models;

namespace RaceDay.Controllers
{
	public class BaseApiController : ApiController
	{
		public string UserId { get; set; }
        public int GroupId { get; set; }

		protected override void Initialize(System.Web.Http.Controllers.HttpControllerContext controllerContext)
		{
			base.Initialize(controllerContext);

			if (controllerContext.Request.Headers.Authorization != null)
			{
				string auth_scheme = controllerContext.Request.Headers.Authorization.Scheme.ToLower();
				string access_token = controllerContext.Request.Headers.Authorization.Parameter;

				if ((!string.IsNullOrEmpty(auth_scheme)) && (auth_scheme == "bearer") && (!string.IsNullOrEmpty(access_token)))
				{
					var userMembership = MvcApplication.cache.Get<GroupMember>(access_token, 10, () =>
						{
							Repository repository = new Repository();
							return repository.UserGroupMembership(access_token);
						});

                    if ((userMembership != null) && (userMembership.AccessExpiration >= DateTime.Now.ToUniversalTime()))
                    {
                        UserId = userMembership.UserId;
                        GroupId = userMembership.GroupId;
                    }
				}
			}
		}
	}
}