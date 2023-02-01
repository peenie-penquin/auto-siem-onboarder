using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AutoSiem;
using AutoSiem.Areas.Identity.Data;

namespace test_auth.Controllers
{
    public class SiemController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SiemController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Siem
        public async Task<IActionResult> Index()
        {
            return View(await _context.Siems.ToListAsync());
        }

        // GET: Siem/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var siem = await _context.Siems
                .FirstOrDefaultAsync(m => m.Id == id);
            if (siem == null)
            {
                return NotFound();
            }

            return View(siem);
        }

        // GET: Siem/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Siem/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,IpAddress,Port")] Siem siem)
        {
            if (ModelState.IsValid)
            {
                siem.Id = Guid.NewGuid();
                _context.Add(siem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(siem);
        }

        // GET: Siem/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var siem = await _context.Siems.FindAsync(id);
            if (siem == null)
            {
                return NotFound();
            }
            return View(siem);
        }

        // POST: Siem/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,IpAddress,Port")] Siem siem)
        {
            if (id != siem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(siem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SiemExists(siem.Id))
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
            return View(siem);
        }

        // GET: Siem/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var siem = await _context.Siems
                .FirstOrDefaultAsync(m => m.Id == id);
            if (siem == null)
            {
                return NotFound();
            }

            return View(siem);
        }

        // POST: Siem/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var siem = await _context.Siems.FindAsync(id);
            _context.Siems.Remove(siem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SiemExists(Guid id)
        {
            return _context.Siems.Any(e => e.Id == id);
        }
    }
}
