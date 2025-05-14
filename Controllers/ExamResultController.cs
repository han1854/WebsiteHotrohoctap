using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebsiteHotrohoctap.Models;
using WebsiteHotrohoctap.Repositories;
using Microsoft.AspNetCore.Identity;
using WebsiteHotrohoctap.Services;

namespace WebsiteHotrohoctap.Controllers
{
    public class ExamResultController : Controller
    {
        private readonly IExamRepository _examRepository;
        private readonly IExamResultRepository _examResultRepository;
        private readonly UserManager<User> _userManager;
        private readonly JDoodleService _jdoodleService;

        public ExamResultController(IExamRepository examRepository, IExamResultRepository examResultRepository, UserManager<User> userManager, JDoodleService jdoodleService)
        {
            _examRepository = examRepository;
            _examResultRepository = examResultRepository;
            _userManager = userManager;
            _jdoodleService = jdoodleService;
        }

        public async Task<IActionResult> DoExam(int id)
        {
            var exam = await _examRepository.GetExamWithContentsAsync(id);
            if (exam == null) return NotFound();

            return View(exam);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitExam(int examId, List<string> answers, List<int> contentIds, List<string> codes)
        {
            var exam = await _examRepository.GetExamWithContentsAsync(examId);
            if (exam == null) return NotFound();

            int score = 0;
            var userId = _userManager.GetUserId(User);

            var inputValues = new List<string>();
            var outputValues = new List<string>();

            for (int i = 0; i < contentIds.Count; i++)
            {
                var question = exam.ExamContents.FirstOrDefault(x => x.ExamContentID == contentIds[i]);
                if (question != null)
                {
                    if (question.QuestionType == "MultipleChoice")
                    {
                        if (question.CorrectAnswer == answers[i])
                        {
                            score++;
                        }
                    }
                    else if (question.QuestionType == "Code")
                    {
                        // Nếu không có ngôn ngữ trong câu hỏi, sử dụng mặc định là "cpp"
                        string language = question.Language ?? "cpp";
                        string output = await _jdoodleService.ExecuteCode(codes[i], language);

                        inputValues.Add(question.SampleInput ?? "No input");
                        outputValues.Add(output ?? "No output");

                        // Kiểm tra kết quả từ JDoodle (có thể cần điều chỉnh logic so với yêu cầu bài thi)
                        if (output != null && output.Trim() == question.CorrectAnswer)
                        {
                            score++;
                        }
                    }

                }
            }

            var result = new ExamResult
            {
                UserId = userId,
                ExamID = examId,
                ExamDate = DateTime.Now,
                Score = (score * 100) / exam.ExamContents.Count,
                Status = "Completed",
                Answer = string.Join(";", answers),
                Code = string.Join(";", codes),
                Input = string.Join(";", inputValues),
                Output = string.Join(";", outputValues),
            };

            await _examResultRepository.AddAsync(result);

            return RedirectToAction("ResultDetails", new { id = result.ResultID });
        }

        public async Task<IActionResult> ResultDetails(int id)
        {
            var result = await _examResultRepository.GetByIdAsync(id);
            if (result == null) return NotFound();

            var exam = await _examRepository.GetExamWithContentsAsync(result.ExamID);
            if (exam == null) return NotFound();

            result.Exam = exam;

            return View(result);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var result = await _examResultRepository.GetAllAsync();
            return View(result);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id)
        {
            var result = await _examResultRepository.GetByIdAsync(id);
            if (result == null) return NotFound();

            return View(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Update(int id, ExamResult result)
        {
            if (id != result.ResultID) return NotFound();

            if (ModelState.IsValid)
            {
                await _examResultRepository.UpdateAsync(result);
                return RedirectToAction(nameof(Index));
            }

            return View(result);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _examResultRepository.GetByIdAsync(id);
            if (result == null) return NotFound();

            return View(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _examResultRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
