using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParkingManagement.Data;
using ParkingManagement.Models;

namespace ParkingManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        ApplicationDbContext context;
        public ApiController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet("findpark/{city}")]
        public IActionResult FindPark(string city)
        {
            city.ToUpper();
            var data = context.HiddenSpot.Where(x => x.city.ToUpper().Contains(city) ).ToList();
            
            return Ok(data);
        }
    }
}
