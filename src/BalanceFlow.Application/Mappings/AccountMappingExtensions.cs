namespace BalanceFlow.Application.Mappings;

/// <summary>
/// Extension methods that convert <see cref="Account"/> domain entities to
/// <see cref="AccountDto"/> data transfer objects.
///
/// <para><strong>Why extension methods instead of AutoMapper?</strong>
/// For a portfolio project, manual mapping demonstrates that you understand
/// the conversion logic. It's also compile-time safe — if you add a property
/// to the DTO, the compiler forces you to update the mapping. AutoMapper
/// would silently return <c>null</c> for the new property until you update
/// the profile.</para>
///
/// <para><strong>C# Concept — Extension Methods:</strong>
/// The <c>this</c> keyword before the first parameter "attaches" the method
/// to that type. After importing this namespace, you can call
/// <c>myAccount.ToDto()</c> as if <c>ToDto</c> were defined inside the
/// <c>Account</c> class. The compiler rewrites it to
/// <c>AccountMappingExtensions.ToDto(myAccount)</c> behind the scenes.</para>
/// </summary>
public static class AccountMappingExtensions
{
    /// <summary>
    /// Converts an <see cref="Account"/> domain entity to an <see cref="AccountDto"/>.
    /// </summary>
    public static AccountDto ToDto(this Account account) => new(
        Id: account.Id,
        AccountCode: account.AccountCode,
        Name: account.Name,
        Description: account.Description,
        Type: account.Type,
        TypeName: account.Type.ToString(),
        IsActive: account.IsActive,
        CreatedAt: account.CreatedAt,
        ModifiedAt: account.ModifiedAt
    );
}
