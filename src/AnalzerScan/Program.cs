using System.Reflection;

using Microsoft.CodeAnalysis.Diagnostics;

var root = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.nuget\packages");

var packages = Directory.EnumerateDirectories(root);

foreach (var package in packages)
{
    var version = Directory.EnumerateDirectories(package)
        .Select(path => Version.TryParse(Path.GetFileName(path), out var v) ? new { Path = path, Version = v } : null)
        .ExceptNullItems()
        .MaxBy(item => item.Version);

    if (version is null)
        continue;

    var analyzerDir = Path.Combine(version.Path, "analyzers", "dotnet", "cs");

    if (!Directory.Exists(analyzerDir))
        continue;

    var analyzerAssemblyPaths = Directory.EnumerateFiles(analyzerDir, "*.dll");

    foreach (var analyzerAssemblyPath in analyzerAssemblyPaths)
    {
        var assembly = Assembly.LoadFrom(analyzerAssemblyPath);

        Console.WriteLine(analyzerAssemblyPath);

        var types = assembly.GetTypes();

        var analyzerTypes = types.Where(type => typeof(DiagnosticAnalyzer).IsAssignableFrom(type));

        foreach (var analyzerType in analyzerTypes.Where(type => !type.IsAbstract))
        {
            try
            {
                var analyzer = (DiagnosticAnalyzer?)Activator.CreateInstance(analyzerType);

                if (analyzer is null)
                    continue;

                foreach (var diagnostic in analyzer.SupportedDiagnostics)
                {
                    Console.WriteLine($"\t{diagnostic.Id}\t{analyzer.GetType()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ignore: \t{analyzerType.FullName}\t{ex.Message}");
            }
        }
    }
}
