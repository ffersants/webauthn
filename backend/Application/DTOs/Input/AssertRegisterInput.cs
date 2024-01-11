using Fido2NetLib;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.DTOs.Input
{
    public class AssertRegisterOptions
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Username { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        [Display(Name = "Name")]
        public string DisplayName { get; set; }

        [Required]
        public string UserAgent {get; set;}

        public AuthenticatorAttestationRawResponse AuthenticatorAttestationRawResponse {get; set;}
    }
}
