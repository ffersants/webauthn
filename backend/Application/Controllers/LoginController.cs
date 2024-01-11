using System.Text.Json;
using Application.DTOs.Input;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using NetDevPack.Fido2.EntityFramework.Store.Store;

namespace Application.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {

        private readonly IFido2Store _fido2Store;
        private readonly IFido2 _fido2;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IDataProtector _protector;

        public LoginController(IFido2Store fido2Store,
                               IFido2 fido2,
                               IHttpContextAccessor httpContextAccessor,
                               IDataProtectionProvider protector)
        {
            _fido2Store = fido2Store;
            _fido2 = fido2;
            _httpContext = httpContextAccessor;
            _protector = protector.CreateProtector("dsof");
        }

        [HttpGet("get-options")]
        public async Task<ActionResult> Get([FromQuery] PasswordlessInput.LoginModel query)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var existingCredentials = new List<PublicKeyCredentialDescriptor>();

                if (!string.IsNullOrEmpty(query.Username))
                {

                    // 1. Get registered credentials from database
                    existingCredentials = (await _fido2Store.ListPublicKeysByUser(query.Username)).ToList();
                }

                var exts = new AuthenticationExtensionsClientInputs()
                {
                    UserVerificationMethod = true
                };

                // 2. Create options
                var options = _fido2.GetAssertionOptions(
                    existingCredentials,
                    UserVerificationRequirement.Discouraged,
                    exts
                );

                // 3. Temporarily store options, session/in-memory cache/redis/db
                var cookieOptions = new CookieOptions()
                {
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddMinutes(2),
                    HttpOnly = true,
                };
                var content = _protector.Protect(options.ToJson());
                _httpContext?.HttpContext?.Response.Cookies.Append("fido2.assertionOptions", content, cookieOptions);

                // 5. Return options to client
                return Ok(options);
            }

            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }

        [HttpPost("assert-options")]
        public async Task<IActionResult> Post(PasswordlessInput login)
        {

            // 1. Get the assertion options we sent the client
            if (string.IsNullOrEmpty(_httpContext?.HttpContext?.Request.Cookies["fido2.assertionOptions"]))
                return NotFound();

            var jsonOptions = _protector.Unprotect(_httpContext?.HttpContext?.Request.Cookies["fido2.assertionOptions"]);
            var options = AssertionOptions.FromJson(jsonOptions);

            // 2. Get registered credential from database
            var creds = await _fido2Store.GetCredentialByPublicKeyIdAsync(login.AttestationResponse.Id);
            if (creds is null)
                return Problem("Unknown Credentials");

            // 3. Get credential counter from database
            var storedCounter = creds.SignatureCounter;

            // 4. Create callback to check if userhandle owns the credentialId
            IsUserHandleOwnerOfCredentialIdAsync callback = async (args, cancellationToken) =>
            {
                var storedCreds = await _fido2Store.GetCredentialsByUserHandleAsync(args.UserHandle);
                return storedCreds.ToList().Exists(c => c.Descriptor.Id.SequenceEqual(args.CredentialId));
            };

            // 5. Make the assertion
            var res = await _fido2.MakeAssertionAsync(login.AttestationResponse, options, creds.PublicKey, storedCounter, callback);

            // 6. Store the updated counter
            await _fido2Store.UpdateCounter(res.CredentialId, res.Counter);

            return Ok(res);
        }
    }
}