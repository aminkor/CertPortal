using System;

namespace CertPortal.Models.Institutions
{
    public class StudentResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public string Status { get; set; }
    }
}