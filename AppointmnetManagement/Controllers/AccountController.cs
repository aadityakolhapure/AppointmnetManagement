using AppointmnetManagement.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace AppointmnetManagement.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(User model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            string connStr = ConfigurationManager.ConnectionStrings["ClinicDB"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = @"INSERT INTO Users 
                                (FullName, Email, PasswordHash, Role, Specialization, Address)
                                VALUES
                                (@FullName, @Email, @PasswordHash, @Role, @Specialization, @Address)";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@FullName", model.Name);
                    cmd.Parameters.AddWithValue("@Email", model.Email);
                    cmd.Parameters.AddWithValue("@PasswordHash", model.Password);
                    cmd.Parameters.AddWithValue("@Role", model.Role);
                    cmd.Parameters.AddWithValue("@Specialization", (object)model.Specialization ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", (object)model.Address ?? DBNull.Value);

                    try
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();
                        TempData["SuccessMessage"] = "Registration successful! You can login now.";
                        return RedirectToAction("Login");
                    }
                    catch (SqlException ex)
                    {
                        TempData["ErrorMessage"] = "Data already Exists!!";
                    }
                }
            }

            return View(model);
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Login(User user)
        {
            if (user == null || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                TempData["ErrorMessage"] = "Please fill all fields.";
                return RedirectToAction("Login");
            }

            string connStr = ConfigurationManager.ConnectionStrings["ClinicDB"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = "SELECT * FROM Users WHERE Email=@Email AND PasswordHash=@PasswordHash";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@PasswordHash", user.Password); // Use hashing if implemented
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Session["UserID"] = reader["UserID"];
                            Session["UserName"] = reader["FullName"];
                            Session["UserRole"] = reader["Role"];
                            Session["UserEmail"] = reader["Email"];
                            Session["UserSpecialization"] = reader["Specialization"];
                            Session["UserAddress"] = reader["Address"];

                            string role = reader["Role"].ToString().ToLower();
                            if (role == "admin") return RedirectToAction("AdminDashboard", "Admin");
                            if (role == "doctor") return RedirectToAction("DoctorDashboard", "Doctor");
                            if (role == "patient") return RedirectToAction("PatientDashboard", "Patient");
                        }
                        else
                        {

                            TempData["ErrorMessage"] = "Invalid email or password.";
                            return RedirectToAction("Login");
                        }
                    }
                }
            }
            return RedirectToAction("Login");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "Account");
        }

    }
}