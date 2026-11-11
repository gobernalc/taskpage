// ------------------------------------------------------------------
// Archivo: Program.cs
// Fecha  : 2026-11-11
// Autor  : gobernalc
// Resumen: Punto de entrada de la API. Configura controladores y Swagger.
// ------------------------------------------------------------------

var builder = WebApplication.CreateBuilder(args);

// Registro de controladores (capa Api de Clean Architecture).
builder.Services.AddControllers();

// Configuración de Swagger para exponer y documentar todos los servicios.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "TaskPage API",
        Version = "v1",
        Description = "Servicios de la prueba técnica TaskPage."
    });
});

var app = builder.Build();

// Swagger disponible en /swagger (UI) y /swagger/v1/swagger.json (JSON).
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskPage API v1");
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
