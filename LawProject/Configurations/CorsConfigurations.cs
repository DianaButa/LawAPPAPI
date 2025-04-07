namespace LawProject.Configurations
{
  public static class CorsConfigurations
  {
    public static IServiceCollection ConfigureCors(this IServiceCollection services)
    {
      services.AddCors(options =>
      {
        options.AddPolicy("AllowAllOrigins",
            builder =>
            {
              builder.AllowAnyOrigin()
                             .AllowAnyHeader()
                             .AllowAnyMethod();
            });
      });

      return services;
    }
  }
}
