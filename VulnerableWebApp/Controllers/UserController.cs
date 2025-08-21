using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace VulnerableWebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        // ❌ SECURITY ISSUE: Hardcoded connection string
        private readonly string _connectionString = "Server=localhost;Database=TestDB;User=sa;Password=P@ssw0rd123;";

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            // ❌ SECURITY ISSUE: SQL Injection vulnerability
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            // ❌ CRITICAL: Direct string interpolation in SQL
            var query = $"SELECT * FROM Users WHERE Id = {id} OR '1'='1'";
            var command = new SqlCommand(query, connection);
            
            var reader = await command.ExecuteReaderAsync();
            var result = new List<object>();
            
            while (await reader.ReadAsync())
            {
                result.Add(new
                {
                    Id = reader["Id"],
                    Name = reader["Name"],
                    Email = reader["Email"],
                    // ❌ SECURITY ISSUE: Potentially exposing sensitive data
                    Password = reader["Password"]
                });
            }
            
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User? user)
        {
            if (user is null)
            {
                return BadRequest("User payload is required");
            }

            // ❌ BUG: No null validation
            var upperName = (user.Name ?? string.Empty).ToUpper();
            
            // ❌ SECURITY ISSUE: SQL Injection in INSERT
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var name = user.Name ?? string.Empty;
            var email = user.Email ?? string.Empty;
            var password = user.Password ?? string.Empty;

            var insertQuery = $"INSERT INTO Users (Name, Email, Password) VALUES ('{name}', '{email}', '{password}')";
            var command = new SqlCommand(insertQuery, connection);
            
            await command.ExecuteNonQueryAsync();
            
            return Ok(new { Message = "User created successfully" });
        }

        [HttpGet("search")]
        public IActionResult SearchUsers(string query)
        {
            // ❌ CODE SMELL: Long method with complex logic
            if (query == null)
            {
                return BadRequest("Query cannot be null");
            }

            if (query.Length == 0)
            {
                return BadRequest("Query cannot be empty");
            }

            if (query.Length > 100)
            {
                return BadRequest("Query too long");
            }

            if (query.Contains('\''))
            {
                return BadRequest("Invalid characters");
            }

            if (query.Contains("--"))
            {
                return BadRequest("Invalid characters");
            }

            if (query.Contains(';'))
            {
                return BadRequest("Invalid characters");
            }

            // ❌ PERFORMANCE ISSUE: Inefficient string operations
            var cleanQuery = query.Replace(" ", "").Replace("\t", "").Replace("\n", "");
            cleanQuery = cleanQuery.ToLower();
            cleanQuery = cleanQuery.Trim();

            // ❌ MAGIC NUMBERS: Hardcoded values
            if (cleanQuery.Length < 3)
            {
                return BadRequest("Query too short");
            }

            // ❌ SECURITY ISSUE: Still vulnerable to SQL injection
            var sqlQuery = $"SELECT * FROM Users WHERE Name LIKE '%{cleanQuery}%'";
            
            return Ok(new { Query = sqlQuery, Message = "Search completed" });
        }
    }

    // ❌ CODE SMELL: Missing validation attributes
    public class User
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}