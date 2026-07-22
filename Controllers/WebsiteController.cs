using Microsoft.AspNetCore.Mvc;
using ParkingManagement.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ParkingManagement.Data;


namespace ParkingManagement.Controllers
{
    public class WebsiteController : Controller
    {
        ApplicationDbContext context;
        IWebHostEnvironment env;
        public WebsiteController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            this.context = context;
            this.env = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddParking()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddParking(HiddenSpot data,IFormFile photo)
        {
            string ext = Path.GetExtension(photo.FileName);

            string filename = Guid.NewGuid().ToString();
            filename += ext;
            string folderpath = Path.Combine(env.WebRootPath, "Parkings");
            string filepath = Path.Combine(folderpath, filename);
            var file = new FileStream(filepath, FileMode.Create);

            await photo.CopyToAsync(file);

            if(photo == null)
            {
                data.photo = "photo";
            }
            else
            {
                data.photo = filename;
            }
            data.verification = false;

            if(data.type == "Bike")
            {
                data.carspace = "NOT Available";
            }
            else if(data.type == "Car")
            {
                data.bikespace = "NOT Available";
            }
            var existeddata = context.HiddenSpot.FirstOrDefault(x => x.email == data.email && x.mobile == data.mobile);
            if(existeddata != null)
            {
                TempData["ParkEmailExist"] = "Email OR Mobile Given is Already registered in our database";
                return RedirectToAction("AddParking");
            }
            else
            {
                context.HiddenSpot.Add(data);
                context.SaveChanges();
                TempData["ReqSent"] = "Request Sent Successfully";
                return RedirectToAction("AddParking");
            }


        }

        public IActionResult FindParking()
        {
            

            var data = context.HiddenSpot.ToList();
            return View(data);
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        public IActionResult AdminLogin()
        {
            return View();
        }

        [HttpPost]
        public IActionResult VerifyAdmin(ParkingOwner data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.email) || string.IsNullOrWhiteSpace(data.password))
            {
                TempData["LoginError"] = "Please enter both Email and Password";
                return RedirectToAction("AdminLogin");
            }

            var inputEmail = data.email.Trim().ToLower();
            var olddata = context.ParkingOwner.FirstOrDefault(x => x.email != null && x.email.Trim().ToLower() == inputEmail && x.password == data.password);

            if (olddata != null) 
            {
                HttpContext.Session.SetString("Admin", olddata.email);
                return RedirectToAction("Index", "Admin");
            }

            // Fallback check if user entered SuperAdmin credentials on AdminLogin page
            var superAdminData = context.Admin.FirstOrDefault(x => x.email != null && x.email.Trim().ToLower() == inputEmail && x.password == data.password);
            if (superAdminData != null)
            {
                HttpContext.Session.SetString("SuperAdmin", superAdminData.email);
                return RedirectToAction("Index", "SuperAdmin");
            }

            TempData["LoginError"] = "Email OR Password is Incorrect";
            return RedirectToAction("AdminLogin");
        }
        public IActionResult SuperAdminLogin()
        {
            return View();
        }

        [HttpPost]
        public IActionResult VerifySuperAdmin(Admin data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.email) || string.IsNullOrWhiteSpace(data.password))
            {
                TempData["LoginError"] = "Please enter both Email and Password";
                return RedirectToAction("SuperAdminLogin");
            }

            var inputEmail = data.email.Trim().ToLower();
            var olddata = context.Admin.FirstOrDefault(x => x.email != null && x.email.Trim().ToLower() == inputEmail && x.password == data.password);

            if (olddata != null) 
            {
                HttpContext.Session.SetString("SuperAdmin", olddata.email);
                return RedirectToAction("Index", "SuperAdmin");
            }
            else
            {
                TempData["LoginError"] = "Email OR Password is Incorrect";
                return RedirectToAction("SuperAdminLogin");
            }
        }

        [HttpPost]
        public IActionResult SaveFeedback(IFormCollection form)
        {
            string name = form["name"];
            string email = form["email"];
            string mobile = form["mobile"];
            string message = form["message"];

            Feedback review = new Feedback
            {
                name = name,
                email = email,
                mobile = mobile,
                message = message,
                date = DateTime.Now.ToString("dd MMMM yyyy")
            };

            context.Feedback.Add(review);
            context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult ContactUs()
        {
            return View();
        }
    }
}
