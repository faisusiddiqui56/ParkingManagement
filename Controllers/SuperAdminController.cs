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

        public IActionResult Settings()
        {
            string email = HttpContext.Session.GetString("SuperAdmin");
            if (string.IsNullOrEmpty(email)) return RedirectToAction("SuperAdminLogin", "Website");

            var cleanEmail = email.Trim().ToLower();
            var data = context.Admin.FirstOrDefault(x => x.email != null && x.email.Trim().ToLower() == cleanEmail);

            if (data == null)
            {
                return RedirectToAction("SuperAdminLogin", "Website");
            }

            ViewBag.Name = data.name;
            return View(data);
        }

        [HttpPost]
        public IActionResult UpdateSettings(IFormCollection form)
        {
            string email = HttpContext.Session.GetString("SuperAdmin");
            if (string.IsNullOrEmpty(email)) return RedirectToAction("SuperAdminLogin", "Website");

            var cleanEmail = email.Trim().ToLower();
            var data = context.Admin.FirstOrDefault(x => x.email != null && x.email.Trim().ToLower() == cleanEmail);

            if (data == null)
            {
                return RedirectToAction("SuperAdminLogin", "Website");
            }

            string name = form["name"];
            string oldpass = form["oldpass"];
            string newpass = form["newpass"];
            string confirmpass = form["confirmpass"];

            if (!string.IsNullOrWhiteSpace(name))
            {
                data.name = name.Trim();
            }

            if (!string.IsNullOrEmpty(oldpass) || !string.IsNullOrEmpty(newpass) || !string.IsNullOrEmpty(confirmpass))
            {
                if (oldpass != data.password)
                {
                    TempData["SettingsError"] = "Current Password is Incorrect.";
                    return RedirectToAction("Settings");
                }

                if (newpass != confirmpass)
                {
                    TempData["SettingsError"] = "New Password and Confirm Password do not match.";
                    return RedirectToAction("Settings");
                }

                if (string.IsNullOrWhiteSpace(newpass) || newpass.Length < 4)
                {
                    TempData["SettingsError"] = "New Password must be at least 4 characters long.";
                    return RedirectToAction("Settings");
                }

                data.password = newpass;
            }

            context.Admin.Update(data);
            context.SaveChanges();

            TempData["SettingsSuccess"] = "Settings updated successfully!";
            return RedirectToAction("Settings");
        }

        public IActionResult LogoutSuperAdmin()
        {
            HttpContext.Session.Remove("SuperAdmin");
            return RedirectToAction("Index", "Website");
        }
    }
}
