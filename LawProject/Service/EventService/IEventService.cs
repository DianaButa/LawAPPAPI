using LawProject.DTO;
using LawProject.Models;

namespace LawProject.Service.EventService
{
  public interface IEventService
  {
    Task<EventADTO> AddEventAAsync(EventADTO eventADto);

    Task<IEnumerable<EventC>> GetEventCByClient(int clientId, string clientType);
    Task<IEnumerable<EventA>> GetEventAByClient(int clientId, string clientType);
    Task<List<EventADTO>> GetAllEventsAAsync();

    Task<EventCDTO> AddEventCAsync(EventCDTO eventCDto);
    Task<List<EventCDTO>> GetAllEventsCAsync();
  }
}
