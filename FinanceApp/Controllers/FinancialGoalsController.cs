using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceApp.Data;
using FinanceApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using FinanceApp.Models.ViewModels;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class FinancialGoalsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FinancialGoalsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: FinancialGoals
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var goals = await _context.FinancialGoals
                            .Where(g => g.UserId == userId)
                            .ToListAsync();
            return View(goals);
        }

        // GET: FinancialGoals/Create
        public IActionResult Create()
        {
            return View(new UpdateFinancialGoalViewModel());
        }

        // POST: FinancialGoals/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UpdateFinancialGoalViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var goal = new FinancialGoal
                {
                    UserId = _userManager.GetUserId(User),
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    TargetAmount = viewModel.TargetAmount,
                    CurrentAmount = viewModel.CurrentAmount,
                    StartDate = viewModel.StartDate,
                    EndDate = viewModel.EndDate,
                    Status = viewModel.Status,
                    Currency = viewModel.Currency // Save the selected currency
                };

                _context.Add(goal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }


       
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var goal = await _context.FinancialGoals
                .FirstOrDefaultAsync(m => m.Id == id);
            if (goal == null)
            {
                return NotFound();
            }

            return View(goal);
        }

        // POST: FinancialGoals/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var goal = await _context.FinancialGoals.FindAsync(id);
            _context.FinancialGoals.Remove(goal);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Financial goal deleted successfully.";
            return RedirectToAction(nameof(Index));
        }


        private bool GoalExists(int id)
        {
            return _context.FinancialGoals.Any(e => e.Id == id);
        }

        public async Task<IActionResult> UpdateFinancialGoal(int id)
        {
            var financialGoal = await _context.FinancialGoals.FindAsync(id);

            if (financialGoal == null)
            {
                return NotFound();
            }

            var viewModel = new UpdateFinancialGoalViewModel
            {
                Id = financialGoal.Id,
                UserId = financialGoal.UserId,
                Title = financialGoal.Title,
                Description = financialGoal.Description,
                TargetAmount = financialGoal.TargetAmount,
                CurrentAmount = financialGoal.CurrentAmount,
                StartDate = financialGoal.StartDate,
                EndDate = financialGoal.EndDate,
                Status = financialGoal.Status
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateFinancialGoal(UpdateFinancialGoalViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var financialGoal = await _context.FinancialGoals.FindAsync(viewModel.Id);

                if (financialGoal == null)
                {
                    return NotFound();
                }

                financialGoal.Title = viewModel.Title;
                financialGoal.Description = viewModel.Description;
                financialGoal.TargetAmount = viewModel.TargetAmount;
                financialGoal.CurrentAmount = viewModel.CurrentAmount;
                financialGoal.StartDate = viewModel.StartDate;
                financialGoal.EndDate = viewModel.EndDate;
                financialGoal.Status = viewModel.Status;

                _context.Update(financialGoal);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "FinancialGoals");
            }

            return View(viewModel);
        }



    }
}

