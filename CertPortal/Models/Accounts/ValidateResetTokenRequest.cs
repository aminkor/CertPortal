using System.ComponentModel.DataAnnotations;

namespace CertPortal.Models.Accounts
{
    public class ValidateResetTokenRequest
    {
        [Required]
        public string Token { get; set; }
    }
}