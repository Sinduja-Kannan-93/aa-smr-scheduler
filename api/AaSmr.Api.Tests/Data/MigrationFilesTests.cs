using FluentAssertions;

namespace AaSmr.Api.Tests.Data;

public class MigrationFilesTests
{
    private static string GetMigrationsDir()
    {
        var base_ = AppContext.BaseDirectory;
        return Path.GetFullPath(Path.Combine(base_, "../../../../AaSmr.Api/Data/Migrations"));
    }

    [Fact]
    public void MigrationsDirectory_ExistsAndContainsAtLeastOneCsFile()
    {
        var dir = GetMigrationsDir();
        Directory.Exists(dir).Should().BeTrue($"Expected migrations directory at {dir}");
        Directory.GetFiles(dir, "*.cs").Should().NotBeEmpty();
    }

    [Fact]
    public void MigrationsDirectory_ContainsInitialMigration()
    {
        var dir = GetMigrationsDir();
        var files = Directory.GetFiles(dir, "*Initial*.cs");
        files.Should().NotBeEmpty("an Initial migration file should exist");
    }
}
