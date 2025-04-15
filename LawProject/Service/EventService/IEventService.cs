using LawProject.DTO;

namespace LawProject.Service.EventService
{
  public interface IEventService
  {
    Task<EventADTO> AddEventAAsync(EventADTO eventADto);
    Task<List<EventADTO>> GetAllEventsAAsync();

    Task<EventCDTO> AddEventCAsync(EventCDTO eventCDto);
    Task<List<EventCDTO>> GetAllEventsCAsync();
  }
}
