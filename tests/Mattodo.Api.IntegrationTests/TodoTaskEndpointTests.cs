using System.Net;
using FluentAssertions;
using Mattodo.Api.Models;
using Mattodo.Api.Tests.Integration;
using Mattodo.Api.Data;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Mattodo.Api.IntegrationTests;

public class TodoTaskEndpointTests : IClassFixture<TodoTaskApiFactory>, IAsyncLifetime
{
    private readonly WebApplicationFactory<IApiMarker> _factory;
    private readonly List<string> _createdIds = new();

    public TodoTaskEndpointTests(WebApplicationFactory<IApiMarker> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateTodoTask_ReturnsOK_WhenDataIsCorrect()
    {
        var httpClient = _factory.CreateClient();
        var todoTask = GenerateValidTodoTask();

        var result = await httpClient.PostAsJsonAsync("/tasks", todoTask);
        var createdTask = await result.Content.ReadFromJsonAsync<TodoTask>();
        _createdIds.Add(createdTask?.Id);

        result.StatusCode.Should().Be(HttpStatusCode.Created);
        createdTask?.Id.Should().NotBe(todoTask.Id);
        createdTask?.LastModified.Should().NotBe(todoTask.LastModified);
        createdTask?.Title.Should().Be(todoTask.Title);
        createdTask?.Details.Should().Be(todoTask.Details);
        createdTask?.Started.Should().Be(todoTask.Started);
        createdTask?.Completed.Should().Be(todoTask.Completed);
    }

    [Fact]
    public async Task CreateTodoTask_ReturnsBadRequest_EmptyStringTitle()
    {
        var httpClient = _factory.CreateClient();
        var todoTask = GenerateInvalidTodoTask_MissingTitle();

        var result = await httpClient.PostAsJsonAsync("/tasks", todoTask);

        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTodoTask_ReturnsBadRequest_EmptyStringDetails()
    {
        var httpClient = _factory.CreateClient();
        var todoTask = GenerateInvalidTodoTask_MissingDetails();

        var result = await httpClient.PostAsJsonAsync("/tasks", todoTask);

        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task CreateTodoTask_ReturnsBadRequest_EmptyStringAuthor()
    {
        var httpClient = _factory.CreateClient();
        var todoTask = GenerateInvalidTodoTask_MissingAuthor();

        var result = await httpClient.PostAsJsonAsync("/tasks", todoTask);

        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTodoTask_ReturnsTodoTask_WhenToDoTaskExists()
    {
        var httpClient = _factory.CreateClient();
        var todoTask = GenerateValidTodoTask();
        var result = await httpClient.PostAsJsonAsync("/tasks", todoTask);
        var createdTask = await result.Content.ReadFromJsonAsync<TodoTask>();
        _createdIds.Add(createdTask?.Id);

        result = await httpClient.GetAsync($"/tasks/{createdTask?.Id}");
        var returnedTask = await result.Content.ReadFromJsonAsync<TodoTask>();

        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTodoTasks_ReturnsTodoTasks_WhenToDoTasksExists()
    {
        var httpClient = _factory.CreateClient();
        var todoTask = GenerateValidTodoTask();
        var result = await httpClient.PostAsJsonAsync("/tasks", todoTask);
        var createdTask = await result.Content.ReadFromJsonAsync<TodoTask>();
        _createdIds.Add(createdTask?.Id);
        result = await httpClient.PostAsJsonAsync("/tasks", todoTask);
        var createdTaskTwo = await result.Content.ReadFromJsonAsync<TodoTask>();
        _createdIds.Add(createdTaskTwo?.Id);

        result = await httpClient.GetAsync($"/tasks");
        var returnedTasks = await result.Content.ReadFromJsonAsync<IEnumerable<TodoTask>>();

        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedTasks?.ToList<TodoTask>().Count.Should().Be(2);
    }

    [Fact]
    public async Task UpdateTodoTask_ReturnsOk_IfTodoTaskExists()
    {
        var httpClient = _factory.CreateClient();
        var todoTask = GenerateValidTodoTask();
        var result = await httpClient.PostAsJsonAsync("/tasks", todoTask);
        var createdTask = await result.Content.ReadFromJsonAsync<TodoTask>();
        _createdIds.Add(createdTask?.Id);
        var currentTime = DateTime.UtcNow;
        createdTask.Started = currentTime;

        result = await httpClient.PutAsJsonAsync($"/tasks/{createdTask?.Id}", createdTask);

        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateTodoTask_ReturnsBadRequest_IfStartedIsAfterCompleted()
    {
        var httpClient = _factory.CreateClient();
        var todoTask = GenerateValidTodoTask();
        var result = await httpClient.PostAsJsonAsync("/tasks", todoTask);
        var createdTask = await result.Content.ReadFromJsonAsync<TodoTask>();
        _createdIds.Add(createdTask?.Id);
        var currentTime = DateTime.UtcNow;
        createdTask.Started = currentTime;
        createdTask.Completed = currentTime.AddDays(-5);

        result = await httpClient.PutAsJsonAsync($"/tasks/{createdTask?.Id}", createdTask);

        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTodoTask_ReturnsNotFound_IfTodoTaskDoesNotExist()
    {
        var httpClient = _factory.CreateClient();
        var todoTask = GenerateValidTodoTask();
        var currentTime = DateTime.UtcNow;
        todoTask.Started = currentTime;

        var result = await httpClient.PutAsJsonAsync($"/tasks/{Guid.NewGuid().ToString()}", todoTask);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTodoTask_ReturnsNoContent_IfTodoTaskExists()
    {
        var httpClient = _factory.CreateClient();
        var todoTask = GenerateValidTodoTask();
        var result = await httpClient.PostAsJsonAsync("/tasks", todoTask);
        var createdTask = await result.Content.ReadFromJsonAsync<TodoTask>();
        result.StatusCode.Should().Be(HttpStatusCode.Created);

        result = await httpClient.DeleteAsync($"/tasks/{createdTask?.Id}");

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteTodoTask_ReturnsNotFound_IfTodoTaskDoesNotExist()
    {
        var httpClient = _factory.CreateClient();

        var result = await httpClient.DeleteAsync($"/tasks/{Guid.NewGuid().ToString()}");
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private TodoTask GenerateValidTodoTask()
    {
        return new TodoTask
        {
            Id = String.Empty,
            Title = "Test Title",
            Details = "Test Details yayayaya",
            Author = "Michael Jordan",
            Started = DateTime.MinValue,
            Completed = DateTime.MaxValue,
            LastModified = DateTime.MaxValue
        };
    }

    private TodoTask GenerateInvalidTodoTask_MissingTitle()
    {
        return new TodoTask
        {
            Id = String.Empty,
            Title = String.Empty,
            Details = "Test Details yayayaya",
            Author = "Michael Jordan",
            Started = DateTime.MinValue,
            Completed = DateTime.MaxValue,
            LastModified = DateTime.MaxValue
        };
    }

    private TodoTask GenerateInvalidTodoTask_MissingDetails()
    {
        return new TodoTask
        {
            Id = String.Empty,
            Title = "Test Title",
            Details = String.Empty,
            Author = "Michael Jordan",
            Started = DateTime.MinValue,
            Completed = DateTime.MaxValue,
            LastModified = DateTime.MaxValue
        };
    }

    private TodoTask GenerateInvalidTodoTask_MissingAuthor()
    {
        return new TodoTask
        {
            Id = String.Empty,
            Title = "Test Title",
            Details = "Test Details yayayaya",
            Author = String.Empty,
            Started = DateTime.MinValue,
            Completed = DateTime.MaxValue,
            LastModified = DateTime.MaxValue
        };
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        var httpClient = _factory.CreateClient();
        foreach(var createdId in _createdIds)
        {
            await httpClient.DeleteAsync($"/tasks/{createdId}");
        }
    }
}