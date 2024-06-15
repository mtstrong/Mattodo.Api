using FluentValidation;
using FluentValidation.Results;
using Mattodo.Api.Models;
using Mattodo.Api.Services;
using Mattodo.Api.Endpoints.Internal;

namespace Mattodo.Api.Endpoints;

public class TodoTaskEndpoints : IEndpoints
{
    private const string ContentType = "application/json";
    private const string Tag = "Tasks";
    private const string BaseRoute = "tasks";

    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ITodoTaskService, TodoTaskService>();
    }

    public static void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(BaseRoute, CreateTodoTaskAsync)
            .WithName("CreateBook")
            .Accepts<TodoTask>(ContentType)
            .Produces<TodoTask>(201).Produces<IEnumerable<ValidationFailure>>(400)
            .WithTags(Tag);

        app.MapGet(BaseRoute, GetAllTodoTasksAsync)
            .WithName("GetBooks")
            .Produces<IEnumerable<TodoTask>>(200)
            .WithTags(Tag);

        app.MapGet($"{BaseRoute}/{{id}}", GetTodoTaskByIdAsync)
            .WithName("GetTodoTask")
            .Produces<TodoTask>(200).Produces(404)
            .WithTags(Tag);

        app.MapPut($"{BaseRoute}/{{id}}", UpdateTodoTaskAsync)
            .WithName("UpdateTodoTask")
            .Accepts<TodoTask>(ContentType)
            .Produces<TodoTask>(200).Produces<IEnumerable<ValidationFailure>>(400)
            .WithTags(Tag);

        app.MapDelete($"{BaseRoute}/{{id}}", DeleteTodoTaskAsync)
            .WithName("DeleteBook")
            .Produces(204).Produces(404)
            .WithTags(Tag);
    }

     internal static async Task<IResult> CreateTodoTaskAsync(
        TodoTask task, ITodoTaskService todoTaskService, IValidator<TodoTask> validator)
    {
        var validationResult = await validator.ValidateAsync(task);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var created = await todoTaskService.CreateTodoTaskAsync(task);
        if (!created)
        {
            return Results.BadRequest(new List<ValidationFailure>
            {
                new("Id", "A task with this id already exists")
            });
        }

        return Results.Created($"/{BaseRoute}/{task.Id}", task);
    }

    internal static async Task<IResult> GetAllTodoTasksAsync(
        ITodoTaskService todoTaskService/*, string? searchTerm*/)
    {
        /*if (searchTerm is not null && !string.IsNullOrWhiteSpace(searchTerm))
        {
            var matchedBooks = await bookService.SearchByTitleAsync(searchTerm);
            return Results.Ok(matchedBooks);
        }*/

        var tasks = await todoTaskService.GetAllTodoTasksAsync();
        return Results.Ok(tasks);
    }

    internal static async Task<IResult> GetTodoTaskByIdAsync(
        string id, ITodoTaskService todoTaskService)
    {
        var book = await todoTaskService.GetTodoTaskByIdAsync(id);
        return book is not null ? Results.Ok(book) : Results.NotFound();
    }

    internal static async Task<IResult> UpdateTodoTaskAsync(
        string id, TodoTask task, ITodoTaskService todoTaskService,
        IValidator<TodoTask> validator)
    {
        task.Id = id;
        var validationResult = await validator.ValidateAsync(task);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var updated = await todoTaskService.UpdateTodoTaskAsync(task);
        return updated ? Results.Ok(task) : Results.NotFound();
    }

    internal static async Task<IResult> DeleteTodoTaskAsync(
        string id, ITodoTaskService todoTaskService)
    {
        var deleted = await todoTaskService.DeleteTodoTaskAsync(id);
        return deleted ? Results.NoContent() : Results.NotFound();
    }
} 
