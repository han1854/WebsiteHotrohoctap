using WebsiteHotrohoctap.Models;

namespace WebsiteHotrohoctap.Repositories
{
    public interface IExamContentRepository
    {
        Task<IEnumerable<ExamContent>> GetAllAsync();
        Task<ExamContent> GetByIdAsync(int id);
        Task AddAsync(ExamContent examContent);
        Task UpdateAsync(ExamContent examContent);
        Task DeleteAsync(int id);
    }
}
