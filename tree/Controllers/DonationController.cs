using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using EcoTraceApp.Data;
using EcoTraceApp.Models;
using Microsoft.AspNetCore.Identity;

namespace EcoTraceApp.Controllers
{
    public class DonationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DonationController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index(int? eventId)
        {
            ViewBag.EventId = eventId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCheckoutSession(decimal amount, int? eventId)
        {
            var userId = _userManager.GetUserId(User);

            
            var donation = new Donation
            {
                Amount = amount,
                UserId = userId,
                EventId = eventId,
                Status = "Pending"
            };
            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();

            
            var domain = "https://localhost:7000"; 
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(amount * 100), 
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = eventId.HasValue ? "Mission Support Donation" : "EcoTrace Platform Donation",
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = domain + $"/Donation/Success?donationId={donation.Id}",
                CancelUrl = domain + "/Donation/Cancel",
            };

            var service = new SessionService();
            Session session = service.Create(options);

          
            donation.StripeSessionId = session.Id;
            await _context.SaveChangesAsync();

            
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        [HttpGet]
        public async Task<IActionResult> Success(int donationId)
        {
            
            var donation = await _context.Donations.FindAsync(donationId);
            if (donation != null)
            {
                donation.Status = "Completed";
                await _context.SaveChangesAsync();

            }
            return View();
        }

        [HttpGet]
        public IActionResult Cancel()
        {
            return View();
        }
    }
}