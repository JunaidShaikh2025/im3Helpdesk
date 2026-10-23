using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace iM3Helpdesk.API.Tests.Architecture;

public class ExceptionHandlingArchitectureTests
{
    [Fact]
    public void ControllersAndHubs_DoNotSwallowExceptionsSilently()
    {
        var baseDir = AppContext.BaseDirectory;
        // Walk up to find backend/iM3Helpdesk.API
        var dir = new DirectoryInfo(baseDir);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "backend", "iM3Helpdesk.API")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        var apiPath = Path.Combine(dir.FullName, "backend", "iM3Helpdesk.API");
        var controllersPath = Path.Combine(apiPath, "Controllers");
        var hubsPath = Path.Combine(apiPath, "Hubs");

        var sourceFiles = Directory.GetFiles(controllersPath, "*.cs", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(hubsPath, "*.cs", SearchOption.AllDirectories))
            .ToList();

        var emptyCatchPattern = new Regex(@"catch\s*(\([^\)]*\))?\s*\{\s*\}", RegexOptions.Multiline);
        var violations = 0;
        var failureDetails = "";

        foreach (var file in sourceFiles)
        {
            var content = File.ReadAllText(file);
            var matches = emptyCatchPattern.Matches(content);
            if (matches.Count > 0)
            {
                violations += matches.Count;
                failureDetails += $"{Path.GetFileName(file)} has {matches.Count} silent catch blocks.\n";
            }
        }

        Assert.True(violations == 0, $"Found {violations} silent catch block violations:\n{failureDetails}");
    }
}
