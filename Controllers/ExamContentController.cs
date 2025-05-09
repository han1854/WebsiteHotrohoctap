using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using WebsiteHotrohoctap.Models;
using WebsiteHotrohoctap.Repositories;

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
            if (examcontent.QuestionType == "MultipleChoice")
            {
                // Kiểm tra các điều kiện của câu hỏi trắc nghiệm
                if (examcontent.Options == null || examcontent.Options.Count < 4 || examcontent.Options.Any(opt => string.IsNullOrWhiteSpace(opt)))
                {
                    ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ 4 đáp án.");
                    await LoadExamsAsync();
                    return View(examcontent);
                }

                if (string.IsNullOrWhiteSpace(examcontent.CorrectAnswer))
                {
                    ModelState.AddModelError(string.Empty, "Vui lòng chọn đáp án đúng.");
                    await LoadExamsAsync();
                    return View(examcontent);
                }
            }
            else if (examcontent.QuestionType == "Code")
            {
                // Không cần đáp án cho câu hỏi loại "Code"
                examcontent.CorrectAnswer = null;
                // Kiểm tra các trường khác liên quan đến code nếu cần (ví dụ: starter code, sample input, sample output)
                if (string.IsNullOrWhiteSpace(examcontent.StarterCode) || string.IsNullOrWhiteSpace(examcontent.SampleInput) || string.IsNullOrWhiteSpace(examcontent.SampleOutput))
                {
                    ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ thông tin code mẫu, input mẫu và output mẫu.");
                    await LoadExamsAsync();
                    return View(examcontent);
                }
            }

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
            existingExamContent.Options = examcontent.Options;
            existingExamContent.CorrectAnswer = examcontent.QuestionType == "MultipleChoice" ? examcontent.CorrectAnswer : null;
            existingExamContent.StarterCode = examcontent.StarterCode;
            existingExamContent.SampleInput = examcontent.SampleInput;
            existingExamContent.SampleOutput = examcontent.SampleOutput;
            existingExamContent.ExamID = examcontent.ExamID;

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
