using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using Microsoft.EntityFrameworkCore;

namespace LawProject.Service.ClientService
{
  public class ClientService : IClientService
  {
    private readonly ApplicationDbContext _context;

    public ClientService(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<IEnumerable<ClientDto>> GetAllAsync()
    {
      var clients = await _context.Clients.ToListAsync();

      var filesDTOs = clients.Select(client => new ClientDto
      {
        Id = client.Id,
        FirstName = client.FirstName,
        LastName = client.LastName,
        CNP = client.CNP,
        Email = client.Email,
        Address = client.Address,
      });

      return filesDTOs;
    }
    public async Task AddClient(ClientDto clientDTO)
    {
      var newClient = new Client
      {
        FirstName = clientDTO.FirstName,
        LastName = clientDTO.LastName,
        CNP = clientDTO.CNP,
        Email = clientDTO.Email,
        Address = clientDTO.Address,
      };

      _context.Clients.Add(newClient);
      await _context.SaveChangesAsync();
    }
  }
}
