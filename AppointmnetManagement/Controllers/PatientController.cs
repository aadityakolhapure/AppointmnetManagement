using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AppointmnetManagement.Controllers
{
    public class PatientController : Controller
    {
        // GET: Patient
        public ActionResult PatientDashboard()
        {
            if (Session["UserRole"] != null && Session["UserRole"].ToString() == "Patient")
            {
                
                return View();
            }
            else
            {
                Session.Clear();
                TempData["ErrorMessage"] = "Please log in to access the patient dashboard.";
                return RedirectToAction("Login", "Account");
            }

        }

        
    }
}