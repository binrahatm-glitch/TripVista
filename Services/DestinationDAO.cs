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

        public async Task<bool> UpdateAsync(Destinations destination)
        {
            var existing = await _context.Destinations.FindAsync(destination.DestinationId);
            if (existing == null) return false;

            existing.Name = destination.Name;
            existing.Location = destination.Location;
            existing.Description = destination.Description;
            if (destination.ImageUrl != null)
            {
                existing.ImageUrl = destination.ImageUrl;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsLinkedToPackageAsync(Guid id)
        {
            return await _context.PackageDestinations.AnyAsync(pd => pd.DestinationId == id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            if (await IsLinkedToPackageAsync(id)) return false;

            var destination = await _context.Destinations.FindAsync(id);
            if (destination == null) return false;

            _context.Destinations.Remove(destination);
            await _context.SaveChangesAsync();
            return true;
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

        public async Task<(bool success, string? error)> ApproveAsync(Guid id, Guid adminId, string? remarks)
        {
            var destination = await _context.Destinations.FindAsync(id);
            if (destination == null || destination.Status != "Pending") return (false, "Destination not found or not pending review.");

            var duplicates = await SearchAsyncByName(destination.Name, id);
            if (duplicates.Count > 0)
                return (false, "A destination with this name is already approved.");

            destination.Status = "Approved";
            destination.AdminRemarks = remarks;
            destination.ReviewedByAdminId = adminId;
            destination.ReviewedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return (true, null);
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

        public async Task<List<Destinations>> SearchAsyncByName(string name, Guid? excludeId = null)
        {
            var normalized = name.Trim().ToLower();
            return await _context.Destinations
                .Where(d => d.Status == "Approved" && d.Name.Trim().ToLower() == normalized)
                .Where(d => excludeId == null || d.DestinationId != excludeId)
                .OrderByDescending(d => d.ReviewedAt)
                .ToListAsync();
        }
    }
}
