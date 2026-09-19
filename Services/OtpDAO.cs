using Microsoft.EntityFrameworkCore;
using TripVista.Data;
using TripVista.Models.Entities;
using TripVista.Services.Interfaces;

namespace TripVista.Services
{
    public class OtpDAO : IOtpDAO
    {
        private readonly SmartTripContext _context;

        public OtpDAO(SmartTripContext context)
        {
            _context = context;
        }

        public async Task<OtpVerification> CreateAsync(string email, string otpCode, TimeSpan validFor, string purpose)
        {
            var otp = new OtpVerification
            {
                Id = Guid.NewGuid(),
                Email = email,
                OtpCode = otpCode,
                Purpose = purpose,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(validFor),
                IsUsed = false
            };

            await _context.OtpVerifications.AddAsync(otp);
            await _context.SaveChangesAsync();
            return otp;
        }

        public async Task<OtpVerification?> GetLatestAsync(string email, string purpose)
        {
            return await _context.OtpVerifications
                .Where(o => o.Email == email && o.Purpose == purpose)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<OtpVerification?> GetValidAsync(string email, string otpCode, string purpose)
        {
            return await _context.OtpVerifications
                .Where(o => o.Email == email && o.OtpCode == otpCode && o.Purpose == purpose && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task MarkUsedAsync(OtpVerification otp)
        {
            otp.IsUsed = true;
            await _context.SaveChangesAsync();
        }
    }
}
