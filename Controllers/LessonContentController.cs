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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LessonContent lessoncontent, IFormFile? ImageFile, IFormFile? VideoFile, string? VideoUrl, string? TextContent)
        {
            if (lessoncontent.ContentType == "text")
            {
                if (string.IsNullOrWhiteSpace(TextContent))
                {
                    ModelState.AddModelError("TextContent", "Nhập nội dung văn bản.");
                }
                else
                {
                    lessoncontent.ContentData = TextContent;
                }
            }
            else if (lessoncontent.ContentType == "image")
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }
                    lessoncontent.ContentData = "/images/" + fileName;
                }
                else
                {
                    ModelState.AddModelError("ImageFile", "Chọn một file ảnh.");
                }
            }
            else if (lessoncontent.ContentType == "video")
            {
                if (VideoFile != null && VideoFile.Length > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(VideoFile.FileName);
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/videos", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await VideoFile.CopyToAsync(stream);
                    }
                    lessoncontent.ContentData = "/videos/" + fileName;
                }
                else if (!string.IsNullOrWhiteSpace(VideoUrl))
                {
                    lessoncontent.ContentData = VideoUrl;
                }
                else
                {
                    ModelState.AddModelError("VideoFile", "Chọn file hoặc nhập link video.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Lessons = new SelectList(await _lessonRepository.GetAllAsync(), "LessonID", "LessonName");
                return View(lessoncontent);
            }

            await _lessoncontentRepository.AddAsync(lessoncontent);
            foreach (var modelState in ModelState)
            {
                foreach (var error in modelState.Value.Errors)
                {
                    Console.WriteLine($"Lỗi ở {modelState.Key}: {error.ErrorMessage}");
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImageAsync(IFormFile image)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return "/images/" + fileName;
        }

        private async Task<string> SaveVideoAsync(IFormFile video)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(video.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/videos", fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await video.CopyToAsync(stream);
            }

            return "/videos/" + fileName;
        }


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
        public async Task<IActionResult> Update(int id)
        {
            var lessoncontent = await _lessoncontentRepository.GetByIdAsync(id);
            if (lessoncontent == null)
            {
                return NotFound();
            }

            var lessons = await _lessonRepository.GetAllAsync();
            ViewBag.Lessons = new SelectList(lessons, "LessonID", "LessonName");

            return View(lessoncontent);
        }

        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, LessonContent lessoncontent, IFormFile? ImageFile, IFormFile? VideoFile, string? VideoUrl, string? TextContent)
        {
            if (id != lessoncontent.ContentID)
            {
                return NotFound();
            }

            if (lessoncontent.ContentType == "text")
            {
                if (string.IsNullOrWhiteSpace(TextContent))
                {
                    ModelState.AddModelError("TextContent", "Nhập nội dung văn bản.");
                }
                else
                {
                    lessoncontent.ContentData = TextContent;
                }
            }
            else if (lessoncontent.ContentType == "image")
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Xử lý ảnh mới
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }
                    lessoncontent.ContentData = "/images/" + fileName;
                }
                else if (string.IsNullOrEmpty(lessoncontent.ContentData))
                {
                    // Nếu không có ảnh mới và cũng không có ảnh cũ, yêu cầu chọn ảnh
                    ModelState.AddModelError("ImageFile", "Chọn một file ảnh.");
                }
            }
            else if (lessoncontent.ContentType == "video")
            {
                if (VideoFile != null && VideoFile.Length > 0)
                {
                    // Xử lý video mới
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(VideoFile.FileName);
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/videos", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await VideoFile.CopyToAsync(stream);
                    }
                    lessoncontent.ContentData = "/videos/" + fileName;
                }
                else if (!string.IsNullOrWhiteSpace(VideoUrl))
                {
                    // Nếu có link video, lưu lại URL
                    lessoncontent.ContentData = VideoUrl;
                }
                else if (string.IsNullOrEmpty(lessoncontent.ContentData))
                {
                    // Nếu không có video mới và cũng không có video cũ, yêu cầu chọn video hoặc nhập URL
                    ModelState.AddModelError("VideoFile", "Chọn file hoặc nhập link video.");
                }
            }

            // Kiểm tra tính hợp lệ của ModelState
            if (!ModelState.IsValid)
            {
                var lessons = await _lessonRepository.GetAllAsync();
                ViewBag.Lessons = new SelectList(lessons, "LessonID", "LessonName");
                return View(lessoncontent);
            }

            var existingLessonContent = await _lessoncontentRepository.GetByIdAsync(id);
            existingLessonContent.ContentType = lessoncontent.ContentType;
            existingLessonContent.ContentData = lessoncontent.ContentData;
            existingLessonContent.LessonID = lessoncontent.LessonID;

            // Cập nhật bài học
            await _lessoncontentRepository.UpdateAsync(existingLessonContent);

            return RedirectToAction(nameof(Index));
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
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lessoncontent = await _lessoncontentRepository.GetByIdAsync(id);
            if (lessoncontent == null)
            {
                return NotFound();
            }

            await _lessoncontentRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ByLesson(int lessonId)
        {
            // Kiểm tra lessonId hợp lệ
            if (lessonId <= 0)
            {
                return BadRequest("ID bài học không hợp lệ.");
            }

            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
            {
                return NotFound();
            }

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/json", $"lesson_{lessonId}.json");

            // Kiểm tra nếu file JSON tồn tại
            if (System.IO.File.Exists(filePath))
            {
                string jsonData = System.IO.File.ReadAllText(filePath);
                Lesson lessonjs = JsonConvert.DeserializeObject<Lesson>(jsonData);

                if (lessonjs == null)
                {
                    return NotFound($"Không có nội dung bài học với ID {lessonId}.");
                }

                // Trả về đối tượng lessonjs nếu file JSON tồn tại
                return View(lessonjs);
            }
            else
            {
                // Nếu không có file JSON, lấy tất cả nội dung bài học từ cơ sở dữ liệu
                var lessonContents = await _lessoncontentRepository.GetByLessonIdAsync(lessonId);

                // Gán danh sách nội dung bài học vào đối tượng lesson
                lesson.LessonContents = lessonContents;

                // Truyền đối tượng Lesson vào View (bao gồm cả thông tin bài học và nội dung)
                return View(lesson);
            }
        }


    }

}