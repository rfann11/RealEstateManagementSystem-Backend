using REMS.Backend.Data;
using REMS.Backend.Entities;
using REMS.Backend.DTOs;
using REMS.Backend.Helpers; 
using REMS.Backend.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace REMS.Backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;

        public AuthService(ApplicationDbContext context,JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<User> Register(UserForRegisterDto userForRegisterDto,string role)
        {
            if (await UserExists(userForRegisterDto.Email))
            {
                throw new InvalidOperationException("Kullanıcı zaten mevcut");
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

            await LogActivity(null, "Register", $"Yeni kullanıcı kaydedildi: {user.Email} ({role})", DateTime.UtcNow, "N/A", "Success");


            return user;
        }

        public async Task<string> Login(UserForLoginDto userForLoginDto, string userIp)
        {
            var user = await _context.Kullanıcılar.FirstOrDefaultAsync(x => x.Email == userForLoginDto.Email);

            // Kullanıcı yoksa veya şifre yanlışsa
            if (user == null || !PasswordHasher.VerifyPasswordHash(userForLoginDto.Password, user.PasswordHash, user.PasswordSalt))
            {
                // Loglama (başarısız deneme)
                await LogActivity(user?.Id, "Login", $"Başarısız giriş denemesi: {userForLoginDto.Email}", DateTime.UtcNow, userIp, "Failed");
                return null; // Başarısız giriş
            }

            // Loglama (başarılı deneme)
            await LogActivity(user.Id, "Login", $"Başarılı giriş: {user.Email} ({user.Role})", DateTime.UtcNow, userIp, "Success");

            return _jwtService.GenerateToken(user); // JWT döndür
        }

        public async Task<bool> UserExists(string email)
        {
            return await _context.Kullanıcılar.AnyAsync(x => x.Email == email);
        }

        private async Task LogActivity(int? userId, string operationType, string description, DateTime timestamp, string userIp, string status)
        {
            var log = new Log
            {
                UserId = userId,
                OperationType = operationType,
                Description = description,
                Timestamp = timestamp,
                UserIp = userIp,
                Status = status
            };
            await _context.Loglar.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}
