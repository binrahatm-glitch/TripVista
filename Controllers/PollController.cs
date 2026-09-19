using Microsoft.AspNetCore.Mvc;
using TripVista.Filters;
using TripVista.Models.DTO;
using TripVista.Models.Entities;
using TripVista.Services.Interfaces;

namespace TripVista.Controllers
{
    [RequireRole("user", "admin")]
    public class PollController : Controller
    {
        private readonly IPollDAO _pollDAO;

        public PollController(IPollDAO pollDAO)
        {
            _pollDAO = pollDAO;
        }

        // ---------------- User-facing ----------------

        [HttpGet]
        [RequireRole("user")]
        public async Task<IActionResult> Polls()
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("id")!);
            var activePolls = await _pollDAO.GetActiveAsync();
            var votedPollIds = await _pollDAO.GetVotedPollIdsForUserAsync(userId);

            ViewBag.VotedPollIds = votedPollIds;
            ViewBag.UserVotes = new Dictionary<Guid, Guid?>();
            foreach (var pollId in votedPollIds)
            {
                ((Dictionary<Guid, Guid?>)ViewBag.UserVotes)[pollId] = await _pollDAO.GetUserVoteOptionAsync(pollId, userId);
            }

            return View(activePolls);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("user")]
        public async Task<IActionResult> Vote(Guid pollId, Guid pollOptionId)
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("id")!);
            var (success, error) = await _pollDAO.VoteAsync(pollId, pollOptionId, userId);

            TempData[success ? "msg" : "Error"] = success ? "Vote submitted." : error;
            return RedirectToAction("Polls");
        }

        // ---------------- Admin-facing ----------------

        [HttpGet]
        [RequireRole("admin")]
        public async Task<IActionResult> ManagePolls()
        {
            var polls = await _pollDAO.GetAllAsync();
            return View(polls);
        }

        [HttpGet]
        [RequireRole("admin")]
        public IActionResult CreatePoll()
        {
            return View(new CreatePollViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("admin")]
        public async Task<IActionResult> CreatePoll(CreatePollViewModel vm)
        {
            var options = vm.Options.Where(o => !string.IsNullOrWhiteSpace(o)).ToList();
            if (options.Count < 2)
            {
                ModelState.AddModelError(nameof(vm.Options), "A poll needs at least 2 non-empty options.");
            }

            if (!ModelState.IsValid)
                return View(vm);

            var adminId = Guid.Parse(HttpContext.Session.GetString("id")!);

            var poll = new Polls
            {
                PollId = Guid.NewGuid(),
                Question = vm.Question,
                CreatedByAdminId = adminId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Options = options.Select(o => new PollOptions
                {
                    PollOptionId = Guid.NewGuid(),
                    OptionText = o
                }).ToList()
            };

            await _pollDAO.CreateAsync(poll);
            TempData["msg"] = "Poll created.";
            return RedirectToAction("ManagePolls");
        }

        [HttpGet]
        [RequireRole("admin")]
        public async Task<IActionResult> EditPoll(Guid pollId)
        {
            var poll = await _pollDAO.GetByIdAsync(pollId);
            if (poll == null) return NotFound();

            var results = await _pollDAO.GetResultsAsync(pollId);

            var vm = new EditPollViewModel
            {
                PollId = poll.PollId,
                Question = poll.Question,
                Options = (poll.Options ?? new List<PollOptions>())
                    .Select(o => new EditPollOptionInput
                    {
                        PollOptionId = o.PollOptionId,
                        OptionText = o.OptionText,
                        VoteCount = results.GetValueOrDefault(o.PollOptionId, 0)
                    })
                    .ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("admin")]
        public async Task<IActionResult> EditPoll(EditPollViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var options = vm.Options
                .Select(o => (o.PollOptionId, o.OptionText, o.Remove))
                .ToList();

            var (success, error) = await _pollDAO.UpdateAsync(vm.PollId, vm.Question, options);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error!);
                return View(vm);
            }

            TempData["msg"] = "Poll updated.";
            return RedirectToAction("ManagePolls");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("admin")]
        public async Task<IActionResult> ToggleActive(Guid pollId, bool isActive)
        {
            await _pollDAO.SetActiveAsync(pollId, isActive);
            TempData["msg"] = isActive ? "Poll reopened." : "Poll closed.";
            return RedirectToAction("ManagePolls");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("admin")]
        public async Task<IActionResult> DeletePoll(Guid pollId)
        {
            await _pollDAO.DeleteAsync(pollId);
            TempData["msg"] = "Poll deleted.";
            return RedirectToAction("ManagePolls");
        }

        [HttpGet]
        [RequireRole("admin")]
        public async Task<IActionResult> Results(Guid pollId)
        {
            var poll = await _pollDAO.GetByIdAsync(pollId);
            if (poll == null) return NotFound();

            var results = await _pollDAO.GetResultsAsync(pollId);
            ViewBag.Results = results;
            return View(poll);
        }
    }
}
