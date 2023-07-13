using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
    .WriteTo.File("events.log", rollingInterval: RollingInterval.Day)
    .WriteTo.Console();
});
// Add services to the container.
builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHostedService<AMT.API.Workers.CentralKeepAlive>();
builder.Services.AddSwaggerGen();

var central = new Simple.AMT.AMT8000(new Simple.AMT.AMTModels.ConnectionInfo
{
    IP = builder.Configuration["ip"],
    Password = builder.Configuration["pwd"],
    Port = 9009,
});
builder.Services.AddSingleton(central);
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

var log = app.Services.GetService<Serilog.ILogger>();
log?.Information("[AMT] Connecting to {ip} with password {yesNo}",
                 central.ConnectionInfo.IP,
                 string.IsNullOrEmpty(central.ConnectionInfo.Password) ? "NO" : "YES");
if (!await central.ConnectAsync())
{
    throw new System.Exception("Unable to connect to AMT");
}

app.Run();
