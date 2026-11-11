using FluentValidation;
using TaskPage.Application.Users.DTOs;

namespace TaskPage.Application.Users.Validators;

/// <summary>
/// Validates the input data to create a user.
/// </summary>
public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    /// <summary>Defines the validation rules for user creation.</summary>
    public CreateUserRequestValidator()
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
