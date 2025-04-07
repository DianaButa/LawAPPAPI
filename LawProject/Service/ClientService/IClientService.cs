using LawProject.DTO;

namespace LawProject.Service.ClientService
{
  public interface IClientService
  {
    Task<IEnumerable<ClientPFDto>> GetAllPFAsync();
    Task<IEnumerable<ClientPJDto>> GetAllPJAsync();
    Task AddClientPF(ClientPFDto clientDTO);
    Task AddClientPJ(ClientPJDto clientDTO);
  }
}
