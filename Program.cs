var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapGet("/", () => "Hello World!");
app.MapGet("/health", () => new { status = "ok", timestamp = DateTime.UtcNow });
app.MapGet("/version", () => new { name = "IsLabApp", version = "2.0.0-lab12" });
app.Run();
