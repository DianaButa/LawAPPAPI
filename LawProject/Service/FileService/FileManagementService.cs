using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using LawProject.Service.Notifications;
using Microsoft.EntityFrameworkCore;
using System.ServiceModel;

namespace LawProject.Service.FileService
{
  public class IFileManagementService
  {
    private readonly ApplicationDbContext _context;
    private readonly MyQueryService _queryService;
    private readonly ILogger<IFileManagementService> _logger;
    private readonly INotificationService _notificationService;

    public IFileManagementService(
        ApplicationDbContext context,
        MyQueryService queryService,
        ILogger<IFileManagementService> logger,
        INotificationService notificationService)
    {
      _context = context;
      _queryService = queryService;
      _logger = logger;
      _notificationService = notificationService;
    }

    public async Task UpdateScheduledEventAsync(ScheduledEvent scheduledEvent)
    {
      // Actualizează evenimentul în baza de date
      _context.ScheduledEvents.Update(scheduledEvent);
      await _context.SaveChangesAsync();

      // Create notification for updated event
      var notification = new Notification
      {
        Title = "Eveniment actualizat",
        Message = $"Evenimentul pentru dosarul {scheduledEvent.FileNumber} a fost actualizat",
        Timestamp = DateTime.UtcNow,
        Type = "event_updated",
        FileNumber = scheduledEvent.FileNumber,
        Source=scheduledEvent.Source,
        IsRead = false,
        Details = $"Data: {scheduledEvent.StartTime:dd.MM.yyyy HH:mm}, Tip: {scheduledEvent.TipDosar}",
        UserId = 1 // TODO: Get from current user context
      };

      await _notificationService.CreateNotificationAsync(notification);
    }

    public async Task<List<ScheduledEvent>> GetScheduledEventsAsync()
    {
      return await _context.ScheduledEvents.ToListAsync();
    }

    public async Task<ScheduledEvent> GetScheduledEventAsync(string fileNumber, DateTime startTime)
    {
      return await _context.ScheduledEvents
          .FirstOrDefaultAsync(e => e.FileNumber == fileNumber && e.StartTime == startTime);
    }

    public async Task<IEnumerable<ScheduledEvent>> GetScheduledEventbyClientIdAsync(int clientId, string clientName)
    {
      var events = await _context.ScheduledEvents
  .Where(e => e.ClientId == clientId && e.ClientName.ToLower().Contains(clientName.ToLower()))

     .ToListAsync();

      _logger.LogInformation($"Found {events.Count} scheduled events");

      return events;
    }



      // Metodă pentru a adăuga un eveniment nou în baza de date
      public async Task AddScheduledEventAsync(ScheduledEvent scheduledEvent)
    {
      _context.ScheduledEvents.Add(scheduledEvent);
      await _context.SaveChangesAsync();

      // Create notification for new scheduled event
      var notification = new Notification
      {
        Title = "Eveniment nou programat",
        Message = $"Un nou eveniment a fost programat pentru dosarul {scheduledEvent.FileNumber}",
        Timestamp = DateTime.UtcNow,
        Type = "event_scheduled",
        FileNumber = scheduledEvent.FileNumber,
        Source=scheduledEvent.Source,
        IsRead = false,
        Details = $"Data: {scheduledEvent.StartTime:dd.MM.yyyy HH:mm}, Tip: {scheduledEvent.TipDosar}",
        UserId = 1 // TODO: Get from current user context
      };

      await _notificationService.CreateNotificationAsync(notification);
    }



    public async Task<List<AllFilesDto>> GetAllFilesAsync()
    {
      _logger.LogInformation("Fetching all files from the database...");

      var dbFiles = await _context.Files
        .Include(f => f.Lawyer)
        .ToListAsync();

      var basicDtos = dbFiles.Select(f => new CreateFileDto
      {
        Id = f.Id,
        FileNumber = f.FileNumber,
        ClientName = f.ClientName,
        Details = f.Details,
        TipDosar = f.TipDosar,
        Instanta = f.Instanta,
        Status = f.Status,
        Onorariu = f.Onorariu,
        Outcome = f.Outcome,
        Source = f.Source,
        Moneda = f.Moneda,
        OnorariuRestant = f.OnorariuRestant,
        Delegatie = f.Delegatie,
        DataScadenta = f.DataScadenta,
        NumarContract = f.NumarContract,
        LawyerName = f.Lawyer?.LawyerName ?? string.Empty,
        LawyerId = f.Lawyer?.Id
      }).ToList();

      if (!basicDtos.Any())
      {
        _logger.LogWarning("No files found in the database.");
        return new List<AllFilesDto>();
      }

      var combinedFilesList = new List<AllFilesDto>();

      // Separă dosarele JUST și restul
      var justFiles = basicDtos.Where(f => f.Source?.ToUpper() == "JUST").ToList();
      var otherFiles = basicDtos.Where(f => f.Source?.ToUpper() != "JUST").ToList();

      // Adaugă direct dosarele non-JUST fără apel SOAP
      combinedFilesList.AddRange(otherFiles.Select(f => new AllFilesDto
      {
        Id = f.Id,
        FileNumber = f.FileNumber,
        ClientName = f.ClientName,
        Details = f.Details,
        Status = f.Status,
        Onorariu = f.Onorariu,
        Moneda = f.Moneda,
        OnorariuRestant = f.OnorariuRestant,
        Delegatie = f.Delegatie,
        CuvantCheie = f.CuvantCheie,
        NumarContract = f.NumarContract,
        DataScadenta = f.DataScadenta,
        OutCome = f.Outcome,
        Source = f.Source,
        TipDosar = f.TipDosar,
        Instanta = f.Instanta,
        LawyerName = f.LawyerName,
        LawyerId = f.LawyerId
      }));

      // Trimite apelurile SOAP pe batch-uri de 20
      int batchSize = 20;

      for (int i = 0; i < justFiles.Count; i += batchSize)
      {
        var batch = justFiles.Skip(i).Take(batchSize);

        var tasks = batch.Select(async dbFile =>
        {
          _logger.LogInformation($"Fetching SOAP details for file number: {dbFile.FileNumber}");

          var soapDosare = await _queryService.CautareDosareAsync(dbFile.FileNumber);

          if (soapDosare == null || !soapDosare.Any())
          {
            _logger.LogWarning($"No SOAP details found for file number: {dbFile.FileNumber}");
            return new AllFilesDto
            {
              Id = dbFile.Id,
              FileNumber = dbFile.FileNumber,
              ClientName = dbFile.ClientName,
              Details = dbFile.Details,
              Status = dbFile.Status,
              Onorariu = dbFile.Onorariu,
              Moneda = dbFile.Moneda,
              OnorariuRestant = dbFile.OnorariuRestant,
              Delegatie = dbFile.Delegatie,
              CuvantCheie = dbFile.CuvantCheie,
              NumarContract = dbFile.NumarContract,
              DataScadenta = dbFile.DataScadenta,
              OutCome = dbFile.Outcome,
              Source = dbFile.Source,
              TipDosar = dbFile.TipDosar,
              Instanta = dbFile.Instanta,
              LawyerName = dbFile.LawyerName,
              LawyerId = dbFile.LawyerId
            };
          }

          var mostRecentSoapDosar = soapDosare
              .OrderByDescending(d => d.data)
              .FirstOrDefault();

          if (mostRecentSoapDosar != null)
          {
            return new AllFilesDto
            {
              Id = dbFile.Id,
              FileNumber = dbFile.FileNumber,
              ClientName = dbFile.ClientName,
              Details = dbFile.Details,
              TipDosar = dbFile.TipDosar,
              Status = dbFile.Status,
              Source = dbFile.Source,
              LawyerName = dbFile.LawyerName,
              Instanta = dbFile.Instanta,
              LawyerId = dbFile.LawyerId,
              Onorariu = dbFile.Onorariu,
              Moneda = dbFile.Moneda,
              CuvantCheie = dbFile.CuvantCheie,
              OnorariuRestant = dbFile.OnorariuRestant,
              Delegatie = dbFile.Delegatie,
              NumarContract = dbFile.NumarContract,
              DataScadenta = dbFile.DataScadenta,

              Numar = mostRecentSoapDosar.numar,
              Data = mostRecentSoapDosar.data,
              Institutie = mostRecentSoapDosar.institutie.ToString(),
              ObiectDosar = mostRecentSoapDosar.obiect.ToString(),
              Parti = mostRecentSoapDosar.parti?.Select(p => new ParteDTO
              {
                Nume = p.nume,
                CalitateParte = p.calitateParte
              }).ToList(),
              Sedinte = mostRecentSoapDosar.sedinte?.Select(s => new SedintaDTO
              {
                Complet = s.complet,
                Data = s.data,
                Ora = s.ora,
                Solutie = s.solutie,
                SolutieSumar = s.solutieSumar,
                DataPronuntare = s.dataPronuntare,
                DocumentSedinta = s.documentSedinta?.ToString() ?? string.Empty,
                NumarDocument = s.numarDocument,
                DataDocument = s.dataDocument
              }).ToList()
            };
          }

          // fallback, dacă nu există dosar în SOAP
          return new AllFilesDto
          {
            Id = dbFile.Id,
            FileNumber = dbFile.FileNumber,
            ClientName = dbFile.ClientName,
            Details = dbFile.Details,
            Status = dbFile.Status,
            Onorariu = dbFile.Onorariu,
            Moneda = dbFile.Moneda,
            OnorariuRestant = dbFile.OnorariuRestant,
            Delegatie = dbFile.Delegatie,
            CuvantCheie = dbFile.CuvantCheie,
            NumarContract = dbFile.NumarContract,
            DataScadenta = dbFile.DataScadenta,
            OutCome = dbFile.Outcome,
            Source = dbFile.Source,
            TipDosar = dbFile.TipDosar,
            Instanta = dbFile.Instanta,
            LawyerName = dbFile.LawyerName,
            LawyerId = dbFile.LawyerId
          };
        });

        var results = await Task.WhenAll(tasks);

        combinedFilesList.AddRange(results);
      }

      _logger.LogInformation("Finished processing all file details.");
      return combinedFilesList;
    }





    public async Task AddFileAsync(CreateFileDto dto)
    {
      _logger.LogInformation("Adding new file...");

      try
      {
        // Verificăm că numărul fișierului este valid
        if (string.IsNullOrEmpty(dto.FileNumber))
        {
          _logger.LogError("File number is required.");
          throw new ArgumentException("Numărul fișierului este obligatoriu.", nameof(dto.FileNumber));
        }

        // Verificăm tipul clientului și căutăm în tabela corespunzătoare
        ClientPF clientPF = null;
        ClientPJ clientPJ = null;

        if (dto.ClientType == "PF")
        {
          clientPF = await _context.ClientPFs.FindAsync(dto.ClientId);
        }
        else if (dto.ClientType == "PJ")
        {
          clientPJ = await _context.ClientPJs.FindAsync(dto.ClientId);
        }
        else
        {
          _logger.LogError($"Client type {dto.ClientType} is invalid.");
          throw new ArgumentException($"Tipul clientului {dto.ClientType} este invalid.", nameof(dto.ClientType));
        }

        // Dacă nu găsim clientul în tabelul corespunzător, returnăm eroare
        if (clientPF == null && clientPJ == null)
        {
          _logger.LogError($"Client with ID {dto.ClientId} not found.");
          throw new ArgumentException($"Clientul cu ID {dto.ClientId} nu a fost găsit.", nameof(dto.ClientId));
        }

        // Verificăm dacă avocatul este specificat și dacă există în baza de date
        LawProject.Models.Lawyer lawyer = null;
        if (dto.LawyerId.HasValue)
        {
          lawyer = await _context.Lawyers.FindAsync(dto.LawyerId.Value);
          if (lawyer == null)
          {
            _logger.LogError($"Lawyer with ID {dto.LawyerId} not found.");
            throw new ArgumentException($"Avocatul cu ID {dto.LawyerId} nu a fost găsit.", nameof(dto.LawyerId));
          }
        }


        // Creăm fișierul
        var file = new MyFile
        {
          FileNumber = dto.FileNumber,
          ClientName = clientPF != null ? $"{clientPF.FirstName} {clientPF.LastName}" : clientPJ?.CompanyName,
          ClientId = dto.ClientId,
          Details = dto.Details,
          Onorariu=dto.Onorariu,
          Source=dto.Source,
          CuvantCheie=dto.CuvantCheie,
          Moneda = dto.Moneda.ToUpper(), 
          OnorariuRestant = $"{dto.Onorariu} {dto.Moneda.ToUpper()}",
          DataScadenta =dto.DataScadenta,
          Delegatie=dto.Delegatie,
          NumarContract=dto.NumarContract,
          TipDosar = dto.TipDosar,
          Instanta= dto.Instanta,
          CuloareCalendar = lawyer?.Color ?? "#E0E0E0",
          LawyerId = dto.LawyerId,
          Status = "deschis",
      
        };

        // Adăugăm fișierul în context
        _context.Files.Add(file);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"File with file number {dto.FileNumber} added successfully.");
        var notification = new Notification
        {
          Title = "Fișier nou adăugat",
          Message = $"Un nou fișier a fost adăugat: {dto.FileNumber}",
          Timestamp = DateTime.UtcNow,
          Type = "new_file",
          FileNumber = dto.FileNumber,
          IsRead = false,
          Source=dto.Source,
          Details = $"Client: {file.ClientName}, Tip dosar: {dto.TipDosar}",
          UserId = 1 
        };
        await _notificationService.CreateNotificationAsync(notification);

        _logger.LogInformation($"Notification for file {dto.FileNumber} created successfully.");
      }
      catch (ArgumentException ex)
      {
        _logger.LogError($"Validation error: {ex.Message}");
        throw; 
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while adding the file: {ex.Message}");
        if (ex.InnerException != null)
        {
          _logger.LogError($"Inner exception: {ex.InnerException.Message}");
        }
        throw;
      }
    }


    public async Task CloseFileAsync(int id, string outcome)
    {
      _logger.LogInformation($"Closing file with ID {id}...");

      var fileToClose = await _context.Files.FindAsync(id);

      if (fileToClose == null)
      {
        _logger.LogWarning($"File with ID {id} not found.");
        throw new KeyNotFoundException($"Fișierul cu ID {id} nu a fost găsit.");
      }

      // Setăm statusul la "închis"
      fileToClose.Status = "închis";

      // Setăm rezultatul (Castigat sau Necastigat)
      if (outcome != "Castigat" && outcome != "Necastigat" && outcome != "Neutru")
      {
        _logger.LogError($"Outcome value is invalid: {outcome}");
        throw new ArgumentException($"Rezultatul {outcome} este invalid.");
      }


      fileToClose.Outcome = outcome;
      _context.Files.Update(fileToClose);
      await _context.SaveChangesAsync();

      _logger.LogInformation($"File with ID {id} closed and outcome set to {outcome}.");
    }

    public async Task<List<CreateFileDto>> GetAllClosedFilesAsync()
    {
      return await _context.Files
          .Include(f => f.Lawyer)
          .Where(f => f.Status == "Inchis" || f.Status == "închis")
          .Select(f => new CreateFileDto
          {
            Id = f.Id,
            FileNumber = f.FileNumber,
            ClientId = f.ClientId,
            ClientName = f.ClientName,
            TipDosar = f.TipDosar,
            Source = f.Source,
            Instanta = f.Instanta,
            Details = f.Details,
            NumarContract = f.NumarContract,
            Delegatie = f.Delegatie,
            CuvantCheie=f.CuvantCheie,
            Onorariu = f.Onorariu,
            Moneda = f.Moneda,
            DataScadenta = f.DataScadenta,
            LawyerId = f.Lawyer.Id,
            LawyerName = f.Lawyer.LawyerName,
            Status = f.Status,
            Outcome = f.Outcome
          })
          .ToListAsync();
    }




    public async Task<CreateFileDto> GetFileByNumberAsync(string fileNumber)
    {
      return await _context.Files
          .Include(d => d.Lawyer) 
          .Where(d => d.FileNumber == fileNumber)
          .Select(d => new CreateFileDto
          {
            Id = d.Id,
            FileNumber = d.FileNumber,
            ClientId = d.ClientId,
            ClientName = d.ClientName,
            TipDosar = d.TipDosar,
            Instanta=d.Instanta,
            Source=d.Source,
            Outcome=d.Outcome,
            Status=d.Status,
            Details = d.Details,
            NumarContract=d.NumarContract,
            Delegatie=d.Delegatie,
            Onorariu=d.Onorariu,
            CuvantCheie=d.CuvantCheie,
            Moneda=d.Moneda,
            OnorariuRestant=d.Onorariu,
            DataScadenta=d.DataScadenta,
            LawyerId = d.Lawyer != null ? d.Lawyer.Id : (int?)null, 
            LawyerName = d.Lawyer != null ? d.Lawyer.LawyerName : string.Empty
          })
          .FirstOrDefaultAsync();
    }


    public async Task<FileDetailsDto> GetFileDetailsByNumberAsync(string fileNumber)
    {
      _logger.LogInformation($"Fetching case details for file number: {fileNumber}");

      var dosare = await _queryService.CautareDosareAsync(fileNumber);
      if (dosare == null || !dosare.Any())
      {
        _logger.LogWarning($"No case details found for file number: {fileNumber}");
        return null; 
      }

          var fileDetails = dosare.Select(d => new FileDetailsDto
      {
        Numar = d.numar,
        Data = d.data,
        Institutie = d.institutie.ToString(), 
        Departament = d.departament,
        ObiectDosar= d.obiect.ToString(),
        
        CategorieCaz = d.categorieCaz?.ToString(), 
        StadiuProcesual = d.stadiuProcesual?.ToString(), 
        Parti = d.parti?.Select(p => new ParteDTO
        {
          Nume = p.nume,
          CalitateParte = p.calitateParte
        }).ToList(),
        Sedinte = d.sedinte?.Select(s => new SedintaDTO
        {
          Complet = s.complet,
          Data = s.data,
          Ora = s.ora,
          Solutie = s.solutie,
          SolutieSumar = s.solutieSumar,
          DataPronuntare = s.dataPronuntare,
          NumarDocument = s.numarDocument,
          DataDocument = s.dataDocument
        }).ToList(),
        CaiAtac = d.caiAtac?.Select(c => new CaleAtacDTO
        {
          ParteDeclaratoare = c.parteDeclaratoare,
          TipCaleAtac = c.tipCaleAtac
        }).ToList()
      }).FirstOrDefault();

      // Dacă detaliile fișierului sunt valide, returnează-le
      if (fileDetails != null)
      {
        _logger.LogInformation($"Returning case details for file number: {fileNumber}");
        return fileDetails;
      }

      _logger.LogWarning($"No valid details for file number: {fileNumber}");
      return null;
    }


    public async Task UpdateFileAsync(int id, CreateFileDto dto)
    {
      _logger.LogInformation($"Updating file with ID {id}...");

      var existingFile = await _context.Files
     .Include(f => f.Lawyer)  // Include Lawyer details to access LawyerName
     .FirstOrDefaultAsync(f => f.Id == id);

      if (existingFile == null)
      {
        _logger.LogWarning($"File with ID {id} not found.");
        throw new KeyNotFoundException($"Fișierul cu ID {id} nu a fost găsit.");
      }

      // Fetch client details from database if ClientId is provided
      ClientPF clientPF = null;
      ClientPJ clientPJ = null;

      if (dto.ClientId > 0)
      {
        clientPF = await _context.ClientPFs.FindAsync(dto.ClientId);
        clientPJ = await _context.ClientPJs.FindAsync(dto.ClientId);
      }

      if (clientPF == null && clientPJ == null)
      {
        _logger.LogError($"Client with ID {dto.ClientId} not found.");
        throw new ArgumentException($"Clientul cu ID {dto.ClientId} nu a fost găsit.", nameof(dto.ClientId));
      }

      // Update file details
      existingFile.FileNumber = dto.FileNumber;
      existingFile.Details = dto.Details ?? existingFile.Details;
      existingFile.TipDosar = dto.TipDosar;
      existingFile.CuloareCalendar = dto.TipDosar switch
      {
        "civil" => "#D3D3D3",
        "penal" => "#808080",
        _ => existingFile.CuloareCalendar
      };

      // Update Lawyer if specified in the DTO (optional)
      if (dto.LawyerId.HasValue)
      {
        var lawyer = await _context.Lawyers.FindAsync(dto.LawyerId.Value);
        if (lawyer == null)
        {
          _logger.LogError($"Lawyer with ID {dto.LawyerId} not found.");
          throw new ArgumentException($"Avocatul cu ID {dto.LawyerId} nu a fost găsit.", nameof(dto.LawyerId));
        }
        existingFile.LawyerId = dto.LawyerId.Value;
      }

      // Save changes to database
      _context.Files.Update(existingFile);
      await _context.SaveChangesAsync();

      // Get the LawyerName (if available) for the notification
      string lawyerName = existingFile.Lawyer != null ? existingFile.Lawyer.LawyerName : "N/A";

      // Create notification for file update
      var notification = new Notification
      {
        Title = "Fișier actualizat",
        Message = $"Fișierul {dto.FileNumber} a fost actualizat",
        Timestamp = DateTime.UtcNow,
        Type = "file_updated",
        FileNumber = dto.FileNumber,
        IsRead = false,
        Details = $"Client: {existingFile.ClientName}, Tip dosar: {dto.TipDosar}, Avocat: {lawyerName}",
        UserId = 1 // TODO: Get from current user context
      };

      await _notificationService.CreateNotificationAsync(notification);

      _logger.LogInformation($"File with ID {id} updated successfully.");
    }

    public async Task DeleteFileAsync(int id)
    {
      _logger.LogInformation($"Deleting file with ID {id}...");

      var fileToDelete = await _context.Files.FindAsync(id);

      if (fileToDelete == null)
      {
        _logger.LogWarning($"File with ID {id} not found.");
        throw new KeyNotFoundException($"Fișierul cu ID {id} nu a fost găsit.");
      }

      // Create notification before deleting the file
      var notification = new Notification
      {
        Title = "Fișier șters",
        Message = $"Fișierul {fileToDelete.FileNumber} a fost șters",
        Timestamp = DateTime.UtcNow,
        Type = "file_deleted",
        FileNumber = fileToDelete.FileNumber,
        IsRead = false,
        Details = $"Client: {fileToDelete.ClientName}, Tip dosar: {fileToDelete.TipDosar}",
        UserId = 1 // TODO: Get from current user context
      };

      await _notificationService.CreateNotificationAsync(notification);

      // Delete the file
      _context.Files.Remove(fileToDelete);
      await _context.SaveChangesAsync();

      _logger.LogInformation($"File with ID {id} deleted successfully.");
    }


    public async Task<CreateFileDto> GetFileDetailsByIdAsync(int id)
    {
      // Căutăm fișierul în baza de date după ID
      var file = await _context.Files
         .Include(f => f.Lawyer) // Include entitatea Lawyer
          .Where(d => d.Id == id)
          .Select(d => new CreateFileDto
          {
            Id = d.Id,
            FileNumber = d.FileNumber,
            ClientName = d.ClientName,
            ClientType=d.ClientType,
            Status= d.Status,
            Source=d.Source,
            CuvantCheie=d.CuvantCheie,
            TipDosar = d.TipDosar,
            Instanta=d.Instanta,
            Details = d.Details,
            LawyerName = d.Lawyer != null ? d.Lawyer.LawyerName : string.Empty, 
          })
          .FirstOrDefaultAsync();

      return file;
    }

    internal Task GetAllFileDetailsAsync()
    {
      throw new NotImplementedException();
    }


    public async Task<IEnumerable<MyFile>> GetFilesForClientAsync(int clientId)
    {
      
      var clientFiles = await _context.Files
                                      .Where(f => f.ClientId == clientId)
                                      .ToListAsync();

      if (clientFiles == null || !clientFiles.Any())
      {
        _logger.LogWarning($"Nu au fost găsite dosare pentru clientul cu ID {clientId}.");
      }
      return clientFiles;
    }


    public async Task<CreateFileDto> GetFileByIdAsync(int id)
    {
      return await _context.Files
        .Include(f => f.Lawyer)
        .Where(f => f.Id == id)
        .Select(f => new CreateFileDto
        {
          Id = f.Id,
          FileNumber = f.FileNumber,
          ClientId = f.ClientId,
          ClientName = f.ClientName,
          TipDosar = f.TipDosar,
          Source=f.Source,
          Instanta = f.Instanta,
          Details = f.Details,
          NumarContract = f.NumarContract,
          Delegatie = f.Delegatie,
          Onorariu = f.Onorariu,
          CuvantCheie=f.CuvantCheie,
          Outcome=f.Outcome,
          Moneda=f.Moneda,
          DataScadenta = f.DataScadenta,
          LawyerId = f.Lawyer.Id,
          LawyerName = f.Lawyer != null ? f.Lawyer.LawyerName : string.Empty,
        })
        .FirstOrDefaultAsync();
    }

    public async Task<List<CreateFileDto>> GetFilesByLawyerIdAsync(int lawyerId)
    {
      return await _context.Files
          .Include(f => f.Lawyer)
          .Where(f => f.LawyerId == lawyerId)
          .Select(f => new CreateFileDto
          {
            Id = f.Id,
            FileNumber = f.FileNumber,
            ClientId = f.ClientId,
            ClientName = f.ClientName,
            TipDosar = f.TipDosar,
            Source = f.Source,
            Instanta = f.Instanta,
            Details = f.Details,
            NumarContract = f.NumarContract,
            Delegatie = f.Delegatie,
            Onorariu = f.Onorariu,
            CuvantCheie=f.CuvantCheie,
            Moneda = f.Moneda,
            DataScadenta = f.DataScadenta,
            LawyerId = f.Lawyer.Id,
            LawyerName = f.Lawyer.LawyerName,
            Status = f.Status
          })
          .ToListAsync();
    }

    public async Task<List<CreateFileDto>> GetClosedFilesByLawyerIdAsync(int lawyerId)
    {
      return await _context.Files
          .Include(f => f.Lawyer)
        .Where(f => f.LawyerId == lawyerId && (f.Status == "Inchis" || f.Status == "închis"))
          .Select(f => new CreateFileDto
          {
            Id = f.Id,
            FileNumber = f.FileNumber,
            ClientId = f.ClientId,
            ClientName = f.ClientName,
            TipDosar = f.TipDosar,
            Source = f.Source,
            Instanta = f.Instanta,
            Details = f.Details,
            NumarContract = f.NumarContract,
            Delegatie = f.Delegatie,
            Onorariu = f.Onorariu,
            CuvantCheie = f.CuvantCheie,
            Moneda = f.Moneda,
            DataScadenta = f.DataScadenta,
            LawyerId = f.Lawyer.Id,
            LawyerName = f.Lawyer.LawyerName,
            Status = f.Status,
            Outcome = f.Outcome
          })
          .ToListAsync();
    }


    public async Task<List<CreateFileDto>> GetOpenFilesByLawyerIdAsync(int lawyerId)
    {
      return await _context.Files
          .Include(f => f.Lawyer)
          .Where(f => f.LawyerId == lawyerId && f.Status == "Deschis")
          .Select(f => new CreateFileDto
          {
            Id = f.Id,
            FileNumber = f.FileNumber,
            ClientId = f.ClientId,
            ClientName = f.ClientName,
            TipDosar = f.TipDosar,
            Source = f.Source,
            Instanta = f.Instanta,
            Details = f.Details,
            CuvantCheie = f.CuvantCheie,
            NumarContract = f.NumarContract,
            Delegatie = f.Delegatie,
            Onorariu = f.Onorariu,
            Moneda = f.Moneda,
            DataScadenta = f.DataScadenta,
            LawyerId = f.Lawyer.Id,
            LawyerName = f.Lawyer.LawyerName,
            Status = f.Status
          })
          .ToListAsync();
    }





    public async Task<List<CreateFileDto>> GetFilesByDueDateAsync()
    {
      try
      {
        var today = DateTime.Now;
        var filesDue = await _context.Files
                                       .Include(f => f.Lawyer)
                                      .Where(f => f.DataScadenta >= today)
                                      .ToListAsync();

        var filesDueDtos = filesDue.Select(f => new CreateFileDto
        {
          FileNumber = f.FileNumber,
          ClientName = f.ClientName,
          DataScadenta = f.DataScadenta,
          Onorariu= f.Onorariu,
          Moneda= f.Moneda,
          OnorariuRestant=f.OnorariuRestant,
          LawyerId=f.Lawyer.Id,
          LawyerName = f.Lawyer != null ? f.Lawyer.LawyerName : string.Empty,


        }).ToList();

        return filesDueDtos;
      }
      catch (Exception ex)
      {
        // Logare eroare
        throw new InvalidOperationException("Eroare la obținerea dosarelor cu termenul de scadență.", ex);
      }
    }
  }
}

