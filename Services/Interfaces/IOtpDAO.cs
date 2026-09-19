using TripVista.Models.Entities;

namespace TripVista.Services.Interfaces
{
    public interface IOtpDAO
    {
        Task<OtpVerification> CreateAsync(string email, string otpCode, TimeSpan validFor, string purpose);
        Task<OtpVerification?> GetLatestAsync(string email, string purpose);
        Task<OtpVerification?> GetValidAsync(string email, string otpCode, string purpose);
        Task MarkUsedAsync(OtpVerification otp);
    }
}
