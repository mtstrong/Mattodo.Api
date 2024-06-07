using FluentValidation;
using Mattodo.Api;
using Mattodo.Api.Models;
using Mattodo.Api.Services;
using Mattodo.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using FluentValidation.Results;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(ApiKeySchemeConstants.SchemeName)
    .AddScheme<ApiKeyAuthSchemeOptions, ApiKeyAuthHandler>(ApiKeySchemeConstants.SchemeName, _ => { });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new SqliteConnectionFactory(
        builder.Configuration.GetValue<string>("Database:ConnectionString")));
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddSingleton<ITodoTaskService, TodoTaskService>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

//app.UseAuthorization();

app.MapGet("/", () => "Hello World!")
.WithTags("ForTheLulz");

app.MapPost("tasks", 
[Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)]
async (TodoTask task, ITodoTaskService todoTaskService, IValidator<TodoTask> validator) =>
{
    var validationResult = await validator.ValidateAsync(task);
    if(!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors);
    }

    task.Id = Guid.NewGuid().ToString();
    task.LastModified = DateTime.UtcNow;
    task.Started = DateTime.MinValue;
    task.Completed = DateTime.MaxValue;

    var created = await todoTaskService.CreateAsync(task);
    if(!created)
    {
        return Results.BadRequest(new
        {
            errorMessage = "A task with this id already exists"
        });
    }

    return Results.Created($"/tasks/{task.Id}", task);
})
.Accepts<TodoTask>("application/json")
.Produces<TodoTask>(201)
.Produces<IEnumerable<ValidationFailure>>(400)
.WithTags("Tasks");

app.MapGet("tasks",
[Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)]
async (ITodoTaskService todoTaskService) =>
{
    var tasks = await todoTaskService.GetAllAsync();
    return Results.Ok(tasks); 
})
.Produces<IEnumerable<TodoTask>>(200)
.WithTags("Tasks");

app.MapGet("tasks/{id}", 
[Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)]
async (string id, ITodoTaskService todoTaskService) =>
{
    var task = await todoTaskService.GetTodoTaskByIdAsync(id);
    return task is not null ? Results.Ok(task) : Results.NotFound();
})
.Produces<TodoTask>(200)
.Produces(404)
.WithTags("Tasks");

app.MapPut("tasks/{id}", 
[Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)]
async (string id, TodoTask task, ITodoTaskService todoTaskService, 
    IValidator<TodoTask> validator) =>
{
    if(id != task.Id)
    {
        return Results.NotFound();
    }

    var validationResult = await validator.ValidateAsync(task);
    if(!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors);
    }

    var updated = await todoTaskService.UpdateAsync(task);
    return updated ? Results.Ok(task) : Results.NotFound();
})
.Accepts<TodoTask>("application/json")
.Produces<TodoTask>(200)
.Produces<IEnumerable<ValidationFailure>>(400)
.WithTags("Tasks");

app.MapDelete("tasks/{id}", 
[Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)]
async (string id, ITodoTaskService todoTaskService) =>
{
    var deleted = await todoTaskService.DeleteAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
})
.Produces(204)
.Produces(404)
.WithTags("Tasks");

var dbInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await dbInitializer.InitializeAsync();

app.Run();
