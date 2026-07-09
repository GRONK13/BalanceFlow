using BalanceFlow.Domain.Entities;
using BalanceFlow.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace BalanceFlow.Infrastructure.Data;

/// <summary>
/// Seeds default user credentials with PBKDF2 hashed passwords if the Users registry is empty.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Users.AnyAsync())
        {
            return; // Already seeded
        }

        var adminUser = new User(
            "admin",
            PasswordHasher.HashPassword("AdminPass123!"),
            "Admin");

        var accountantUser = new User(
            "accountant",
            PasswordHasher.HashPassword("AccountantPass123!"),
            "Accountant");

        var auditorUser = new User(
            "auditor",
            PasswordHasher.HashPassword("AuditorPass123!"),
            "Auditor");

        await context.Users.AddRangeAsync(adminUser, accountantUser, auditorUser);
        await context.SaveChangesAsync();
    }
}
