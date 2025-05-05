using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebsiteHotrohoctap.Models;
using WebsiteHotrohoctap.Models.ViewModels;
using WebsiteHotrohoctap.Repositories;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace WebsiteHotrohoctap.Controllers
{
    public class ExerciseController : Controller
    {
        private readonly IExamRepository _examRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ExerciseController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ILessonRepository _lessonRepository;

        public ExerciseController(IExamRepository examRepository, IHttpClientFactory httpClientFactory, ILogger<ExerciseController> logger, IConfiguration configuration, ILessonRepository lessonRepository)
        {
            _examRepository = examRepository;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
            _lessonRepository = lessonRepository;
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

            var examDescription = JsonConvert.DeserializeObject<ExamDescription>(exam.Description);
            var viewModel = new ExamViewModel
            {
                ExamID = exam.ExamID,
                ExamName = exam.ExamName,
                ProblemDescription = examDescription?.ProblemDescription,
                SampleInput = examDescription?.SampleInput,
                SampleOutput = examDescription?.SampleOutput,
                TotalMarks = exam.TotalMarks,
                LessonID = exam.LessonID
            };

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RunCode(int examId, string code, string input, string language)
        {
            _logger.LogInformation("RunCode called with examId: {ExamId}, code: {Code}, input: {Input}, language: {Language}", examId, code, input, language);

            var exam = await _examRepository.GetByIdAsync(examId);
            if (exam == null)
            {
                _logger.LogWarning("Exam not found for examId: {ExamId}", examId);
                return Json(new { success = false, error = "Không tìm thấy bài tập." });
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                return Json(new { success = false, error = "Code không được để trống." });
            }

            if (string.IsNullOrWhiteSpace(language))
            {
                return Json(new { success = false, error = "Vui lòng chọn ngôn ngữ lập trình." });
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var requestBody = new
                {
                    clientId = _configuration["JDoodle:ClientId"],
                    clientSecret = _configuration["JDoodle:ClientSecret"],
                    script = code,
                    language = language,
                    versionIndex = "0",
                    stdin = input ?? ""
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("https://api.jdoodle.com/v1/execute", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("JDoodle API failed with status: {StatusCode}, content: {Content}", response.StatusCode, errorContent);
                    return Json(new { success = false, error = "Lỗi khi gọi JDoodle API: " + (string.IsNullOrEmpty(errorContent) ? "Không có chi tiết lỗi." : errorContent) });
                }

                var result = await response.Content.ReadFromJsonAsync<JDoodleResponse>();
                if (result.statusCode != 200)
                {
                    _logger.LogWarning("JDoodle API returned error: {Error}", result.output);
                    return Json(new { success = false, error = "JDoodle API trả về lỗi: " + result.output });
                }

                _logger.LogInformation("RunCode successful, output: {Output}", result.output);
                return Json(new { success = true, output = result.output });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in RunCode: {Message}", ex.Message);
                return Json(new { success = false, error = $"Lỗi: {ex.Message}" });
            }
        }

        private async Task<string> GetIdeoneSubmissionResult(HttpClient client, string link)
        {
            // Phương thức này không cần thiết cho JDoodle API vì nó trả về kết quả ngay lập tức
            return "Phương thức không sử dụng cho JDoodle API.";
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SubmitCode(int examId, string code, string language)
        {
            _logger.LogInformation("SubmitCode called with examId: {ExamId}, code: {Code}, language: {Language}", examId, code, language);

            var exam = await _examRepository.GetByIdAsync(examId);
            if (exam == null)
            {
                _logger.LogWarning("Exam not found for examId: {ExamId}", examId);
                return Json(new { success = false, error = "Không tìm thấy bài tập." });
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                return Json(new { success = false, error = "Code không được để trống." });
            }

            if (string.IsNullOrWhiteSpace(language))
            {
                return Json(new { success = false, error = "Vui lòng chọn ngôn ngữ lập trình." });
            }

            try
            {
                // Lấy test case từ ExamContents
                var testCases = await _examRepository.GetTestCasesAsync(examId);
                if (!testCases.Any())
                {
                    return Json(new { success = false, error = "Không tìm thấy test case cho bài tập này." });
                }

                int totalTestCases = testCases.Count();
                int passedTestCases = 0;
                string allInputs = "";
                string allOutputs = "";
                string status = "Accepted";

                var client = _httpClientFactory.CreateClient();
                foreach (var testCase in testCases)
                {
                    var requestBody = new
                    {
                        clientId = _configuration["JDoodle:ClientId"],
                        clientSecret = _configuration["JDoodle:ClientSecret"],
                        script = code,
                        language = language,
                        versionIndex = "0",
                        stdin = testCase.QuestionText ?? ""
                    };

                    var jsonContent = new StringContent(
                        JsonSerializer.Serialize(requestBody),
                        Encoding.UTF8,
                        "application/json"
                    );

                    var response = await client.PostAsync("https://api.jdoodle.com/v1/execute", jsonContent);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("JDoodle API failed with status: {StatusCode}, content: {Content}", response.StatusCode, errorContent);
                        return Json(new { success = false, error = "Lỗi khi gọi JDoodle API: " + errorContent });
                    }

                    var result = await response.Content.ReadFromJsonAsync<JDoodleResponse>();
                    if (result.statusCode != 200)
                    {
                        status = "Runtime Error";
                        _logger.LogWarning("JDoodle API returned error: {Error}", result.output);
                        allOutputs += result.output + "\n";
                        continue; // Tiếp tục với test case tiếp theo
                    }

                    // So sánh output với đáp án đúng
                    string actualOutput = result.output?.Trim();
                    string expectedOutput = testCase.CorrectAnswer?.Trim();
                    allInputs += testCase.QuestionText + "\n";
                    allOutputs += actualOutput + "\n";

                    if (actualOutput != expectedOutput)
                    {
                        status = "Wrong Answer";
                    }
                    else
                    {
                        passedTestCases++;
                    }
                }

                // Tính điểm theo cơ chế LeetCode
                int totalMarks = exam.TotalMarks; // Giả sử TotalMarks = 100
                int score = (passedTestCases * totalMarks) / totalTestCases;
                if (status == "Runtime Error" || status == "Wrong Answer")
                {
                    score = (passedTestCases * totalMarks) / totalTestCases; // Điểm dựa trên test case đúng
                }

                // Lưu kết quả vào ExamResults
                var userId = int.Parse(User.Identity.Name); // Giả sử UserId lấy từ tên đăng nhập, cần điều chỉnh
                var examResult = new ExamResult
                {
                    ExamID = examId,
                    UserId = userId.ToString(),
                    Score = score,
                    ExamDate = DateTime.Now,
                    Status = status,
                    Code = code,
                    Input = allInputs.Trim(),
                    Output = allOutputs.Trim()
                };

                await _examRepository.AddExamAsync(exam);

                _logger.LogInformation("SubmitCode successful, score: {Score}, status: {Status}", score, status);
                return Json(new { success = true, score = score, status = status, passed = passedTestCases, total = totalTestCases });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in SubmitCode: {Message}", ex.Message);
                return Json(new { success = false, error = $"Lỗi: {ex.Message}" });
            }
        }
    }

    // Định nghĩa lớp mô hình cho phản hồi của JDoodle API
    public class JDoodleResponse
    {
        public string output { get; set; }
        public int statusCode { get; set; }
        public string memory { get; set; }
        public string cpuTime { get; set; }
    }

    // Lớp mô hình hiện tại (có thể xóa nếu không dùng Ideone nữa)
    public class IdeoneCreateResponse
    {
        public string error { get; set; }
        public string link { get; set; }
    }

    public class IdeoneStatusResponse
    {
        public int status { get; set; }
        public string output { get; set; }
        public string error { get; set; }
    }
}