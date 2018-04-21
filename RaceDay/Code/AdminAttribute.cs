using RaceDay.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RaceDay
{
    public class AdminAttribute : AuthorizeAttribute
    {
        private RaceDayEntities db = new RaceDayEntities();

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext))
            {
                return false;
            }

            var fbUser = httpContext.User as Facebook.FacebookUser;
            if (fbUser == null)
            {
                return false;
            }

            var groups = db.GroupMembers.Where(r => r.UserId == fbUser.id).ToList();
            foreach(var group in groups)
            {
                if (group.Role == (int)GroupRoleEnum.admin)
                    return true;
            }

            return false;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(
                            new
                            {
                                controller = "Home",
                                action = "Index"
                            })
                        );
        }
    }
}