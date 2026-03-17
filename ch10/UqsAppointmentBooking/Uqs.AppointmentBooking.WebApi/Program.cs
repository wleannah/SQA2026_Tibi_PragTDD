using Microsoft.Azure.Cosmos;
using Uqs.AppointmentBooking.Domain;
using Uqs.AppointmentBooking.Domain.Database;
using Uqs.AppointmentBooking.Domain.Repository;
using Uqs.AppointmentBooking.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

// Add DB Services
#if DEBUG
// Development: bypass the Cosmos Emulator's self-signed certificate.
// HttpClientHandler.DangerousAcceptAnyServerCertificateValidator is intentional here —
// the emulator cert is localhost-only and this code path is excluded from Release builds.
var cosmosHttpHandler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
};
builder.Services.AddSingleton<CosmosClient>(
    new CosmosClient(builder.Configuration.GetConnectionString("AppointmentBooking"),
    new CosmosClientOptions
    {
        HttpClientFactory = () => new HttpClient(cosmosHttpHandler),
        ConnectionMode = ConnectionMode.Gateway,
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    }));
#else
builder.Services.AddSingleton<CosmosClient>(
    new CosmosClient(builder.Configuration.GetConnectionString("AppointmentBooking"),
    new CosmosClientOptions
    {
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    }));
#endif
builder.Services.AddSingleton<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddSingleton<IServiceRepository, ServiceRepository>();
builder.Services.AddSingleton<ICustomerRepository, CustomerRepository>();
builder.Services.AddSingleton<IAppointmentRepository, AppointmentRepository>();

// Add services to the container.
builder.Services.AddScoped<INowService, NowService>();
builder.Services.AddScoped<IServicesService, ServicesService>();
builder.Services.AddScoped<IEmployeesService, EmployeesService>();
builder.Services.AddScoped<ISlotsService, SlotsService>();
builder.Services.Configure<ApplicationSettings>(builder.Configuration.GetSection(nameof(ApplicationSettings)));
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        await SeedData.Initialize(services);
    }
}

// Configure the HTTP request pipeline.

// Enable CORS
app.UseCors(x => x
   .AllowAnyMethod()
   .AllowAnyHeader()
   .SetIsOriginAllowed(_ => true) // allow any origin  
   .AllowCredentials());               // allow credentials 

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();