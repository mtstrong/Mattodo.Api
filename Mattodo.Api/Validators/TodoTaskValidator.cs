using FluentValidation;
using Mattodo.Api.Models;

namespace Mattodo.Api.Validators;

public class TodoTaskValidator : AbstractValidator<TodoTask>
{
    public TodoTaskValidator()
    {
        RuleFor(task => task.Title).NotEmpty();
        RuleFor(task => task.Details).NotEmpty();
        RuleFor(task => task.Author).NotEmpty();
    }
}