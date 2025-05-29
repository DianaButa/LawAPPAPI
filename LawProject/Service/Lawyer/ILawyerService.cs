using LawProject.DTO;

namespace LawProject.Service.Lawyer
{
  public interface ILawyerService
  {
    Task<IEnumerable<LawyerDto>> GetAllLawyersAsync();
    Task<LawyerDto> AddLawyerAsync(LawyerDto lawyerDto);

    Task<LawyerOverviewDto> GetOverviewByLawyerIdAsync(int lawyerId);

    Task<List<LawyerOverviewDto>> GetAllLawyerOverviewsAsync();
    Task<LawyerDashboardDto> GetLawyerDashboardDataAsync(int lawyerId, DateTime? startDate, DateTime? endDate);
  }
}
