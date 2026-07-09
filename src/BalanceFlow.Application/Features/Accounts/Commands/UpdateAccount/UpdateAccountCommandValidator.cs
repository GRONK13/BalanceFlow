namespace BalanceFlow.Application.Features.Accounts.Commands.UpdateAccount;

/// <summary>
/// Validates <see cref="UpdateAccountCommand"/> input before the handler executes.
/// </summary>
public sealed class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Account ID is required.");

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
