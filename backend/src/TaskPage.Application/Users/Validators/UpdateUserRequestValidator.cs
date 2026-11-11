using FluentValidation;
using TaskPage.Application.Users.DTOs;

namespace TaskPage.Application.Users.Validators;

/// <summary>
/// Validates the input data to update a user.
/// </summary>
public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    /// <summary>Defines the validation rules for user update.</summary>
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(200)
            .EmailAddress();
    }
}
