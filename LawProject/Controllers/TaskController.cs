using LawProject.DTO;
using LawProject.Service.TaskService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LawProject.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class TaskController : ControllerBase
  {

    private readonly ITaskService _taskService;
    private readonly ILogger<TaskController> _logger;

    public TaskController(ITaskService taskService, ILogger<TaskController> logger)
    {
      _taskService = taskService;
      _logger = logger;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      try
      {
        var taskCreated = await _taskService.CreateTaskAsync(dto);
        return Ok(taskCreated);
      }
      catch (Exception ex)
      {
        _logger.LogError($"Eroare la crearea task-ului: {ex.Message}");
        return BadRequest(ex.Message);
      }
    }

    [HttpPut("close/{id}")]
    public async Task<IActionResult> CloseTask(int id, [FromBody] CloseTaskDto dto)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      try
      {
        var taskClosed = await _taskService.CloseTaskAsync(id, dto);
        return Ok(taskClosed);
      }
      catch (Exception ex)
      {
        _logger.LogError($"Eroare la închiderea task-ului: {ex.Message}");
        return BadRequest(ex.Message);
      }
    }


    [HttpGet]
    public async Task<IActionResult> GetAllTasks()
    {
      try
      {
        var tasks = await _taskService.GetAllTasksAsync();
        return Ok(tasks);
      }
      catch (Exception ex)
      {
        _logger.LogError($"Eroare la obținerea task-urilor: {ex.Message}");
        return BadRequest(ex.Message);
      }
    }


    [HttpGet("by-lawyer")]

    public async Task<IActionResult> GetTasksByLawyerId([FromQuery] int lawyerId)
    {
      try
      {
        var tasks = await _taskService.GetTasksByLawyerIdAsync(lawyerId);
        return Ok(tasks);
      }
      catch (Exception ex)
      {
        _logger.LogError($"Eroare la obținerea task-urilor pentru avocatul cu ID {lawyerId}: {ex.Message}");
        return BadRequest(ex.Message);
      }
    }



    [HttpPut("edit/{id}")]
    public async Task<IActionResult> EditTask(int id, [FromBody] CreateTaskDto dto)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      try
      {
        var updatedTask = await _taskService.EditTaskAsync(id, dto);
        return Ok(updatedTask);
      }
      catch (Exception ex)
      {
        _logger.LogError($"Eroare la actualizarea task-ului: {ex.Message}");
        return BadRequest(ex.Message);
      }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
      try
      {
        var success = await _taskService.DeleteTaskAsync(id);
        if (!success)
        {
          return NotFound($"Taskul cu ID {id} nu a fost găsit.");
        }

        return Ok($"Taskul cu ID {id} a fost șters.");
      }
      catch (Exception ex)
      {
        _logger.LogError($"Eroare la ștergerea task-ului: {ex.Message}");
        return BadRequest(ex.Message);
      }
    }

  }
}
