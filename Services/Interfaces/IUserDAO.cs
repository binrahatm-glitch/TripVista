using TripVista.Models.Entities;

namespace TripVista.Services.Interfaces
{
    public interface IUserDAO
    {
        Task<Users> CreateUserAsync(Users user);
        Task<Users?> GetUserByIdAsync(Guid id);
        Task<Users?> GetUserByEmailAsync(string email);
        Task<List<Users>> GetAllUsersAsync();
        Task<bool> UpdateUserAsync(Users user);
        Task<bool> DeleteUserAsync(Guid id);
        Task<Users?> ValidateLoginAsync(string email, string password);
        Task<bool> ChangePasswordAsync(Guid userId, string newPassword);
    }
}
