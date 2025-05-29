using LawProject.DTO;
using LawProject.Models;
using System.Linq;

public class FullFileDataDto
{
  public string FileNumber { get; set; }
  public string FileStatus { get; set; }
  public List<CreateFileDto> Files { get; set; } = new();
  public List<DailyEventsDto> DailyEvents { get; set; } = new();
  public List<WorkTask> ClosedTasks { get; set; } = new();
  public List<WorkTask> OpenedTasks { get; set; } = new();
  public IEnumerable<Raport> Rapoarte { get; set; }

  public int FilesCount => Files.Count;            
  public int DailyEventsCount => DailyEvents.Count; 
  public int ClosedTasksCount => ClosedTasks.Count;
  public int OpenedTasksCount => OpenedTasks.Count;
  public int RapoarteCount => Rapoarte.Count();       
}
