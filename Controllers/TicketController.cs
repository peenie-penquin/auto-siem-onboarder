using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AutoSiem;
using AutoSiem.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;

namespace test_auth.Controllers
{
    // User needs to login to use the route in this controller 
    // This can be done other controllers too. And Should. TODO: add tag to all controllers before release. IMPORTANT!
    [Authorize] // all the routes in this controller are protected
    public class TicketController : Controller
    {
        // the database injected into this variable
        private readonly ApplicationDbContext _context;

        public TicketController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ticket
        // This is the main page of the website
        // Shows a table view of all tickets
        public async Task<IActionResult> Index()
        {
            return View(await _context.OnboardTickets.ToListAsync());
        }

        // GET: Ticket/Details/5
        // The detail page of each ticket 
        // This is where you see all the information about a particular ticket
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var onboardTicket = await _context.OnboardTickets
                .Include(x => x.Comments)
                .Include(x => x.Assignees)
                .Include(x => x.Creator)
                .Include(x => x.Platform)
                .Include(x => x.Platform.Nodes)
                .Include(x => x.Platform.Siem)
                .Include(x => x.Platform.Settings)
                .Where(x => x.Id == id).FirstOrDefaultAsync();
                // reason we include is because entity framework does not load by default to save performance.
                // hence to use them, include them else they are null!

            if (onboardTicket == null)
            {
                return NotFound();
            }

            return View(onboardTicket);
        }

        [HttpPost]
        public async Task<IActionResult> PostComment(Guid id, string message)
        {
            var onboardTicket = await _context.OnboardTickets
                .Include(x => x.Platform)
                .Include(x => x.Comments)
                .Where(x => x.Id == id).FirstOrDefaultAsync();

            if(onboardTicket == null)
                return RedirectToAction("Index");
            
            if(onboardTicket.Comments == null) 
                onboardTicket.Comments = new List<Comment>();

            
            // todo sanitise message string here
            
            onboardTicket.Comments.Add(new Comment{
                Created =  DateTime.Now,
                Message = message,
                Owner = GetCurrentUser().Name
            });

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new {id = id});
        }

        // GET: Ticket/Create
        // Returns to the view to ticket create
        // Views or Front-end can be found at the Views folder it correlates with the controller name as well 
        // E.g. this view file will be in Views/Tickets/Create.cshtml
        public IActionResult Create()
        {
            return View();
        }

        // Returns a view page to select a platform for the ticket
        public async Task<IActionResult> SelectPlatform(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var onboardTicket = await _context.OnboardTickets.FindAsync(id);
            if (onboardTicket == null)
            {
                return NotFound();
            }

            var platforms = _context.Platforms.ToList();

            ViewData["Id"] = id; // ViewData is used to pass data to the frontend
            ViewData["Platforms"] = platforms;

            return View(onboardTicket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectPlatform(Guid ticketId, Guid platformId)
        {
            var onboardTicket = await _context.OnboardTickets.FindAsync(ticketId);
            var platform = await _context.Platforms.FindAsync(platformId);
            if (onboardTicket == null) return NotFound();
            if (platform == null) return NotFound();

            onboardTicket.Platform = platform;
            await _context.SaveChangesAsync(); // attach a platform to the ticket

            return RedirectToAction("Details", new { id = ticketId });
        }

        public User GetCurrentUser()
        {
            return _context.MyUsers.Where(x => x.Email == User.Identity.Name).FirstOrDefault();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description")] OnboardTicket onboardTicket)
        {
            if (ModelState.IsValid)
            {
                // initialize new variables in the new ticket
                onboardTicket.Id = Guid.NewGuid();
                onboardTicket.Created = DateTime.Now;
                onboardTicket.Creator = GetCurrentUser();
                onboardTicket.Assignees = new List<User>();
                onboardTicket.Comments = new List<Comment>();
                onboardTicket.Platform = null;

                _context.Add(onboardTicket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(onboardTicket);
        }

        // This method sets the status of the ticket to closed
        public async Task<IActionResult> CloseTicket(Guid? id)
        {
            var onboardTicket = await _context.OnboardTickets.FindAsync(id);
            if (onboardTicket == null)
                return NotFound();

            onboardTicket.Closed = DateTime.Now;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = id });
        }

        // This method sets the status of the ticket to open / active
        public async Task<IActionResult> OpenTicket(Guid? id)
        {
            var onboardTicket = await _context.OnboardTickets.FindAsync(id);
            if (onboardTicket == null)
                return NotFound();

            onboardTicket.Closed = new DateTime();
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = id });
        }

        // Returns a  view to ticket edit page
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var onboardTicket = await _context.OnboardTickets.FindAsync(id);
            if (onboardTicket == null)
            {
                return NotFound();
            }
            return View(onboardTicket);
        }

        // POST: Ticket/Edit/5
        // Backend of how edit is handled
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Title,Description")] OnboardTicket onboardTicket)
        {
            if (id != onboardTicket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // update the ticket
                    _context.Update(onboardTicket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OnboardTicketExists(onboardTicket.Id))
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
            return View(onboardTicket);
        }

        // GET: Ticket/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var onboardTicket = await _context.OnboardTickets
                .FirstOrDefaultAsync(m => m.Id == id);
            if (onboardTicket == null)
            {
                return NotFound();
            }

            return View(onboardTicket);
        }

        // POST: Ticket/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var onboardTicket = await _context.OnboardTickets.FindAsync(id);
            _context.OnboardTickets.Remove(onboardTicket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OnboardTicketExists(Guid id)
        {
            return _context.OnboardTickets.Any(e => e.Id == id);
        }
    }
}
