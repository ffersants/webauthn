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
using Domain.Interfaces.Services;
using Domain.Entities;

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
        private readonly IEmailService _emailService;
        private AuthenticatorOptions authenticatorOptions {get; set;}
        public RegisterController(IFido2 fido2, 
                                  IFido2Store fido2Store, 
                                  IHttpContextAccessor httpContext, 
                                  IDataProtectionProvider protector,
                                  IEmailService emailService,
                                  AuthenticatorOptions authenticatorOptions)
        {
            _fido2Store = fido2Store;
            _fido2 = fido2;
            _httpContext = httpContext;
            _protector = protector.CreateProtector("dsof");
            _emailService = emailService;
            this.authenticatorOptions = authenticatorOptions;
        }

        [HttpPost("get-options")]
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
                    UserVerification = UserVerificationRequirement.Preferred,
                };

                var exts = new AuthenticationExtensionsClientInputs()
                {
                    Extensions = true,
                    UserVerificationMethod = true,

                };

                CredentialCreateOptions options = _fido2.RequestNewCredential(fidoUser, existingKeys.ToList(), authenticatorSelection, AttestationConveyancePreference.None, exts);

                // 3. Temporarily store options, session/in-memory cache/redis/db 
                var cookieOptions = new CookieOptions()
                {
                    Expires = DateTimeOffset.UtcNow.AddMinutes(2),
                };
                /*
                var content = _protector.Protect(options.ToJson());
                _httpContext?.HttpContext?.Response.Cookies.Append("fido2.attestationOptions", content, cookieOptions);
                */
                
                authenticatorOptions.Options = options;
                
                // 4. return options to client
                return Ok(options);
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }

        [HttpPost("assert-options")]
        public async Task<ActionResult> Post([FromBody] AssertRegisterOptions model)
        {
            try
            {
               // 1. get the options we sent the client
                //if (string.IsNullOrEmpty(_httpContext?.HttpContext?.Request.Cookies["fido2.attestationOptions"]))
                //    return NotFound();
                /*
                var jsonOptions = _protector.Unprotect(_httpContext?.HttpContext?.Request.Cookies["fido2.attestationOptions"]);
                CredentialCreateOptions options = CredentialCreateOptions.FromJson(jsonOptions);
                */
                var options = authenticatorOptions.Options;

                // 2. Create callback so that lib can verify credential id is unique to this user
                IsCredentialIdUniqueToUserAsyncDelegate callback = async (args, cancellationToken) =>
                {
                    var users = await _fido2Store.ListCredentialsByPublicKeyIdAsync(args.CredentialId);
                    if (users.Count() > 0)
                        return false;
                    return true;
                };

                // 2. Verify and make the credentials
                var success = await _fido2.MakeNewCredentialAsync(model.AuthenticatorAttestationRawResponse, options, callback);

                _fido2Store.Store(model.UserAgent,
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
                        AaGuid = success.Result.Aaguid,
    
                        CredType = success.Result.CredType,
                        SignatureCounter = success.Result.Counter,
                        RegDate = DateTime.Now,
                    });

                _emailService.EnviarEmailRegistroDispositivo(DateTime.Now, model.Username);

                return Ok(success);
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }
    }
}
