using FinanceApp.Data;
using FinanceApp.Enums;
using FinanceApp.Hubs;
using FinanceApp.Models;
using FinanceApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<FinanceAppHub> _hubContext;

        public TransactionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHubContext<FinanceAppHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
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

        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions.FindAsync(id);
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
                try
                {
                    transaction.UserId = _userManager.GetUserId(User);
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                    await SendChartDataUpdate();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
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

            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(m => m.Id == id);
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
            var transaction = await _context.Transactions.FindAsync(id);
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            await SendChartDataUpdate();
            return RedirectToAction(nameof(Index));
        }


        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.Id == id);
        }


        private async Task SendChartDataUpdate()
        {
            await _hubContext.Clients.All.SendAsync("UpdateChartData");
        }
        // POST: Transactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TransactionViewModel transactionVM)
        {
            if (ModelState.IsValid)
            {
                var transaction = new Transaction
                {
                    Category = (TransactionCategory)Enum.Parse(typeof(TransactionCategory), transactionVM.Category),
                    SubType = (TransactionSubType)Enum.Parse(typeof(TransactionSubType), transactionVM.SubType),
                    Amount = transactionVM.Amount,
                    Date = transactionVM.Date,
                    Description = transactionVM.Description, // Add this line
                    Currency = (Currency)Enum.Parse(typeof(Currency), transactionVM.Currency),
                    UserId = _userManager.GetUserId(User)
                };


                _context.Add(transaction);
                await _context.SaveChangesAsync();

                // Add this line to send the chart data update signal.
                await SendChartDataUpdate();

                return RedirectToAction(nameof(Index), "Dashboard");
            }

            return View(transactionVM);
        }

    }
}


