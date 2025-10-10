using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppointmnetManagement.Models
{
    public class Doctor
    {
        public int DoctorID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Specialization { get; set; }
        public string Phone { get; set; }
        public string FullName { get; internal set; }
    }
}