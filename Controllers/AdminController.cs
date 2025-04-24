using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using WebsiteHotrohoctap.Models;
using WebsiteHotrohoctap.Models.ViewModels;
using WebsiteHotrohoctap.Repositories;
namespace WebsiteHotrohoctap.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IExamRepository _examRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly EFLessonRepository _efLessonRepository;

        public AdminController(ApplicationDbContext context, ILessonRepository lessonRepository, IExamRepository examRepository)
        {
            _context = context;
            _lessonRepository = lessonRepository;
            _examRepository = examRepository;
        }

        // Hiển thị form tạo bài tập
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateExam()
        {
            ViewBag.Lessons = await _lessonRepository.GetAllAsync(); // Lấy danh sách bài học
            return View();
        }

        // Xử lý tạo bài tập

        [HttpPost]
        public async Task<IActionResult> CreateExam(Exam exam)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem LessonID có tồn tại trong bảng Lessons không
                var lesson = await _lessonRepository.GetByIdAsync(exam.LessonID);
                if (lesson == null)
                {
                    ModelState.AddModelError("LessonID", "Bài học được chọn không tồn tại. Vui lòng chọn bài học khác.");
                    ViewBag.Lessons = await _lessonRepository.GetAllAsync();
                    return View(exam);
                }

                try
                {
                    await _examRepository.AddExamAsync(exam);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo bài tập: " + ex.Message);
                }
            }

            ViewBag.Lessons = await _lessonRepository.GetAllAsync();
            return View(exam);
        }
        // Hiển thị form chỉnh sửa bài tập
        [Authorize]
        public async Task<IActionResult> EditExam(int id)
        {
            var exam = await _examRepository.GetByIdAsync(id);
            if (exam == null)
            {
                return NotFound();
            }

            try
            {
                var examDescription = JsonSerializer.Deserialize<ExamDescription>(exam.Description);
                var viewModel = new ExamViewModel
                {
                    ExamID = exam.ExamID,
                    ExamName = exam.ExamName,
                    LessonID = exam.LessonID,
                    ProblemDescription = examDescription.ProblemDescription,
                    SampleInput = examDescription.SampleInput,
                    SampleOutput = examDescription.SampleOutput,
                    TotalMarks = exam.TotalMarks
                };

                ViewBag.Lessons = await _lessonRepository.GetAllAsync();
                return View(viewModel);
            }
            catch
            {
                var viewModel = new ExamViewModel
                {
                    ExamID = exam.ExamID,
                    ExamName = exam.ExamName,
                    LessonID = exam.LessonID,
                    ProblemDescription = exam.Description,
                    SampleInput = "Không có ví dụ",
                    SampleOutput = "Không có ví dụ",
                    TotalMarks = exam.TotalMarks
                };

                ViewBag.Lessons = await _lessonRepository.GetAllAsync();
                return View(viewModel);
            }
        }

        // Xử lý cập nhật bài tập
        [HttpPost]
        public async Task<IActionResult> EditExam(ExamViewModel model)
        {
            if (ModelState.IsValid)
            {
                var exam = await _context.Exams.FindAsync(model.ExamID);
                if (exam == null)
                {
                    return NotFound();
                }

                var examDescription = new
                {
                    ProblemDescription = model.ProblemDescription,
                    SampleInput = model.SampleInput,
                    SampleOutput = model.SampleOutput
                };
                var jsonDescription = JsonSerializer.Serialize(examDescription);

                exam.ExamName = model.ExamName;
                exam.Description = jsonDescription;
                exam.TotalMarks = model.TotalMarks;

                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Exercise");
            }

            return View(model);
        }
    }
}