using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcoTraceApp.Data;
using EcoTraceApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace EcoTraceApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _context.Events.OrderByDescending(e => e.CreatedAt).ToListAsync();
            return View(events);
        }

        public async Task<IActionResult> Details(int id)
        {
            // CRITICAL: Include Registrations to check the 'Join' status in the view
            var eventItem = await _context.Events
                .Include(e => e.Registrations)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null) return NotFound();
            return View(eventItem);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> JoinEvent(int eventId)
        {
            var userId = _userManager.GetUserId(User);

            var alreadyJoined = await _context.EventRegistrations
                .AnyAsync(r => r.EventId == eventId && r.UserId == userId);

            if (alreadyJoined)
            {
                TempData["SuccessMessage"] = "You have already joined this event!";
                return RedirectToAction("Details", new { id = eventId });
            }

            var registration = new EventRegistration
            {
                EventId = eventId,
                UserId = userId ?? "",
                RegistrationDate = DateTime.Now
            };

            _context.EventRegistrations.Add(registration);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Success! You have officially joined the mission.";
            return RedirectToAction("Details", new { id = eventId });
        }
    }
}