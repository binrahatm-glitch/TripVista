using Microsoft.EntityFrameworkCore;
using TripVista.Data;
using TripVista.Models.Entities;
using TripVista.Services.Interfaces;

namespace TripVista.Services
{
    public class PollDAO : IPollDAO
    {
        private readonly SmartTripContext _context;

        public PollDAO(SmartTripContext context)
        {
            _context = context;
        }

        public async Task<List<Polls>> GetAllAsync()
        {
            return await _context.Polls
                .Include(p => p.Options)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Polls>> GetActiveAsync()
        {
            return await _context.Polls
                .Include(p => p.Options)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Polls?> GetByIdAsync(Guid id)
        {
            return await _context.Polls
                .Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.PollId == id);
        }

        public async Task<Polls> CreateAsync(Polls poll)
        {
            await _context.Polls.AddAsync(poll);
            await _context.SaveChangesAsync();
            return poll;
        }

        public async Task<bool> HasVotesAsync(Guid pollId)
        {
            return await _context.PollVotes.AnyAsync(v => v.PollId == pollId);
        }

        public async Task<bool> SetActiveAsync(Guid pollId, bool isActive)
        {
            var poll = await _context.Polls.FindAsync(pollId);
            if (poll == null) return false;

            poll.IsActive = isActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid pollId)
        {
            var poll = await _context.Polls.FindAsync(pollId);
            if (poll == null) return false;

            _context.Polls.Remove(poll); // cascades to options + votes
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<HashSet<Guid>> GetVotedPollIdsForUserAsync(Guid userId)
        {
            var pollIds = await _context.PollVotes
                .Where(v => v.UserId == userId)
                .Select(v => v.PollId)
                .ToListAsync();

            return pollIds.ToHashSet();
        }

        public async Task<Guid?> GetUserVoteOptionAsync(Guid pollId, Guid userId)
        {
            var vote = await _context.PollVotes
                .FirstOrDefaultAsync(v => v.PollId == pollId && v.UserId == userId);

            return vote?.PollOptionId;
        }

        public async Task<(bool success, string? error)> VoteAsync(Guid pollId, Guid pollOptionId, Guid userId)
        {
            var poll = await _context.Polls.FirstOrDefaultAsync(p => p.PollId == pollId);
            if (poll == null || !poll.IsActive)
                return (false, "This poll is not available for voting.");

            var alreadyVoted = await _context.PollVotes.AnyAsync(v => v.PollId == pollId && v.UserId == userId);
            if (alreadyVoted)
                return (false, "You have already voted on this poll.");

            var optionBelongsToPoll = await _context.PollOptions.AnyAsync(o => o.PollOptionId == pollOptionId && o.PollId == pollId);
            if (!optionBelongsToPoll)
                return (false, "Invalid option selected.");

            _context.PollVotes.Add(new PollVotes
            {
                PollVoteId = Guid.NewGuid(),
                PollId = pollId,
                PollOptionId = pollOptionId,
                UserId = userId,
                VotedAt = DateTime.Now
            });

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Unique index (PollId, UserId) caught a race.
                return (false, "You have already voted on this poll.");
            }

            return (true, null);
        }

        public async Task<Dictionary<Guid, int>> GetResultsAsync(Guid pollId)
        {
            return await _context.PollVotes
                .Where(v => v.PollId == pollId)
                .GroupBy(v => v.PollOptionId)
                .Select(g => new { OptionId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.OptionId, x => x.Count);
        }
    }
}
