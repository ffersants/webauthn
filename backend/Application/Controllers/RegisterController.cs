using Fido2NetLib.Objects;
using Fido2NetLib;
using Microsoft.AspNetCore.Mvc;
using NetDevPack.Fido2.EntityFramework.Store.Store;
using System.Text;
using Microsoft.Extensions.Options;
using Application.DTOs.Input;
using Data.Migrations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.AspNetCore.DataProtection;
using Fido2NetLib.Development;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Application.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly IFido2Store _fido2Store;
        private readonly IFido2 _fido2;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IDataProtector _protector;

        public RegisterController(IFido2 fido2, IFido2Store fido2Store, IHttpContextAccessor httpContext, IDataProtectionProvider protector)
        {
            _fido2Store = fido2Store;
            _fido2 = fido2;
            _httpContext = httpContext;
            _protector = protector.CreateProtector("dsof");
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CreateRegisterOptionsInput input)
        {
            try
            {
                var username = input.Username;
                var displayName = input.DisplayName;

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var fidoUser = input.Get();
                // 1. Get user existing keys by username
                var existingKeys = await _fido2Store.ListPublicKeysByUser(fidoUser.Name);


                // 2. Create options
                var authenticatorSelection = new AuthenticatorSelection
                {
                    RequireResidentKey = false,
                    UserVerification = UserVerificationRequirement.Preferred
                };

                var exts = new AuthenticationExtensionsClientInputs()
                {
                    Extensions = true,
                    UserVerificationMethod = true,
                };

                var options = _fido2.RequestNewCredential(fidoUser, existingKeys.ToList(), authenticatorSelection, AttestationConveyancePreference.None, exts);

                // 3. Temporarily store options, session/in-memory cache/redis/db 
                var cookieOptions = new CookieOptions()
                {
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddMinutes(2),
                    HttpOnly = true,
                };
                var content = _protector.Protect(options.ToJson());
                _httpContext?.HttpContext?.Response.Cookies.Append("fido2.attestationOptions", content, cookieOptions);

                // 4. return options to client
                return Ok(options);
            }
            catch (Exception e)
            {
                return Problem();
            }
        }

        [HttpPost]
        public async Task<ActionResult> Index(PasswordlessModel model, string returnUrl)
        {
            var response = JsonSerializer.Deserialize<AuthenticatorAttestationRawResponse>(model.AttestationResponse);
            // 1. get the options we sent the client
            if (string.IsNullOrEmpty(_httpContext?.HttpContext?.Request.Cookies["fido2.attestationOptions"]))
                return NotFound();

            var jsonOptions = _protector.Unprotect(_httpContext?.HttpContext?.Request.Cookies["fido2.attestationOptions"]);
            var options = CredentialCreateOptions.FromJson(jsonOptions);

            // 2. Create callback so that lib can verify credential id is unique to this user
            IsCredentialIdUniqueToUserAsyncDelegate callback = async (args, cancellationToken) =>
            {
                var users = await _fido2Store.ListCredentialsByPublicKeyIdAsync(args.CredentialId);
                if (users.Count() > 0)
                    return false;
                return true;
            };

            // 2. Verify and make the credentials
            var success = await _fido2.MakeNewCredentialAsync(response, options, callback);

            _fido2Store.Store("", 
                new Fido2User
                {
                    DisplayName = model.DisplayName,
                    Name = model.Username,
                    Id = Encoding.UTF8.GetBytes(model.Username) // byte representation of userID is required
                }, 
                new StoredCredential
                {
                    Descriptor = new PublicKeyCredentialDescriptor(success.Result.CredentialId),
                    PublicKey = success.Result.PublicKey,
                    UserHandle = success.Result.User.Id,
                    SignatureCounter = success.Result.Counter,
                    CredType = success.Result.CredType,
                    RegDate = DateTime.Now,
                    AaGuid = success.Result.Aaguid
                });

            // 4. Create user at ASP.NET Identity
            var result = await _userManager.CreateAsync(user);

            // 5. Default ASP.NET Identity flow. (e-mail confirmation, ReturnUrl, etc.)
            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account without password.");

                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(model.Username, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return RedirectToPage("/Account/RegisterConfirmation", new { email = model.Username, returnUrl = returnUrl, area = "Identity" });
                }
                else
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
            }

            // 6. In case of errors 
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }


            return View(model);
        }
    }
}
