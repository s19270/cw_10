using System;
using System.Collections.Generic;

namespace cw_3.NewModels
{
    public partial class Doctor
    {
        public Doctor()
        {
            Prescription = new HashSet<Prescription>();
        }

        public int IdDoctor { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public virtual ICollection<Prescription> Prescription { get; set; }
    }
}
