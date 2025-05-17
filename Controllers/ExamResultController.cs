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
            _jdoodleService = jdoodleService; // Inject JDoodleService
        }

        // Hiển thị bài thi cho người dùng làm
        public async Task<IActionResult> DoExam(int id)
        {
            var exam = await _examRepository.GetExamWithContentsAsync(id); // Lấy bài thi và các câu hỏi liên quan
            if (exam == null) return NotFound();

            return View(exam); // Trả về view với bài thi và câu hỏi
        }

        // Xử lý kết quả khi người dùng nộp bài
        [HttpPost]
        public async Task<IActionResult> SubmitExam(int examId, List<string> codes)
        {
            var exam = await _examRepository.GetExamWithContentsAsync(examId); // Lấy lại bài thi và các câu hỏi
            if (exam == null) return NotFound();

            int score = 0;
            var userId = _userManager.GetUserId(User); // Lấy UserId của người dùng hiện tại

            // Khởi tạo Input và Output
            var inputValues = new List<string>();
            var outputValues = new List<string>();

            // Duyệt qua các câu hỏi và kiểm tra mã code của người dùng
            for (int i = 0; i < exam.ExamContents.Count; i++)
            {
                var question = exam.ExamContents[i];
                if (question != null && question.QuestionType == "Code")
                {
                    var input = question.SampleInput; // Lấy input mẫu từ câu hỏi
                    var expectedOutput = question.ExpectedOutput; // Lấy output mong đợi từ câu hỏi

                    inputValues.Add(input ?? ""); // Lưu input mẫu
                    outputValues.Add(expectedOutput ?? ""); // Lưu output mong đợi

                    // Chạy mã người dùng với input mẫu
                    var output = await _jdoodleService.ExecuteCode(codes[i], question.Language); // Chạy mã người dùng nhập

                    // Kiểm tra nếu có lỗi khi gọi dịch vụ JDoodle
                    if (string.IsNullOrEmpty(output))
                    {
                        output = "Error or no output received";
                    }

                    // So sánh kết quả trả về với output mong đợi
                    if (output == expectedOutput)
                    {
                        score++; // Cộng điểm nếu kết quả đúng
                    }
                }
            }

            // Tính điểm tỷ lệ (score / tổng số câu hỏi)
            var result = new ExamResult
            {
                UserId = userId,
                ExamID = examId,
                ExamDate = DateTime.Now,
                Score = (score * 100) / exam.ExamContents.Count, // Tính điểm theo tỷ lệ
                Status = "Completed", // Trạng thái bài làm
                Code = string.Join(";", codes), // Lưu mã code người dùng nộp
                Input = string.Join(";", inputValues), // Lưu input đầu vào
                Output = string.Join(";", outputValues), // Lưu output
            };

            await _examResultRepository.AddAsync(result); // Lưu kết quả vào DB

            // Chuyển đến trang kết quả bài thi
            return RedirectToAction("ResultDetails", new { id = result.ResultID });
        }

        // Xem chi tiết kết quả bài thi của người dùng
        public async Task<IActionResult> ResultDetails(int id)
        {
            var result = await _examResultRepository.GetByIdAsync(id);
            if (result == null) return NotFound();

            // Lấy bài thi từ cơ sở dữ liệu và gán vào kết quả thi
            var exam = await _examRepository.GetExamWithContentsAsync(result.ExamID);
            if (exam == null) return NotFound();

            result.Exam = exam; // Gán bài thi vào kết quả thi

            return View(result); // Trả về view với kết quả thi đã có bài thi kèm theo
        }

        // Hiển thị danh sách kết quả thi của người dùng (Admin chỉ xem)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var result = await _examResultRepository.GetAllAsync();
            if (result == null)
            {
                return View();  // Trả về view nếu không có kết quả
            }

            return View(result);  // Đảm bảo rằng đối tượng result không null khi truyền vào view
        }

        // Chỉnh sửa kết quả thi (Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id)
        {
            var result = await _examResultRepository.GetByIdAsync(id);
            if (result == null) return NotFound();

            return View(result); // Hiển thị form chỉnh sửa kết quả thi
        }

        // Xử lý khi Admin chỉnh sửa kết quả thi
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Update(int id, ExamResult result)
        {
            if (id != result.ResultID) return NotFound();

            if (ModelState.IsValid)
            {
                await _examResultRepository.UpdateAsync(result); // Lưu chỉnh sửa kết quả thi
                return RedirectToAction(nameof(Index)); // Quay lại danh sách kết quả
            }

            return View(result); // Nếu có lỗi, quay lại form chỉnh sửa
        }

        // Xóa kết quả thi (Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _examResultRepository.GetByIdAsync(id);
            if (result == null) return NotFound();

            return View(result); // Hiển thị xác nhận xóa kết quả
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _examResultRepository.DeleteAsync(id); // Xóa kết quả thi
            return RedirectToAction(nameof(Index)); // Quay lại danh sách kết quả
        }
    }
}