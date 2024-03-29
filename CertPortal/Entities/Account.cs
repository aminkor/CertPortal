using System;
using System.Collections.Generic;

namespace CertPortal.Entities
{
    public class Account
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public bool AcceptTerms { get; set; }
        public UserRole UserRole { get; set; }
        public string VerificationToken { get; set; }
        public DateTime? Verified { get; set; }
        public bool IsVerified => Verified.HasValue || PasswordReset.HasValue;
        public string ResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }
        public DateTime? PasswordReset { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; }
        public int? InstitutionId { get; set; }
        public string Address { get; set; }
        public string ContactNo { get; set; }

        public virtual Institution Institution { get; set; }

        
        public virtual ICollection<InstitutionStudent> InstitutionStudent { get; set; }
        public virtual ICollection<Certificate> Certificates { get; set; }


        public bool OwnsToken(string token) 
        {
            return this.RefreshTokens?.Find(x => x.Token == token) != null;
        }

        public string FullName()
        {
            return this.FirstName + " " + this.LastName;
        }
    }
}