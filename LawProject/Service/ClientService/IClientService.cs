using LawProject.DTO;

namespace LawProject.Service.ClientService
{
  public interface IClientService
  {
    Task<IEnumerable<DailyEventDto>> GetAllPFAsync();
    Task<IEnumerable<ClientPJDto>> GetAllPJAsync();
    Task AddClientPF(DailyEventDto clientDTO);
    Task AddClientPJ(ClientPJDto clientDTO);

    Task<FisaClientDetaliataDto> GetFisaClientDetaliataAsync(int clientId, string clientType, string clientName);

    Task<FullFileDataDto> GetFullDataByFileNumberAsync(string fileNumber);
  }
}
