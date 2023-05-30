using Microsoft.EntityFrameworkCore;
using BrinquedosAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços ao container.
builder.Services.AddControllers();
builder.Services.AddDbContext<BrinquedoContext>(opt =>
    opt.UseInMemoryDatabase("BrinquedosList"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
