using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BalanceFlow.Infrastructure.Data.Interceptors;
using BalanceFlow.Infrastructure.Repositories;
using BalanceFlow.Infrastructure.Services;

namespace BalanceFlow.Infrastructure;

/// <summary>
/// Registers all Infrastructure-layer services with the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Register the audit interceptor as a singleton.
        services.AddSingleton<AuditableEntityInterceptor>();

        // 2. Register DbContext configured for PostgreSQL/Supabase.
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<AuditableEntityInterceptor>();
            
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            options.UseNpgsql(connectionString)
                   .AddInterceptors(interceptor);
        });

        // 3. Register UoW and Repositories.
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IOcrService, MockOcrService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
