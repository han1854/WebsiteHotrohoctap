using Microsoft.AspNetCore.Mvc;
using WebsiteHotrohoctap.Models;
using WebsiteHotrohoctap.Repositories;

namespace WebsiteHotrohoctap.Controllers
{
    public class CourseController : Controller
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ILessonRepository _lessonRepository;

        public CourseController(ICourseRepository courseRepository, ILessonRepository lessonRepository)
        {
            _courseRepository = courseRepository;
            _lessonRepository = lessonRepository;
        }
        public async Task<IActionResult> Index()
        {
            var course = await _courseRepository.GetAllAsync();
            return View(course);
        }

        public async Task<IActionResult> Details(int id)
        {

            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Course course)
        {
            if (ModelState.IsValid)
            {
                await _courseRepository.AddAsync(course);
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        public async Task<IActionResult> Update(int id)
        {


            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, Course course)
        {
            if (id != course.CourseID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _courseRepository.UpdateAsync(course);
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int CourseID)
        {
            var course = await _courseRepository.GetByIdAsync(CourseID);
            if (course != null)
            {
                await _courseRepository.DeleteAsync(CourseID);
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
