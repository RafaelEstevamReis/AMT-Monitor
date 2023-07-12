using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
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

if (!await central.ConnectAsync())
{
    throw new System.Exception("Unable to connect to AMT");
}

app.Run();
