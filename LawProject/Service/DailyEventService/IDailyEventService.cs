using LawProject.DTO;
using LawProject.Models;

namespace LawProject.Service.DailyEventService
{
  public interface IDailyEventService
  {
    Task AddDailyEventAsync(DailyEventsDto Dto);
    Task<List<DailyEventsDto>> GetAllDailyEventsAsync();
    Task<List<DailyEventsDto>> GetDailyEventsByLawyerIdAsync(int lawyerId);

    Task<IEnumerable<DailyEventsDto>> GetEventsByClient(string clientName);
  }
}
