using TripVista.Models.Entities;

namespace TripVista.Services.Interfaces
{
    public interface IUserSubmissionDAO
    {
        IEnumerable<UserSubmissions> GetAll();
        UserSubmissions? GetById(Guid id);
        IEnumerable<UserSubmissions> GetByUserId(Guid userId);

        void Create(UserSubmissions submission);
        void Update(UserSubmissions submission);
        void Delete(Guid id);

        void Save();
    }
}
