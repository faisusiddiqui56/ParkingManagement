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
                context.Result = RedirectToAction("AdminLogin", "Website");
            }
        }
        private HiddenSpot? GetCurrentHiddenSpot()
        {
            var email = HttpContext.Session.GetString("Admin");
            if (string.IsNullOrEmpty(email)) return null;

            var cleanEmail = email.Trim().ToLower();
            var admindata = context.ParkingOwner.FirstOrDefault(x => x.email != null && x.email.Trim().ToLower() == cleanEmail);
            if (admindata == null) return null;

            return GetOrCreateHiddenSpot(admindata);
        }

        private HiddenSpot GetOrCreateHiddenSpot(ParkingOwner admindata)
        {
            var cleanEmail = admindata.email?.Trim().ToLower();
            int parkingId = admindata.parkingid ?? 0;

            var spot = context.HiddenSpot.FirstOrDefault(x => (parkingId > 0 && x.id == parkingId) || (x.email != null && x.email.Trim().ToLower() == cleanEmail));
            
            if (spot == null)
            {
                spot = new HiddenSpot
                {
                    ownername = !string.IsNullOrEmpty(admindata.email) ? admindata.email.Split('@')[0] : "Admin",
                    email = admindata.email,
                    parkingname = "My Parking Spot",
                    type = "Car",
                    carspace = "10",
                    bikespace = "10",
                    hourrate = "20",
                    verification = true
                };

                context.HiddenSpot.Add(spot);
                context.SaveChanges();

                admindata.parkingid = spot.id;
                context.ParkingOwner.Update(admindata);
                context.SaveChanges();
            }
            else if (admindata.parkingid != spot.id)
            {
                admindata.parkingid = spot.id;
                context.ParkingOwner.Update(admindata);
                context.SaveChanges();
            }

            return spot;
        }

        public IActionResult Index()
        {
            var email = HttpContext.Session.GetString("Admin");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("AdminLogin", "Website");
            }

            var cleanEmail = email.Trim().ToLower();
            var Admindata = context.ParkingOwner.FirstOrDefault(x => x.email != null && x.email.Trim().ToLower() == cleanEmail);
            if (Admindata == null)
            {
                HttpContext.Session.Remove("Admin");
                TempData["LoginError"] = "Admin account record not found.";
                return RedirectToAction("AdminLogin", "Website");
            }

            var a = GetOrCreateHiddenSpot(Admindata);

            ViewBag.AdminName = a.ownername ?? "Admin";
            ViewBag.City = a.city ?? "";
           
            string date = DateTime.Now.ToString("dd MMMM yyyy");

            try
            {
                ViewBag.Todayparking = context.DailyParking.Where(x => x.parkingid == a.id && x.date == date).Count();
                ViewBag.Totalparking = context.DailyParking.Where(x => x.parkingid == a.id).Count();
            }
            catch (Exception ex) 
            {
                ViewBag.Todayparking = 0;
                ViewBag.Totalparking = 0;
            }

            //GETTING PARKING DATA
            var parkingdata = context.DailyParking.Where(x => x.parkingid == a.id && x.amount != null).ToList();

            //CALCULATING TODAYS REVENUE
            int todayrevenue = 0;
            foreach(var vehicle in parkingdata)
            {
                if(vehicle.outdate == DateTime.Now.ToString("dd MMMM yyyy"))
                {
                    if (int.TryParse(vehicle.amount, out int amt))
                    {
                        todayrevenue += amt; 
                    }
                }
            }
            ViewBag.TodayRevenue = todayrevenue;

            //CALCULATING TOTAL REVENUE
            int totalrevenue = 0;
            foreach(var vehicle in parkingdata)
            {
                if (int.TryParse(vehicle.amount, out int amt))
                {
                    totalrevenue += amt;         
                }
            }
            ViewBag.TotalRevenue = totalrevenue;

            //AVAILABLE SPOTS 
            var daily = context.DailyParking.Where(x => x.parkingid == a.id && x.outtime == "-").ToList();

            ViewBag.Parkname = a.parkingname;
            if(a.type == "Bike")
            {
                int.TryParse(a.bikespace, out int vacant);
                int totalSpace = vacant + daily.Count;

                ViewBag.Avl = vacant;
                ViewBag.Total = totalSpace;

                ViewBag.Percent = totalSpace > 0 ? (daily.Count * 100) / totalSpace : 0;
            }
            else if(a.type == "Car")
            {
                int.TryParse(a.carspace, out int vacant);
                int totalSpace = vacant + daily.Count;

                ViewBag.Avl = vacant;
                ViewBag.Total = totalSpace;

                ViewBag.Percent = totalSpace > 0 ? (daily.Count * 100) / totalSpace : 0;
            }
            else
            {
                ViewBag.Avl = 0;
                ViewBag.Total = daily.Count;
                ViewBag.Percent = 0;
            }


            //CALCULATING %INCREASE IN DAILY PARKING 
            var presentdate = DateTime.Now.ToString("dd MMMM yyyy");
            var pastdate = DateTime.Now.AddDays(-1).ToString("dd MMMM yyyy");

            int PresentDay = context.DailyParking.Where(x => x.parkingid == a.id && x.date == presentdate).ToList().Count();
            int PastDay = context.DailyParking.Where(x => x.parkingid == a.id && x.date == pastdate).ToList().Count();

            try
            {
                ViewBag.IncrementRate = PastDay > 0 ? ((PresentDay * 100) / PastDay - 100) : 0;
            }
            catch (Exception ex)
            {
                ViewBag.IncrementRate = 0;
            }

            return View();
        }

        public IActionResult ManageParking()
        {
            var parkingData = GetCurrentHiddenSpot();
            if (parkingData == null) return RedirectToAction("AdminLogin", "Website");

            ViewBag.AdminName = parkingData.ownername ?? "Admin";

            //AVAILABLE SPOTS 
            var daily = context.DailyParking.Where(x => x.parkingid == parkingData.id && x.outtime == "-").ToList();

            if (parkingData.type == "Bike")
            {
                if (int.TryParse(parkingData.bikespace, out int vacant))
                {
                    var totalSpace = vacant + daily.Count;
                    parkingData.bikespace = totalSpace.ToString();
                }
            }
            else if (parkingData.type == "Car")
            {
                if (int.TryParse(parkingData.carspace, out int vacant))
                {
                    var totalSpace = vacant + daily.Count;
                    parkingData.carspace = totalSpace.ToString();
                }
            }

            return View(parkingData);   
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Admin");
            return RedirectToAction("Index", "Website");
        }

        public IActionResult ChangeAdminPassword()
        {
            var spot = GetCurrentHiddenSpot();
            if (spot == null) return RedirectToAction("AdminLogin", "Website");

            ViewBag.AdminName = spot.ownername ?? "Admin";
            return View();
        }

        public IActionResult VerifyChangePassword(IFormCollection form)
        {
            var spot = GetCurrentHiddenSpot();
            if (spot == null) return RedirectToAction("AdminLogin", "Website");

            ViewBag.AdminName = spot.ownername ?? "Admin";

            string oldpass = form["oldpass"];
            string newpass = form["newpass"];
            string confirmpass = form["confirmpass"];

            var email = HttpContext.Session.GetString("Admin");
            var cleanEmail = email?.Trim().ToLower();
            var data = context.ParkingOwner.FirstOrDefault(x => x.email != null && x.email.Trim().ToLower() == cleanEmail); 

            if (data == null) return RedirectToAction("AdminLogin", "Website");

            if (confirmpass != newpass && confirmpass != null && newpass != null)
            {
                TempData["changepass"] = "Password AND Confirm Password did not Match";
                return RedirectToAction("ChangeAdminPassword");
            }
            else
            {
                if (oldpass == data.password)
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
            var data = GetCurrentHiddenSpot();
            if (data == null) return RedirectToAction("AdminLogin", "Website");

            ViewBag.AdminName = data.ownername ?? "Admin";
            ViewBag.Type = data.type ?? "";

            string date = DateTime.Now.ToString("dd MMMM yyyy");
            var parkingdata = context.DailyParking.Where(x => x.date == date && x.parkingid == data.id).ToList();

            return View(parkingdata);
        }

        public IActionResult SaveParking(IFormCollection form)
        {
            var data = GetCurrentHiddenSpot();
            if (data == null) return RedirectToAction("AdminLogin", "Website");

            //updating available spots
            if (data.carspace == "NOT Available")
            {
                if (int.TryParse(data.bikespace, out int bikeCount) && bikeCount > 0)
                {
                    data.bikespace = (bikeCount - 1).ToString();
                }
            }
            else if (data.bikespace == "NOT Available")
            {
                if (int.TryParse(data.carspace, out int carCount) && carCount > 0)
                {
                    data.carspace = (carCount - 1).ToString();
                }
            }

            string name = form["name"];
            string number = form["number"];
            string type = form["type"];

            string dt = DateTime.Now.ToString("dd MMMM yyyy HH:mm:ss");    
            string date = DateTime.Now.ToString("dd MMMM yyyy");    
            string time = DateTime.Now.ToString("HH:mm:ss");

            var parkdata = context.DailyParking.FirstOrDefault(x => x.vehiclenumber == number && x.outtime == "-");
            if (parkdata == null)
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
            var data = GetCurrentHiddenSpot();
            if (data == null) return RedirectToAction("AdminLogin", "Website");

            ViewBag.AdminName = data.ownername ?? "Admin";
            ViewBag.Type = data.type ?? "";

            var parkingdata = context.DailyParking.Where(x => x.parkingid == data.id).ToList();

            return View(parkingdata);
        }

        public IActionResult ParkReceipt(int id)
        {
            var spotdata = GetCurrentHiddenSpot();
            if (spotdata == null) return RedirectToAction("AdminLogin", "Website");

            ViewBag.AdminName = spotdata.ownername ?? "Admin";

            var data = context.DailyParking.Find(id);
            if (data == null) return RedirectToAction("Parkhistory");

            if (data.outtime == "-")
            {
                ViewBag.Hour = spotdata.hourrate;
                return View(data);
            }
            else
            {   
                return RedirectToAction("PrintBill", new { id = id });
            }
        }

        public IActionResult PrintBill(int id)
        {
            var spotdata = GetCurrentHiddenSpot();
            if (spotdata == null) return RedirectToAction("AdminLogin", "Website");

            ViewBag.AdminId = spotdata.id;
            ViewBag.AdminName = spotdata.parkingname ?? "Parking";

            var data = context.DailyParking.Find(id);
            return View(data);
        }

        public IActionResult ExitVehicle(int id)
        {
            string time = DateTime.Now.ToString("dd MMMM yyyy HH:mm:ss");

            //RECEIVING DATA FROM DATABASE OF PARTICULAR PARKED VEHICLE
            var data = context.DailyParking.Find(id);
            if (data == null) return RedirectToAction("Parkhistory");

            //SETTING OUT DATE AND TIME 
            data.outtime = time;
            data.outdate = DateTime.Now.ToString("dd MMMM yyyy");

            //GETTING DATA OF PARKING OWNER
            var admindata = GetCurrentHiddenSpot();
            if (admindata != null)
            {
                //UPDATING AVAILABLE SPOTS
                if (admindata.carspace == "NOT Available")
                {
                    if (int.TryParse(admindata.bikespace, out int b))
                    {
                        admindata.bikespace = (b + 1).ToString();
                        context.HiddenSpot.Update(admindata);
                        context.SaveChanges();
                    }
                }
                else if (admindata.bikespace == "NOT Available")
                {
                    if (int.TryParse(admindata.carspace, out int c))
                    {
                        admindata.carspace = (c + 1).ToString();
                        context.HiddenSpot.Update(admindata);
                        context.SaveChanges();
                    }
                }

                //CALCULATING THE AMOUNT TO BE PAID BY VEHICLE OWNER
                string hourrate = admindata.hourrate ?? "0";

                DateTime dt1 = DateTime.ParseExact(time, "dd MMMM yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                DateTime dt2 = DateTime.ParseExact(data.datetime, "dd MMMM yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                TimeSpan diff = dt1 - dt2;

                if (double.TryParse(hourrate, out double rate))
                {
                    data.amount = (Math.Ceiling(diff.TotalHours) * rate).ToString();
                }
            }

            //UPDATING THE CHANGES DONE
            context.DailyParking.Update(data);
            context.SaveChanges();

            TempData["parkid"] = id.ToString();
            
            var printUrl = Url.Action("PrintBill", "Admin") ?? "/Admin/PrintBill";
            return Redirect(printUrl + $"?id={id}");
        }

        public IActionResult deleteparkhistory(int id)
        {
            var data = context.DailyParking.Find(id);
            if (data == null) return RedirectToAction("Parkhistory");

            var admindata = GetCurrentHiddenSpot();
            if (admindata != null)
            {
                if (admindata.carspace == "NOT Available" && data.outtime == "-")
                {
                    if (int.TryParse(admindata.bikespace, out int b))
                    {
                        admindata.bikespace = (b + 1).ToString();
                        context.HiddenSpot.Update(admindata);
                        context.SaveChanges();
                    }
                }
                else if (admindata.bikespace == "NOT Available" && data.outtime == "-")
                {
                    if (int.TryParse(admindata.carspace, out int c))
                    {
                        admindata.carspace = (c + 1).ToString();
                        context.HiddenSpot.Update(admindata);
                        context.SaveChanges();
                    }
                }
            }

            context.DailyParking.Remove(data);
            context.SaveChanges();
            return RedirectToAction("Parkhistory");
        }



    }
}
