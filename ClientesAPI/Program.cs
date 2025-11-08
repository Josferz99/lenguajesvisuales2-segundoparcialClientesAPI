using Microsoft.EntityFrameworkCore;
using ClientesAPI.Data;
using ClientesAPI.Middleware;
var builder = WebApplication.CreateBuilder(args);

// Configurar DbContext con SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.WebHost.UseUrls("http://localhost:5131");
// Registrar el filtro global de excepciones
builder.Services.AddScoped<GlobalExceptionFilter>();

// Add services to the container.
builder.Services.AddControllers(options =>
{
    // Agregar el filtro global
    options.Filters.Add<GlobalExceptionFilter>();
});

// Configurar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                         .AllowAnyMethod()
                         .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// IMPORTANTE: Agregar el middleware de logging
app.UseMiddleware<LoggingMiddleware>();

// Configurar archivos estáticos
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Uploads")),
    RequestPath = "/Uploads"
});

app.UseAuthorization();

app.MapControllers();

app.Run();