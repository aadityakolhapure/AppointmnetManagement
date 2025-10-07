using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppointmnetManagement.Models
{
    public class User
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public string Role { get; set; } // admin, doctor, patient

        public string Specialization { get; set; } // for doctors

        public string Address { get; set; }  // for patients
    }
}