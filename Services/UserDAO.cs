using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TripVista.Data;
using TripVista.Models.Entities;
using TripVista.Services.Interfaces;

namespace TripVista.Services
{
    public class UserDAO : IUserDAO
    {
        private readonly SmartTripContext _context;

        public UserDAO(SmartTripContext context)
        {
            _context = context;
        }

        // Create User
        public async Task<Users> CreateUserAsync(Users user)
        {
            user.CreatedAt = DateTime.Now;
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        // Get User By ID
        public async Task<Users?> GetUserByIdAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.Bookings)
                .Include(u => u.Reviews)
                .Include(u => u.UserSubmissions)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        // Get User By Email
        public async Task<Users?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        // Get All Users
        public async Task<List<Users>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        // Update User
        public async Task<bool> UpdateUserAsync(Users user)
        {
            var existing = await _context.Users.FindAsync(user.UserId);
            if (existing == null) return false;

            existing.FullName = user.FullName;
            existing.Email = user.Email;
            existing.Password = user.Password;
            existing.Role = user.Role;

            await _context.SaveChangesAsync();
            return true;
        }

        // Delete User
        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        // Validate Login
        public async Task<Users?> ValidateLoginAsync(string email, string password)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
        }

        // Change Password
        public async Task<bool> ChangePasswordAsync(Guid userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.Password = newPassword;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
