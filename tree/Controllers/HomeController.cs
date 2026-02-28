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

        [Authorize]
        public async Task<IActionResult> MyMissions()
        {
            var userId = _userManager.GetUserId(User);

            var joinedEvents = await _context.EventRegistrations
                .Where(r => r.UserId == userId)
                .Include(r => r.Event)
                .OrderByDescending(r => r.RegistrationDate)
                .ToListAsync();

            return View(joinedEvents);
        }

    

        [Authorize]
        public async Task<IActionResult> EventChat(int id)
        {
            var userId = _userManager.GetUserId(User);

            
            var hasJoined = await _context.EventRegistrations
                .AnyAsync(r => r.EventId == id && r.UserId == userId);

            if (!hasJoined && !User.IsInRole("Admin"))
            {
                TempData["SuccessMessage"] = "You must join the event to enter the community chat.";
                return RedirectToAction("Details", new { id = id });
            }

            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null) return NotFound();

            
            var messages = await _context.ChatMessages
                .Include(c => c.User)
                .Where(c => c.EventId == id)
                .OrderBy(c => c.Timestamp)
                .ToListAsync();

            ViewBag.EventTitle = eventItem.Title;
            ViewBag.EventId = id;
            ViewBag.CurrentUserId = userId;

            return View(messages);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SendMessage(int eventId, string messageText)
        {
            if (string.IsNullOrWhiteSpace(messageText))
                return RedirectToAction("EventChat", new { id = eventId });

            var userId = _userManager.GetUserId(User);

            var chatMessage = new ChatMessage
            {
                EventId = eventId,
                UserId = userId ?? "",
                MessageText = messageText,
                Timestamp = DateTime.Now
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            return RedirectToAction("EventChat", new { id = eventId });
        }
    }
}