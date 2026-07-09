namespace BalanceFlow.Application.Features.Accounts.Commands.CreateAccount;

/// <summary>
/// Handles the <see cref="CreateAccountCommand"/> by:
/// 1. Checking that the account code is unique.
/// 2. Creating a new <see cref="Account"/> domain entity.
/// 3. Persisting it through the repository + unit of work.
/// 4. Returning the created account as a DTO.
///
/// <para><strong>C# Concept — Constructor Injection:</strong>
/// The constructor parameters (<c>IAccountRepository</c>, <c>IUnitOfWork</c>)
/// are automatically provided by the DI container at runtime. You never write
/// <c>new CreateAccountCommandHandler(...)</c> yourself — MediatR asks the
/// DI container to build it, and the container resolves all dependencies
/// recursively. This is the same concept as NestJS's <c>@Inject()</c>
/// or Spring's <c>@Autowired</c>.</para>
/// </summary>
public sealed class CreateAccountCommandHandler
    : IRequestHandler<CreateAccountCommand, Result<AccountDto>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAccountCommandHandler(IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AccountDto>> Handle(
        CreateAccountCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Business rule: account codes must be unique.
        var existing = await _accountRepository.GetByAccountCodeAsync(
            request.AccountCode, cancellationToken);

        if (existing is not null)
            return Result<AccountDto>.Failure(
                $"An account with code '{request.AccountCode}' already exists.");

        // 2. Create the domain entity (constructor validates invariants).
        var account = new Account(
            request.AccountCode,
            request.Name,
            request.Type,
            request.Description);

        // 3. Persist: add to repo, then commit via unit of work.
        await _accountRepository.AddAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. Map to DTO and return success.
        return Result<AccountDto>.Success(account.ToDto());
    }
}
