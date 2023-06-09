using Microsoft.EntityFrameworkCore;
using BrinquedosAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Adiciona servi�os ao container.
builder.Services.AddControllers();
builder.Services.AddDbContext<BrinquedoContext>(opt =>
    opt.UseInMemoryDatabase("BrinquedosList"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(builder =>
        {
            builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
        });
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors();

app.MapControllers();

app.Run();
