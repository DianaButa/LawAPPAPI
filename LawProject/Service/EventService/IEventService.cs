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

    Task<EventCDTO?> UpdateEventCAsync(int eventId, EventCDTO dto);

    Task<EventADTO?> UpdateEventAAsync(int eventId, EventADTO dto);
    Task<EventCDTO?> GetEventCByIdAsync(int eventId);

    Task<EventADTO?> GetEventAByIdAsync(int eventId);
    Task DeleteEventC(int eventId);
    Task DeleteEventA(int eventId);


  }
}
