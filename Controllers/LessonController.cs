using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using WebsiteHotrohoctap.Models;
using WebsiteHotrohoctap.Repositories;
using Microsoft.EntityFrameworkCore;

namespace WebsiteHotrohoctap.Controllers
{
    public class LessonController : Controller
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ILessonRepository _lessonRepository;

        public LessonController(ICourseRepository courseRepository, ILessonRepository lessonRepository)
        {
            _courseRepository = courseRepository;
            _lessonRepository = lessonRepository;
        }

        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Index()
        {
            var lessons = await _lessonRepository.GetAllAsync();
            return View(lessons);
        }

        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Create()
        {
            var courses = await _courseRepository.GetAllAsync();
            ViewBag.Courses = new SelectList(courses, "CourseID", "CourseName");

            return View();
        }
        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public async Task<IActionResult> Create(Lesson lesson)
        {
            if (!ModelState.IsValid)
            {
                // In ra các lỗi để kiểm tra
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }

            if (ModelState.IsValid)
            {
                await _lessonRepository.AddAsync(lesson);
                return RedirectToAction(nameof(Index));
            }

            var courses = await _courseRepository.GetAllAsync();
            ViewBag.Courses = new SelectList(courses, "CourseID", "CourseName");
            return View(lesson);
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/images", image.FileName);
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + image.FileName; // Trả về đường dẫn tương đối 
        }
        public async Task<IActionResult> Details(int id)
        {
            var lesson = await _lessonRepository.GetByIdAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }
            return View(lesson);
        }
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Update(int id)
        {
            var lesson = await _lessonRepository.GetByIdAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            var courses = await _courseRepository.GetAllAsync();
            ViewBag.Courses = new SelectList(courses, "CourseID", "CourseName", lesson.CourseID);
            return View(lesson);
        }
        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public async Task<IActionResult> Update(int id, Lesson lesson)
        {
            if (id != lesson.LessonID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingLesson = await _lessonRepository.GetByIdAsync(id);
                existingLesson.LessonName = lesson.LessonName;
                existingLesson.LessonDescription = lesson.LessonDescription;
                existingLesson.CourseID = lesson.CourseID;
                await _lessonRepository.UpdateAsync(existingLesson);
                return RedirectToAction(nameof(Index));
            }
            var courses = await _courseRepository.GetAllAsync();
            ViewBag.Courses = new SelectList(courses, "CourseID", "CourseName");
            return View(lesson);
        }

        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var lesson = await _lessonRepository.GetByIdAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }
            return View(lesson);
        }

        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var lesson = await _lessonRepository.GetByIdAsync(id);
                if (lesson == null)
                {
                    return NotFound();
                }
                await _lessonRepository.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting lesson: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra khi xóa bài học. Vui lòng thử lại.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        
    }

    }
}
