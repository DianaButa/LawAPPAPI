using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using LawProject.Service;
using LawProject.Service.EmailService;
using LawProject.Service.FileService;
using LawProject.Service.ICCJ;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LawProject.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class FilesController : ControllerBase


  {

    private readonly IFileManagementService _fileManagementService;
    private readonly FileToCalendarService _fileToCalendarService;
    private readonly MyQueryService _queryService;
    private readonly ILogger<FilesController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IIccjService _ccjService;
    public FilesController(IFileManagementService fileManagementService, FileToCalendarService fileToCalendarService, MyQueryService queryService,
                                ILogger<FilesController> logger, IEmailService emailService, ApplicationDbContext _context, IIccjService iccjService)
    {
      _fileManagementService = fileManagementService;
      _fileToCalendarService = fileToCalendarService;
      _queryService = queryService;
      _logger = logger;
      _context = _context;
      _ccjService = iccjService;
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
    public async Task<IActionResult> GetFileByNumberWithSource(string fileNumber, [FromQuery] string source)
    {
      try
      {
        // Decodifică numărul de dosar din URL
        fileNumber = Uri.UnescapeDataString(fileNumber);

        if (string.IsNullOrEmpty(source))
        {
          return BadRequest("Source is required.");
        }

        if (source.Equals("JUST", StringComparison.OrdinalIgnoreCase))
        {
          // Execută logica pentru source = "JUST"
          var fileDetail = await _fileManagementService.GetFileDetailsByNumberAsync(fileNumber);

          if (fileDetail == null)
          {
            return NotFound($"File with number {fileNumber} not found.");
          }

          return Ok(fileDetail);
        }
        else if (source.Equals("ICCJ", StringComparison.OrdinalIgnoreCase))
        {
          // Execută logica pentru source = "ICCJ"
          var dosare = await _ccjService.CautareDosareAsync(fileNumber);

          if (dosare == null || !dosare.Any())
          {
            _logger.LogWarning($"No case details found for file number: {fileNumber}");
            return NotFound($"No case details found for file number: {fileNumber}");
          }

          return Ok(dosare);
        }
        else
        {
          
          return BadRequest("Invalid source value. It should be either 'JUST' or 'ICCJ'.");
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "An error occurred while retrieving the file details.");
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    [HttpGet("by-fileNumber")]
    public async Task<IActionResult> GetFileByFileNumber(string fileNumber)
    {
      try
      {
        var file = await _fileManagementService.GetFileByNumberAsync(fileNumber);
        if (file == null)
        {
          return NotFound($"File with number {fileNumber} not found");
        }
        return Ok(file);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal Server Error: {ex.Message}");
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

        await _fileManagementService.AddFileAsync(dto);

        if (dto.Source == "ICCJ")
        {
   
          _logger.LogInformation($"Processing ICCJ file {dto.FileNumber}");
          await _fileToCalendarService.ProcessSingleFileIccjAsync(dto.FileNumber);
        }
        else if (dto.Source == "JUST")
        {
          
          _logger.LogInformation($"Processing JUST file {dto.FileNumber}");
          await _fileToCalendarService.ProcessSingleFileAsync(dto.FileNumber);
        }
        else
        {
         
          _logger.LogInformation($"No specific source. Saving file {dto.FileNumber} without additional processing.");
        }

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
        return Ok(scheduledEvents);
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error fetching scheduled events: {ex.Message}");
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }


    [HttpGet("scheduledEvent-clientId")]
    public async Task<IActionResult> GetScheduledEventsbyClientId(int clientId, string clientName)
    {
      try
      {
        var scheduledEvents = await _fileManagementService.GetScheduledEventbyClientIdAsync(clientId, clientName);
        return Ok(scheduledEvents);
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

    //Inchidere dosar
    [HttpPut("close-file")]
    public async Task<IActionResult> CloseFileAsync(int id, [FromBody] CloseFileDto closeFileDto)
    {
      try
      {
        if (closeFileDto.Outcome != "Castigat" && closeFileDto.Outcome != "Necastigat")
        {
          return BadRequest("Rezultatul trebuie să fie 'Castigat' sau 'Necastigat'.");
        }
        await _fileManagementService.CloseFileAsync(id, closeFileDto.Outcome);

        return Ok(new { Message = "Fișierul a fost închis cu succes." });
      }
      catch (KeyNotFoundException ex)
      { 
        return NotFound(new { Message = ex.Message });
      }
      catch (ArgumentException ex)
      {
        return BadRequest(new { Message = ex.Message });
      }
      catch (Exception ex)
      {
        _logger.LogError($"Eroare la închiderea dosarului {id}: {ex.Message}");
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }


    [HttpGet("closed-files")]
    public async Task<IActionResult> GetAllClosedFilesAsync()
    {
      try
      {
        var closedFiles = await _fileManagementService.GetAllClosedFilesAsync();

        if (closedFiles == null || !closedFiles.Any())
        {
          return NotFound(new { Message = "Nu există dosare închise." });
        }

        return Ok(closedFiles);
      }
      catch (Exception ex)
      {
        _logger.LogError($"Eroare la obținerea dosarelor închise: {ex.Message}");
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
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

    [HttpGet("by-lawyer")]
    public async Task<IActionResult> GetFilesByLawyerId(int lawyerId)
    {
      try
      {
        var files = await _fileManagementService.GetFilesByLawyerIdAsync(lawyerId);

        if (files == null || files.Count == 0)
        {
          return NotFound($"No files found for Lawyer with ID {lawyerId}.");
        }

        return Ok(files);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    // Controller - Endpoint pentru dosarele deschise ale unui avocat
    [HttpGet("by-lawyer/{lawyerId}/closed")]
    public async Task<IActionResult> GetClosedFilesByLawyerId(int lawyerId)
    {
      try
      {
        var files = await _fileManagementService.GetClosedFilesByLawyerIdAsync(lawyerId);

        if (files == null || !files.Any())
        {
          return NotFound($"No closed files found for Lawyer with ID {lawyerId}.");
        }

        return Ok(files);  // Returnează fișierele închise
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error retrieving closed files for Lawyer with ID {lawyerId}: {ex.Message}");
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    // Endpoint pentru fișierele deschise ale unui avocat
    [HttpGet("by-lawyer/{lawyerId}/open")]
    public async Task<IActionResult> GetOpenFilesByLawyerId(int lawyerId)
    {
      try
      {
        var files = await _fileManagementService.GetOpenFilesByLawyerIdAsync(lawyerId);

        if (files == null || !files.Any())
        {
          return NotFound($"No open files found for Lawyer with ID {lawyerId}.");
        }

        return Ok(files);  // Returnează fișierele deschise
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error retrieving open files for Lawyer with ID {lawyerId}: {ex.Message}");
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }




    }
}
