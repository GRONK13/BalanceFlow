namespace BalanceFlow.Application.Features.JournalEntries.Commands.CreateJournalEntry;

/// <summary>
/// Validates <see cref="CreateJournalEntryCommand"/> input:
/// - Reference number is required and within length limits.
/// - Transaction date is provided.
/// - At least 2 lines are required (double-entry minimum).
/// - Each line has valid account ID and amount constraints.
///
/// <para><strong>C# Concept — <c>RuleForEach</c> + <c>ChildRules</c>:</strong>
/// <c>RuleForEach</c> iterates over a collection property and applies rules
/// to each element. <c>ChildRules</c> lets you define nested rules for the
/// child object's properties — similar to nested schema validation in
/// Joi/Zod (JavaScript) or Marshmallow (Python).</para>
/// </summary>
public sealed class CreateJournalEntryCommandValidator
    : AbstractValidator<CreateJournalEntryCommand>
{
    public CreateJournalEntryCommandValidator()
    {
        RuleFor(x => x.ReferenceNumber)
            .NotEmpty().WithMessage("Reference number is required.")
            .MaximumLength(50).WithMessage("Reference number must not exceed 50 characters.");

        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("Transaction date is required.");

        RuleFor(x => x.Lines)
            .NotNull().WithMessage("Journal entry lines are required.")
            .Must(lines => lines.Count >= 2)
            .WithMessage("A journal entry must have at least 2 lines.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.AccountId)
                .NotEmpty().WithMessage("Account ID is required for each line.");

            line.RuleFor(l => l.DebitAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Debit amount must not be negative.");

            line.RuleFor(l => l.CreditAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Credit amount must not be negative.");

            line.RuleFor(l => l)
                .Must(l => l.DebitAmount > 0 || l.CreditAmount > 0)
                .WithMessage("Each line must have either a debit or credit amount greater than zero.")
                .Must(l => !(l.DebitAmount > 0 && l.CreditAmount > 0))
                .WithMessage("A line cannot have both a debit and credit amount.");
        });
    }
}
