using System;

namespace CertPortal.Models.Accounts
{
    public class AccountResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserRole { get; set; }
        public string Address { get; set; }
        public string ContactNo { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public bool IsVerified { get; set; }
        
    }
}