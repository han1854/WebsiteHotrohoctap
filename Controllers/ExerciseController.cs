using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebsiteHotrohoctap.Models;
using WebsiteHotrohoctap.Models.ViewModels;
using WebsiteHotrohoctap.Repositories;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WebsiteHotrohoctap.Controllers
{
    public class ExerciseController : Controller
    {
        private readonly IExamRepository _examRepository;
        private readonly IExamResultRepository _examResultRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly UserManager<User> _userManager;
        private readonly IHttpClientFactory _httpClientFactory;

        public ExerciseController(
            IExamRepository examRepository,
            IExamResultRepository examResultRepository,
            ILessonRepository lessonRepository,
            UserManager<User> userManager,
            IHttpClientFactory httpClientFactory)
        {
            _examRepository = examRepository;
            _examResultRepository = examResultRepository;
            _lessonRepository = lessonRepository;
            _userManager = userManager;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(int? lessonId)
        {
            var lessons = await _lessonRepository.GetAllAsync();
            ViewBag.Lessons = lessons;

            List<Exam> exams;
            if (lessonId.HasValue)
            {
                var lesson = await _lessonRepository.GetByIdAsync(lessonId.Value);
                exams = lesson?.Exams.ToList() ?? new List<Exam>();
                ViewBag.SelectedLesson = lesson;
            }
            else
            {
                exams = (List<Exam>)await _examRepository.GetAllAsync();
                ViewBag.SelectedLesson = null;
            }

            return View(exams);
        }

        [Authorize]
        public async Task<IActionResult> Solve(int id)
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
                    ProblemDescription = examDescription.ProblemDescription,
                    SampleInput = examDescription.SampleInput,
                    SampleOutput = examDescription.SampleOutput
                };
                return View(viewModel);
            }
            catch
            {
                var viewModel = new ExamViewModel
                {
                    ExamID = exam.ExamID,
                    ExamName = exam.ExamName,
                    ProblemDescription = exam.Description,
                    SampleInput = "Không có ví dụ",
                    SampleOutput = "Không có ví dụ"
                };
                return View(viewModel);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RunCode(int examId, string code, string input, string language)
        {
            var exam = await _examRepository.GetByIdAsync(examId);
            if (exam == null)
            {
                return Json(new { success = false, error = "Không tìm thấy bài tập." });
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var requestBody = new Dictionary<string, string>
                {
                    { "user", "YOUR_IDEONE_USERNAME" },
                    { "pass", "YOUR_IDEONE_PASSWORD" },
                    { "source", code },
                    { "lang", language },
                    { "input", input },
                    { "run", "1" },
                    { "private", "1" }
                };

                var content = new FormUrlEncodedContent(requestBody);
                var response = await client.PostAsync("https://ideone.com/api/submission/create", content);

                if (!response.IsSuccessStatusCode)
                {
                    return Json(new { success = false, error = "Lỗi khi gọi Ideone API." });
                }

                var result = await response.Content.ReadFromJsonAsync<IdeoneCreateResponse>();
                if (result.error != "OK")
                {
                    return Json(new { success = false, error = result.error });
                }

                string link = result.link;
                string output = await GetIdeoneSubmissionResult(client, link);

                return Json(new { success = true, output = output });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SubmitCode(int examId, string code, string language)
        {
            var exam = await _examRepository.GetByIdAsync(examId);
            if (exam == null)
            {
                return Json(new { success = false, error = "Không tìm thấy bài tập." });
            }

            ExamDescription examDescription;
            try
            {
                examDescription = JsonSerializer.Deserialize<ExamDescription>(exam.Description);
            }
            catch
            {
                return Json(new { success = false, error = "Không tìm thấy test case." });
            }

            try
            {
                var client = _httpClientFactory.CreateClient("IdeoneClient");
                var requestBody = new Dictionary<string, string>
        {
            { "user", "YOUR_IDEONE_USERNAME" },
            { "pass", "YOUR_IDEONE_PASSWORD" },
            { "source", code },
            { "lang", language },
            { "input", examDescription.SampleInput },
            { "run", "1" },
            { "private", "1" }
        };

                var content = new FormUrlEncodedContent(requestBody);
                var response = await client.PostAsync("submission/create", content);

                if (!response.IsSuccessStatusCode)
                {
                    return Json(new { success = false, error = "Lỗi khi gọi Ideone API." });
                }

                var result = await response.Content.ReadFromJsonAsync<IdeoneCreateResponse>();
                if (result.error != "OK")
                {
                    return Json(new { success = false, error = result.error });
                }

                string link = result.link;
                string output = await GetIdeoneSubmissionResult(client, link);

                // So sánh output thực tế với output mong đợi
                bool passed = output != null && output.Trim() == examDescription.SampleOutput.Trim();
                int score = passed ? exam.TotalMarks : 0;

                // Thêm thông tin chi tiết về lỗi nếu mã sai
                string errorMessage = null;
                if (!passed)
                {
                    if (string.IsNullOrEmpty(output))
                    {
                        errorMessage = "Không có đầu ra. Kiểm tra lại mã của bạn.";
                    }
                    else
                    {
                        errorMessage = "Đầu ra không khớp với đầu ra mong đợi.";
                    }
                }

                var user = await _userManager.GetUserAsync(User);
                var examResult = new ExamResult
                {
                    User = user,
                    ExamID = examId,
                    Code = code,
                    Input = examDescription.SampleInput,
                    Output = output,
                    Status = passed ? "Accepted" : "Wrong Answer",
                    Score = score,
                    ExamDate = DateTime.Now
                };
                await _examResultRepository.AddAsync(examResult);

                return Json(new
                {
                    success = true,
                    passed = passed,
                    output = output,
                    expectedOutput = examDescription.SampleOutput,
                    errorMessage = errorMessage // Trả về thông báo lỗi nếu mã sai
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi gọi Ideone API: " + ex.Message });
            }
        }

        private async Task<string> GetIdeoneSubmissionResult(HttpClient client, string link)
        {
            while (true)
            {
                var statusRequest = new Dictionary<string, string>
                {
                    { "user", "YOUR_IDEONE_USERNAME" },
                    { "pass", "YOUR_IDEONE_PASSWORD" },
                    { "link", link }
                };

                var statusContent = new FormUrlEncodedContent(statusRequest);
                var statusResponse = await client.PostAsync("https://ideone.com/api/submission/status", statusContent);
                var statusResult = await statusResponse.Content.ReadFromJsonAsync<IdeoneStatusResponse>();

                if (statusResult.status == 0)
                {
                    var detailsRequest = new Dictionary<string, string>
                    {
                        { "user", "YOUR_IDEONE_USERNAME" },
                        { "pass", "YOUR_IDEONE_PASSWORD" },
                        { "link", link },
                        { "withOutput", "1" }
                    };

                    var detailsContent = new FormUrlEncodedContent(detailsRequest);
                    var detailsResponse = await client.PostAsync("https://ideone.com/api/submission/details", detailsContent);
                    var detailsResult = await detailsResponse.Content.ReadFromJsonAsync<IdeoneDetailsResponse>();

                    return detailsResult.output;
                }

                await Task.Delay(1000);
            }
        }
    }

    public class IdeoneCreateResponse
    {
        public string error { get; set; }
        public string link { get; set; }
    }

    public class IdeoneStatusResponse
    {
        public string error { get; set; }
        public int status { get; set; }
    }

    public class IdeoneDetailsResponse
    {
        public string error { get; set; }
        public string output { get; set; }
    }
}