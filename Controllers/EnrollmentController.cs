using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteHotrohoctap.Models;

namespace WebsiteHotrohoctap.Controllers
{
    [Authorize]
    public class EnrollmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public EnrollmentController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Register(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var alreadyEnrolled = _context.Enrollments
                .Any(e => e.CourseID == courseId && e.User.Id == user.Id);

            if (!alreadyEnrolled)
            {
                var enrollment = new Enrollment
                {
                    CourseID = courseId,
                    User = user,
                    EnrollmentDate = DateOnly.FromDateTime(DateTime.Now),
                    CompletionStatus = "In Progress"
                };

                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ByCourse", "Lesson", new { courseId = courseId });
        }

    }
}
