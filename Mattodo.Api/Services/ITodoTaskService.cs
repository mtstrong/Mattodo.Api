using Mattodo.Api.Models;

namespace Mattodo.Api.Services;

public interface ITodoTaskService
{
    public Task<bool> CreateTodoTaskAsync(TodoTask task);
    public Task<TodoTask?> GetTodoTaskByIdAsync(string id);
    public Task<IEnumerable<TodoTask>> GetAllTodoTasksAsync();
    public Task<bool> UpdateTodoTaskAsync(TodoTask task);
    public Task<bool> DeleteTodoTaskAsync(string id);
}