


using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Hangfire;
using Hangfire.SqlServer;
using LawProject.Configurations;
using LawProject.Database;
using LawProject.Hubs;
using LawProject.Service;
using LawProject.Service.AccountService;
using LawProject.Service.CheltuieliService;
using LawProject.Service.ClientService;
using LawProject.Service.ContractService;
using LawProject.Service.DailyEventService;
using LawProject.Service.DelegatieService;
using LawProject.Service.EmailService;
using LawProject.Service.EventService;
using LawProject.Service.FileService;
using LawProject.Service.ICCJ;
using LawProject.Service.InvoiceSerices;
using LawProject.Service.Lawyer;
using LawProject.Service.Notifications;
using LawProject.Service.POSService;
using LawProject.Service.RaportService;
using LawProject.Service.ReceiptService;
using LawProject.Service.TaskService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PdfSharp.Charting;
using ServiceReference1;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:ConnectionString"]);



// Configurarea Hangfire
builder.Services.AddHangfire(config =>
{
  config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(builder.Configuration.GetConnectionString("ProdConnection"), new SqlServerStorageOptions
        {
          CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
          SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
          QueuePollInterval = TimeSpan.Zero,
          UseRecommendedIsolationLevel = true,
          UsePageLocksOnDequeue = true,
          DisableGlobalLocks = true
        });
});

// Adăugarea serverului Hangfire
builder.Services.AddHangfireServer();

builder.Services.AddHttpClient<IccjService>(client =>
{
  // Configurăm URL-ul de bază pentru HttpClient
  client.BaseAddress = new Uri("https://www.scj.ro/api/");
});


// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProdConnection")));

builder.Services.AddControllers().AddJsonOptions(opts =>
{
  opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IIccjService, IccjService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IRaportService, RaportService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IDelegatieService, DelegatieService>();
builder.Services.AddScoped<IDailyEventService,DailyEventService>();
builder.Services.AddScoped<ICheltuieliService, CheltuieliService>();
builder.Services.AddScoped<IFileManagementService>();
builder.Services.AddScoped<IReceiptService, ReceiptService>();
builder.Services.AddScoped<FileToCalendarService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ILawyerService, LawyerService>();
builder.Services.AddScoped<IPOSService, POSService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddMemoryCache();


builder.Services.AddScoped<MyQueryService>();
builder.Services.AddScoped<QuerySoapClient>(provider =>
{
  var configuration = provider.GetRequiredService<IConfiguration>();
  var endpointAddress = configuration["QuerySoapClient:Endpoint"];
  var binding = new System.ServiceModel.BasicHttpBinding();
  var endpoint = new System.ServiceModel.EndpointAddress(endpointAddress);
  return new QuerySoapClient(binding, endpoint);
});

builder.Services.AddHealthChecks()
    // Aici este magia: adaugi o verificare SPECIFICĂ pentru Hangfire
    .AddHangfire(options => {
      options.MinimumAvailableServers = 1;
    });

builder.Services.AddSignalR();
builder.Services.AddLogging();
builder.Services.ConfigureCors(builder.Configuration);
builder.Services.AddControllers();



builder.Services.AddAuthentication(options =>
{
  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = builder.Configuration["JWT:Issuer"],
    ValidAudience = builder.Configuration["JWT:Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
  };
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();




var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
  app.UseSwagger();
  app.UseSwaggerUI();
//}s

app.UseCors("MyAllowSpecificOrigins");

app.UseHttpsRedirection();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");


app.MapHealthChecks("/health");
// Folosirea dashboard-ului Hangfire pentru monitorizare la ruta "/hangfire"
app.UseHangfireDashboard("/hangfire");
app.UseHangfireServer();
app.Logger.LogInformation("Hangfire Server started.");

RecurringJob.AddOrUpdate<FileToCalendarService>(
    "process-today-files-periodically",                
    service => service.ProcessDosareFromScheduledEventsTodayAsync(),  
    Cron.MinuteInterval(30));

RecurringJob.AddOrUpdate<FileToCalendarService>(
    "process-just-files-periodically",
    service => service.ProcessJustFilesAsync(),
    Cron.MinuteInterval(30));

RecurringJob.AddOrUpdate<BackgroundFileCacheUpdater>(
    "update-soap-cache-periodically",
    service => service.RefreshCacheJob(),
    "0 6 * * 6");

app.Run();


