using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ParkingManagement.Data;
using ParkingManagement.Models;

namespace ParkingManagement.Controllers
{
    public class SuperAdminController : Controller
    {
        ApplicationDbContext context;

        public SuperAdminController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (HttpContext.Session.GetString("SuperAdmin") == null)
            {
                context.Result = RedirectToAction("Index", "Website");
            }
        }
        public IActionResult Index()
        {
            //parkings 
            ViewBag.Spots = context.HiddenSpot.Where(x => x.verification == true).ToList().Count();

            //requested parking
            ViewBag.Requested = context.HiddenSpot.Where(x => x.verification == false).ToList().Count();

            //contact form
            ViewBag.Contact = context.Feedback.ToList().Count();

            //admin name
            string email = HttpContext.Session.GetString("SuperAdmin");

            var data = context.Admin.FirstOrDefault(x => x.email == email);

            ViewBag.Name = data.name;

            var feedbackdata = context.Feedback.ToList();
            return View(feedbackdata);
        }

        public IActionResult ReviewParking()
        {
            var data = context.HiddenSpot.ToList();
            return View(data);
        }

        public IActionResult DeleteRequest(int id)
        {
            var data = context.HiddenSpot.Find(id);
            context.HiddenSpot.Remove(data);
            context.SaveChanges();

            return RedirectToAction("Parking");
        }
        public IActionResult DeleteSpot(int id)
        {
            var data = context.HiddenSpot.Find(id);
            context.HiddenSpot.Remove(data);
            context.SaveChanges();

            var ownerdata = context.ParkingOwner.FirstOrDefault(x => x.parkingid == id);
            context.ParkingOwner.Remove(ownerdata);
            context.SaveChanges();

            return RedirectToAction("ReviewParking");
        }

        public IActionResult VerifyParking(int id)
        {
            //verifying Parking 
            var data = context.HiddenSpot.Find(id);
            data.verification = true;
            context.HiddenSpot.Update(data);
            context.SaveChanges();

            //creating admin/parking-owner account
            ParkingOwner x = new ParkingOwner();
            x.email = data.email;
            x.password = "Admin@123";
            x.parkingid = id;

            context.ParkingOwner.Add(x);
            context.SaveChanges();

            return RedirectToAction("ReviewParking");
        }

        public IActionResult Parking()
        {
            var data = context.HiddenSpot.ToList();
            return View(data);
        }

        public IActionResult ContactForm()
        {
            var data = context.Feedback.ToList();
            return View(data);
        }
        
        public IActionResult DeleteFeedback(int id)
        {
            var data = context.Feedback.Find(id);
            context.Feedback.Remove(data);
            context.SaveChanges();
            return RedirectToAction("ContactForm");
        }

        public IActionResult LogoutSuperAdmin()
        {
            HttpContext.Session.Remove("SuperAdmin");
            return RedirectToAction("Index", "Website");
        }
    }
}
