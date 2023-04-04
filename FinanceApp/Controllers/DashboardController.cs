using FinanceApp.Data;
using FinanceApp.Models;
using FinanceApp.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId && t.Date.Month == DateTime.Now.Month && t.Date.Year == DateTime.Now.Year)
                .ToListAsync();

            var financialGoals = await _context.FinancialGoals
                .Where(g => g.UserId == userId)
                .ToListAsync();

            var income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var expenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
            var balance = income - expenses;

            ViewBag.Income = income;
            ViewBag.Expenses = expenses;
            ViewBag.Balance = balance;

            var spendingAnalysis = Enum.GetValues(typeof(TransactionCategory))
                .Cast<TransactionCategory>()
                .Select(category => new SpendingAnalysis
                {
                    Category = category.ToString(),
                    Amount = transactions
                        .Where(t => t.Category == category && t.Type == TransactionType.Expense)
                        .Sum(t => t.Amount)
                })
                        .ToList();
            var model = new DashboardViewModel
            {
                Transactions = transactions,
                FinancialGoals = financialGoals,
                SpendingAnalysis = spendingAnalysis
            };

            return View(model);


        }
    }
}


