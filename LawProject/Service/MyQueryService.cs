

using ServiceReference1;

namespace LawProject.Service
{
  public class MyQueryService

  {
    private readonly QuerySoapClient _client;
    private readonly ILogger<MyQueryService> _logger;

    public MyQueryService(IConfiguration configuration, ILogger<MyQueryService> logger)
    {
      var endpointAddress = configuration["QuerySoapClient:Endpoint"];
      var binding = new System.ServiceModel.BasicHttpBinding();
      var endpoint= new System.ServiceModel.EndpointAddress(endpointAddress);
      _client= new QuerySoapClient(binding, endpoint);
      _logger = logger;
    }

    public async Task<Dosar[]> CautareDosareAsync(string numarDosar, string? obiectDosar = null, string? numeParte = null, Institutie? institutie = null, DateTime? dataStart = null, DateTime? dataStop = null)
    {
      if (string.IsNullOrEmpty(numarDosar))
      {
        throw new ArgumentException("Numărul dosarului este obligatoriu.", nameof(numarDosar));
      }

      try
      {
        _logger.LogInformation("Initiating request to CautareDosareAsync with parameters: numarDosar={NumarDosar}, obiectDosar={ObiectDosar}, numeParte={NumeParte}, institutie={Institutie}, dataStart={DataStart}, dataStop={DataStop}",
            numarDosar, obiectDosar, numeParte, institutie, dataStart, dataStop);

        var response = await _client.CautareDosareAsync(numarDosar, obiectDosar, numeParte, institutie, dataStart, dataStop);

        _logger.LogInformation("Response received: {@Response}", response);
        return response.Body.CautareDosareResult;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error occurred while fetching dosare");
        throw;
      }
    }

    public async Task<Sedinta[]> CautareSedinteAsync(DateTime dataSedinta, Institutie institutie)
    {
      try
      {
        _logger.LogInformation("Initiating request to CautareSedinteAsync with parameters: dataSedinta={DataSedinta}, institutie={Institutie}",
            dataSedinta, institutie);

        var response = await _client.CautareSedinteAsync(dataSedinta, institutie);

        _logger.LogInformation("Response received: {@Response}", response);
        return response.Body.CautareSedinteResult;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error occurred while fetching sedinte");
        throw;
      }
    }


  }
}
