namespace BalanceFlow.Application.Features.Invoices.Commands.ApproveInvoice;

/// <summary>
/// Validator for ApproveInvoiceCommand inputs.
/// </summary>
public sealed class ApproveInvoiceCommandValidator : AbstractValidator<ApproveInvoiceCommand>
{
    public ApproveInvoiceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Invoice ID is required.");

        RuleFor(x => x.AccountsPayableAccountId)
            .NotEmpty().WithMessage("Accounts Payable Account ID is required.");
    }
}
