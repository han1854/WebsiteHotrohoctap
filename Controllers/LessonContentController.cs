using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            ViewBag.Lessons = new SelectList(lessons, "LessonID", "LessonName", lessoncontent.LessonID);
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
            ViewBag.Lessons = new SelectList(lessons, "LessonID", "LessonName");
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

        public async Task<IActionResult> ByLesson(int lessonId)
        {
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
            {
                return NotFound();
            }

            ViewBag.Lesson = lesson;
            var lessonContents = await _lessoncontentRepository.GetByLessonIdAsync(lesson.LessonID);

            return View(lessonContents);
        }
    }
}
