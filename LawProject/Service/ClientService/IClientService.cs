using LawProject.DTO;

namespace LawProject.Service.ClientService
{
  public interface IClientService
  {
    Task<IEnumerable<ClientDto>> GetAllAsync();

    Task AddClient(ClientDto clientDTO);
  }
}
