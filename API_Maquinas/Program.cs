using API_Maquinas.DTOs;
using API_Maquinas.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IMaquinaService, MaquinaService>();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Maquinarias",
        Version = "v1",
        Description = "Gestión de stock y búsqueda de máquinas"
    });
});


builder.Services.AddDbContext<StoredContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("StoreConnection"));
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Vinculo desde appsettings.json la conexion a la base de datos sql



app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
