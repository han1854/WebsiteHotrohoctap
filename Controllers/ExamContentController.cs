using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

            var model = new ExamContent
            {
                Options = new List<string> { "", "", "", "" }
            };

            return View(model);
        }

        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExamContent examcontent)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra câu hỏi trắc nghiệm
                if (examcontent.QuestionType == "MultipleChoice")
                {
                    if (examcontent.Options == null || examcontent.Options.Count < 4 || examcontent.Options.Any(opt => string.IsNullOrWhiteSpace(opt)))
                    {
                        ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ 4 đáp án.");
                        await LoadExamsAsync();
                        return View(examcontent);
                    }
                    examcontent.CorrectAnswer = examcontent.SelectedAnswer;
                }
                // Kiểm tra câu hỏi lập trình
                else if (examcontent.QuestionType == "Code")
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

                    // Đảm bảo đáp án đúng (cho Code) là null, vì không cần phải có đáp án đúng khi làm bài Code.
                    examcontent.CorrectAnswer = null;
                }

                // Lưu vào Database
                await _examcontentRepository.AddAsync(examcontent);
                return RedirectToAction(nameof(Index));
            }

            // Trả lại danh sách Exams nếu có lỗi
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

                // Cập nhật các trường
                existingExamContent.QuestionType = examcontent.QuestionType;
                existingExamContent.QuestionText = examcontent.QuestionText;
                existingExamContent.Options = examcontent.Options;
                existingExamContent.SelectedAnswer = examcontent.SelectedAnswer;
                existingExamContent.StarterCode = examcontent.StarterCode;
                existingExamContent.SampleInput = examcontent.SampleInput;
                existingExamContent.ExpectedOutput = examcontent.ExpectedOutput;
                existingExamContent.Language = examcontent.Language;
                existingExamContent.ExamID = examcontent.ExamID;

                // Xử lý lại đáp án đúng
                if (examcontent.QuestionType == "MultipleChoice")
                {
                    existingExamContent.CorrectAnswer = examcontent.SelectedAnswer;
                }
                else if (examcontent.QuestionType == "Code")
                {
                    // Không có cần gán SelectedAnswer cho câu hỏi code
                    existingExamContent.CorrectAnswer = null;
                }

                await _examcontentRepository.UpdateAsync(existingExamContent);
                return RedirectToAction(nameof(Index));
            }

            // Trả lại danh sách Exams nếu có lỗi
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
