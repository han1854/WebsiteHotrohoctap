using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using WebsiteHotrohoctap.Models;
using WebsiteHotrohoctap.Repositories;
using WebsiteHotrohoctap.Services;

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
            await LoadExamsAsync();
            return View(new ExamContent());
        }

        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExamContent examcontent)
        {
            if (ModelState.IsValid)
            {
                // Chỉ xử lý câu hỏi lập trình (Code)
                if (examcontent.QuestionType == "Code")
                {
                    if (string.IsNullOrWhiteSpace(examcontent.StarterCode))
                    {
                        ModelState.AddModelError(string.Empty, "Vui lòng nhập mã nguồn cho câu hỏi.");
                        await LoadExamsAsync();
                        return View(examcontent);
                    }
                    if (string.IsNullOrWhiteSpace(examcontent.SampleInput) || string.IsNullOrWhiteSpace(examcontent.ExpectedOutput))
                    {
                        ModelState.AddModelError(string.Empty, "Vui lòng nhập input mẫu và output mẫu.");
                        await LoadExamsAsync();
                        return View(examcontent);
                    }
                    examcontent.CorrectAnswer = null; // Không cần đáp án đúng cho câu hỏi Code
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Chỉ hỗ trợ loại câu hỏi lập trình (Code).");
                    await LoadExamsAsync();
                    return View(examcontent);
                }

                // Lưu vào Database
                await _examcontentRepository.AddAsync(examcontent);
                return RedirectToAction(nameof(Index));
            }

            await LoadExamsAsync();
            return View(examcontent);
        }

        private async Task LoadExamsAsync()
        {
            var exams = await _examRepository.GetAllAsync();
            ViewBag.Exams = new SelectList(exams, "ExamID", "ExamName");
        }

        public async Task<IActionResult> Details(int id)
        {
            var examcontent = await _examcontentRepository.GetByIdAsync(id);
            if (examcontent == null)
            {
                return NotFound();
            }
            return View(examcontent);
        }

        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Update(int id)
        {
            var examcontent = await _examcontentRepository.GetByIdAsync(id);
            if (examcontent == null)
            {
                return NotFound();
            }

            await LoadExamsAsync();
            return View(examcontent);
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
                existingExamContent.Language = examcontent.Language;
                existingExamContent.StarterCode = examcontent.StarterCode;
                existingExamContent.SampleInput = examcontent.SampleInput;
                existingExamContent.ExpectedOutput = examcontent.ExpectedOutput;
                existingExamContent.ExamID = examcontent.ExamID;

                // Xử lý câu hỏi Code
                if (existingExamContent.QuestionType == "Code")
                {
                    existingExamContent.CorrectAnswer = null; // Không cần đáp án đúng
                }

                await _examcontentRepository.UpdateAsync(existingExamContent);
                return RedirectToAction(nameof(Index));
            }

            await LoadExamsAsync();
            return View(examcontent);
        }

        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var examcontent = await _examcontentRepository.GetByIdAsync(id);
            if (examcontent == null)
            {
                return NotFound();
            }
            return View(examcontent);
        }

        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _examcontentRepository.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentNullException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View();
            }
        }
    }
}