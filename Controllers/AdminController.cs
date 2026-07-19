using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ParkingManagement.Data;
using ParkingManagement.Models;
using System;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ParkingManagement.Controllers
{
    public class AdminController : Controller
    {
        ApplicationDbContext context;
        public AdminController(ApplicationDbContext context)
        {
            this.context = context;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (HttpContext.Session.GetString("Admin") == null)
            {
                context.Result = RedirectToAction("Index", "Website");
            }
        }
        public IActionResult Index()
        {
            var email = HttpContext.Session.GetString("Admin");
            ViewBag.AdminName = context.HiddenSpot.FirstOrDefault(x => x.email == email).ownername;

            var Admindata = context.ParkingOwner.FirstOrDefault(x => x.email == email);

            ViewBag.City = context.HiddenSpot.FirstOrDefault(x => x.email == email).city;
           
            string date = DateTime.Now.ToString("dd MMMM yyyy");

            try
            {
                ViewBag.Todayparking = context.DailyParking.Where(x => x.parkingid == Admindata.parkingid && x.date == date).ToList().Count();
                ViewBag.Totalparking = context.DailyParking.Where(x => x.parkingid == Admindata.parkingid).ToList().Count();
            }
            catch (Exception ex) 
            {
                ViewBag.Todayparking = 0;
                ViewBag.Totalparking = 0;
            }
            var a = context.HiddenSpot.FirstOrDefault(x => x.email == email);

            //GETTING PARKING DATA
            var parkingdata = context.DailyParking.Where(x => x.parkingid == a.id && x.amount != null).ToList();

            //CALCULATING TODAYS REVENUE
            int todayrevenue = 0;
            foreach(var vehicle in parkingdata)
            {
                if(vehicle.outdate == DateTime.Now.ToString("dd MMMM yyyy"))
                {
                    todayrevenue += int.Parse(vehicle.amount); 
                }
            }
            ViewBag.TodayRevenue = todayrevenue;

            //CALCULATING TOTAL REVENUE
            int totalrevenue = 0;
            foreach(var vehicle in parkingdata)
            {
               totalrevenue += int.Parse(vehicle.amount);         
            }
            ViewBag.TotalRevenue = totalrevenue;

            //AVAILABEL SPOTS 
            var daily = context.DailyParking.Where(x => x.parkingid == a.id && x.outtime == "-").ToList();

            ViewBag.Parkname = a.parkingname;
            if(a.type == "Bike")
            {
                var Vacant = a.bikespace;
                var Totalspace = int.Parse(a.bikespace.ToString()) + int.Parse(daily.Count().ToString());

                ViewBag.Avl = Vacant;
                ViewBag.Total = Totalspace;

                ViewBag.Percent = (daily.Count() / Totalspace) * 100;
            }
            else if(a.type == "Car")
            {
                var Vacant = a.carspace;
                var Totalspace = int.Parse(a.carspace.ToString()) + int.Parse(daily.Count().ToString());

                ViewBag.Avl = Vacant;
                ViewBag.Total = Totalspace;

                //ViewBag.Percent = (daily / Totalspace) * 100;
                ViewBag.Percent =   (daily.Count() * 100 )/ Totalspace ;
            }
            else if (a.type == "Both")
            {

            }


            //CALCULATING %INCRESE IN DAILY PARKING 
            
            var presentdate = DateTime.Now.ToString("dd MMMM yyyy");
            var pastdate = DateTime.Now.AddDays(-1).ToString("dd MMMM yyyy");

            int PresentDay = context.DailyParking.Where(x => x.parkingid == a.id && x.date == presentdate).ToList().Count();
            int PastDay = context.DailyParking.Where(x => x.parkingid == a.id && x.date == pastdate).ToList().Count();

            try
            {
                ViewBag.IncrementRate = (PresentDay * 100) / PastDay -100;
            }
            catch (Exception ex)
            {
                ViewBag.IncrementRate = 0;
                

            }

            return View();
        }

        public IActionResult ManageParking()
        {
            var email = HttpContext.Session.GetString("Admin");
            ViewBag.AdminName = context.HiddenSpot.FirstOrDefault(x => x.email == email).ownername;
            var Admindata = context.ParkingOwner.FirstOrDefault(x => x.email == email);

            var parkingData = context.HiddenSpot.Find(Admindata.parkingid);

            var a = context.HiddenSpot.FirstOrDefault(x => x.email == email);
            //AVAILABEL SPOTS 
            var daily = context.DailyParking.Where(x => x.parkingid == a.id && x.outtime == "-").ToList();



            if(a.type == "Bike")
            {
                var Totalspace = int.Parse(a.bikespace.ToString()) + int.Parse(daily.Count().ToString());
                parkingData.bikespace = Totalspace.ToString();     
            }
            else if (a.type == "Car")
            {
                var Totalspace = int.Parse(a.carspace.ToString()) + int.Parse(daily.Count().ToString());
                parkingData.carspace = Totalspace.ToString();

            }
            

            return View(parkingData);   
        }

        public IActionResult Logout ()
        {
            HttpContext.Session.Remove("Admin");
            return RedirectToAction("index", "Website");
        }

        public IActionResult ChangeAdminPassword()
        {
            var email = HttpContext.Session.GetString("Admin");
            ViewBag.AdminName = context.HiddenSpot.FirstOrDefault(x => x.email == email).ownername;
            return View();
        }
        public IActionResult VerifyChangePassword(IFormCollection form)
        {
            var email = HttpContext.Session.GetString("Admin");
            ViewBag.AdminName = context.HiddenSpot.FirstOrDefault(x => x.email == email).ownername;

            string oldpass = form["oldpass"];
            string newpass = form["newpass"];
            string confirmpass = form["confirmpass"];

            var data = context.ParkingOwner.FirstOrDefault(x => x.email == email); 

            if(confirmpass != newpass && confirmpass != null && newpass != null)
            {
                TempData["changepass"] = "Password AND Confirm Password did not Match";
                return RedirectToAction("ChangeAdminPassword");
            }
            else
            {
                if(oldpass == data.password)
                {
                    data.password = newpass;
                    context.ParkingOwner.Update(data);
                    context.SaveChanges();
                    return RedirectToAction("Index");

                }
                else
                {
                    TempData["changepass"] = "Old Password did not match Please enter Correct Password";
                    return RedirectToAction("ChangeAdminPassword");
                }
            }
        }

        public IActionResult ParkVehicle()
        {
            var email = HttpContext.Session.GetString("Admin");

            var data = context.HiddenSpot.FirstOrDefault(x => x.email == email);
            ViewBag.AdminName = data.ownername;

            ViewBag.Type = data.type;

            string date = DateTime.Now.ToString("dd MMMM yyyy");
            var parkingdata = context.DailyParking.Where(x => x.date.ToString() == date && x.parkingid == data.id).ToList();

            return View(parkingdata);
        }

        public IActionResult SaveParking(IFormCollection form)
        {
            var email = HttpContext.Session.GetString("Admin");
            var data = context.HiddenSpot.FirstOrDefault(x => x.email == email);

            //updating availablle spots
            if(data.carspace == "NOT Available")
            {
                data.bikespace = (int.Parse(data.bikespace)-1).ToString();
            }
            else if(data.bikespace == "NOT Available")
            {
                data.carspace = (int.Parse(data.carspace) - 1).ToString();
            }

                string name = form["name"];
            string number = form["number"];
            string type = form["type"];

            string dt = DateTime.Now.ToString("dd MMMM yyyy HH:mm:ss");    
            string date = DateTime.Now.ToString("dd MMMM yyyy");    
            string time = DateTime.Now.ToString("HH:mm:ss");

            var parkdata = context.DailyParking.FirstOrDefault(x => x.vehiclenumber == number && x.outtime == "-");
            if(parkdata == null)
            { 
                dailyParking x = new dailyParking
                {
                    ownername = name,
                    vehiclenumber = number,
                    type = type,
                    date = date,
                    intime = time,
                    datetime = dt,
                    outtime = "-",
                    parkingid = data.id,
                    outdate = "-"
                
                };

                context.DailyParking.Add(x);
                context.SaveChanges();

                TempData["saved"] = number.ToString();
 
                return RedirectToAction("ParkVehicle");
            }
            else
            {
                TempData["notsaved"] = "Please Enter a Valid Vehicle number";
                return RedirectToAction("ParkVehicle");
            }
        }
        public IActionResult Parkhistory()
        {
            var email = HttpContext.Session.GetString("Admin");

            var data = context.HiddenSpot.FirstOrDefault(x => x.email == email);
            ViewBag.AdminName = data.ownername;

            var parkingdata = context.DailyParking.Where(x => x.parkingid == data.id).ToList();

            ViewBag.Type = data.type;
            return View(parkingdata);
        }

        public IActionResult ParkReceipt(int id)
        {
            var email = HttpContext.Session.GetString("Admin");

            var spotdata = context.HiddenSpot.FirstOrDefault(x => x.email == email);
            ViewBag.AdminName = spotdata.ownername;

            var data = context.DailyParking.Find(id);
            if(data.outtime == "-")
            {
                if(spotdata.type == "Bike")
                {
                    ViewBag.Hour = spotdata.hourrate;
                }
                else if(spotdata.type == "Car")
                {
                    ViewBag.Hour = spotdata.hourrate;
                }
                else if(data.type == "Both")
                {
                    ViewBag.Hour = spotdata.hourrate;
                }
                    return View(data);
            }
            else
            {   
                return RedirectToAction("PrintBill");
            }
        }
        public IActionResult PrintBill(int id)
        {
            var email = HttpContext.Session.GetString("Admin");

            var spotdata = context.HiddenSpot.FirstOrDefault(x => x.email == email);
            ViewBag.AdminId = spotdata.id;
            ViewBag.AdminName = spotdata.parkingname;

            var data = context.DailyParking.Find(id);
            
            return View(data);
        }

        public IActionResult ExitVehicle(int id)
        {
            string time = DateTime.Now.ToString("dd MMMM yyyy HH:mm:ss");

            //GETTING SESSION 
            var email = HttpContext.Session.GetString("Admin");

            //RECEIVING DATA FROM DATABASE OF PARTICULAR PARKED VEHICLE
            var data = context.DailyParking.Find(id);
            //SETTING OUT DATE AND TIME 
            data.outtime = time;
            data.outdate = DateTime.Now.ToString("dd MMMM yyyy");

            //GETTING DATA OF PARKING OWNER
            var admindata = context.HiddenSpot.FirstOrDefault(x => x.email == email);
            
            //UPDATING AVAILABLE SPOTS
            if (admindata.carspace == "NOT Available")
            {
                admindata.bikespace = (int.Parse(admindata.bikespace) + 1).ToString();
                context.HiddenSpot.Update(admindata);
                context.SaveChanges();
            }
            else if (admindata.bikespace == "NOT Available")
            {
                admindata.carspace = (int.Parse(admindata.carspace) + 1).ToString();
                context.HiddenSpot.Update(admindata);
                context.SaveChanges();
            }

            //CALCULATING THE AMOUNT TO BE PAID BY VEHICLE OWNER
            string hourrate = admindata.hourrate;

            // Parse the "HH:mm:ss" strings back to DateTime
            DateTime dt1 = DateTime.ParseExact(time, "dd MMMM yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime dt2 = DateTime.ParseExact(data.datetime, "dd MMMM yyyy HH:mm:ss", CultureInfo.InvariantCulture);

            // Subtract to get a TimeSpan
            TimeSpan diff = dt1 - dt2;

            data.amount = (Math.Ceiling(diff.TotalHours) * Convert.ToDouble(hourrate)).ToString();

            //UPDATING THE CHANGES DONE
            context.DailyParking.Update(data);
            context.SaveChanges();

            TempData["parkid"] = id.ToString();
            
            // Redirect to PrintBill and pass id as a query string
            var printUrl = Url.Action("PrintBill", "Admin") ?? "/Admin/PrintBill";
            return Redirect(printUrl + $"?id={id}");
        }

        public IActionResult deleteparkhistory(int id)
        {
            //GETTING SESSION 
            var email = HttpContext.Session.GetString("Admin");

            var data = context.DailyParking.Find(id);

            //GETTING DATA OF PARKING OWNER
            var admindata = context.HiddenSpot.FirstOrDefault(x => x.email == email);

            //GETTING PARKING DATA
            

            //UPDATING AVAILABLE SPOTS
            if (admindata.carspace == "NOT Available" && data.outtime == "-")
            {
                admindata.bikespace = (int.Parse(admindata.bikespace) + 1).ToString();
                context.HiddenSpot.Update(admindata);
                context.SaveChanges();
            }
            else if (admindata.bikespace == "NOT Available" && data.outtime == "-")
            {
                admindata.carspace = (int.Parse(admindata.carspace) + 1).ToString();
                context.HiddenSpot.Update(admindata);
                context.SaveChanges();
            }

            context.DailyParking.Remove(data);
            context.SaveChanges();
            return RedirectToAction("Parkhistory");
        }



    }
}
