using API_Maquinas.DTOs;
using API_Maquinas.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

//Clave secreta
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);


// Add services to the container.

builder.Services.AddControllers();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IMachineService, MachineService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<PdfService>();
builder.Services.AddScoped<ILoginAuth, LoginAuth>();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwagger>();


///Versionado de API
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; 
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddDbContext<StoredContext>(options =>
{
    // Obtener la cadena de conexión de la variable de entorno de Heroku
    var connStr = Environment.GetEnvironmentVariable("DATABASE_URL");

    // Si no existe, usar la cadena de conexión local
    if (connStr is null)
    {
        connStr = builder.Configuration.GetConnectionString("StoreConnection");
        options.UseSqlServer(connStr);
    }
    else
    {
        // Convertir la URL de la base de datos de Heroku en una cadena de conexión
        connStr = connStr.Replace("postgres://", string.Empty);
        var pgUserPass = connStr.Split("@")[0];
        var pgHostPortDb = connStr.Split("@")[1];
        var pgHostPort = pgHostPortDb.Split("/")[0];
        var pgDb = pgHostPortDb.Split("/")[1];
        var pgUser = pgUserPass.Split(":")[0];
        var pgPass = pgUserPass.Split(":")[1];
        var pgHost = pgHostPort.Split(":")[0];
        var pgPort = pgHostPort.Split(":")[1];

        connStr = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};SSL Mode=Require;Trust Server Certificate=true";

        // Usar el proveedor de PostgreSQL
        options.UseNpgsql(connStr);
    }
});
var app = builder.Build();

var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                                $"Maquinarias {description.GroupName.ToUpperInvariant()}");
    }
});


//Vinculo desde appsettings.json la conexion a la base de datos sql

app.UseAuthentication();

app.UseAuthorization();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
