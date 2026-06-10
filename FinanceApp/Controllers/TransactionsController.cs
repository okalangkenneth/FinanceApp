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
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TransactionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Transactions
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            var transactions = await _context.Transactions
                                    .Where(t => t.UserId == userId)
                                    .ToListAsync();
            var viewModel = new TransactionsReportViewModel
            {
                Transactions = transactions,
                PreferredCurrency = user.PreferredCurrency // Assuming you have a PreferredCurrency property in ApplicationUser
            };
            return View(viewModel);
        }



        // GET: Transactions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Transactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Date,Description,Amount,Type,Category,Currency")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                transaction.UserId = _userManager.GetUserId(User);
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(transaction);
        }


        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (transaction == null)
            {
                return NotFound();
            }
            return View(transaction);
        }

        // POST: Transactions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,Description,Amount,Type,Category,Currency")] Transaction transaction)
        {
            if (id != transaction.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Load the tracked entity scoped to the current user; never
                // attach the posted entity (UserId must not be client-settable)
                var userId = _userManager.GetUserId(User);
                var existing = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
                if (existing == null)
                {
                    return NotFound();
                }

                existing.Date = transaction.Date;
                existing.Description = transaction.Description;
                existing.Amount = transaction.Amount;
                existing.Type = transaction.Type;
                existing.Category = transaction.Category;
                existing.Currency = transaction.Currency;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (transaction == null)
            {
                return NotFound();
            }
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}


