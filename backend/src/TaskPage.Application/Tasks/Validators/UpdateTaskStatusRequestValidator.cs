using FluentValidation;
using TaskPage.Application.Tasks.DTOs;

namespace TaskPage.Application.Tasks.Validators;

/// <summary>
/// Validates the input data to change the status of a task.
/// </summary>
public class UpdateTaskStatusRequestValidator : AbstractValidator<UpdateTaskStatusRequest>
{
    /// <summary>Defines the validation rules for the status change.</summary>
    public UpdateTaskStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum();
    }
}
