using LawProject.Models;

namespace LawProject.DTO
{
  public class LawyerDashboardDto
  {
    public int LawyerId { get; set; }
    public string LawyerName { get; set; }
    public List<CreateFileDto> Files { get; set; } = new();
    public List<WorkTask> OpenTasks { get; set; } = new();
    public List<WorkTask> ClosedTasks { get; set; } = new();
    public List<RaportDto> Raport { get; set; } = new();

    public List<DailyEventsDto> DailyEvents { get; set; } =new();

    public int FilesCount => Files.Count;
    public int OpenTasksCount => OpenTasks.Count;
    public int ClosedTasksCount => ClosedTasks.Count;
    public int RapoarteCount => Raport.Count;
  }
}
