using Microsoft.OpenApi.Models;
using webapi_peso.Dbcontext;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi


builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer(); // <!-- Add this line


builder.Services.AddSwaggerGen(); // <!-- Add this line



builder.Services.AddSingleton<TupadRepository>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger(); // <!-- Add this line
    app.UseSwaggerUI(); // <!-- Add this line
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.Run();
