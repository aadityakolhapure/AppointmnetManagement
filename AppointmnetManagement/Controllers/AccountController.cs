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

        //Registration logic with SQL parameterization to prevent SQL injection
        [HttpPost]
        public ActionResult Register(User model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string connStr = ConfigurationManager.ConnectionStrings["ClinicDB"].ConnectionString;
            int IsActive = 0;

            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();

                string checkEmailQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
                using (SqlCommand checkCmd = new SqlCommand(checkEmailQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@Email", model.Email);
                    int exists = (int)checkCmd.ExecuteScalar();

                    if (exists > 0)
                    {
                        TempData["ErrorMessage"] = "Email already exists!";
                        return View(model);
                    }
                }

                // 2️⃣ Insert into Users table and get the new UserID
                string query = @"INSERT INTO Users 
                         (FullName, Email, PasswordHash, Role, Specialization, Address, IsActive)
                         OUTPUT INSERTED.UserID
                         VALUES
                         (@FullName, @Email, @PasswordHash, @Role, @Specialization, @Address, @IsActive)";

                int newUserId;
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@FullName", model.Name);
                    cmd.Parameters.AddWithValue("@Email", model.Email);
                    cmd.Parameters.AddWithValue("@PasswordHash", model.Password);
                    cmd.Parameters.AddWithValue("@Role", model.Role);
                    cmd.Parameters.AddWithValue("@Specialization", (object)model.Specialization ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", (object)model.Address ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsActive", IsActive);

                    try
                    {
                        newUserId = (int)cmd.ExecuteScalar(); // Get the new UserID
                    }
                    catch (SqlException ex)
                    {
                        TempData["ErrorMessage"] = "Error occurred while registering the user!";
                        return View(model);
                    }
                }

                if (model.Role == "Patient")
                {
                    string insertPatientQuery = @"INSERT INTO Patients (UserID, Gender, DOB, Phone, Address)
                                          VALUES (@UserID, 'Unknown', NULL, 'Not Provided', @Address)";
                    using (SqlCommand patientCmd = new SqlCommand(insertPatientQuery, con))
                    {
                        patientCmd.Parameters.AddWithValue("@UserID", newUserId);
                        patientCmd.Parameters.AddWithValue("@Address", (object)model.Address ?? DBNull.Value);
                        patientCmd.ExecuteNonQuery();
                    }
                }

                TempData["SuccessMessage"] = "Registration successful! You can login now.";
                return RedirectToAction("Login");
            }
        }


        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        //logic with SQL parameterization to prevent SQL injection
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
                            Session["IsActive"] = reader["IsActive"];

                            string role = reader["Role"].ToString().ToLower();
                            if (role == "admin") return RedirectToAction("AdminDashboard", "Admin");
                            if (role == "doctor")
                            {
                                if ((int)Session["IsActive"] == 0)
                                {
                                    TempData["ErrorMessage"] = "Your account is not activated yet. Please contact admin.";
                                    Session.Clear();
                                    return RedirectToAction("Login");
                                }
                                return RedirectToAction("DoctorDashboard", "Doctor");
                            }
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