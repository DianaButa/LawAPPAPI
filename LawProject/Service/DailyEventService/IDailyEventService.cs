using LawProject.DTO;

namespace LawProject.Service.DailyEventService
{
  public interface IDailyEventService
  {
    Task AddDailyEventAsync(DailyEventsDto Dto);
    Task<List<DailyEventsDto>> GetAllDailyEventsAsync();
    Task<List<DailyEventsDto>> GetDailyEventsByLawyerIdAsync(int lawyerId);
  }
}
