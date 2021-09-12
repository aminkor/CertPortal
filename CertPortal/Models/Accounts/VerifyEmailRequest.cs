using System.ComponentModel.DataAnnotations;

namespace CertPortal.Models.Accounts
{
    public class VerifyEmailRequest
    {
        [Required]
        public string Token { get; set; }
    }
}