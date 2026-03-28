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

        [HttpGet]
        public async Task<IActionResult> Analytics()
        {
            var totalEvents = await _context.Events.CountAsync();
            var completedEvents = await _context.Events.CountAsync(e => e.IsCompleted);
            var totalVolunteers = await _context.EventRegistrations.CountAsync();
            var checkedInCount = await _context.EventRegistrations.CountAsync(r => r.IsCheckedIn);

            var topRatedEvents = await _context.Events
                .Include(e => e.Comments)
                .OrderByDescending(e => e.Comments.Any() ? e.Comments.Average(c => c.Rating) : 0)
                .Take(3)
                .ToListAsync();

            var topVolunteers = await _context.EventRegistrations
                .GroupBy(r => r.UserId)
                .Select(g => new {
                    Email = _context.Users.Where(u => u.Id == g.Key).Select(u => u.Email).FirstOrDefault(),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalEvents = totalEvents;
            ViewBag.CompletedEvents = completedEvents;
            ViewBag.TotalVolunteers = totalVolunteers;
            ViewBag.CheckInRate = totalVolunteers > 0 ? (checkedInCount * 100 / totalVolunteers) : 0;
            ViewBag.TopVolunteers = topVolunteers;

            return View(topRatedEvents);
        }



        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            var reports = await _context.Reports.OrderByDescending(r => r.CreatedAt).ToListAsync();
            return View(reports);
        }

        [HttpPost]
        public async Task<IActionResult> DismissReport(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report != null)
            {
                _context.Reports.Remove(report);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Report dismissed successfully.";
            }
            return RedirectToAction(nameof(Reports));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReportedTarget(int id, int targetId, string targetType)
        {
            if (targetType == "Event")
            {
                var ev = await _context.Events.FindAsync(targetId);
                if (ev != null) _context.Events.Remove(ev);
            }
            else if (targetType == "Comment")
            {
                var comm = await _context.Comments.FindAsync(targetId);
                if (comm != null) _context.Comments.Remove(comm);
            }

            var report = await _context.Reports.FindAsync(id);
            if (report != null) _context.Reports.Remove(report);

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"The reported {targetType} and its associated report have been removed.";
            return RedirectToAction(nameof(Reports));
        }


        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Registrations).ThenInclude(r => r.User)
                .Include(e => e.Tasks).ThenInclude(t => t.AssignedUser)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null) return NotFound();
            return View(ev);
        }

        [HttpPost]
        public async Task<IActionResult> EditEvent(int id, Event updatedEvent, IFormFile? mediaFile)
        {
            if (id != updatedEvent.Id) return NotFound();

            var existingEvent = await _context.Events.FindAsync(id);
            if (existingEvent == null) return NotFound();

            existingEvent.Title = updatedEvent.Title;
            existingEvent.VideoUrl = updatedEvent.VideoUrl;

            if (mediaFile != null && mediaFile.Length > 0)
            {
                existingEvent.MediaFilePath = await SaveFile(mediaFile);
            }

            _context.Update(existingEvent);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Mission parameters updated successfully.";

            return RedirectToAction(nameof(EditEvent), new { id = id });
        }



        [HttpPost]
        public async Task<IActionResult> AssignTaskAdmin(int taskId, string? userId, int eventId)
        {
            var task = await _context.EventTasks.FindAsync(taskId);
            if (task != null)
            {
                task.AssignedUserId = string.IsNullOrEmpty(userId) ? null : userId;

                if (!string.IsNullOrEmpty(userId))
                {
                    await SendNotification(userId, $"Mission Assignment: You've been assigned the task '{task.Title}'.", $"/Home/Details/{eventId}", NotificationType.TaskAssignment);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Task assigned and volunteer notified!";
            }
            return RedirectToAction(nameof(EditEvent), new { id = eventId });
        }

        [HttpPost]
        public async Task<IActionResult> VerifyTask(int taskId)
        {
            var task = await _context.EventTasks.Include(t => t.Event).FirstOrDefaultAsync(t => t.Id == taskId);

            if (task != null && task.IsPendingVerification)
            {
                task.IsVerified = true;
                task.IsPendingVerification = false;

                await SendNotification(task.AssignedUserId, $"Verification Success! Your proof for '{task.Title}' was approved. +30 Eco-Points!", $"/Home/Details/{task.EventId}", NotificationType.TaskVerification);

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Task verified and points awarded!";
            }
            return RedirectToAction(nameof(EditEvent), new { id = task?.EventId });
        }

        [HttpPost]
        public async Task<IActionResult> AddTaskAdmin(int eventId, string title)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                _context.EventTasks.Add(new EventTask { EventId = eventId, Title = title });
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Task added to the management board!";
            }
            return RedirectToAction(nameof(EditEvent), new { id = eventId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTaskAdmin(int taskId, int eventId)
        {
            var task = await _context.EventTasks.FindAsync(taskId);
            if (task != null)
            {
                _context.EventTasks.Remove(task);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Task removed.";
            }
            return RedirectToAction(nameof(EditEvent), new { id = eventId });
        }



        [HttpGet]
        public async Task<IActionResult> DownloadCertificateAdmin(int eventId, string userId)
        {
            var ev = await _context.Events.FindAsync(eventId);
            var reg = await _context.EventRegistrations
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId && r.IsCheckedIn);

            if (reg == null || !ev.IsCompleted)
            {
                TempData["SuccessMessage"] = "Certificate is only available for attendees of completed missions.";
                return RedirectToAction("EditEvent", new { id = eventId });
            }

            ViewBag.UserEmail = _context.Users.Find(userId)?.Email;
            return View("CertificateTemplate", ev);
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

        [HttpPost]
        public async Task<IActionResult> CompleteEvent(int id, string impact)
        {
            var eventItem = await _context.Events.Include(e => e.Registrations).FirstOrDefaultAsync(e => e.Id == id);
            if (eventItem != null)
            {
                eventItem.IsCompleted = true;
                eventItem.ImpactSummary = impact;

                foreach (var reg in eventItem.Registrations.Where(r => r.IsCheckedIn))
                {
                    await SendNotification(reg.UserId, $"Mission Completed! Your Certificate of Contribution is ready.", "/Home/Profile", NotificationType.Achievement);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Mission closed. Certificates issued to attendees.";
            }
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> AddAnnouncement(int eventId, string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                _context.EventAnnouncements.Add(new EventAnnouncement { EventId = eventId, Message = message });

                var volunteers = await _context.EventRegistrations.Where(r => r.EventId == eventId).Select(r => r.UserId).ToListAsync();
                foreach (var vId in volunteers)
                {
                    await SendNotification(vId, $"Urgent Mission Update: {message}", $"/Home/Details/{eventId}", NotificationType.Announcement);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Announcement broadcasted!";
            }
            return RedirectToAction("Details", "Home", new { id = eventId });
        }

        [HttpPost]
        public async Task<IActionResult> UploadGalleryImages(int eventId, List<IFormFile> galleryFiles)
        {
            var eventItem = await _context.Events.FindAsync(eventId);
            if (eventItem == null) return NotFound();

            if (galleryFiles != null && galleryFiles.Count > 0)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "gallery");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                foreach (var file in galleryFiles)
                {
                    if (file.Length > 0)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string filePath = Path.Combine(uploadsFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create)) { await file.CopyToAsync(stream); }

                        _context.EventGalleryImages.Add(new EventGalleryImage { EventId = eventId, ImagePath = "/uploads/gallery/" + fileName });
                    }
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Mission gallery updated!";
            }
            return RedirectToAction("Details", "Home", new { id = eventId });
        }

   

        private async Task SendNotification(string? userId, string message, string? url, NotificationType type)
        {
            if (string.IsNullOrEmpty(userId)) return;
            _context.Notifications.Add(new Notification
            {
                UserId = userId,
                Message = message,
                LinkUrl = url,
                Type = type,
                CreatedAt = DateTime.UtcNow
            });
        }

        private async Task<string> SaveFile(IFormFile file)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string uploads = Path.Combine(wwwRootPath, "uploads");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
            using (var stream = new FileStream(Path.Combine(uploads, fileName), FileMode.Create)) { await file.CopyToAsync(stream); }
            return "/uploads/" + fileName;
        }
    }
}