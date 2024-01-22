using Fido2NetLib.Objects;
using Fido2NetLib;
using Microsoft.AspNetCore.Mvc;
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

namespace Application.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class DispositivosController : ControllerBase
    {
        readonly IDispositivosService _service;

        public DispositivosController(IDispositivosService service)
        {
            _service = service;
        }

        [HttpGet("{matricula}")]
        public async Task<ActionResult> Get(string matricula)
        {
            try {
                var result = await _service.Get(matricula);
                return Ok(result);
            } 
            catch(Exception e) { 
                return Problem(e.Message);
            }
        }

        [HttpDelete]
        public async Task<ActionResult> Delete([FromBody] DeleteDispositivoInput input)
        {
            try {
                await _service.Delete(input.ChavePublicaId);
                return Ok();
            } 
            catch(Exception e) { 
                return Problem(e.Message);
            }
        }
    }
}
