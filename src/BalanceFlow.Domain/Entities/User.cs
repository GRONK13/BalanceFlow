namespace BalanceFlow.Domain.Entities;

/// <summary>
/// Represents a system user with role claims and a cryptographically hashed password.
/// Used to authenticate ledger actions and enforce role-based access control.
/// </summary>
public sealed class User : BaseEntity
{
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private User()
    {
        // EF Core constructor
    }

    public User(string username, string passwordHash, string role)
    {
        Username = username;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        ModifiedAt = DateTime.UtcNow;
    }
}
