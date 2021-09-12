using System.ComponentModel.DataAnnotations;

namespace CertPortal.Models.Accounts
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}