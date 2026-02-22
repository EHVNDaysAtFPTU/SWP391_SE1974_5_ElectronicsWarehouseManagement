using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.Security.Claims;

namespace ElectronicsWarehouseManagement.WebAPI.Controllers;

[ApiController]
public sealed class ViewCtrl : ControllerBase
{
    private void DisableClientCache()
    {
        Response.Headers.CacheControl = "no-store, no-cache, must-revalidate, max-age=0";
        Response.Headers.Pragma = "no-cache";
        Response.Headers.Expires = "0";
    }

    [HttpGet("/")]
    public IActionResult GetView([FromServices] IWebHostEnvironment env)
    {
        if (User?.Identity?.IsAuthenticated == false)
            return Redirect("/login");
        DisableClientCache();
        var highestRoleName = HttpContext.User.Claims
            .Where(c => c.Type == ClaimTypes.NameIdentifier)
            .Select(c => c.Value)
            .FirstOrDefault() ?? "";

        var physicalPath = Path.Combine(env.WebRootPath, "view", highestRoleName, "home.html");
        if (!System.IO.File.Exists(physicalPath))
            return NotFound();
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(physicalPath, out var contentType))
            contentType = "application/octet-stream";
        return PhysicalFile(physicalPath, contentType);
    }

    [HttpGet("/{**path}")]
    public IActionResult GetView([FromRoute] string path, [FromServices] IWebHostEnvironment env)
    {
        if (path.StartsWith("uploads/", StringComparison.InvariantCultureIgnoreCase))
        {
            string filePath = Path.Combine(Path.GetDirectoryName(env.WebRootPath)!, path);
            if (!System.IO.File.Exists(filePath))
                return NotFound();
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var contentType))
                contentType = "application/octet-stream";
            return PhysicalFile(filePath, contentType);
        }
        else
        {
            if (User?.Identity?.IsAuthenticated == false)
            {
                if (path.EndsWith("/") || path.EndsWith("\\") || path.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                    return Redirect("/login");
                return NotFound();
            }
            DisableClientCache();
            var highestRoleName = HttpContext.User.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier)
                .Select(c => c.Value)
                .FirstOrDefault() ?? "";
            var physicalPath = Path.Combine(env.WebRootPath, "view", highestRoleName, path);
            if (!System.IO.File.Exists(physicalPath))
                return NotFound();
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(physicalPath, out var contentType))
                contentType = "application/octet-stream";
            return PhysicalFile(physicalPath, contentType);
        }
    }

    [AllowAnonymous]
    [HttpGet("/login")]
    public IActionResult LoginView([FromServices] IWebHostEnvironment env)
    {
        if (User?.Identity?.IsAuthenticated == true)
            return Forbid();
        DisableClientCache();
        var physicalPath = Path.Combine(env.WebRootPath, "view", "Login", "login.html");
        if (!System.IO.File.Exists(physicalPath))
            return NotFound();
        return PhysicalFile(physicalPath, "text/html; charset=utf-8");
    }

    [Authorize(Roles = "2")]
    [HttpGet("/manager/view-list")]
    public IActionResult ManagerViewList([FromServices] IWebHostEnvironment env)
    {
        if (User?.Identity?.IsAuthenticated == false)
            return Redirect("/login");
        DisableClientCache();
        var physicalPath = Path.Combine(env.WebRootPath, "view", "manager", "itemlist.html");
        if (!System.IO.File.Exists(physicalPath))
            return NotFound();
        return PhysicalFile(physicalPath, "text/html; charset=utf-8");
    }
    
}