using LawProject.DTO;

namespace LawProject.Service.ClientService
{
  public interface IClientService
  {
    Task<IEnumerable<ClientPFDto>> GetAllPFAsync();
    Task<IEnumerable<ClientPJDto>> GetAllPJAsync();
    Task AddClientPF(ClientPFDto clientDTO);
    Task AddClientPJ(ClientPJDto clientDTO);

    Task<FullFileDataDto> GetFullDataByFileNumberAsync(string fileNumber, DateTime? startDate, DateTime? endDate);
    Task<FisaClientDetaliataDto> GetFisaClientDetaliataAsync(int clientId, string clientType, string clientName, DateTime? startDate, DateTime? endDate);


    Task UpdateClientPF(int clientId, ClientPFDto clientDto);
    Task UpdateClientPJ(int clientId, ClientPJDto clientDto);
    Task DeleteClientPF(int clientId);
    Task DeleteClientPJ(int clientId);

    Task<object> GetClientEntityByIdAndTypeAsync(int clientId, string clientType);

  }
}
