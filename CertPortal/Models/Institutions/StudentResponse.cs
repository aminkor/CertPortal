using System;

namespace CertPortal.Models.Institutions
{
    public class StudentResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public string Status { get; set; }
        public string CertificateStatus { get; set; }
        public string UserRole { get; set; }
        public bool IsVerified { get; set; }
        public string RegisteredTo { get; set; }
        
        public int InstitutionId { get; set; }


    }
}