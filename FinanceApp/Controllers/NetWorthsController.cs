using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinanceApp.Data;
using FinanceApp.Models;

namespace FinanceApp.Controllers
{
    public class NetWorthsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NetWorthsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: NetWorths
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.NetWorths.Include(n => n.ApplicationUser);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: NetWorths/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var netWorth = await _context.NetWorths
                .Include(n => n.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (netWorth == null)
            {
                return NotFound();
            }

            return View(netWorth);
        }

        // GET: NetWorths/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: NetWorths/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,TotalAssets,TotalLiabilities,Date")] NetWorth netWorth)
        {
            if (ModelState.IsValid)
            {
                _context.Add(netWorth);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", netWorth.UserId);
            return View(netWorth);
        }

        // GET: NetWorths/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var netWorth = await _context.NetWorths.FindAsync(id);
            if (netWorth == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", netWorth.UserId);
            return View(netWorth);
        }

        // POST: NetWorths/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,TotalAssets,TotalLiabilities,Date")] NetWorth netWorth)
        {
            if (id != netWorth.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(netWorth);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NetWorthExists(netWorth.Id))
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", netWorth.UserId);
            return View(netWorth);
        }

        // GET: NetWorths/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var netWorth = await _context.NetWorths
                .Include(n => n.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (netWorth == null)
            {
                return NotFound();
            }

            return View(netWorth);
        }

        // POST: NetWorths/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var netWorth = await _context.NetWorths.FindAsync(id);
            _context.NetWorths.Remove(netWorth);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NetWorthExists(int id)
        {
            return _context.NetWorths.Any(e => e.Id == id);
        }
    }
}
