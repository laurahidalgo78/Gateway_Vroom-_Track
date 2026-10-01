using Gateway.Application.Services;
using Gateway.Domain.Interfaces;
using Gateway.Infrastructure.Configuration;
using Gateway.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "VroomService API Gateway - SuiteCRM Integration",
        Version = "v1",
        Description = "API Gateway para integración y validación de consumo con SuiteCRM REST v4_1."
    });
});

// Configure SuiteCRM Options
builder.Services.Configure<SuiteCrmOptions>(
    builder.Configuration.GetSection(SuiteCrmOptions.SectionName));

// Register HttpClient for SuiteCRM
builder.Services.AddHttpClient<ISuiteCrmClient, SuiteCrmClient>((serviceProvider, httpClient) =>
{
    var options = builder.Configuration.GetSection(SuiteCrmOptions.SectionName).Get<SuiteCrmOptions>() ?? new SuiteCrmOptions();
    httpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

// Register Domain & Infrastructure services
builder.Services.AddScoped<ISuiteCrmValidationService, SuiteCrmValidationService>();
builder.Services.AddScoped<SuiteCrmAppService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "VroomService SuiteCRM API Gateway v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
