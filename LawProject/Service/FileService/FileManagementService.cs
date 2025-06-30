using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using LawProject.Service.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.ServiceModel;
using System.Text.Json;

namespace LawProject.Service.FileService
{
  public class IFileManagementService
  {
    private readonly ApplicationDbContext _context;
    private readonly MyQueryService _queryService;
    private readonly ILogger<IFileManagementService> _logger;
    private readonly INotificationService _notificationService;
    private readonly IMemoryCache _cache;

    private static Dictionary<string, List<SoapDosar>> _soapCache = new();
    private static DateTime _lastCacheUpdate = DateTime.MinValue;
    private static readonly TimeSpan CacheValidity = TimeSpan.FromMinutes(30);

    private const string CacheKey = "AllFilesWithSoapData";

    public IFileManagementService(
        ApplicationDbContext context,
        MyQueryService queryService,
        ILogger<IFileManagementService> logger,
        INotificationService notificationService, IMemoryCache memoryCache)
    {
      _context = context;
      _queryService = queryService;
      _logger = logger;
      _notificationService = notificationService;
      _cache = memoryCache;
    }

    public async Task UpdateScheduledEventAsync(ScheduledEvent scheduledEvent)
    {
      var existing = await _context.ScheduledEvents
      .AsNoTracking()
      .FirstOrDefaultAsync(e => e.Id == scheduledEvent.Id);

      if (existing == null)
        return; // Sau aruncă excepție

      bool isModified =
        existing.StartTime != scheduledEvent.StartTime ||
        existing.EndTime != scheduledEvent.EndTime ||
        existing.TipDosar != scheduledEvent.TipDosar ||
        existing.Description != scheduledEvent.Description ||
        existing.Color != scheduledEvent.Color ||
        existing.ClientName != scheduledEvent.ClientName ||
        existing.LawyerName != scheduledEvent.LawyerName ||
        existing.Source != scheduledEvent.Source;

      if (!isModified)
        return; // Nu facem update dacă nu există modificări relevante

      // Actualizăm entitatea în context
      _context.ScheduledEvents.Update(scheduledEvent);
      await _context.SaveChangesAsync();

      // Trimitem notificare despre update
      var notification = new Notification
      {
        Title = "Eveniment actualizat",
        Message = $"Evenimentul pentru dosarul {scheduledEvent.FileNumber} a fost actualizat",
        Timestamp = DateTime.UtcNow,
        Type = "event_updated",
        FileNumber = scheduledEvent.FileNumber,
        Source = scheduledEvent.Source,
        IsRead = false,
        Details = $"Data: {scheduledEvent.StartTime:dd.MM.yyyy HH:mm}, Tip: {scheduledEvent.TipDosar}",
      };

      await _notificationService.CreateNotificationAsync(notification);
    }



    public async Task<List<ScheduledEvent>> GetScheduledEventsAsync()
    {
      return await _context.ScheduledEvents.ToListAsync();
  
    }
    public async Task<List<ScheduledEvent>> GetScheduledEventsByTodayAsync()
    {
      var today = DateTime.Today;
      var tomorrow = today.AddDays(1);

      return await _context.ScheduledEvents
          .Where(ev => ev.StartTime >= today && ev.StartTime < tomorrow)
          .ToListAsync();
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

    public async Task RefreshCacheAsync()
    {
      // aici apelezi metoda ta GetAllFilesAsync() sau orice logică de refresh cache ai
      var files = await GetAllFilesAsync();

      // eventual actualizezi cache-ul manual aici dacă nu e deja în GetAllFilesAsync
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
        Message = $"Un nou eveniment a fost programat pentru dosarul {scheduledEvent.FileNumber}, avocat {scheduledEvent.LawyerName}, client {scheduledEvent.ClientName}",
        Timestamp = DateTime.UtcNow,
        Type = "event_scheduled",
        FileNumber = scheduledEvent.FileNumber,
        Source = scheduledEvent.Source,
        IsRead = false,
        Details = $"Data: {scheduledEvent.StartTime:dd.MM.yyyy HH:mm}, Tip: {scheduledEvent.TipDosar}",
        UserId = 1 // TODO: Get from current user context
      };

      await _notificationService.CreateNotificationAsync(notification);
    }

    // Aceasta metoda obtine lista din cache daca exista, altfel arunca exceptie sau returneaza lista goala


    public async Task UpdateSoapCacheAsync()
    {
      var allFileNumbers = await _context.Files
          .Where(f => f.Source.ToUpper() == "JUST")
          .Select(f => f.FileNumber)
          .ToListAsync();

      var newCache = new Dictionary<string, List<SoapDosar>>();

      foreach (var fileNumber in allFileNumbers)
      {
        ServiceReference1.Dosar[] soapDosareRaw = await _queryService.CautareDosareAsync(fileNumber);

        if (soapDosareRaw != null)
        {
          var soapDosare = soapDosareRaw.Select(d => new SoapDosar
          {
            numar = d.numar,
            data = d.data,
            institutie = d.institutie.ToString(),  // Convertim tipul la string

            obiect = d.obiect.ToString(),

            sedinte = d.sedinte?.Select(s => new Sedinta
            {
              complet = s.complet,
              data = s.data,
              ora = s.ora,
              solutie = s.solutie,
              solutieSumar = s.solutieSumar,
              dataPronuntare = s.dataPronuntare
            }).ToList()
          }).ToList();



          newCache[fileNumber] = soapDosare;
        }
      }

      _soapCache = newCache;
      _lastCacheUpdate = DateTime.Now;
      _logger.LogInformation("SOAP cache updated.");
    }


    public async Task<List<AllFilesDto>> GetAllFilesAsync()
    {
      _logger.LogInformation("Fetching all files from the database...");

      // Dacă cache-ul e vechi, actualizează-l sincron
      if ((DateTime.Now - _lastCacheUpdate) > CacheValidity)
      {
        await UpdateSoapCacheAsync();
      }

      var dbFiles = await _context.Files
          .Include(f => f.Lawyer)
          .ToListAsync();

      var basicDtos = dbFiles.Select(f => new CreateFileDto
      {
        Id = f.Id,
        FileNumber = f.FileNumber,
        ClientName = f.ClientName,
        ClientType = f.ClientType,
        ClientId = f.ClientId,
        Details = f.Details,
        TipDosar = f.TipDosar,
        Instanta = f.Instanta,
        Status = f.Status,
        Parola=f.Parola,
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

      var justFiles = basicDtos.Where(f => f.Source?.ToUpper() == "JUST").ToList();
      var otherFiles = basicDtos.Where(f => f.Source?.ToUpper() != "JUST").ToList();

      combinedFilesList.AddRange(otherFiles.Select(f => new AllFilesDto
      {
        Id = f.Id,
        FileNumber = f.FileNumber,
        ClientName = f.ClientName,
        ClientType = f.ClientType,
        ClientId = f.ClientId,
        Details = f.Details,
        Status = f.Status,
        Parola= f.Parola,
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

      int batchSize = 20;
      for (int i = 0; i < justFiles.Count; i += batchSize)
      {
        var batch = justFiles.Skip(i).Take(batchSize);
        foreach (var dbFile in batch)
        {
          _soapCache.TryGetValue(dbFile.FileNumber, out var soapDosare);

          if (soapDosare == null || !soapDosare.Any())
          {
            combinedFilesList.Add(new AllFilesDto
            {
              Id = dbFile.Id,
              FileNumber = dbFile.FileNumber,
              ClientName = dbFile.ClientName,
              Details = dbFile.Details,
              Status = dbFile.Status,
              Onorariu = dbFile.Onorariu,
              Moneda = dbFile.Moneda,
              Parola=dbFile.Parola,
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
            });
            continue;
          }

          var mostRecentSoapDosar = soapDosare.OrderByDescending(d => d.data).FirstOrDefault();

          if (mostRecentSoapDosar != null)
          {
            combinedFilesList.Add(new AllFilesDto
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
              Parola = dbFile.Parola,
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
              Institutie = mostRecentSoapDosar.institutie?.ToString(),
              ObiectDosar = mostRecentSoapDosar.obiect?.ToString(),

              Sedinte = mostRecentSoapDosar.sedinte?.Select(s => new SedintaDTO
              {
                Complet = s.complet,
                Data = s.data,
                Ora = s.ora,
                Solutie = s.solutie,
                SolutieSumar = s.solutieSumar,
                DataPronuntare = s.dataPronuntare,
              }).ToList()
            });
          }
          else
          {
            combinedFilesList.Add(new AllFilesDto
            {
              Id = dbFile.Id,
              FileNumber = dbFile.FileNumber,
              ClientName = dbFile.ClientName,
              ClientType = dbFile.ClientType,
              ClientId = dbFile.ClientId,
              Details = dbFile.Details,
              Status = dbFile.Status,
              Onorariu = dbFile.Onorariu,
              Moneda = dbFile.Moneda,
              Parola=dbFile.Parola,
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
            });
          }
        }
      }

      _logger.LogInformation("Finished processing all file details.");
      return combinedFilesList;
    }







    //public async Task<List<AllFilesDto>> GetAllFilesAsync()
    //{
    //  _logger.LogInformation("Fetching all files from the database...");

    //  var dbFiles = await _context.Files
    //    .Include(f => f.Lawyer)
    //    .ToListAsync();

    //  var basicDtos = dbFiles.Select(f => new CreateFileDto
    //  {
    //    Id = f.Id,
    //    FileNumber = f.FileNumber,
    //    ClientName = f.ClientName,
    //    Details = f.Details,
    //    TipDosar = f.TipDosar,
    //    Instanta = f.Instanta,
    //    Status = f.Status,
    //    Onorariu = f.Onorariu,
    //    Outcome = f.Outcome,
    //    Source = f.Source,
    //    Moneda = f.Moneda,
    //    OnorariuRestant = f.OnorariuRestant,
    //    Delegatie = f.Delegatie,
    //    DataScadenta = f.DataScadenta,
    //    NumarContract = f.NumarContract,
    //    LawyerName = f.Lawyer?.LawyerName ?? string.Empty,
    //    LawyerId = f.Lawyer?.Id
    //  }).ToList();

    //  if (!basicDtos.Any())
    //  {
    //    _logger.LogWarning("No files found in the database.");
    //    return new List<AllFilesDto>();
    //  }

    //  var combinedFilesList = new List<AllFilesDto>();

    //  // Separă dosarele JUST și restul
    //  var justFiles = basicDtos.Where(f => f.Source?.ToUpper() == "JUST").ToList();
    //  var otherFiles = basicDtos.Where(f => f.Source?.ToUpper() != "JUST").ToList();

    //  // Adaugă direct dosarele non-JUST fără apel SOAP
    //  combinedFilesList.AddRange(otherFiles.Select(f => new AllFilesDto
    //  {
    //    Id = f.Id,
    //    FileNumber = f.FileNumber,
    //    ClientName = f.ClientName,
    //    Details = f.Details,
    //    Status = f.Status,
    //    Onorariu = f.Onorariu,
    //    Moneda = f.Moneda,
    //    OnorariuRestant = f.OnorariuRestant,
    //    Delegatie = f.Delegatie,
    //    CuvantCheie = f.CuvantCheie,
    //    NumarContract = f.NumarContract,
    //    DataScadenta = f.DataScadenta,
    //    OutCome = f.Outcome,
    //    Source = f.Source,
    //    TipDosar = f.TipDosar,
    //    Instanta = f.Instanta,
    //    LawyerName = f.LawyerName,
    //    LawyerId = f.LawyerId
    //  }));

    //  // Trimite apelurile SOAP pe batch-uri de 20
    //  int batchSize = 20;

    //  for (int i = 0; i < justFiles.Count; i += batchSize)
    //  {
    //    var batch = justFiles.Skip(i).Take(batchSize);

    //    var tasks = batch.Select(async dbFile =>
    //    {
    //      _logger.LogInformation($"Fetching SOAP details for file number: {dbFile.FileNumber}");

    //      var soapDosare = await _queryService.CautareDosareAsync(dbFile.FileNumber);

    //      if (soapDosare == null || !soapDosare.Any())
    //      {
    //        _logger.LogWarning($"No SOAP details found for file number: {dbFile.FileNumber}");
    //        return new AllFilesDto
    //        {
    //          Id = dbFile.Id,
    //          FileNumber = dbFile.FileNumber,
    //          ClientName = dbFile.ClientName,
    //          Details = dbFile.Details,
    //          Status = dbFile.Status,
    //          Onorariu = dbFile.Onorariu,
    //          Moneda = dbFile.Moneda,
    //          OnorariuRestant = dbFile.OnorariuRestant,
    //          Delegatie = dbFile.Delegatie,
    //          CuvantCheie = dbFile.CuvantCheie,
    //          NumarContract = dbFile.NumarContract,
    //          DataScadenta = dbFile.DataScadenta,
    //          OutCome = dbFile.Outcome,
    //          Source = dbFile.Source,
    //          TipDosar = dbFile.TipDosar,
    //          Instanta = dbFile.Instanta,
    //          LawyerName = dbFile.LawyerName,
    //          LawyerId = dbFile.LawyerId
    //        };
    //      }

    //      var mostRecentSoapDosar = soapDosare
    //          .OrderByDescending(d => d.data)
    //          .FirstOrDefault();

    //      if (mostRecentSoapDosar != null)
    //      {
    //        return new AllFilesDto
    //        {
    //          Id = dbFile.Id,
    //          FileNumber = dbFile.FileNumber,
    //          ClientName = dbFile.ClientName,
    //          Details = dbFile.Details,
    //          TipDosar = dbFile.TipDosar,
    //          Status = dbFile.Status,
    //          Source = dbFile.Source,
    //          LawyerName = dbFile.LawyerName,
    //          Instanta = dbFile.Instanta,
    //          LawyerId = dbFile.LawyerId,
    //          Onorariu = dbFile.Onorariu,
    //          Moneda = dbFile.Moneda,
    //          CuvantCheie = dbFile.CuvantCheie,
    //          OnorariuRestant = dbFile.OnorariuRestant,
    //          Delegatie = dbFile.Delegatie,
    //          NumarContract = dbFile.NumarContract,
    //          DataScadenta = dbFile.DataScadenta,

    //          Numar = mostRecentSoapDosar.numar,
    //          Data = mostRecentSoapDosar.data,
    //          Institutie = mostRecentSoapDosar.institutie.ToString(),
    //          ObiectDosar = mostRecentSoapDosar.obiect.ToString(),

    //          Sedinte = mostRecentSoapDosar.sedinte?.Select(s => new SedintaDTO
    //          {
    //            Complet = s.complet,
    //            Data = s.data,
    //            Ora = s.ora,
    //            Solutie = s.solutie,
    //            SolutieSumar = s.solutieSumar,
    //            DataPronuntare = s.dataPronuntare,

    //          }).ToList()
    //        };
    //      }

    //      // fallback, dacă nu există dosar în SOAP
    //      return new AllFilesDto
    //      {
    //        Id = dbFile.Id,
    //        FileNumber = dbFile.FileNumber,
    //        ClientName = dbFile.ClientName,
    //        Details = dbFile.Details,
    //        Status = dbFile.Status,
    //        Onorariu = dbFile.Onorariu,
    //        Moneda = dbFile.Moneda,
    //        OnorariuRestant = dbFile.OnorariuRestant,
    //        Delegatie = dbFile.Delegatie,
    //        CuvantCheie = dbFile.CuvantCheie,
    //        NumarContract = dbFile.NumarContract,
    //        DataScadenta = dbFile.DataScadenta,
    //        OutCome = dbFile.Outcome,
    //        Source = dbFile.Source,
    //        TipDosar = dbFile.TipDosar,
    //        Instanta = dbFile.Instanta,
    //        LawyerName = dbFile.LawyerName,
    //        LawyerId = dbFile.LawyerId
    //      };
    //    });

    //    var results = await Task.WhenAll(tasks);

    //    combinedFilesList.AddRange(results);
    //  }

    //  _logger.LogInformation("Finished processing all file details.");
    //  return combinedFilesList;
    //}

    public async Task<AllFilesDto?> GetFileCombinedByIdAsync(int fileId)
    {
      _logger.LogInformation($"Fetching file with ID {fileId} from the database...");

      var file = await _context.Files
        .Include(f => f.Lawyer)
        .FirstOrDefaultAsync(f => f.Id == fileId);

      if (file == null)
      {
        _logger.LogWarning($"File with ID {fileId} not found.");
        return null;
      }

      var dto = new CreateFileDto
      {
        Id = file.Id,
        FileNumber = file.FileNumber,
        ClientName = file.ClientName,
        Details = file.Details,
        TipDosar = file.TipDosar,
        Instanta = file.Instanta,
        Status = file.Status,
        Onorariu = file.Onorariu,
        Outcome = file.Outcome,
        Source = file.Source,
        Moneda = file.Moneda,
        OnorariuRestant = file.OnorariuRestant,
        Delegatie = file.Delegatie,
        DataScadenta = file.DataScadenta,
        NumarContract = file.NumarContract,
        LawyerName = file.Lawyer?.LawyerName ?? string.Empty,
        LawyerId = file.Lawyer?.Id
      };

      if (dto.Source?.ToUpper() != "JUST")
      {
        // Dosar non-JUST
        return new AllFilesDto
        {
          Id = dto.Id,
          FileNumber = dto.FileNumber,
          ClientName = dto.ClientName,
          Details = dto.Details,
          Status = dto.Status,
          Onorariu = dto.Onorariu,
          Moneda = dto.Moneda,
          OnorariuRestant = dto.OnorariuRestant,
          Delegatie = dto.Delegatie,
          CuvantCheie = dto.CuvantCheie,
          NumarContract = dto.NumarContract,
          DataScadenta = dto.DataScadenta,
          OutCome = dto.Outcome,
          Source = dto.Source,
          TipDosar = dto.TipDosar,
          Instanta = dto.Instanta,
          LawyerName = dto.LawyerName,
          LawyerId = dto.LawyerId
        };
      }

      // Dosar JUST – cere detalii SOAP
      _logger.LogInformation($"Fetching SOAP details for file number: {dto.FileNumber}");

      var soapDosare = await _queryService.CautareDosareAsync(dto.FileNumber);

      if (soapDosare == null || !soapDosare.Any())
      {
        _logger.LogWarning($"No SOAP details found for file number: {dto.FileNumber}");

        return new AllFilesDto
        {
          Id = dto.Id,
          FileNumber = dto.FileNumber,
          ClientName = dto.ClientName,
          Details = dto.Details,
          Status = dto.Status,
          Onorariu = dto.Onorariu,
          Moneda = dto.Moneda,
          OnorariuRestant = dto.OnorariuRestant,
          Delegatie = dto.Delegatie,
          CuvantCheie = dto.CuvantCheie,
          NumarContract = dto.NumarContract,
          DataScadenta = dto.DataScadenta,
          OutCome = dto.Outcome,
          Source = dto.Source,
          TipDosar = dto.TipDosar,
          Instanta = dto.Instanta,
          LawyerName = dto.LawyerName,
          LawyerId = dto.LawyerId
        };
      }

      var mostRecentSoapDosar = soapDosare
        .OrderByDescending(d => d.data)
        .FirstOrDefault();

      return new AllFilesDto
      {
        Id = dto.Id,
        FileNumber = dto.FileNumber,
        ClientName = dto.ClientName,
        Details = dto.Details,
        TipDosar = dto.TipDosar,
        Status = dto.Status,
        Source = dto.Source,
        LawyerName = dto.LawyerName,
        Instanta = dto.Instanta,
        LawyerId = dto.LawyerId,
        Onorariu = dto.Onorariu,
        Moneda = dto.Moneda,
        CuvantCheie = dto.CuvantCheie,
        OnorariuRestant = dto.OnorariuRestant,
        Delegatie = dto.Delegatie,
        NumarContract = dto.NumarContract,
        DataScadenta = dto.DataScadenta,

        Numar = mostRecentSoapDosar?.numar,
        Data = mostRecentSoapDosar.data,
        Institutie = mostRecentSoapDosar?.institutie.ToString(),
        ObiectDosar = mostRecentSoapDosar?.obiect?.ToString(),

        Sedinte = mostRecentSoapDosar?.sedinte?.Select(s => new SedintaDTO
        {
          Complet = s.complet,
          Data = s.data,
          Ora = s.ora,
          Solutie = s.solutie,
          SolutieSumar = s.solutieSumar,
          DataPronuntare = s.dataPronuntare
        }).ToList()
      };
    }



    public async Task UpdateSoapCacheForFileAsync(string fileNumber)
    {
      ServiceReference1.Dosar[] soapDosareRaw = await _queryService.CautareDosareAsync(fileNumber);

      if (soapDosareRaw != null)
      {
        var soapDosare = soapDosareRaw.Select(d => new SoapDosar
        {
          numar = d.numar,
          data = d.data,
          institutie = d.institutie.ToString(),
          obiect = d.obiect.ToString(),
          sedinte = d.sedinte?.Select(s => new Sedinta
          {
            complet = s.complet,
            data = s.data,
            ora = s.ora,
            solutie = s.solutie,
            solutieSumar = s.solutieSumar,
            dataPronuntare = s.dataPronuntare
          }).ToList()
        }).ToList();

        if (_soapCache == null) _soapCache = new Dictionary<string, List<SoapDosar>>();

        _soapCache[fileNumber] = soapDosare;
        _lastCacheUpdate = DateTime.Now;
      }
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
          ClientType=dto.ClientType,
          Details = dto.Details,
          Onorariu=dto.Onorariu,
          Source=dto.Source,
          Parola=dto.Parola,
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
          LawyerName=dto.LawyerName,
          Status = "deschis",
      
        };

        // Adăugăm fișierul în context
        _context.Files.Add(file);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"File with file number {dto.FileNumber} added successfully.");
        var notification = new Notification
        {
          Title = "Fișier nou adăugat",
          Message = $"Un nou fișier a fost adăugat: {dto.FileNumber}, pentru avocat {dto.LawyerName}",
          Timestamp = DateTime.UtcNow,
          Type = "new_file",
          FileNumber = dto.FileNumber,
          LawyerName=dto.LawyerName,
          IsRead = false,
          Source=dto.Source,
          Details = $"Client: {file.ClientName}, Tip dosar: {dto.TipDosar}",
          UserId = 1 
        };
        await _notificationService.CreateNotificationAsync(notification);

        _logger.LogInformation($"Notification for file {dto.FileNumber} created successfully.");

        if (dto.Source != null && dto.Source.ToUpper() == "JUST")
        {
          await UpdateSoapCacheForFileAsync(dto.FileNumber);
          _logger.LogInformation($"SOAP cache updated for new file number {dto.FileNumber}.");
        }
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
            Parola = f.Parola,
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
            Parola=d.Parola,
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
            LawyerName = d.Lawyer != null ? d.Lawyer.LawyerName : string.Empty,
            LawyerEmail=d.Lawyer != null ? d.Lawyer.Email: string.Empty,
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
      existingFile.CuvantCheie = dto.CuvantCheie ?? existingFile.CuvantCheie;
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


    public async Task<IEnumerable<MyFile>> GetFilesForClientAsync(int clientId, string clientType)
    {
      
      var clientFiles = await _context.Files
                                      .Where(f => f.ClientId == clientId && f.ClientType == clientType)
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
          Parola=f.Parola,
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
            Parola=f.Parola,
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
            Parola=f.Parola,
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
            ClientType = f.ClientType,
            ClientId = f.ClientId,
            ClientName=f.ClientName,
            TipDosar = f.TipDosar,
            Source = f.Source,
            Parola=f.Parola,
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



    public async Task<int> GetOpenFilesCountByLawyerIdAsync(int lawyerId)
    {
      return await _context.Files
          .Where(f => f.LawyerId == lawyerId && f.Status == "Deschis")
          .CountAsync();
    }

    public async Task<int> GetClosedFilesCountByLawyerIdAsync(int lawyerId)
    {
      return await _context.Files
          .Where(f => f.LawyerId == lawyerId && f.Status == "Inchis")
          .CountAsync();
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

