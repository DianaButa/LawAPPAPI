


using Hangfire;
using Hangfire.SqlServer;
using LawProject.Configurations;
using LawProject.Database;
using LawProject.Service;
using LawProject.Service.ClientService;
using LawProject.Service.EmailService;
using LawProject.Service.EventService;
using LawProject.Service.FileService;
using LawProject.Service.InvoiceSerices;
using LawProject.Service.Lawyer;
using LawProject.Service.Notifications;
using LawProject.Service.ReceiptService;
using LawProject.Service.TaskService;
using Microsoft.EntityFrameworkCore;
using PdfSharp.Charting;
using ServiceReference1;

var builder = WebApplication.CreateBuilder(args);


// Configurarea Hangfire
builder.Services.AddHangfire(config =>
{
  config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
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

// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<FileManagementService>();
builder.Services.AddScoped<IReceiptService, ReceiptService>();
builder.Services.AddScoped<FileToCalendarService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ILawyerService, LawyerService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddScoped<MyQueryService>();
builder.Services.AddScoped<QuerySoapClient>(provider =>
{
  var configuration = provider.GetRequiredService<IConfiguration>();
  var endpointAddress = configuration["QuerySoapClient:Endpoint"];
  var binding = new System.ServiceModel.BasicHttpBinding();
  var endpoint = new System.ServiceModel.EndpointAddress(endpointAddress);
  return new QuerySoapClient(binding, endpoint);
});

builder.Services.AddSignalR();

builder.Services.ConfigureCors();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseCors("AllowAllOrigins");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


// Folosirea dashboard-ului Hangfire pentru monitorizare la ruta "/hangfire"
app.UseHangfireDashboard("/hangfire");
app.UseHangfireServer();

// Adăugăm un job recurent care rulează la fiecare 1 ore pentru a procesa dosarele
RecurringJob.AddOrUpdate<FileToCalendarService>(
    "process-all-files-periodically",                
    service => service.ProcessAllFilesAsync(),     
    Cron.HourInterval(1));
app.Run();
