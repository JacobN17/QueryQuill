using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QueryQuill.Domain.Interfaces;
using QueryQuill.Infrastructure.Data;

namespace QueryQuill.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Resolve connection string via appsettings/environment/factory fallback
        var connectionString = configuration.GetConnectionString("Database")
                               ?? DbContextOptionsFactory.GetConnectionString();

        // 2. Register ApplicationDbContext & IApplicationDbContext
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, pg =>
            {
                pg.UseVector();
                pg.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            });
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IEvaluationRepository, EvaluationRepository>();
        services.AddScoped<IVectorSearchRepository, VectorSearchRepository>();

        return services;
    }
}