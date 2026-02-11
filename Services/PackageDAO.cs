using Microsoft.EntityFrameworkCore;
using TripVista.Data;
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

        public IEnumerable<Packages> GetAll()
        {
            return _context.Packages
                .Include(p => p.PackageDestinations)
                    .ThenInclude(pd => pd.Destination)
                .ToList();
        }

        public async Task<List<Packages>> GetAllPackagesWithDestinationsAsync()
        {
            return await _context.Packages
                .Include(p => p.PackageDestinations)
                .ThenInclude(pd => pd.Destination)
                .ToListAsync();
        }

        public Packages? GetById(Guid id)
        {
            Packages? packages = _context.Packages
                .Include(p => p.PackageDestinations)
                    .ThenInclude(pd => pd.Destination)
                .FirstOrDefault(x => x.PackageId == id);
            return packages;
        }

        public void Create(Packages pkg)
        {
            _context.Packages.Add(pkg);
        }

        public void Update(Packages pkg)
        {
            _context.Packages.Update(pkg);
        }

        public void Delete(Guid id)
        {
            var pkg = GetById(id);
            if (pkg != null)
            {
                _context.Packages.Remove(pkg);
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
