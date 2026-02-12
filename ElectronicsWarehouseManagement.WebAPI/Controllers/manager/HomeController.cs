using Microsoft.AspNetCore.Mvc;

namespace ElectronicsWarehouseManagement.WebAPI.Controllers.Manager
{
    [ApiController]
    [Route("api/manager/dashboard")]
    public class DashboardController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetDashboard()
        {
            return Ok(new
            {
                message = "Dashboard API running",
                totalProducts = 120,
                totalOrders = 45
            });
        }
    }
}
