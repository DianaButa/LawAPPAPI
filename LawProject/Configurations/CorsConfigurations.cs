namespace LawProject.Configurations
{
  public static class CorsConfigurations
  {
    public static IServiceCollection ConfigureCors(this IServiceCollection services, IConfiguration configuration)
    {
      var allowedOrigins = configuration
          .GetSection("Cors:AllowedOrigins")
          .Get<string[]>();

      services.AddCors(options =>
      {
        options.AddPolicy("MyAllowSpecificOrigins", builder =>
        {
          if (allowedOrigins != null && allowedOrigins.Any())
          {
            builder.WithOrigins(allowedOrigins)
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials(); // dacă folosești cookies sau SignalR
          }
          else
          {
            // fallback de siguranță în dev/test
            builder.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod(); 
          }
        });
      });

      return services;
    }

  }
}
