using LawProject.Models;

namespace LawProject.DTO
{
  public class RaportTaskDto
  {
    public int WorkTaskId { get; set; }

    public string WorkTaskTitle { get; set; }
    public string WorkTaskFileNumber { get; set; }
    public double? OreLucrate { get; set; }
    public WorkTask? WorkTask { get; set; }
  }
}
