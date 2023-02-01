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
    public class PlatformController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlatformController(ApplicationDbContext context)
        {
            _context = context; // depdency injection of database to this controller
        }

        // GET: Platform
        // returns the page that has all the platforms
        public async Task<IActionResult> Index()
        {
            return View(await _context.Platforms.ToListAsync());
        }

        // GET: Platform/Details/5
        // detail view of a particular platform
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Platform platform = await _context.Platforms
                .Include(x => x.SMEs)
                .Include(x => x.Owners)
                .Include(x => x.Siem)
                .Include(x => x.Settings)
                .Include(x => x.Nodes)
                .Where(x => x.Id == id).FirstOrDefaultAsync();

            if (platform == null)
            {
                return NotFound();
            }
            return View(platform);
        }

        // GET: Platform/Create
        // page used to fill out the creation of new platform
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Division,Category,Description")] Platform platform)
        {
            if (ModelState.IsValid) // validate model against the object rules
            {
                platform.Id = Guid.NewGuid();
                platform.Nodes = new List<Node>();
                platform.SMEs = new List<User>();
                platform.Owners = new List<User>(); 

                _context.Add(platform); // add to database 
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // return to the main page of this controller
            }
            return View(platform);
        }

        // GET: Platform/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
                return NotFound();

            Platform platform = await _context.Platforms.FindAsync(id); // find the platform in database

            if (platform == null)
                return NotFound();

            return View(platform); // populate the front-end
        }

        // POST: Platform/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Division,Category,Description")] Platform platform)
        {
            // return not found if id doesnt exist
            if (id != platform.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(platform); // update the database if the validation passes
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) // avoid concurrency exception. Rare.
                {
                    if (!PlatformExists(platform.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index)); // return to platform listing view
            }

            return View(platform); // if validation fail, return back to edit page and tell user
        }

        // GET: Platform/Delete/5
        // will return a deletion confirmation
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Platform platform = await _context.Platforms
                .FirstOrDefaultAsync(m => m.Id == id);

            if (platform == null)
            {
                return NotFound();
            }

            return View(platform);
        }

        // POST: Platform/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        // simple find and delete of a platform
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            Platform platform = await _context.Platforms.FindAsync(id);
            _context.Platforms.Remove(platform);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Return the views used to add automation settings to the platform
        public async Task<IActionResult> AddSettings(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Platform platform = await _context.Platforms.FindAsync(id);
            if (platform == null)
            {
                return NotFound();
            }

            List<Siem> siems = _context.Siems.ToList();
            ViewData["Siems"] = siems;
            ViewData["Id"] = id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSettings(ScriptSettings settings)
        {
            Guid platformId = settings.Id; // the id passed in is actually the platform id
            settings.Id = Guid.NewGuid(); // create new guid, this time for the script settings 

            if (!PlatformExists(platformId))
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
                return View(settings); 


            Platform platform = await _context.Platforms.FindAsync(platformId);

            // if there is an existing setting already, remove it. Because we are trying to add new one.
            if (platform.Settings != null)
            {
                _context.ScriptSettings.Remove(platform.Settings);
            }

            _context.ScriptSettings.Add(settings); // add new settings.
            await _context.SaveChangesAsync(); // save database

            return RedirectToAction("Details", new { id = platformId }); //  redirect to previous page it navigated from
        }

        // returns the view for attaching a siem to a platform
        public async Task<IActionResult> AddSiem(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Platform  platform = await _context.Platforms.FindAsync(id);
            if (platform == null)
            {
                return NotFound();
            }

            List<Siem> siems = _context.Siems.ToList();
            ViewData["Siems"] = siems;
            ViewData["Id"] = id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSiem(Guid id, Guid siemId)
        {
            if (!PlatformExists(id)) // check to see platform exists
            {
                return NotFound();
            }

            if (!SiemExists(siemId)) // check to see siem exists
            {
                return NotFound();
            }

            Platform platform = await _context.Platforms.FindAsync(id);
            Siem siem = await _context.Siems.FindAsync(siemId);

            platform.Siem = siem; // add to siem to platform
            await _context.SaveChangesAsync(); // save database

            return RedirectToAction("Details", new { id = id }); 
        }

        public async Task<IActionResult> AddOwner(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Platform platform = await _context.Platforms.FindAsync(id);
            if (platform == null)
            {
                return NotFound();
            }

            List<User> users = _context.MyUsers.ToList();
            ViewData["Users"] = users; // pass users object to the front end
            ViewData["Id"] = id; // pass id object to the front end
            return View();
        }

        public async Task<IActionResult> AddSme(Guid? id)
        {
            if (id == null)
                return NotFound();

            Platform platform = await _context.Platforms.FindAsync(id);
            if (platform == null)
                return NotFound();

            List<User> users = _context.MyUsers.ToList();
            ViewData["Users"] = users;
            ViewData["Id"] = id;
            return View();
        }

        public async Task<IActionResult> RemoveSme(Guid? id, Guid smeId)
        {
            Platform platform = await _context.Platforms
                .Include(x => x.SMEs)
                .Where(x => x.Id == id).FirstOrDefaultAsync(); 

            if (platform == null)
                return NotFound();

            try
            {
               platform.SMEs.Remove(platform.SMEs.Single(x => x.Id == smeId)); // find the right sme, then use that object to remove in the list.
            }
            catch(Exception e)
            {
                return BadRequest();
            }

            await _context.SaveChangesAsync(); 
            return RedirectToAction("Details", new { id = id });
        }
        public async Task<IActionResult> RemoveNode(Guid? id, Guid nodeId)
        {
            Platform platform = await _context.Platforms
            .Include(x => x.Nodes)
            .Where(x => x.Id == id).FirstOrDefaultAsync();

            if (platform == null)
                return NotFound();

            try
            {
                // remove the specified node
                platform.Nodes.Remove(platform.Nodes.Single(x => x.Id == nodeId)); // find the right node object then use that obj to remove in the list
            }
            catch(Exception e)
            {
                return BadRequest();
            }

            await _context.SaveChangesAsync(); 
            return RedirectToAction("Details", new { id = id });
        }

        public async Task<IActionResult> RemoveOwner(Guid? id, Guid ownerId)
        {
            Platform platform = await _context.Platforms
            .Include(x => x.Owners)
            .Where(x => x.Id == id).FirstOrDefaultAsync();

            if (platform == null)
                return NotFound();

            try
            {
                platform.Owners.Remove(platform.Owners.Single(x => x.Id == ownerId));
            }
            catch(Exception e)
            {
                return BadRequest();
            }
            
            await _context.SaveChangesAsync(); 
            return RedirectToAction("Details", new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSme(Guid platformId, Guid userId, string type)
        {
            if (!PlatformExists(platformId))
            {
                return NotFound();
            }

            if (!UserExists(userId))
            {
                return NotFound();
            }

            Platform platform = await _context.Platforms.FindAsync(platformId);
            User user = await _context.MyUsers.FindAsync(userId);

            // determine where to add user to 
            // default case is bad request, you got to specify what for
            // owner: add user to owner group
            // sme: add user to sme group

            if (platform.Owners == null)
                platform.Owners = new List<User>();

            if (platform.SMEs == null)
                platform.SMEs = new List<User>();

            switch (type)
            {
                default:
                    BadRequest();
                    break;

                case "owner":
                    if (!platform.Owners.Any(x => x.Id == user.Id)) // checks to avoid adding duplicates
                        platform.Owners.Add(user);
                    break;

                case "sme":
                    if (!platform.SMEs.Any(x => x.Id == user.Id))  // checks to avoid adding duplicates
                        platform.SMEs.Add(user);
                    break;
            }

            await _context.SaveChangesAsync(); // save database

            return RedirectToAction("Details", new { id = platformId }); // return to the detail page of the particular platform
        }

        // helper function to check if platform exists
        private bool PlatformExists(Guid id)
        {
            return _context.Platforms.Any(e => e.Id == id);
        }

        // helper function to check if a user exists
        private bool UserExists(Guid id)
        {
            return _context.MyUsers.Any(e => e.Id == id);
        }

        // helper function to check if a Siem exists
        private bool SiemExists(Guid id)
        {
            return _context.Siems.Any(e => e.Id == id);
        }

    }
}
