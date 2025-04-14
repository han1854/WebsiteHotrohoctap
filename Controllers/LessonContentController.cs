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

        public async Task<IActionResult> ByLesson(int lessonId)
        {
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
            {
                return NotFound();
            }

            ViewBag.Lesson = lesson;

            var lessonContents = await _lessoncontentRepository.GetByLessonIdAsync(lessonId);
            return View(lessonContents);
        }
    }
}
