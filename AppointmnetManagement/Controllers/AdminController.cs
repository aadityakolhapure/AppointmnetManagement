using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AppointmnetManagement.Models;

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
            if (Session["UserRole"] == null || Session["UserRole"].ToString() != "Admin")
            {
                Session.Clear();
                TempData["ErrorMessage"] = "Please log in to access the Admin dashboard.";
                return RedirectToAction("Login", "Account");
            }

            List<Doctor> doctors = new List<Doctor>();

            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = @"SELECT d.DoctorID, u.FullName, u.Email, d.Specialization, d.Phone
                                 FROM Doctors d
                                 INNER JOIN Users u ON d.UserID = u.UserID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        doctors.Add(new Doctor
                        {
                            DoctorID = Convert.ToInt32(reader["DoctorID"]),
                            FullName = reader["FullName"].ToString(),
                            Email = reader["Email"].ToString(),
                            Specialization = reader["Specialization"].ToString(),
                            Phone = reader["Phone"].ToString()
                        });
                    }
                }
            }

            return View(doctors);
        }

        string connStr = ConfigurationManager.ConnectionStrings["ClinicDB"].ConnectionString;

        [HttpPost]
        public ActionResult DeleteDoctor(int id)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = "DELETE FROM Doctors WHERE DoctorID = @DoctorID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@DoctorID", id);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }

            TempData["SuccessMessage"] = "Doctor deleted successfully.";
            return RedirectToAction("ManageDoctor");
        }


    }
}