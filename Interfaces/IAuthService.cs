using REMS.Backend.DTOs;
using REMS.Backend.Entities;

namespace REMS.Backend.Interfaces
{
    public interface IAuthService
    {
        Task<User> Register(UserForRegisterDto userForRegisterDto, string role);
        Task<string> Login(UserForLoginDto userForLoginDto, string role);
        Task<bool> UserExists(string email);
        
    }
}
