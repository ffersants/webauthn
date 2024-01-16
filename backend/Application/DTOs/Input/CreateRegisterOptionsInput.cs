using Fido2NetLib;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.DTOs.Input
{
    public class CreateRegisterOptionsInput
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        public string DisplayName { get; set; }

        public string? AttestationResponse { get; set; }

        public Fido2User Get()
        {
            return new Fido2User
            {
                DisplayName = DisplayName,
                Name = Username,
                Id = Encoding.UTF8.GetBytes(Username) // byte representation of userID is required
            };
        }
    }
}
