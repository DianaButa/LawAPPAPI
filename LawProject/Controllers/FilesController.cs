using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using LawProject.Service;
using LawProject.Service.EmailService;
using LawProject.Service.FileService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LawProject.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class FilesController : ControllerBase


  {

    private readonly FileManagementService _fileManagementService;
    private readonly FileToCalendarService _fileToCalendarService;
    private readonly MyQueryService _queryService;
    private readonly ILogger<FilesController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    public FilesController(FileManagementService fileManagementService, FileToCalendarService fileToCalendarService, MyQueryService queryService,
                                ILogger<FilesController> logger, IEmailService emailService, ApplicationDbContext _context)
    {
      _fileManagementService = fileManagementService;
      _fileToCalendarService = fileToCalendarService;
      _queryService = queryService;
      _logger = logger;
      _context = _context;
      _emailService = emailService;

    }

    [HttpGet]
    public async Task<IActionResult> GetFiles()
    {
      try
      {
        var fileDetails = await _fileManagementService.GetAllFilesAsync();
        return Ok(fileDetails);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    [HttpGet("{fileNumber}")]
    public async Task<IActionResult> GetFileByNumber(string fileNumber)
    {
      try
      {
        // Decodifică numărul de dosar din URL
        fileNumber = Uri.UnescapeDataString(fileNumber);

        var fileDetail = await _fileManagementService.GetFileDetailsByNumberAsync(fileNumber);

        if (fileDetail == null)
        {
          return NotFound($"File with number {fileNumber} not found.");
        }

        return Ok(fileDetail);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    [HttpGet("by-id/{id}")]
    public async Task<IActionResult> GetFileById(int id)
    {
      try
      {
        var fileDetail = await _fileManagementService.GetFileByIdAsync(id);

        if (fileDetail == null)
        {
          return NotFound($"File with ID {id} not found.");
        }

        return Ok(fileDetail);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }



    [HttpPost]
    public async Task<IActionResult> CreateFile([FromBody] CreateFileDto dto)
    {
      try
      {
        if (string.IsNullOrEmpty(dto.Details))
        {
          dto.Details = "Default details";
        }

        // Adaugă dosarul în baza de date
        await _fileManagementService.AddFileAsync(dto);

        // După ce dosarul este adăugat, verifică dacă există ședințe pentru acesta
        _logger.LogInformation($"Checking for hearings for newly added file: {dto.FileNumber}");

        // Procesarea ședințelor imediat după adăugare
        await _fileToCalendarService.ProcessSingleFileAsync(dto.FileNumber);

        // Trimite email de confirmare
        try
        {
          string email = dto.Email;
          string name = dto.ClientName;

          if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(name))
          {
            _logger.LogInformation($"Sending confirmation email to {email} for file {dto.FileNumber}");
            await _emailService.SendConfirmationEmail(email, name, dto.FileNumber);
          }
          else
          {
            _logger.LogWarning("Email or client name is missing in the provided data. Email not sent.");
          }
        }
        catch (Exception emailEx)
        {
          _logger.LogError($"Error sending confirmation email: {emailEx.Message}");
        }

        return CreatedAtAction(nameof(GetFiles), new { fileNumber = dto.FileNumber }, dto);
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error adding file and processing hearings: {ex.Message}");
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }



    [HttpGet("scheduled-events")]
    public async Task<IActionResult> GetScheduledEvents()
    {
      try
      {
        var scheduledEvents = await _fileManagementService.GetScheduledEventsAsync();
        return Ok(scheduledEvents); // Returnează evenimentele ca JSON
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error fetching scheduled events: {ex.Message}");
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> EditFile(int id, [FromBody] CreateFileDto dto)
    {
      try
      {
        // Verifică dacă fișierul există
        var existingFile = await _fileManagementService.GetFileDetailsByIdAsync(id);
        if (existingFile == null)
        {
          return NotFound($"File with id {id} not found.");
        }

        // Actualizează fișierul
        await _fileManagementService.UpdateFileAsync(id, dto);

        // Verifică din nou dacă sunt ședințe programate și actualizează
        _logger.LogInformation($"Checking for hearings for updated file: {dto.FileNumber}");
        await _fileToCalendarService.ProcessSingleFileAsync(dto.FileNumber);

        return Ok(dto);
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error updating file: {ex.Message}");
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    [HttpPut("close-file/{id}")]
    public async Task<IActionResult> CloseFileAsync(int id, [FromBody] CloseFileDto closeFileDto)
    {
      // Căutăm dosarul în baza de date
      var file = await _context.Files.FindAsync(id);

      if (file == null)
      {
        return NotFound(new { Message = "Fișierul nu a fost găsit." });
      }

      // Actualizăm statusul dosarului și outcome-ul
      file.Status = "închis";  // Setăm statusul la "închis"
      file.Outcome = closeFileDto.Outcome;  // Setăm outcome-ul

      // Salvăm modificările
      await _context.SaveChangesAsync();


      return Ok(new { Message = "Fișierul a fost închis cu succes." });
    }


    // Ștergerea unui fișier existent - DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFile(int id)
    {
      try
      {
        var existingFile = await _fileManagementService.GetFileDetailsByIdAsync(id);
        if (existingFile == null)
        {
          return NotFound($"File with id {id} not found.");
        }

        // Șterge fișierul din baza de date
        await _fileManagementService.DeleteFileAsync(id);

        return NoContent(); // Returnează un status 204 pentru succes
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error deleting file: {ex.Message}");
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    // Endpoint pentru actualizarea stadiului dosarului
    [HttpPut("update-status/{id}")]
    public async Task<IActionResult> UpdateFileStatus(int id, [FromBody] string newStatus)
    {
      var dosar = await _context.Files.FindAsync(id);
      if (dosar == null)
      {
        return NotFound($"Dosarul cu ID {id} nu a fost găsit.");
      }

      // Verificăm dacă stadiul este valid
      if (newStatus != "Deschis" && newStatus != "Inchis")
      {
        return BadRequest("Stadiul trebuie să fie 'Deschis' sau 'Închis'.");
      }

      // Actualizăm stadiul dosarului
      //dosar.Stadiu = newStatus;
      await _context.SaveChangesAsync();

      _logger.LogInformation($"Stadiul dosarului {id} a fost actualizat la {newStatus}.");

      return Ok(new { Message = "Stadiul dosarului a fost actualizat cu succes." });
    }

    [HttpGet("GetFilesForClient/{clientId}")]
    public async Task<IActionResult> GetFilesForClient(int clientId)
    {
      try
      {
        // Se apelează metoda din serviciul de dosare (FileManagementService) care extrage dosarele pentru un client
        var clientFiles = await _fileManagementService.GetFilesForClientAsync(clientId);

        if (clientFiles == null || !clientFiles.Any())
        {
          return NotFound($"Nu au fost găsite dosare pentru clientul cu ID {clientId}.");
        }

        return Ok(clientFiles);
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error fetching files for client {clientId}: {ex.Message}");
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    [HttpGet("scadenta")]

    public async Task<IActionResult> GetFilesByDueDate()
    {
      try
      {
     
        var filesDue = await _fileManagementService.GetFilesByDueDateAsync();

        if (filesDue == null || !filesDue.Any())
        {
          return NotFound("Nu au fost găsite dosare cu termenul de scadență de la data curentă.");
        }

        return Ok(filesDue);
      }
      catch (Exception ex)
      {
        _logger.LogError($"Eroare la obținerea dosarelor: {ex.Message}");
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }




  }
}
