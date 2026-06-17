using System.Security.Claims;
using AcademicAIAssistant.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicAIAssistant.Controllers;

[Authorize] // Bảo mật: Bắt buộc user đăng nhập mới được vào trang này
public class ProfileController : Controller
{
    private readonly AppDbContext _context;

    public ProfileController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Route("Profile")] // Định nghĩa Route /Profile chính xác theo yêu cầu của Issue
    public async Task<IActionResult> Index()
    {
        // Lấy UserId từ Cookie Authentication Claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // Cơ chế dự phòng: Nếu không lấy được từ Claims thì lấy từ Session
        if (string.IsNullOrEmpty(userIdClaim))
        {
            userIdClaim = HttpContext.Session.GetInt32("UserId")?.ToString();
        }

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login", "Account");
        }

        // Truy vấn dữ liệu từ SQL Server
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound();
        }

        return View(user); // Truyền Model User sang View hiển thị
    }
}