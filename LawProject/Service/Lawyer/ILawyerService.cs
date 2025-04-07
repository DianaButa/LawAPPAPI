using LawProject.DTO;

namespace LawProject.Service.Lawyer
{
  public interface ILawyerService
  {
    Task<IEnumerable<LawyerDto>> GetAllLawyersAsync();
    Task<LawyerDto> AddLawyerAsync(LawyerDto lawyerDto);
  }
}
