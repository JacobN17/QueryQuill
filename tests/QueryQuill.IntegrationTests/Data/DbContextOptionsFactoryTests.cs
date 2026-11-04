using QueryQuill.Infrastructure.Common;
using QueryQuill.Infrastructure.Data;

namespace QueryQuill.IntegrationTests.Data;

[Collection("EnvironmentVariablesDependent")]
public class DbContextOptionsFactoryTests
{
    [Fact]
    public void GetConnectionString_WithoutEnvVars_ShouldReturnDefaultLocalConnectionString()
    {
        // Arrange & Act
        var connectionString = DbContextOptionsFactory.GetConnectionString();

        // Assert
        Assert.NotNull(connectionString);
        Assert.Equal(ConfigurationConstants.Database.DefaultLocalConnectionString, connectionString);
    }

    [Fact]
    public void GetConnectionString_WithExplicitOverride_ShouldReturnExplicitValue()
    {
        // Arrange
        const string explicitConn = "Host=remotehost;Port=5432;Database=customdb;Username=test;Password=secret";

        // Act
        var result = DbContextOptionsFactory.GetConnectionString(explicitConn);

        // Assert
        Assert.Equal(explicitConn, result);
    }

    [Fact]
    public void GetConnectionString_WithDirectEnvVar_ShouldReturnEnvVarValue()
    {
        // Arrange
        const string customConn = "Host=envhost;Port=5432;Database=envdb;Username=envuser;Password=envpass";
        Environment.SetEnvironmentVariable(ConfigurationConstants.Database.ConnectionStringEnvVar, customConn);

        try
        {
            // Act
            var result = DbContextOptionsFactory.GetConnectionString();

            // Assert
            Assert.Equal(customConn, result);
        }
        finally
        {
            Environment.SetEnvironmentVariable(ConfigurationConstants.Database.ConnectionStringEnvVar, null);
        }
    }

    [Fact]
    public void GetConnectionString_WithComponentEnvVars_ShouldComposeConnectionString()
    {
        // Arrange
        Environment.SetEnvironmentVariable(ConfigurationConstants.Database.HostEnvVar, "dbserver.internal");
        Environment.SetEnvironmentVariable(ConfigurationConstants.Database.PortEnvVar, "5432");
        Environment.SetEnvironmentVariable(ConfigurationConstants.Database.DatabaseNameEnvVar, "ragproduction");
        Environment.SetEnvironmentVariable(ConfigurationConstants.Database.UserEnvVar, "appuser");
        Environment.SetEnvironmentVariable(ConfigurationConstants.Database.PasswordEnvVar, "strongpass");

        try
        {
            // Act
            var result = DbContextOptionsFactory.GetConnectionString();

            // Assert
            Assert.Equal("Host=dbserver.internal;Port=5432;Database=ragproduction;Username=appuser;Password=strongpass",
                result);
        }
        finally
        {
            Environment.SetEnvironmentVariable(ConfigurationConstants.Database.HostEnvVar, null);
            Environment.SetEnvironmentVariable(ConfigurationConstants.Database.PortEnvVar, null);
            Environment.SetEnvironmentVariable(ConfigurationConstants.Database.DatabaseNameEnvVar, null);
            Environment.SetEnvironmentVariable(ConfigurationConstants.Database.UserEnvVar, null);
            Environment.SetEnvironmentVariable(ConfigurationConstants.Database.PasswordEnvVar, null);
        }
    }

    [Fact]
    public void CreateOptions_ShouldReturnValidDbContextOptionsWithNpgsql()
    {
        // Act
        var options = DbContextOptionsFactory.CreateOptions();

        // Assert
        Assert.NotNull(options);
        Assert.Equal(typeof(ApplicationDbContext), options.ContextType);

        using var context = new ApplicationDbContext(options);
        Assert.NotNull(context.Model);
    }
}