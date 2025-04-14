using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebsiteHotrohoctap.Models;
using WebsiteHotrohoctap.Repositories;

namespace WebsiteHotrohoctap.Controllers
{
    public class ExamContentController : Controller
    {
        private readonly IExamRepository _examRepository;
        private readonly IExamContentRepository _examcontentRepository;
        public ExamContentController(IExamRepository examRepository, IExamContentRepository examcontentRepository)
        {
            _examRepository = examRepository;
            _examcontentRepository = examcontentRepository;
        }
        public async Task<IActionResult> Index()
        {
            var examcontents = await _examcontentRepository.GetAllAsync();
            return View(examcontents);
        }
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Create()
        {
            var exams = await _examRepository.GetAllAsync();
            ViewBag.Exams = new SelectList(exams, "Id", "Name");

            return View();
        }
        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public async Task<IActionResult> Create(ExamContent examcontent)
        {
            if (ModelState.IsValid)
            {
                await _examcontentRepository.AddAsync(examcontent);
                return RedirectToAction(nameof(Index));
            }
            var exams = await _examRepository.GetAllAsync();
            ViewBag.Exams = new SelectList(exams, "Id", "Name");
            return View(examcontent);
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
            var examcontents = await _examcontentRepository.GetByIdAsync(id);
            if (examcontents == null)
            {
                return NotFound();
            }
            return View(examcontents);
        }
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Update(int id)
        {
            var examcontents = await _examcontentRepository.GetByIdAsync(id);
            if (examcontents == null)
            {
                return NotFound();
            }

            var exams = await _examRepository.GetAllAsync();
            ViewBag.Exams = new SelectList(exams, "Id", "Name", examcontents.ExamID);
            return View(examcontents);
        }
        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public async Task<IActionResult> Update(int id, ExamContent examcontent)
        {
            if (id != examcontent.ExamContentID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingExamContent = await _examcontentRepository.GetByIdAsync(id);
                existingExamContent.QuestionType = examcontent.QuestionType;
                existingExamContent.QuestionText = examcontent.QuestionText;
                existingExamContent.CorrectAnswer = examcontent.CorrectAnswer;
                existingExamContent.ExamID = examcontent.ExamID;
                await _examcontentRepository.UpdateAsync(existingExamContent);
                return RedirectToAction(nameof(Index));
            }
            var exams = await _examRepository.GetAllAsync();
            ViewBag.Exams = new SelectList(exams, "Id", "Name");
            return View(examcontent);
        }

        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var examcontents = await _examcontentRepository.GetByIdAsync(id);
            if (examcontents == null)
            {
                return NotFound();
            }
            return View(examcontents);
        }
        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _examcontentRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
