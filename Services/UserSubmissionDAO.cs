using Microsoft.EntityFrameworkCore;
using TripVista.Data;
using TripVista.Models.Entities;
using TripVista.Services.Interfaces;

namespace TripVista.Services
{
    public class UserSubmissionDAO : IUserSubmissionDAO
    {
        private readonly SmartTripContext _context;

        public UserSubmissionDAO(SmartTripContext context)
        {
            _context = context;
        }

        public IEnumerable<UserSubmissions> GetAll()
        {
            return _context.UserSubmissions
                           .Include(x => x.User)
                           .OrderByDescending(x => x.CreatedAt)
                           .ToList();
        }

        public UserSubmissions? GetById(Guid id)
        {
            return _context.UserSubmissions
                           .Include(x => x.User)
                           .FirstOrDefault(x => x.SubmissionId == id);
        }

        public IEnumerable<UserSubmissions> GetByUserId(Guid userId)
        {
            return _context.UserSubmissions
                           .Where(x => x.UserId == userId)
                           .OrderByDescending(x => x.CreatedAt)
                           .ToList();
        }

        public void Create(UserSubmissions submission)
        {
            _context.UserSubmissions.Add(submission);
        }

        public void Update(UserSubmissions submission)
        {
            _context.UserSubmissions.Update(submission);
        }

        public void Delete(Guid id)
        {
            var submission = GetById(id);
            if (submission != null)
            {
                _context.UserSubmissions.Remove(submission);
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
