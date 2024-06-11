using Dapper;
using Mattodo.Api.Models;

namespace Mattodo.Api.Services;

public class TodoTaskService : ITodoTaskService
{
    private readonly IDbConnectionFactory _connectionFactory;
    public TodoTaskService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;    
    }

    public async Task<bool> CreateTodoTaskAsync(TodoTask task)
    {
        task.Id = Guid.NewGuid().ToString();
        task.LastModified = DateTime.UtcNow;

        var existingTask = await GetTodoTaskByIdAsync(task.Id);
        if(existingTask is not null)
        {
            return false;
        }

        using var connection = await _connectionFactory.CreateConnectionAsync();
        var result = await connection.ExecuteAsync(
            @"INSERT INTO Tasks (Id, Title, Details, Author, Started, Completed, LastModified)
            VALUES (@Id, @Title, @Details, @Author, @Started, @Completed, @LastModified)",
            task);
        return result > 0;
    }

    public async Task<TodoTask?> GetTodoTaskByIdAsync(string id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return connection.QuerySingleOrDefault<TodoTask>(
            "SELECT * FROM Tasks WHERE Id = @Id LIMIT 1", new {Id = id});
    }
 
    public async Task<IEnumerable<TodoTask>> GetAllTodoTasksAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<TodoTask>(
            "SELECT * FROM Tasks");
    }

    public async Task<bool> UpdateTodoTaskAsync(TodoTask task)
    {
        var existingTask = await GetTodoTaskByIdAsync(task.Id);
        if(existingTask is null)
        {
            return false;
        }

        //:todo move to datetime service
        task.LastModified = DateTime.UtcNow;

        using var connection = await _connectionFactory.CreateConnectionAsync();
        var result = await connection.ExecuteAsync(
            @"UPDATE Tasks SET Title = @Title, Details = @Details, Author = @Author, 
                Started = @Started, Completed = @Completed, LastModified = @LastModified",
            task);
        return result > 0;
    }

    public async Task<bool> DeleteTodoTaskAsync(string id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var result = await connection.ExecuteAsync(
            @"DELETE FROM Tasks WHERE Id = @Id", new {Id = id});
        return result > 0;
    }
}