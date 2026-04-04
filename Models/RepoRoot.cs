namespace Dostar.Cli;

internal sealed record RepoRoot(string Root, string SlnxPath)
{
    internal static RepoRoot Find()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir is not null)
        {
            var slnx = dir.GetFiles("*.slnx").FirstOrDefault();
            if (slnx is not null)
                return new(dir.FullName, slnx.FullName);
            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not find repo root (no .slnx file found).");
    }
}
