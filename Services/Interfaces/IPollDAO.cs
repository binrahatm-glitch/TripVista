using TripVista.Models.Entities;

namespace TripVista.Services.Interfaces
{
    public interface IPollDAO
    {
        Task<List<Polls>> GetAllAsync();
        Task<List<Polls>> GetActiveAsync();
        Task<Polls?> GetByIdAsync(Guid id);
        Task<Polls> CreateAsync(Polls poll);
        Task<(bool success, string? error)> UpdateAsync(Guid pollId, string question, List<(Guid? OptionId, string Text, bool Remove)> options);
        Task<bool> HasVotesAsync(Guid pollId);
        Task<bool> SetActiveAsync(Guid pollId, bool isActive);
        Task<bool> DeleteAsync(Guid pollId);
        Task<HashSet<Guid>> GetVotedPollIdsForUserAsync(Guid userId);
        Task<Guid?> GetUserVoteOptionAsync(Guid pollId, Guid userId);
        Task<(bool success, string? error)> VoteAsync(Guid pollId, Guid pollOptionId, Guid userId);
        Task<Dictionary<Guid, int>> GetResultsAsync(Guid pollId);
    }
}
