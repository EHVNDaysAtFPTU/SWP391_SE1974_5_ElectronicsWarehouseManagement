using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.Security.Claims;

namespace ElectronicsWarehouseManagement.WebAPI.Controllers;

[ApiController]
public sealed class ViewCtrl : ControllerBase
{
    [HttpGet("/")]
    public IActionResult GetView([FromServices] IWebHostEnvironment env)
    {
        if (User?.Identity?.IsAuthenticated == false)
            return Redirect("/login");
        var highestRoleName = HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).FirstOrDefault() ?? "";
        var physicalPath = Path.Combine(env.WebRootPath, "view", highestRoleName, "home.html");
        if (!System.IO.File.Exists(physicalPath))
            return NotFound();
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(physicalPath, out var contentType))
            contentType = "application/octet-stream";
        return PhysicalFile(physicalPath, contentType);
    }
    
    [HttpGet("/{**path}")]
    public IActionResult GetView([FromRoute]string path, [FromServices] IWebHostEnvironment env)
    {
        if (User?.Identity?.IsAuthenticated == false)
        {
            if (path.EndsWith("/") || path.EndsWith("\\") || path.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                return Redirect("/login");
            return NotFound();
        }
        var highestRoleName = HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).FirstOrDefault() ?? "";
        var physicalPath = Path.Combine(env.WebRootPath, "view", highestRoleName, path);
        if (!System.IO.File.Exists(physicalPath))
            return NotFound();
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(physicalPath, out var contentType))
            contentType = "application/octet-stream";
        return PhysicalFile(physicalPath, contentType);
    }

    [AllowAnonymous]
    [HttpGet("/login")]
    public IActionResult LoginView([FromServices] IWebHostEnvironment env)
    {
        if (User?.Identity?.IsAuthenticated == true)
            return Forbid();
        var physicalPath = Path.Combine(env.WebRootPath, "view", "Login", "login.html");
        if (!System.IO.File.Exists(physicalPath))
            return NotFound();
        return PhysicalFile(physicalPath, "text/html; charset=utf-8");
    }
}