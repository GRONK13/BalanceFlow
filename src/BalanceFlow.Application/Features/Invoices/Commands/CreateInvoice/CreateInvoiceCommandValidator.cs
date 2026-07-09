namespace BalanceFlow.Application.Features.Invoices.Commands.CreateInvoice;

/// <summary>
/// Validator enforcing formatting rules and boundary validations on CreateInvoiceCommand.
/// </summary>
public sealed class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.InvoiceNumber)
            .NotEmpty().WithMessage("Invoice number is required.")
            .MaximumLength(50).WithMessage("Invoice number must not exceed 50 characters.");

        RuleFor(x => x.VendorName)
            .NotEmpty().WithMessage("Vendor name is required.")
            .MaximumLength(150).WithMessage("Vendor name must not exceed 150 characters.");

        RuleFor(x => x.IssueDate)
            .NotEmpty().WithMessage("Issue date is required.");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Due date is required.")
            .GreaterThanOrEqualTo(x => x.IssueDate)
            .WithMessage("Due date cannot be earlier than the issue date.");

        RuleFor(x => x.TaxAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Tax amount must be zero or positive.");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0).WithMessage("Total amount must be greater than zero.");

        RuleFor(x => x.LineItems)
            .NotNull().WithMessage("Line items are required.")
            .Must(lines => lines.Count >= 1).WithMessage("Invoice must contain at least 1 line item.");

        RuleForEach(x => x.LineItems).ChildRules(line =>
        {
            line.RuleFor(l => l.AccountId)
                .NotEmpty().WithMessage("Account ID is required for each line item.");

            line.RuleFor(l => l.Description)
                .NotEmpty().WithMessage("Line item description is required.")
                .MaximumLength(250).WithMessage("Line item description must not exceed 250 characters.");

            line.RuleFor(l => l.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

            line.RuleFor(l => l.UnitPrice)
                .GreaterThan(0).WithMessage("Unit price must be greater than zero.");
        });
    }
}
