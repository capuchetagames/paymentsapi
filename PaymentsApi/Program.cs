using Core.Models;
using Core.Repository;
using Infrastructure.Repository;
using Microsoft.Extensions.Options;
using PaymentsApi.Configs;
using PaymentsApi.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();


// Configuração do HttpClient para comunicação com UserAPI
builder.Services.AddHttpClient("UsersApi", client =>
{
    var usersApiUrl = builder.Configuration["Services:UsersApi:BaseUrl"] ?? "http://usersapi:5000/";
    client.BaseAddress = new Uri(usersApiUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Registrar serviços de validação de token
builder.Services.AddScoped<ITokenValidationService, TokenValidationService>();

builder.Services.AddScoped<IPaymentProcessor, PaymentProcessorService>();

builder.Services.AddHealthChecks();

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddSingleton<IRabbitMqService>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<RabbitMqSettings>>().Value;
    return new RabbitMqService(settings);
});

builder.Services.AddHostedService<OrderEventsConsumer>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.ApplyMigrations();
    
    
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();


app.MapHealthChecks("/health");

// Adicionar middleware de validação JWT customizado
//app.UseMiddleware<JwtValidationMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();