using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using LawProject.Service.Notifications;
using Microsoft.EntityFrameworkCore;

namespace LawProject.Service.FileService
{
  public class FileManagementService
  {
    private readonly ApplicationDbContext _context;
    private readonly MyQueryService _queryService;
    private readonly ILogger<FileManagementService> _logger;
    private readonly INotificationService _notificationService;

    public FileManagementService(
        ApplicationDbContext context,
        MyQueryService queryService,
        ILogger<FileManagementService> logger,
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

    // Metodă pentru a obține un eveniment specific bazat pe numărul dosarului și ora de început
    public async Task<ScheduledEvent> GetScheduledEventAsync(string fileNumber, DateTime startTime)
    {
      return await _context.ScheduledEvents
          .FirstOrDefaultAsync(e => e.FileNumber == fileNumber && e.StartTime == startTime);
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
        IsRead = false,
        Details = $"Data: {scheduledEvent.StartTime:dd.MM.yyyy HH:mm}, Tip: {scheduledEvent.TipDosar}",
        UserId = 1 // TODO: Get from current user context
      };

      await _notificationService.CreateNotificationAsync(notification);
    }



    public async Task<List<AllFilesDto>> GetAllFilesAsync()
    {
      _logger.LogInformation("Fetching all files from the database...");

      // Preluăm din baza de date toate dosarele (informații de bază)
      var dbFiles = await _context.Files
        .Include(f => f.Lawyer)  // Eager load Lawyer details
           .Select(f => new CreateFileDto
           {
             Id = f.Id,
             FileNumber = f.FileNumber,
             ClientName = f.ClientName,
             Details = f.Details,

             TipDosar = f.TipDosar,
              LawyerName = f.Lawyer != null ? f.Lawyer.LawyerName : string.Empty  // Get Lawyer's name if available
           })
           .ToListAsync();

      if (dbFiles == null || !dbFiles.Any())
      {
        _logger.LogWarning("No files found in the database.");
        return new List<AllFilesDto>();
      }

      var combinedFilesList = new List<AllFilesDto>();

      // Pentru fiecare dosar din baza de date, se interoghează serviciul SOAP
      foreach (var dbFile in dbFiles)
      {
        _logger.LogInformation($"Fetching SOAP details for file number: {dbFile.FileNumber}");

        // Apelăm serviciul SOAP pentru a obține detaliile dosarului
        var soapDosare = await _queryService.CautareDosareAsync(dbFile.FileNumber);

        if (soapDosare == null || !soapDosare.Any())
        {
          _logger.LogWarning($"No SOAP details found for file number: {dbFile.FileNumber}");
          // Poți decide dacă vrei să adaugi totuși obiectul din DB, chiar și fără detalii SOAP,
          // sau să treci peste el. Aici vom adăuga obiectul din DB fără completări SOAP.
          combinedFilesList.Add(new AllFilesDto
          {
            Id = dbFile.Id,
            FileNumber = dbFile.FileNumber,
            ClientName = dbFile.ClientName,
            Details = dbFile.Details,
            Email = dbFile.Email,
            TipDosar = dbFile.TipDosar,
             LawyerName = dbFile.LawyerName  // Add Lawyer's name
            // Restul proprietăților rămân nule/default
          });
          continue;
        }

        // Dacă serviciul SOAP returnează mai multe dosare, poți decide cum le combini.
        // În exemplul de mai jos, pentru fiecare dosar SOAP se creează o intrare.
        foreach (var soapDosar in soapDosare)
        {
          var combinedDto = new AllFilesDto
          {
            // Informații din baza de date
            Id = dbFile.Id,
            FileNumber = dbFile.FileNumber,
            ClientName = dbFile.ClientName,
            Details = dbFile.Details,
            Email = dbFile.Email,
            TipDosar = dbFile.TipDosar,
            LawyerName = dbFile.LawyerName,  // Add Lawyer's name

            // Informații din SOAP
            Numar = soapDosar.numar,
            NumarVechi = soapDosar.numarVechi, // presupunând că există o proprietate similară
            Data = soapDosar.data,
            Institutie = soapDosar.institutie.ToString(),
            Departament = soapDosar.departament,
            CategorieCaz = soapDosar.categorieCaz?.ToString(),
            StadiuProcesual = soapDosar.stadiuProcesual?.ToString(),
            Parti = soapDosar.parti?.Select(p => new ParteDTO
            {
              Nume = p.nume,
              CalitateParte = p.calitateParte
            }).ToList(),
            Sedinte = soapDosar.sedinte?.Select(s => new SedintaDTO
            {
              Complet = s.complet,
              // Dacă s.data este de tip DateTime (non-nullable), îl atribuim direct:
              Data = s.data,
              Ora = s.ora,
              Solutie = s.solutie,
              SolutieSumar = s.solutieSumar,
              DataPronuntare = s.dataPronuntare,
              // Convertim s.documentSedinta la string:
              DocumentSedinta = s.documentSedinta?.ToString() ?? string.Empty,
              NumarDocument = s.numarDocument,
              DataDocument = s.dataDocument
            }).ToList(),


          };

          combinedFilesList.Add(combinedDto);
        }
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
          TipDosar = dto.TipDosar,
          CuloareCalendar = dto.TipDosar switch
          {
            "civil" => "#D3D3D3",
            "penal" => "#808080",
            _ => throw new ArgumentException("Tipul de dosar este invalid.")
          },
          // Optional: Setting the lawyer if specified
          LawyerId = dto.LawyerId
        };

        // Adăugăm fișierul în context
        _context.Files.Add(file);

        // Salvăm modificările în baza de date
        await _context.SaveChangesAsync();
        _logger.LogInformation($"File with file number {dto.FileNumber} added successfully.");

        // Creăm notificarea
        var notification = new Notification
        {
          Title = "Fișier nou adăugat",
          Message = $"Un nou fișier a fost adăugat: {dto.FileNumber}",
          Timestamp = DateTime.UtcNow,
          Type = "new_file",
          FileNumber = dto.FileNumber,
          IsRead = false,
          Details = $"Client: {file.ClientName}, Tip dosar: {dto.TipDosar}",
          UserId = 1 // Trebuie să obții UserId din contextul actual
        };

        // Creăm notificarea în sistem
        await _notificationService.CreateNotificationAsync(notification);

        _logger.LogInformation($"Notification for file {dto.FileNumber} created successfully.");
      }
      catch (ArgumentException ex)
      {
        _logger.LogError($"Validation error: {ex.Message}");
        throw; // Re-throw the exception or handle accordingly
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while adding the file: {ex.Message}");
        if (ex.InnerException != null)
        {
          _logger.LogError($"Inner exception: {ex.InnerException.Message}");
        }
        throw; // Ensure that the error is propagated correctly
      }
    }




    public async Task<CreateFileDto> GetFileByNumberAsync(string fileNumber)
    {
      return await _context.Files
          .Where(d => d.FileNumber == fileNumber)
          .Select(d => new CreateFileDto
          {
            Id = d.Id,
            FileNumber = d.FileNumber,
            ClientName = d.ClientName,
            ClientId = d.ClientId,
            TipDosar = d.TipDosar,
            Details = d.Details
          })
      .FirstOrDefaultAsync();
    }

    public async Task<FileDetailsDto> GetFileDetailsByNumberAsync(string fileNumber)
    {
      _logger.LogInformation($"Fetching case details for file number: {fileNumber}");

      // Apelează serviciul SOAP pentru numărul de dosar dat
      var dosare = await _queryService.CautareDosareAsync(fileNumber);

      // Verifică dacă există dosare returnate
      if (dosare == null || !dosare.Any())
      {
        _logger.LogWarning($"No case details found for file number: {fileNumber}");
        return null; // Returnează null dacă nu sunt detalii despre dosar
      }

      // Maparea dosarului la DTO-ul FileDetailsDTO
      var fileDetails = dosare.Select(d => new FileDetailsDto
      {
        Numar = d.numar,
        Data = d.data,
        Institutie = d.institutie.ToString(), // Verifică dacă `institutie` este null
        Departament = d.departament,
        CategorieCaz = d.categorieCaz?.ToString(), // Verifică dacă `categorieCaz` este null
        StadiuProcesual = d.stadiuProcesual?.ToString(), // Verifică dacă `stadiuProcesual` este null
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
            TipDosar = d.TipDosar,
            Details = d.Details,
            LawyerName = d.Lawyer != null ? d.Lawyer.LawyerName : string.Empty, // Obține numele avocatului
          })
          .FirstOrDefaultAsync();

      return file;
    }

    internal Task GetAllFileDetailsAsync()
    {
      throw new NotImplementedException();
    }
  }
}

