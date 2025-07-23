using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using REMS.Backend.DTOs;
using REMS.Backend.Interfaces;
using REMS.Backend.Entities;

namespace REMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            var userToCreate = await _authService.Register(userForRegisterDto, "User");

            if (userToCreate != null)
            {
                return BadRequest("Başarısız.");
            }

            return StatusCode(201, "Başarılı");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var token = await _authService.Login(userForLoginDto, HttpContext.Connection.RemoteIpAddress?.ToString());
            if (token == null)
            {
                return Unauthorized("Email veya şifre yanlış"); // 401
            }

            return Ok(new { token });
        }
    }
}
