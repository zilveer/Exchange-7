using System.ComponentModel.DataAnnotations;

namespace Auth.Models
{
    public class VerifyTOTPModel
    {
        public string ReturnUrl { get; set; }

        [Required]
        [StringLength(maximumLength: 6, MinimumLength = 6)]
        public string TOTP { get; set; }
    }
}
