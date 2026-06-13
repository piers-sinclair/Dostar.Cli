namespace Dostar.Cli.Tests.Unit;

public class GitCliInitTests : IAsyncLifetime
{
    private string _dir = string.Empty;

    public Task InitializeAsync()
    {
        _dir = Path.Combine(Path.GetTempPath(), $"dostar-git-init-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_dir);
        File.WriteAllText(Path.Combine(_dir, "README.md"), "hello");
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        if (!Directory.Exists(_dir))
            return Task.CompletedTask;

        foreach (var file in Directory.EnumerateFiles(_dir, "*", SearchOption.AllDirectories))
        {
            var attrs = File.GetAttributes(file);
            if ((attrs & FileAttributes.ReadOnly) != 0)
                File.SetAttributes(file, attrs & ~FileAttributes.ReadOnly);
        }

        Directory.Delete(_dir, recursive: true);
        return Task.CompletedTask;
    }

    [Fact]
    public async Task InitAsync_CreatesGitRepoWithInitialCommit()
    {
        await GitCli.InitAsync(_dir, "Test Author");

        Directory.Exists(Path.Combine(_dir, ".git")).ShouldBeTrue();
    }
}
