// API entry point: controllers, Swagger and HTTP pipeline.

var builder = WebApplication.CreateBuilder(args);

// Register controllers (Api layer of Clean Architecture).
builder.Services.AddControllers();

// Configure Swagger to expose and document every service.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "TaskPage API",
        Version = "v1",
        Description = "TaskPage technical test services."
    });
});

var app = builder.Build();

// Swagger is available at /swagger (UI) and /swagger/v1/swagger.json (JSON).
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskPage API v1");
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
