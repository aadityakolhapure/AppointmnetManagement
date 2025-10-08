using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AppointmnetManagement.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult AdminDashboard()
        {
            if (Session["UserRole"] != null && Session["UserRole"].ToString() == "Admin")
            {
                return View();
            }
            else
            {
                Session.Clear();
                TempData["ErrorMessage"] = "Please log in to access the Admin dashboard.";
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult ManageDoctor()
        {
            if (Session["UserRole"] != null && Session["UserRole"].ToString() == "Admin")
            {
                return View();
            }
            else
            {
                Session.Clear();
                TempData["ErrorMessage"] = "Please log in to access the Admin dashboard.";
                return RedirectToAction("Login", "Account");
            }
        }



    }
}