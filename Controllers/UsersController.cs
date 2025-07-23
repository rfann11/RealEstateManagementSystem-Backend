using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using REMS.Backend.DTOs;
using REMS.Backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace REMS.Backend.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public UsersController(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        // GET: api/users - Tüm kullanıcıları listele (Sadece Admin)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // GET: api/users/{id} - Belirli bir kullanıcıyı ID'ye göre getir (Sadece Admin)
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı");
            }
            return Ok(user);
        }

        // POST: api/users - Yeni kullanıcı ekle (Sadece Admin)
        [HttpPost]

        public async Task<IActionResult> AddUser(UserForRegisterDto userForRegisterDto)
        {
            try
            {
                var newUser = await _userService.AddUserAsync(userForRegisterDto, "User");
                return StatusCode(201, "Kullanıcı eklendi.");
            }
            catch (InvalidOperationException ex)
            {

                return BadRequest(ex.Message);
            }
            catch (Exception ex) 
            {
                return StatusCode(500, $"Bir hata oluştu{ex.Message}");
            }
        }

        // PUT: api/users/{id} - Kullanıcı güncelle (Sadece Admin)
        [HttpPut("{id}")]

        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            if (id != userForUpdateDto.Id)
            {
                return BadRequest("URL'deki ID ile bodydeki ID eşleşmiyor.");
            }

            if (!await _userService.UserExistsByIdAsync(id))
            {
                return NotFound("Güncellenecek kullannıcı bulunamadı.");
            }

            try
            {
                var succes = await _userService.UpdateUserAsync(userForUpdateDto);
                if(!succes) { return NotFound("Kullanıcı bulunamadı veya güncelleme başarısız"); }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message); // Email benzersizlik hatası gibi
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Bir hata oluştu: {ex.Message}");
            }
        }

        // DELETE: api/users/{id} - Kullanıcı sil (Sadece Admin)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if(!await _userService.UserExistsByIdAsync(id)) { return NotFound("Silinecek kullanıcı bulunamadı"); }
            try
            {
                var succes = await _userService.DeleteUserAsync(id);
                if (!succes) { return StatusCode(500, "Kullanıcı silinirken bir sorun oluştu."); }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Bir hata oluştu: {ex.Message}");
            }
        }


    }
}
