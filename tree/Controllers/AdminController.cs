using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcoTraceApp.Data;
using EcoTraceApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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

        // Fixes the buffering/loading error by using ToListAsync
        public async Task<IActionResult> Dashboard()
        {
            var events = await _context.Events.ToListAsync();
            return View(events);
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

        // NEW: Edit Event Logic
        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null) return NotFound();
            return View(eventItem);
        }

        [HttpPost]
        public async Task<IActionResult> EditEvent(Event model, IFormFile? mediaFile)
        {
            ModelState.Remove("CreatorId");
            ModelState.Remove("Creator");

            if (ModelState.IsValid)
            {
                if (mediaFile != null) model.MediaFilePath = await SaveFile(mediaFile);
                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Dashboard");
            }
            return View(model);
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