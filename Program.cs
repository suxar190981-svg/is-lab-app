var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapGet("/", () => "Hello World!");
app.MapGet("/health", () => new { status = "ok", timestamp = DateTime.UtcNow });
app.MapGet("/version", () => new { name = "IsLabApp", version = "1.0.1-lab11" });
app.Run();
