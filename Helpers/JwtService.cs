using Microsoft.Extensions.Configuration; // IConfiguration için
using Microsoft.IdentityModel.Tokens; // SymmetricSecurityKey için
using System.IdentityModel.Tokens.Jwt; // JwtSecurityTokenHandler için
using System.Security.Claims; // Claims için
using System.Text; // Encoding için
using REMS.Backend.Entities; // User entity için

namespace REMS.Backend.Helpers
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;


        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
            _secretKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(_secretKey))
            {
                throw new InvalidOperationException("JWT Secret Key 'Jwt:Key' in appsettings.json is not configured.");
            }
            System.Diagnostics.Debug.WriteLine($"JWT Secret Key Loaded: {_secretKey}"); // Visual Studio Output penceresine yazar
            Console.WriteLine($"JWT Secret Key Loaded: {_secretKey}"); // Konsol penceresine yazar
            Console.WriteLine($"JWT Secret Key Bytes Length (Generate): {Encoding.ASCII.GetBytes(_secretKey).Length}"); // BURAYI EKLEYİN

        }

        public string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Kullanıcı ID
                    new Claim(ClaimTypes.Email, user.Email),                 // Kullanıcı Email
                    new Claim(ClaimTypes.Role, user.Role)                    // Kullanıcı Rolü
                }),
                Expires = DateTime.UtcNow.AddHours(2), // Token'ın geçerlilik süresi (2 saat)
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor); // Token nesnesini oluştur
            return tokenHandler.WriteToken(token); // Token'ı string olarak döndür

        }
    }
}
