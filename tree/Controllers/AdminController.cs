using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcoTraceApp.Data;
using EcoTraceApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EcoTraceApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _hostEnvironment;

        public AdminController(AppDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> Dashboard()
        {
            var events = await _context.Events.Include(e => e.Registrations).ToListAsync();
            return View(events);
        }

        // NEW PHASE 2: Complete Mission Logic
        [HttpPost]
        public async Task<IActionResult> CompleteEvent(int id, string impact)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem != null)
            {
                eventItem.IsCompleted = true;
                eventItem.ImpactSummary = impact;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Mission Success! Impact results updated.";
            }
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public IActionResult CreateEvent() => View();

        [HttpPost]
        public async Task<IActionResult> CreateEvent(Event model, IFormFile? mediaFile)
        {
            var userId = _userManager.GetUserId(User);
            ModelState.Remove("CreatorId");
            ModelState.Remove("Creator");
            ModelState.Remove("Registrations");

            if (ModelState.IsValid)
            {
                if (mediaFile != null) model.MediaFilePath = await SaveFile(mediaFile);
                model.CreatorId = userId ?? "";
                _context.Events.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Dashboard");
            }
            return View(model);
        }

        public async Task<IActionResult> EventVolunteers(int id)
        {
            var eventItem = await _context.Events
                .Include(e => e.Registrations).ThenInclude(r => r.User)
                .FirstOrDefaultAsync(e => e.Id == id);
            return View(eventItem);
        }

        private async Task<string> SaveFile(IFormFile file)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string uploads = Path.Combine(wwwRootPath, "uploads");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
            using (var stream = new FileStream(Path.Combine(uploads, fileName), FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return "/uploads/" + fileName;
        }
    }
}