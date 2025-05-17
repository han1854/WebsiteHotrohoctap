using WebsiteHotrohoctap.Models;

namespace WebsiteHotrohoctap.Repositories
{
    public interface IExamRepository
    {
        Task<Exam> GetByIdAsync(int id);
        Task<Exam> GetExamWithContentsAsync(int examId);
        Task<List<Exam>> GetAllAsync();
        Task AddExamAsync(Exam exam);
        Task UpdateAsync(Exam exam);
        Task DeleteAsync(int id);
        Task<IEnumerable<ExamContent>> GetTestCasesAsync(int examId);
        Task AddExamResultAsync(ExamResult result);
    }
}
