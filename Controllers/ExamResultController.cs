using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebsiteHotrohoctap.Models;
using WebsiteHotrohoctap.Repositories;

namespace WebsiteHotrohoctap.Controllers
{
    public class ExamResultController : Controller
    {
        private readonly IExamRepository _examRepository;
        private readonly IExamResultRepository _examresultRepository;
        public ExamResultController(IExamRepository examRepository, IExamResultRepository examresultRepository)
        {
            _examRepository = examRepository;
            _examresultRepository = examresultRepository;
        }
        public async Task<IActionResult> Index()
        {
            var examresults = await _examresultRepository.GetAllAsync();
            return View(examresults);
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
        public async Task<IActionResult> Create(ExamResult examresult)
        {
            if (ModelState.IsValid)
            {
                await _examresultRepository.AddAsync(examresult);
                return RedirectToAction(nameof(Index));
            }
            var exams = await _examRepository.GetAllAsync();
            ViewBag.Exams = new SelectList(exams, "Id", "Name");
            return View(examresult);
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
            var examresult = await _examresultRepository.GetByIdAsync(id);
            if (examresult == null)
            {
                return NotFound();
            }
            return View(examresult);
        }
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Update(int id)
        {
            var examresult = await _examresultRepository.GetByIdAsync(id);
            if (examresult == null)
            {
                return NotFound();
            }

            var exams = await _examRepository.GetAllAsync();
            ViewBag.Exams = new SelectList(exams, "Id", "Name", examresult.ExamID);
            return View(examresult);
        }
        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public async Task<IActionResult> Update(int id, ExamResult examresult)
        {
            if (id != examresult.ResultID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingExamResult = await _examresultRepository.GetByIdAsync(id);
                existingExamResult.ResultID = examresult.ResultID;
                existingExamResult.ExamID = examresult.ExamID;
                existingExamResult.Score = examresult.Score;
                existingExamResult.ExamDate = examresult.ExamDate;
                existingExamResult.User = examresult.User;
                await _examresultRepository.UpdateAsync(existingExamResult);
                return RedirectToAction(nameof(Index));
            }
            var exams = await _examRepository.GetAllAsync();
            ViewBag.Exams = new SelectList(exams, "Id", "Name");
            return View(examresult);
        }

        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var examresult = await _examresultRepository.GetByIdAsync(id);
            if (examresult == null)
            {
                return NotFound();
            }
            return View(examresult);
        }
        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _examresultRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
