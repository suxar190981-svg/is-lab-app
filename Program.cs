var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Регистрируем сервис для работы с заметками
builder.Services.AddSingleton<NoteService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ===== ДИАГНОСТИЧЕСКИЕ ЭНДПОИНТЫ =====

// GET /health
app.MapGet("/health", () => new 
{ 
    status = "ok", 
    timestamp = DateTime.UtcNow 
});

// GET /version
app.MapGet("/version", (IConfiguration config) => new 
{ 
    name = config["App:Name"], 
    version = config["App:Version"] 
});

// GET /db/ping
app.MapGet("/db/ping", async (IConfiguration config) =>
{
    var connectionString = config.GetConnectionString("Mssql");
    try
    {
        using var connection = new System.Data.SqlClient.SqlConnection(connectionString);
        await connection.OpenAsync();
        return new { status = "ok", message = "Database connection successful" };
    }
    catch (Exception ex)
    {
        return new { status = "error", message = ex.Message };
    }
});

// ===== API ЗАМЕТОК =====

var notesApi = app.MapGroup("/api/notes");

// GET /api/notes - получить все заметки
notesApi.MapGet("/", (NoteService service) => service.GetAll());

// GET /api/notes/{id} - получить заметку по ID
notesApi.MapGet("/{id:int}", (int id, NoteService service) =>
{
    var note = service.GetById(id);
    return note is null ? Results.NotFound() : Results.Ok(note);
});

// POST /api/notes - создать заметку
notesApi.MapPost("/", (CreateNoteRequest request, NoteService service) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
        return Results.BadRequest("Title is required");
    
    var note = service.Create(request.Title, request.Text ?? "");
    return Results.Created($"/api/notes/{note.Id}", note);
});

// DELETE /api/notes/{id} - удалить заметку
notesApi.MapDelete("/{id:int}", (int id, NoteService service) =>
{
    return service.Delete(id) ? Results.NoContent() : Results.NotFound();
});

app.Run();

// Вспомогательные классы
record CreateNoteRequest(string Title, string? Text);

class NoteService
{
    private readonly List<Note> _notes = new();
    private int _nextId = 1;

    public List<Note> GetAll() => _notes.ToList();
    
    public Note? GetById(int id) => _notes.FirstOrDefault(n => n.Id == id);
    
    public Note Create(string title, string text)
    {
        var note = new Note
        {
            Id = _nextId++,
            Title = title,
            Text = text,
            CreatedAt = DateTime.UtcNow
        };
        _notes.Add(note);
        return note;
    }
    
    public bool Delete(int id)
    {
        var note = GetById(id);
        if (note == null) return false;
        return _notes.Remove(note);
    }
}

class Note
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}