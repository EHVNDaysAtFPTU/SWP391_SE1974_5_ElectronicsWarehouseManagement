using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
    public async Task<IActionResult> GetView([FromServices] IWebHostEnvironment env)
    {
        if (User?.Identity?.IsAuthenticated == false)
            return Redirect("/login");
        string? idStr = HttpContext.Session.GetString("UserId");
        if (idStr is null || !int.TryParse(idStr, out int id))
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/login");
        }
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
    public async Task<IActionResult> GetView([FromRoute] string path, [FromServices] IWebHostEnvironment env)
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
        if (User?.Identity?.IsAuthenticated == false)
        {
            if (path.EndsWith("/") || path.EndsWith("\\") || path.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                return Redirect("/login");
            return NotFound();
        }
        string? idStr = HttpContext.Session.GetString("UserId");
        if (idStr is null || !int.TryParse(idStr, out int id))
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/login");
        }
        if (path.StartsWith("me/", StringComparison.InvariantCultureIgnoreCase))
        {
            string filePath = Path.Combine(env.WebRootPath, "view", "me", path.Substring(3));
            if (!System.IO.File.Exists(filePath))
                return NotFound();
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var contentType))
                contentType = "application/octet-stream";
            return PhysicalFile(filePath, contentType);
        }
        else
        {
            DisableClientCache();
            //TODO: selectable current role
            var highestRoleName = HttpContext.User.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier)
                .Select(c => c.Value)
                .FirstOrDefault() ?? "";
            string filePath = Path.Combine(env.WebRootPath, "view", highestRoleName, path);
            if (!System.IO.File.Exists(filePath))
                return NotFound();
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var contentType))
                contentType = "application/octet-stream";
            return PhysicalFile(filePath, contentType);
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
    
    [AllowAnonymous]
    [HttpGet("/auth/reset-password")]
    public IActionResult ResetPasswordView([FromQuery] string token, [FromServices] IWebHostEnvironment env)
    {
        if (User?.Identity?.IsAuthenticated == true)
            return Forbid();
        if (string.IsNullOrWhiteSpace(token))
            return Redirect("/login");
        DisableClientCache();
        var physicalPath = Path.Combine(env.WebRootPath, "reset-password.html");
        if (!System.IO.File.Exists(physicalPath))
            return NotFound();
        return PhysicalFile(physicalPath, "text/html; charset=utf-8");
    }
}