using WebsiteHotrohoctap.Models;

namespace WebsiteHotrohoctap.Repositories
{
    public interface ILessonContentRepository
    {
        Task<IEnumerable<LessonContent>> GetAllAsync();
        Task<LessonContent> GetByIdAsync(int id);
        Task AddAsync(LessonContent lessonContent);
        Task UpdateAsync(LessonContent lessonContent);
        Task DeleteAsync(int id);
        Task<List<LessonContent>> GetByLessonIdAsync(int lessonId);

    }
}
