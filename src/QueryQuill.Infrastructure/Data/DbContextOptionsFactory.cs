using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using QueryQuill.Infrastructure.Common;

namespace QueryQuill.Infrastructure.Data;

/// <summary>
/// Factory utility for resolving PostgreSQL connection strings and constructing configured DbContextOptions.
/// Evaluates environment variables, explicit overrides, and deterministic fallback defaults.
/// </summary>
public static class DbContextOptionsFactory
{
    /// <summary>
    /// Resolves the optimal PostgreSQL connection string based on prioritized sources:
    /// 1. Explicit connection string (if provided)
    /// 2. Direct environment variable (QUERYQUILL_DB_CONNECTION)
    /// 3. Composed environment variables (QUERYQUILL_DB_HOST, QUERYQUILL_DB_PORT, QUERYQUILL_DB_NAME, QUERYQUILL_DB_USER, QUERYQUILL_DB_PASSWORD)
    /// 4. Default local development connection string.
    /// </summary>
    /// <param name="explicitConnectionString">Optional explicit connection string override.</param>
    /// <returns>A fully qualified PostgreSQL connection string.</returns>
    public static string GetConnectionString(string? explicitConnectionString = null)
    {
        if (!string.IsNullOrWhiteSpace(explicitConnectionString))
        {
            return explicitConnectionString;
        }

        // 1. Full connection string environment variable
        var envConnection = Environment.GetEnvironmentVariable(ConfigurationConstants.Database.ConnectionStringEnvVar);
        if (!string.IsNullOrWhiteSpace(envConnection))
        {
            return envConnection;
        }

        // 2. Composable environment variable components (e.g. for Kubernetes, ECS, or cloud secret injection)
        var host = Environment.GetEnvironmentVariable(ConfigurationConstants.Database.HostEnvVar);
        var port = Environment.GetEnvironmentVariable(ConfigurationConstants.Database.PortEnvVar) ?? "5432";
        var database = Environment.GetEnvironmentVariable(ConfigurationConstants.Database.DatabaseNameEnvVar) ??
                       "queryquilldb";
        var user = Environment.GetEnvironmentVariable(ConfigurationConstants.Database.UserEnvVar) ?? "user";
        var password = Environment.GetEnvironmentVariable(ConfigurationConstants.Database.PasswordEnvVar) ?? "password";

        return !string.IsNullOrWhiteSpace(host)
            ? $"Host={host};Port={port};Database={database};Username={user};Password={password}"
            : ConfigurationConstants.Database.DefaultLocalConnectionString;
    }

    /// <summary>
    /// Builds configured DbContextOptions with Npgsql and pgvector extension mapping.
    /// </summary>
    /// <param name="connectionString">Optional connection string override.</param>
    /// <returns>Configured DbContextOptions for ApplicationDbContext.</returns>
    public static DbContextOptions<ApplicationDbContext> CreateOptions(string? connectionString = null)
    {
        var resolvedString = GetConnectionString(connectionString);

        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(resolvedString, pgOptions => { pgOptions.UseVector(); })
            .Options;
    }
}