using LawProject.Models;

namespace LawProject.DTO
{
  public class LawyerOverviewDto
  {
    public int LawyerId { get; set; }
    public string LawyerName { get; set; }

    public List<CreateFileDto> OpenFiles { get; set; }
    public List<CreateFileDto> ClosedFiles { get; set; }

    public List<WorkTask> OpenTasks { get; set; }
    public List<WorkTask> ClosedTasks { get; set; }


    public int OpenFilesCount { get; set; }
    public int ClosedFilesCount { get; set; }
    public int OpenTasksCount { get; set; }
    public int ClosedTasksCount { get; set; }
  }
}
