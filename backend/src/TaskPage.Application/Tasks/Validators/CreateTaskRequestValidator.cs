using FluentValidation;
using TaskPage.Application.Common;
using TaskPage.Application.Tasks.DTOs;

namespace TaskPage.Application.Tasks.Validators;

/// <summary>
/// Validates the input data to create a task.
/// </summary>
public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    /// <summary>Defines the validation rules for task creation.</summary>
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.UserId)
            .GreaterThan(0);

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue);

        RuleFor(x => x.AdditionalData)
            .Must(JsonGuard.IsValid)
            .When(x => !string.IsNullOrWhiteSpace(x.AdditionalData));
    }
}
