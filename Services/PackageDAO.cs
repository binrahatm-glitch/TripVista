using Microsoft.EntityFrameworkCore;
using TripVista.Data;
using TripVista.Helpers;
using TripVista.Models.Entities;
using TripVista.Services.Interfaces;

namespace TripVista.Services
{
    public class PackageDAO : IPackageDAO
    {
        private readonly SmartTripContext _context;

        public PackageDAO(SmartTripContext context)
        {
            _context = context;
        }

        public async Task<List<Packages>> GetAllAsync()
        {
            return await _context.Packages
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Packages>> GetActiveAsync()
        {
            var today = DateTimeExtensions.PakistanNow().Date;

            return await _context.Packages
                .Where(p => p.IsActive && p.TourStartDate.Date >= today)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Packages?> GetByIdAsync(Guid id)
        {
            var package = await _context.Packages
                .Include(p => p.PackageDestinations!)
                    .ThenInclude(pd => pd.Destination)
                .FirstOrDefaultAsync(x => x.PackageId == id);

            if (package?.PackageDestinations != null)
            {
                package.PackageDestinations = package.PackageDestinations
                    .OrderBy(pd => pd.SequenceNo)
                    .ToList();
            }

            return package;
        }

        public async Task CreateAsync(Packages pkg, List<Guid> destinationIds)
        {
            await _context.Packages.AddAsync(pkg);
            await AddDestinationsAsync(pkg.PackageId, destinationIds);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByTitleAsync(string title, Guid? excludeId)
        {
            var normalized = title.Trim().ToLower();
            return await _context.Packages
                .Where(p => excludeId == null || p.PackageId != excludeId)
                .AnyAsync(p => p.Title.Trim().ToLower() == normalized);
        }

        public async Task<bool> UpdateAsync(Packages pkg, List<Guid> destinationIds)
        {
            var existing = await _context.Packages.FindAsync(pkg.PackageId);
            if (existing == null) return false;

            existing.Title = pkg.Title;
            existing.ImagePath = pkg.ImagePath;
            existing.Region = pkg.Region;
            existing.PricePerPerson = pkg.PricePerPerson;
            existing.DurationDays = pkg.DurationDays;
            existing.MaxGroupSize = pkg.MaxGroupSize;
            existing.TourStartDate = pkg.TourStartDate;
            existing.TourEndDate = pkg.TourEndDate;
            existing.Details = pkg.Details;
            existing.IsActive = pkg.IsActive;

            var currentLinks = await _context.PackageDestinations
                .Where(pd => pd.PackageId == pkg.PackageId)
                .ToListAsync();
            _context.PackageDestinations.RemoveRange(currentLinks);

            await AddDestinationsAsync(pkg.PackageId, destinationIds);

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task AddDestinationsAsync(Guid packageId, List<Guid> destinationIds)
        {
            for (int i = 0; i < destinationIds.Count; i++)
            {
                await _context.PackageDestinations.AddAsync(new PackageDestinations
                {
                    Id = Guid.NewGuid(),
                    PackageId = packageId,
                    DestinationId = destinationIds[i],
                    SequenceNo = i + 1
                });
            }
        }

        public async Task<bool> DeactivateAsync(Guid id)
        {
            var pkg = await _context.Packages.FindAsync(id);
            if (pkg == null) return false;

            pkg.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasBookingsAsync(Guid id)
        {
            return await _context.Bookings.AnyAsync(b => b.PackageId == id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            if (await HasBookingsAsync(id)) return false;

            var pkg = await _context.Packages.FindAsync(id);
            if (pkg == null) return false;

            _context.Packages.Remove(pkg);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
