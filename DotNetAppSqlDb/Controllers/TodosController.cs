using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using DotNetAppSqlDb.Models;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Web;

namespace DotNetAppSqlDb.Controllers
{
    [Authorize]
    public class TodosController : Controller
    {
        private MyDatabaseContext db = new MyDatabaseContext();

        public ActionResult Index()
        {
            //SignIn();
            Trace.WriteLine("GET /Todos/Index");
            return View(db.Todoes.ToList());
        }

        // GET: Todos/Details/5
        public ActionResult Details(int? id)
        {
            Trace.WriteLine("GET /Todos/Details/" + id);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Todo todo = db.Todoes.Find(id);
            if (todo == null)
            {
                return HttpNotFound();
            }
            return View(todo);
        }

        // GET: Todos/Create
        public ActionResult Create()
        {
            Trace.WriteLine("GET /Todos/Create");
            return View(new Todo { CreatedDate = DateTime.Now });
        }

        // POST: Todos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Description,CreatedDate")] Todo todo)
        {
            Trace.WriteLine("POST /Todos/Create");
            todo.Status = "Active";
            if (ModelState.IsValid)
            {
                db.Todoes.Add(todo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(todo);
        }

        // GET: Todos/Edit/5
        public ActionResult Edit(int? id)
        {
            Trace.WriteLine("GET /Todos/Edit/" + id);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Todo todo = db.Todoes.Find(id);
            if (todo == null)
            {
                return HttpNotFound();
            }
            return View(todo);
        }

        // POST: Todos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Exclude = "Image")] Todo todo)
        {
            Trace.WriteLine("POST /Todos/Edit/" + todo.ID);

            string imageName = null;
            if (Request.Files.Count > 0)
            {
                HttpPostedFileBase objFiles = Request.Files["Image"];
                imageName = Path.GetFileName(objFiles.FileName);
                string physicalPath = Server.MapPath("~/Content/images/" + imageName);
                objFiles.SaveAs(physicalPath);
            }

            if (ModelState.IsValid)
            {
                todo.Status = "Done";
                todo.ImageUrl = imageName;
                db.Entry(todo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(todo);
        }

        // GET: Todos/Delete/5
        public ActionResult Delete(int? id)
        {
            Trace.WriteLine("GET /Todos/Delete/" + id);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Todo todo = db.Todoes.Find(id);
            if (todo == null)
            {
                return HttpNotFound();
            }
            return View(todo);
        }

        // POST: Todos/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Trace.WriteLine("POST /Todos/Delete/" + id);
            Todo todo = db.Todoes.Find(id);
            db.Todoes.Remove(todo);
            string physicalPath = Server.MapPath("~/Content/images/" + todo.ImageUrl);
            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }
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
