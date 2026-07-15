using TripVista.Models.Entities;

namespace TripVista.Services.Interfaces
{
    public interface IOtpDAO
    {
        Task<OtpVerification> CreateAsync(string email, string otpCode, TimeSpan validFor);
        Task<OtpVerification?> GetLatestAsync(string email);
        Task<OtpVerification?> GetValidAsync(string email, string otpCode);
        Task MarkUsedAsync(OtpVerification otp);
    }
}
