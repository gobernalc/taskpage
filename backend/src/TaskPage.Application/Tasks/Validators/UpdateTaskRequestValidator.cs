using FluentValidation;
using TaskPage.Application.Common;
using TaskPage.Application.Tasks.DTOs;

namespace TaskPage.Application.Tasks.Validators;

/// <summary>
/// Validates the input data to update a task.
/// </summary>
public class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    /// <summary>Defines the validation rules for task update.</summary>
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.UserId)
            .GreaterThan(0);

        RuleFor(x => x.AdditionalData)
            .Must(JsonGuard.IsValid)
            .When(x => !string.IsNullOrWhiteSpace(x.AdditionalData));
    }
}
