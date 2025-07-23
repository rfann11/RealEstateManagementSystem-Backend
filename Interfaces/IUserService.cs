using REMS.Backend.DTOs;
using REMS.Backend.Entities;

namespace REMS.Backend.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> GetUserByIdAsync(int id);
        Task<User> AddUserAsync(UserForRegisterDto userForRegisterDto, string rol);
        Task<bool> UpdateUserAsync(UserForUpdateDto userForUpdateDto);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> UserExistsByIdAsync(int id);
        Task<bool> IsEmailUnique(string email, int? userId = null);
    }
}
