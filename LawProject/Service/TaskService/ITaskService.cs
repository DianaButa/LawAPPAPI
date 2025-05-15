using LawProject.DTO;
using LawProject.Models;

namespace LawProject.Service.TaskService
{
  public interface ITaskService
  {
    Task<WorkTask> CreateTaskAsync(CreateTaskDto dto);
    Task<WorkTask> CloseTaskAsync(int taskId, CloseTaskDto dto);


    Task<IEnumerable<WorkTask>> GetAllTasksAsync();

    Task<IEnumerable<WorkTask>> GetTaskByClient(int clientId, string clientType);

    Task<IEnumerable<WorkTask>> GetClosedTasksByClient(int clientId, string clientType);

    Task<IEnumerable<WorkTask>> GetTasksByLawyerIdAsync(int lawyerId);

    Task<List<WorkTask>> GetTasksByLawyerIdAndClosedStatusAsync(int lawyerId);

    Task<List<WorkTask>> GetTasksByLawyerIdAndOpenStatusAsync(int lawyerId);


    Task<WorkTask> EditTaskAsync(int taskId, CreateTaskDto dto);
    Task<bool> DeleteTaskAsync(int taskId);





  }
}
