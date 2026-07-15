using Microsoft.EntityFrameworkCore;
using TripVista.Data;
using TripVista.Models.Entities;
using TripVista.Services.Interfaces;

namespace TripVista.Services
{
    public class DestinationDAO : IDestinationDAO
    {
        private readonly SmartTripContext _context;

        public DestinationDAO(SmartTripContext context)
        {
            _context = context;
        }

        public async Task<Destinations> CreateAsync(Destinations destination)
        {
            await _context.Destinations.AddAsync(destination);
            await _context.SaveChangesAsync();
            return destination;
        }

        public async Task<Destinations?> GetByIdAsync(Guid id)
        {
            return await _context.Destinations
                .Include(d => d.SubmittedByUser)
                .FirstOrDefaultAsync(d => d.DestinationId == id);
        }

        public async Task<List<Destinations>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Destinations
                .Where(d => d.SubmittedByUserId == userId)
                .OrderByDescending(d => d.SubmittedAt)
                .ToListAsync();
        }

        public async Task<List<Destinations>> GetApprovedAsync()
        {
            return await _context.Destinations
                .Where(d => d.Status == "Approved")
                .OrderByDescending(d => d.ReviewedAt)
                .ToListAsync();
        }

        public async Task<List<Destinations>> GetPendingAsync()
        {
            return await _context.Destinations
                .Include(d => d.SubmittedByUser)
                .Where(d => d.Status == "Pending")
                .OrderBy(d => d.SubmittedAt)
                .ToListAsync();
        }

        public async Task<List<Destinations>> GetReviewedAsync()
        {
            return await _context.Destinations
                .Include(d => d.SubmittedByUser)
                .Include(d => d.ReviewedByAdmin)
                .Where(d => d.Status == "Approved" || d.Status == "Rejected")
                .OrderByDescending(d => d.ReviewedAt)
                .ToListAsync();
        }

        public async Task<bool> ApproveAsync(Guid id, Guid adminId, string? remarks)
        {
            var destination = await _context.Destinations.FindAsync(id);
            if (destination == null || destination.Status != "Pending") return false;

            destination.Status = "Approved";
            destination.AdminRemarks = remarks;
            destination.ReviewedByAdminId = adminId;
            destination.ReviewedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectAsync(Guid id, Guid adminId, string remarks)
        {
            var destination = await _context.Destinations.FindAsync(id);
            if (destination == null || destination.Status != "Pending") return false;

            destination.Status = "Rejected";
            destination.AdminRemarks = remarks;
            destination.ReviewedByAdminId = adminId;
            destination.ReviewedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
