using System.ComponentModel.DataAnnotations;

namespace LawProject.DTO
{
  public class CloseTaskDto
  {
    [Required(ErrorMessage = "TaskId este obligatoriu.")]
    public int TaskId { get; set; }

    [Required(ErrorMessage = "Numărul de ore lucrate este obligatoriu la închidere.")]
    public double HoursWorked { get; set; }

 
  }
}
