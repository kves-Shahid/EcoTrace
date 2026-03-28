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
        private readonly IWebHostEnvironment _hostEnvironment;

        public HomeController(AppDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .Include(e => e.Registrations)
                .Include(e => e.Comments)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return View(events);
        }

        public async Task<IActionResult> Details(int id)
        {
            var eventItem = await _context.Events
                .Include(e => e.Registrations).ThenInclude(r => r.User)
                .Include(e => e.Comments).ThenInclude(c => c.User)
                .Include(e => e.GalleryImages)
                .Include(e => e.Tasks).ThenInclude(t => t.AssignedUser)
                .Include(e => e.Announcements)
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

            await SendNotification(userId, "Welcome! You officially joined a new mission. (+10 Points)", $"/Home/Details/{eventId}", NotificationType.System);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Success! You have officially joined the mission. (+10 Eco-Points)";
            return RedirectToAction("Details", new { id = eventId });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CheckIn(int eventId)
        {
            var userId = _userManager.GetUserId(User);
            var registration = await _context.EventRegistrations
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);

            if (registration != null && !registration.IsCheckedIn)
            {
                var ev = await _context.Events.FindAsync(eventId);
                if (ev != null && ev.EventDate.Date <= DateTime.Now.Date)
                {
                    registration.IsCheckedIn = true;
                    registration.CheckInTime = DateTime.Now;

                    await SendNotification(userId, $"Great to see you! Check-in confirmed for {ev.Title}. (+50 Points)", $"/Home/Details/{eventId}", NotificationType.System);

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "You're checked in! Thank you for showing up. (+50 Eco-Points)";
                }
                else
                {
                    TempData["SuccessMessage"] = "Check-in is only available on the day of the mission.";
                }
            }
            return RedirectToAction("Details", new { id = eventId });
        }

       
        [Authorize]
        public IActionResult ScanQR()
        {
            return View();
        }

       
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ProcessQRCheckIn(int eventId)
        {
            var userId = _userManager.GetUserId(User);

           
            var registration = await _context.EventRegistrations
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);

            if (registration == null)
            {
                TempData["SuccessMessage"] = "You must Join the mission first before you can check in!";
                return RedirectToAction("Details", new { id = eventId });
            }

            if (registration.IsCheckedIn)
            {
                TempData["SuccessMessage"] = "You are already checked in!";
                return RedirectToAction("Details", new { id = eventId });
            }

            var ev = await _context.Events.FindAsync(eventId);
            if (ev != null && ev.EventDate.Date <= DateTime.Now.Date)
            {
                
                registration.IsCheckedIn = true;
                registration.CheckInTime = DateTime.Now;

              
                await SendNotification(userId, $"QR Scan Confirmed! Welcome to {ev.Title}. (+50 Points)", $"/Home/Details/{eventId}", NotificationType.System);

                await _context.SaveChangesAsync();

                
                TempData["SuccessMessage"] = "QR Check-In Successful! You earned 50 Eco-Points!";
            }
            else
            {
                TempData["SuccessMessage"] = "Check-in is only available on the day of the mission.";
            }

            return RedirectToAction("Details", new { id = eventId });
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ClaimTask(int taskId, int eventId)
        {
            var userId = _userManager.GetUserId(User);
            var task = await _context.EventTasks.FindAsync(taskId);

            if (task != null && string.IsNullOrEmpty(task.AssignedUserId))
            {
                task.AssignedUserId = userId;
                await SendNotification(userId, $"Task Claimed: '{task.Title}'. Remember to upload proof once finished!", $"/Home/Details/{eventId}", NotificationType.TaskAssignment);

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "You claimed a task! Step up and complete it.";
            }
            return RedirectToAction("Details", new { id = eventId });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UploadTaskProof(int taskId, IFormFile proofFile)
        {
            var userId = _userManager.GetUserId(User);
            var task = await _context.EventTasks.FirstOrDefaultAsync(t => t.Id == taskId && t.AssignedUserId == userId);

            if (task != null && proofFile != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(proofFile.FileName);
                string uploads = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "proofs");
                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                using (var stream = new FileStream(Path.Combine(uploads, fileName), FileMode.Create))
                {
                    await proofFile.CopyToAsync(stream);
                }

                task.ProofImagePath = "/uploads/proofs/" + fileName;
                task.IsPendingVerification = true;
                task.CompletedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Proof uploaded! Pending Admin verification.";
            }
            return RedirectToAction("Details", new { id = task?.EventId });
        }

        private async Task SendNotification(string? userId, string message, string? url, NotificationType type)
        {
            if (string.IsNullOrEmpty(userId)) return;

            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                LinkUrl = url,
                Type = type,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            _context.Notifications.Add(notification);
        }

        [Authorize]
        public async Task<IActionResult> Notifications()
        {
            var userId = _userManager.GetUserId(User);
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            foreach (var n in notifications) n.IsRead = true;
            await _context.SaveChangesAsync();

            return View(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            var userId = _userManager.GetUserId(User);
            var count = await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
            return Json(new { count = count });
        }


        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = _userManager.GetUserId(User);

            var registrations = await _context.EventRegistrations
                .Where(r => r.UserId == userId)
                .Include(r => r.Event)
                .ToListAsync();

            var verifiedTasks = await _context.EventTasks
                .Where(t => t.AssignedUserId == userId && t.IsVerified)
                .CountAsync();

            int totalScore = (registrations.Count * 10) +
                             (registrations.Count(r => r.IsCheckedIn) * 50) +
                             (verifiedTasks * 30);

            ViewBag.EcoScore = totalScore;
            ViewBag.MissionsJoined = registrations.Count;
            ViewBag.CheckIns = registrations.Count(r => r.IsCheckedIn);
            ViewBag.CompletedTasks = verifiedTasks;

            return View(registrations);
        }

        [Authorize]
        public async Task<IActionResult> DownloadCertificate(int eventId)
        {
            var userId = _userManager.GetUserId(User);
            var ev = await _context.Events.FindAsync(eventId);
            var reg = await _context.EventRegistrations
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId && r.IsCheckedIn);

            if (reg == null || !ev.IsCompleted)
            {
                TempData["SuccessMessage"] = "Certificate is only available for completed missions you attended.";
                return RedirectToAction("Profile");
            }

            return View("CertificateTemplate", ev);
        }

        public async Task<IActionResult> Leaderboard()
        {
            var users = await _userManager.Users.ToListAsync();
            var leaderboard = new List<VolunteerRank>();

            foreach (var user in users)
            {
                var regCount = await _context.EventRegistrations.CountAsync(r => r.UserId == user.Id);
                var checkInCount = await _context.EventRegistrations.CountAsync(r => r.UserId == user.Id && r.IsCheckedIn);
                var taskCount = await _context.EventTasks.CountAsync(t => t.AssignedUserId == user.Id && t.IsVerified);

                int totalScore = (regCount * 10) + (checkInCount * 50) + (taskCount * 30);

                if (totalScore > 0)
                {
                    leaderboard.Add(new VolunteerRank
                    {
                        Email = user.Email ?? "Anonymous",
                        Score = totalScore,
                        Missions = regCount
                    });
                }
            }

            return View(leaderboard.OrderByDescending(x => x.Score).ToList());
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
            var hasJoined = await _context.EventRegistrations.AnyAsync(r => r.EventId == id && r.UserId == userId);

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
            if (string.IsNullOrWhiteSpace(messageText)) return RedirectToAction("EventChat", new { id = eventId });

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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(int eventId, string text, int rating)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var comment = new Comment { EventId = eventId, UserId = userId, Text = text, Rating = rating, CreatedAt = DateTime.UtcNow };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = eventId });
        }

        public async Task<IActionResult> Search(string query)
        {
            var eventsQuery = _context.Events.Include(e => e.Registrations).Include(e => e.Comments).AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var lowerQuery = query.ToLower();
                eventsQuery = eventsQuery.Where(e =>
                    e.Title.ToLower().Contains(lowerQuery) || e.Description.ToLower().Contains(lowerQuery) ||
                    e.LocationName.ToLower().Contains(lowerQuery) || e.EventType.ToLower().Contains(lowerQuery));
            }

            var eventsList = await eventsQuery.ToListAsync();
            var sortedEvents = eventsList.OrderByDescending(e => e.Comments != null && e.Comments.Any() ? e.Comments.Average(c => c.Rating) : 0).ThenByDescending(e => e.CreatedAt).ToList();

            ViewBag.SearchQuery = query;
            return View("Index", sortedEvents);
        }
    }

    public class VolunteerRank
    {
        public string Email { get; set; } = "";
        public int Score { get; set; }
        public int Missions { get; set; }
    }
}