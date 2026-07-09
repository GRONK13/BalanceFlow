using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using BalanceFlow.Application.Behaviors;

namespace BalanceFlow.Application;

/// <summary>
/// Registers all Application-layer services with the dependency injection container.
/// The API layer calls <c>builder.Services.AddApplicationServices()</c> — one line.
///
/// <para><strong>C# Concept — Extension Methods for DI Registration:</strong>
/// This pattern keeps DI registration organized by layer. Each layer (Application,
/// Infrastructure, API) has its own <c>AddXxxServices()</c> extension method.
/// The API's <c>Program.cs</c> just chains them:
/// <code>
/// builder.Services
///     .AddApplicationServices()
///     .AddInfrastructureServices(configuration);
/// </code>
/// This is the standard pattern used across all large ASP.NET Core codebases.</para>
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register MediatR: auto-discovers all IRequestHandler<,> implementations
        // in this assembly and registers them with the DI container.
        // Also registers the ValidationBehavior as an open generic pipeline behavior.
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // Register FluentValidation: auto-discovers all AbstractValidator<T>
        // implementations in this assembly and registers them as IValidator<T>.
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
