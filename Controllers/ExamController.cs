using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebsiteHotrohoctap.Models;
using WebsiteHotrohoctap.Repositories;

namespace WebsiteHotrohoctap.Controllers
{
    public class ExamController : Controller
    {
        private readonly IExamRepository _examRepository;
        private readonly ILessonRepository _lessonRepository;
        public ExamController(IExamRepository examRepository, ILessonRepository lessonRepository)
        {
            _examRepository = examRepository;
            _lessonRepository = lessonRepository;
        }
        public async Task<IActionResult> Index()
        {
            var exams = await _examRepository.GetAllAsync();
            return View(exams);
        }
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Create()
        {
            var lessons = await _lessonRepository.GetAllAsync();
            ViewBag.Lessons = new SelectList(lessons, "Id", "Name");

            return View();
        }
        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public async Task<IActionResult> Create(Exam exam)
        {
            if (ModelState.IsValid)
            {
                await _examRepository.AddExamAsync(exam);
                return RedirectToAction(nameof(Index));
            }
            var lessons = await _lessonRepository.GetAllAsync();
            ViewBag.Lessons = new SelectList(lessons, "Id", "Name");
            return View(exam);
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
            var exams = await _examRepository.GetByIdAsync(id);
            if (exams == null)
            {
                return NotFound();
            }
            return View(exams);
        }
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Update(int id)
        {
            var exams = await _examRepository.GetByIdAsync(id);
            if (exams == null)
            {
                return NotFound();
            }

            var lessons = await _lessonRepository.GetAllAsync();
            ViewBag.Lessons = new SelectList(lessons, "Id", "Name", exams.LessonID);
            return View(exams);
        }
        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public async Task<IActionResult> Update(int id, Exam exam)
        {
            if (id != exam.ExamID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingExam = await _examRepository.GetByIdAsync(id);
                existingExam.ExamName = exam.ExamName;
                existingExam.Description = exam.Description;
                existingExam.TotalMarks = exam.TotalMarks;
                existingExam.LessonID = exam.LessonID;
                await _examRepository.UpdateAsync(existingExam);
                return RedirectToAction(nameof(Index));
            }
            var lessons = await _lessonRepository.GetAllAsync();
            ViewBag.Lessons = new SelectList(lessons, "Id", "Name");
            return View(exam);
        }

        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var exams = await _examRepository.GetByIdAsync(id);
            if (exams == null)
            {
                return NotFound();
            }
            return View(exams);
        }
        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _examRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
