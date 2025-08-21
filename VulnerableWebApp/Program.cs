using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using VulnerableWebApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ❌ SECURITY ISSUE: Hardcoded connection string with credentials
var connectionString = "Server=localhost;Database=TestDB;User=admin;Password=password123;";

app.MapGet("/users/{id}", async (string id) =>
{
    // ❌ SECURITY ISSUE: SQL Injection vulnerability
    var connection = new SqlConnection(connectionString);
    connection.Open();
    
    var command = new SqlCommand($"SELECT * FROM Users WHERE Id = '{id}'", connection);
    var reader = await command.ExecuteReaderAsync();
    
    var users = new List<object>();
    while (reader.Read())
    {
        users.Add(new { 
            Id = reader["Id"], 
            Name = reader["Name"],
            Email = reader["Email"]
        });
    }
    
    // ❌ SECURITY ISSUE: Connection not properly disposed
    return Results.Ok(users);
});

app.MapPost("/login", ([FromBody] LoginRequest request) =>
{
    // ❌ SECURITY ISSUE: Hardcoded API key
    var apiKey = "sk-1234567890abcdef";
    
    // ❌ SECURITY ISSUE: Password comparison without proper validation
    if (request.Username == "admin" && request.Password == "admin123")
    {
        return Results.Ok(new { Token = apiKey, Message = "Login successful" });
    }
    
    return Results.Unauthorized();
});

app.MapGet("/file/{filename}", (string filename) =>
{
    // ❌ SECURITY ISSUE: Path traversal vulnerability
    var filePath = Path.Combine("uploads", filename);
    
    if (File.Exists(filePath))
    {
        var content = File.ReadAllText(filePath);
        return Results.Ok(new { Content = content });
    }
    
    return Results.NotFound();
});

app.Run();

// Keep file-scoped namespace; models moved to separate file