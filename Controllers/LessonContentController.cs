using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebsiteHotrohoctap.Models;
using WebsiteHotrohoctap.Repositories;

namespace WebsiteHotrohoctap.Controllers
{
    public class LessonContentController : Controller
    {
        private readonly ILessonContentRepository _lessoncontentRepository;
        private readonly ILessonRepository _lessonRepository;
        public LessonContentController(ILessonContentRepository lessoncontentRepository, ILessonRepository lessonRepository)
        {
            _lessoncontentRepository = lessoncontentRepository;
            _lessonRepository = lessonRepository;
        }
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Index()
        {
            var lessoncontent = await _lessoncontentRepository.GetAllAsync();
            return View(lessoncontent);
        }
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Create()
        {
            var lessons = await _lessonRepository.GetAllAsync();
            ViewBag.Lessons = new SelectList(lessons, "LessonID", "LessonName");

            return View();
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public async Task<IActionResult> Create(
    LessonContent lessoncontent,
    IFormFile? ImageFile,
    IFormFile? VideoFile,
    string? VideoUrl,
    string? TextContent)
        {
            Console.WriteLine("ContentType: " + lessoncontent.ContentType);
            Console.WriteLine("TextContent: " + TextContent);
            Console.WriteLine("ImageFile: " + (ImageFile != null ? ImageFile.FileName : "null"));
            Console.WriteLine("VideoFile: " + (VideoFile != null ? VideoFile.FileName : "null"));
            Console.WriteLine("VideoUrl: " + VideoUrl);

            // Test trước: chỉ xử lý nếu hợp lệ
            if (lessoncontent.ContentType == "text")
            {
                lessoncontent.ContentData = TextContent;
            }
            else if (lessoncontent.ContentType == "image" && ImageFile != null)
            {
                string path = await SaveImage(ImageFile);
                lessoncontent.ContentData = path;
            }
            else if (lessoncontent.ContentType == "video")
            {
                if (VideoFile != null)
                {
                    string path = await SaveVideo(VideoFile);
                    lessoncontent.ContentData = path;
                }
                else if (!string.IsNullOrWhiteSpace(VideoUrl))
                {
                    lessoncontent.ContentData = VideoUrl;
                }
            }

            if (ModelState.IsValid)
            {
                await _lessoncontentRepository.AddAsync(lessoncontent);
                return RedirectToAction("Index");
            }

            var lessons = await _lessonRepository.GetAllAsync();
            ViewBag.Lessons = new SelectList(lessons, "LessonID", "LessonName");
            return View(lessoncontent);
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

        private async Task<string> SaveVideo(IFormFile video)
        {
            var fileName = Path.GetFileName(video.FileName);
            var savePath = Path.Combine("wwwroot/videos", fileName);

            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await video.CopyToAsync(stream);
            }

            return "/videos/" + fileName;
        }

        // Xem chi tiết nội dung bài học
        public async Task<IActionResult> Details(int id)
        {
            var lessoncontent = await _lessoncontentRepository.GetByIdAsync(id);
            if (lessoncontent == null)
            {
                return NotFound();
            }
            return View(lessoncontent);
        }
        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public async Task<IActionResult> Update(int id, LessonContent lessoncontent)
        {
            if (id != lessoncontent.ContentID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingLessonContent = await _lessoncontentRepository.GetByIdAsync(id);
                existingLessonContent.ContentType = lessoncontent.ContentType;
                existingLessonContent.ContentData = lessoncontent.ContentData;
                existingLessonContent.LessonID = lessoncontent.LessonID;
                await _lessoncontentRepository.UpdateAsync(existingLessonContent);
                return RedirectToAction(nameof(Index));
            }
            var lessons = await _lessonRepository.GetAllAsync();
            ViewBag.Lessons = new SelectList(lessons, "Id", "LessonName");
            return View(lessoncontent);
        }

        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var lessoncontent = await _lessoncontentRepository.GetByIdAsync(id);
            if (lessoncontent == null)
            {
                return NotFound();
            }
            return View(lessoncontent);
        }
        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _lessoncontentRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ByLesson(int lessonId)
        {
            // Kiểm tra lessonId hợp lệ
            if (lessonId <= 0)
            {
                return BadRequest("ID bài học không hợp lệ.");
            }

            // Đường dẫn tới tệp JSON trong thư mục wwwroot/json
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/json", $"lesson_{lessonId}.json");

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound($"Không tìm thấy bài học với ID {lessonId}.");
            }

            // Đọc nội dung JSON
            string jsonData = System.IO.File.ReadAllText(filePath);

            // Chuyển đổi JSON thành đối tượng Lesson
            Lesson lesson = JsonConvert.DeserializeObject<Lesson>(jsonData);

            // Kiểm tra lesson có tồn tại
            if (lesson == null)
            {
                return NotFound($"Không có nội dung bài học với ID {lessonId}.");
            }

            // Truyền toàn bộ đối tượng Lesson vào View
            return View(lesson);
        }
    }

}
