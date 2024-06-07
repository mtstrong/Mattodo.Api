using Mattodo.Api.Models;

namespace Mattodo.Api.Services;

public interface ITodoTaskService
{
    public Task<bool> CreateAsync(TodoTask task);
    public Task<TodoTask?> GetTodoTaskByIdAsync(string id);
    public Task<IEnumerable<TodoTask>> GetAllAsync();
    public Task<bool> UpdateAsync(TodoTask task);
    public Task<bool> DeleteAsync(string id);
}