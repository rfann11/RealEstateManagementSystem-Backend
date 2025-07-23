using REMS.Backend.Data;
using REMS.Backend.Entities;
using REMS.Backend.DTOs;
using REMS.Backend.Helpers; 
using REMS.Backend.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace REMS.Backend.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Kullanıcılar.ToListAsync();
            return users.Select(u => new UserDto { Id = u.Id, Email = u.Email, Role = u.Role });
        }

        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            var user = await _context.Kullanıcılar.FindAsync(id);
            if (user == null) { return null; }
            return new UserDto { Id = user.Id, Email = user.Email, Role = user.Role };
        }

        public async Task<User> AddUserAsync(UserForRegisterDto userForRegisterDto, string role)
        {
            //Email unique kontrolü
            if (!await IsEmailUnique(userForRegisterDto.Email))
            {
                await LogActivity(null, "Add User", $"Kullanıcı ekleme başarısız: Email zaten mevcut: {userForRegisterDto.Email}", DateTime.UtcNow, "N/A", "Failed");
                throw new InvalidOperationException("Bu e-posta adresi zaten kullanılıyor.");
            }

            PasswordHasher.CreatePasswordHash(userForRegisterDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Email = userForRegisterDto.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Role = role
            };

            await _context.Kullanıcılar.AddAsync(user);
            await _context.SaveChangesAsync();

            await LogActivity(user.Id, "Add User", $"Yeni kullanıcı eklendi: {user.Email} ({role})", DateTime.UtcNow, "N/A", "Success");

            return user;
        }

        public async Task<bool> UpdateUserAsync(UserForUpdateDto userForUpdateDto)
        {
            var userToUpdate = await _context.Kullanıcılar.FindAsync(userForUpdateDto.Id);
            if (userToUpdate == null)
            {
                await LogActivity(userForUpdateDto.Id, "Update User", $"Kullanıcı güncelleme başarısız: Kullanıcı bulunamadı: ID {userForUpdateDto.Id}", DateTime.UtcNow, "N/A", "Failed");
                return false;
            }

            //Email Değişişkliği Kontrolü
            if (userToUpdate.Email != userForUpdateDto.Email && !await IsEmailUnique(userForUpdateDto.Email, userForUpdateDto.Id))
            {
                await LogActivity(userForUpdateDto.Id, "Update User", $"Kullanıcı güncelleme başarısız: Email zaten kullanılıyor: {userForUpdateDto.Email}", DateTime.UtcNow, "N/A", "Failed");
                throw new InvalidOperationException("Bu e-posta adresi zaten kullanılıyor.");
            }

            userToUpdate.Email = userForUpdateDto.Email;
            userToUpdate.Role = userForUpdateDto.Role;

            if (!string.IsNullOrEmpty(userForUpdateDto.Password))
            {
                PasswordHasher.CreatePasswordHash(userForUpdateDto.Password, out byte[] passwordHash, out byte[] passwordSalt);
                userToUpdate.PasswordHash = passwordHash;
                userToUpdate.PasswordSalt = passwordSalt;
            }

            _context.Kullanıcılar.Update(userToUpdate);
            await _context.SaveChangesAsync();

            await LogActivity(userForUpdateDto.Id, "Update User", $"Kullanıcı güncellendi: {userForUpdateDto.Email} ({userForUpdateDto.Role})", DateTime.UtcNow, "N/A", "Success");
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var userToDelete = await _context.Kullanıcılar.FindAsync(id);
            if (userToDelete == null) 
            {
                await LogActivity(userToDelete.Id, "Delete User", $"Kullanıcı silme başarısız: Kullanıcı bulunamadı: ID {userToDelete.Id}", DateTime.UtcNow, "N/A", "Failed");
                return false;
            }

            _context.Kullanıcılar.Remove(userToDelete);
            await _context.SaveChangesAsync();

            await LogActivity(id, "Delete User", $"Kullanıcı silindi: {userToDelete.Email}", DateTime.UtcNow, "N/A", "Success");
            return true;
        }

        public async Task<bool> UserExistsByIdAsync(int id)
        {
            return await _context.Kullanıcılar.AnyAsync(u => u.Id == id);
        }

        public async Task<bool> IsEmailUnique(string email, int? userId = null)
        {
            return !await _context.Kullanıcılar.AnyAsync(u => u.Email == email && (userId == null || u.Id != userId));
        }

        private async Task LogActivity(int? userId, string operationType, string description, DateTime timestamp, string userIp, string status)
        {
            var log = new Log
            {
                UserId = userId,
                OperationType = operationType,
                Description = description,
                Timestamp = timestamp,
                UserIp = userIp, // Bu kısım UserService için her zaman uygun olmayabilir (Controller'dan gelmeli)
                Status = status
            };
            await _context.Loglar.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}
