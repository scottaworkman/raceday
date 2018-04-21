using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RaceDay.Models;
using RaceDay.ViewModels;

namespace RaceDay.Controllers
{
    [RaceDay.AdminAttribute]
	[HandleError(View = "Error")]
    public class MFUsersController : BaseController
    {
        private RaceDayEntities db = new RaceDayEntities();

        // GET: MFUsers
        public ActionResult Index()
        {
            return View(db.MFUsers.OrderBy(o => o.FirstName).ThenBy(o => o.LastName).ToList());
        }

        // GET: MFUsers/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MFUser mFUser = db.MFUsers.Find(id);
            if (mFUser == null)
            {
                return HttpNotFound();
            }
            return View(mFUser);
        }

        // GET: MFUsers/Create
        public ActionResult Create()
        {
            return View(new RaceDay.ViewModels.MFUserViewModel());
        }

        // POST: MFUsers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UserId,Name,FirstName,LastName,Email")] MFUserViewModel mFUser)
        {
            if (ModelState.IsValid)
            {
                db.MFUsers.Add(new MFUser
                {
                    UserId = mFUser.UserId,
                    Name = mFUser.Name,
                    FirstName = mFUser.FirstName,
                    LastName = mFUser.LastName,
                    Email = mFUser.Email,
                    LastUpdate = DateTime.Now
                });
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(mFUser);
        }

        // GET: MFUsers/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MFUser mFUser = db.MFUsers.Find(id);
            if (mFUser == null)
            {
                return HttpNotFound();
            }
            return View(new MFUserViewModel
            {
                UserId = mFUser.UserId,
                Name = mFUser.Name,
                FirstName = mFUser.FirstName,
                LastName = mFUser.LastName,
                Email = mFUser.Email,
            });
        }

        // POST: MFUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserId,Name,FirstName,LastName,Email")] MFUserViewModel mFUser)
        {
            if (ModelState.IsValid)
            {
                var user = db.MFUsers.Find(mFUser.UserId);
                if (user != null)
                {
                    user.Name = mFUser.Name;
                    user.FirstName = mFUser.FirstName;
                    user.LastName = mFUser.LastName;
                    user.Email = mFUser.Email;

                    db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return View(mFUser);
        }

        // GET: MFUsers/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MFUser mFUser = db.MFUsers.Find(id);
            if (mFUser == null)
            {
                return HttpNotFound();
            }
            return View(mFUser);
        }

        // POST: MFUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            MFUser mFUser = db.MFUsers.Find(id);
            var groups = mFUser.GroupMembers.ToList();
            foreach(var group in groups)
            {
                db.GroupMembers.Remove(group);
            }
            var races = mFUser.Attendings.ToList();
            foreach(var race in races)
            {
                db.Attendings.Remove(race);
            }
            db.MFUsers.Remove(mFUser);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
