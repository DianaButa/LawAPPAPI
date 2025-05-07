using LawProject.DTO;
using LawProject.Models;

namespace LawProject.Service.TaskService
{
  public interface ITaskService
  {
    Task<WorkTask> CreateTaskAsync(CreateTaskDto dto);
    Task<WorkTask> CloseTaskAsync(int taskId, CloseTaskDto dto);


    Task<IEnumerable<WorkTask>> GetAllTasksAsync();

    Task<IEnumerable<WorkTask>> GetTasksByLawyerIdAsync(int lawyerId);


    Task<WorkTask> EditTaskAsync(int taskId, CreateTaskDto dto);
    Task<bool> DeleteTaskAsync(int taskId);



  }
}
