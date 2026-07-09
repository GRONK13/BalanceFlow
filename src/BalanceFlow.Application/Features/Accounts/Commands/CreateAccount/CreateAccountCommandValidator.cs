namespace BalanceFlow.Application.Features.Accounts.Commands.CreateAccount;

/// <summary>
/// Validates <see cref="CreateAccountCommand"/> properties before the handler runs.
/// If any rule fails, the <see cref="Behaviors.ValidationBehavior{TRequest,TResponse}"/>
/// throws a <see cref="Exceptions.ValidationException"/> and the handler never executes.
///
/// <para><strong>C# Concept — FluentValidation:</strong>
/// Instead of writing <c>if (string.IsNullOrEmpty(x.Name)) throw ...</c> in
/// the handler, you declare rules using a fluent API. This separates
/// "is the input well-formed?" (validator) from "what do we do with valid input?"
/// (handler). Think of it as schema validation (like Joi/Zod in JavaScript)
/// but executed server-side in the MediatR pipeline.</para>
/// </summary>
public sealed class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.AccountCode)
            .NotEmpty().WithMessage("Account code is required.")
            .MaximumLength(20).WithMessage("Account code must not exceed 20 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Account name is required.")
            .MaximumLength(150).WithMessage("Account name must not exceed 150 characters.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("A valid account type is required.");
    }
}
