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

        public List<Destinations> GetAll()
        {
            return _context.Destinations.ToList();
        }

        

        public Destinations? GetById(Guid id)
        {
            return _context.Destinations.FirstOrDefault(x => x.DestinationId == id);
        }

        public void Create(Destinations dest)
        {
            _context.Destinations.Add(dest);
        }

        public void Update(Destinations dest)
        {
            _context.Destinations.Update(dest);
        }

        public void Delete(Guid id)
        {
            var dest = GetById(id);
            if (dest != null)
            {
                _context.Destinations.Remove(dest);
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
