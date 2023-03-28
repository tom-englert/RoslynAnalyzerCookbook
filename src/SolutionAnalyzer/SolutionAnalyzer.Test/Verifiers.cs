using System.Collections.Immutable;
using System.Reflection;

using JetBrains.Annotations;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

using Verifier = Microsoft.CodeAnalysis.Testing.Verifiers.MSTestVerifier;

namespace SolutionAnalyzer.Test;

// begin-snippet:  CSharpAnalyzerVerifier
internal static class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public class Test : CSharpAnalyzerTest<TAnalyzer, Verifier>
    {
        public Test(string? source = null)
        {
            // ! TestCode nullability annotation is wrong
            TestCode = source!;
            ReferenceAssemblies = Default.ReferenceAssemblies;
        }

        public List<PackageIdentity> AdditionalPackages { get; } = new();

        public List<DiagnosticAnalyzer> AdditionalAnalyzers { get; } = new();

        protected override CompilationOptions CreateCompilationOptions() => Default.CompilationOptions;

        protected override ParseOptions CreateParseOptions() => Default.ParseOptions;

        // end-snippet

        // begin-snippet:  CSharpAnalyzerVerifier_Suppressor

        private bool _reportSuppressedDiagnostics;

        protected override Task RunImplAsync(CancellationToken cancellationToken)
        {
            // Workaround https://github.com/dotnet/roslyn-sdk/issues/1078
            _reportSuppressedDiagnostics = GetDiagnosticAnalyzers().Any(analyzer => analyzer is DiagnosticSuppressor);
            if (_reportSuppressedDiagnostics)
            {
                TestBehaviors |= TestBehaviors.SkipSuppressionCheck;
            }

            ReferenceAssemblies = ReferenceAssemblies.AddPackages(AdditionalPackages.ToImmutableArray());

            return base.RunImplAsync(cancellationToken);
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
        {
            return base.GetDiagnosticAnalyzers().Concat(AdditionalAnalyzers);
        }

        protected override CompilationWithAnalyzers CreateCompilationWithAnalyzers(Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerOptions options, CancellationToken cancellationToken)
        {
            return compilation.WithAnalyzers(analyzers, new CompilationWithAnalyzersOptions(options, null, true, false, _reportSuppressedDiagnostics));
        }
        // end-snippet
    }
}

internal static class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    public class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, Verifier>
    {
        public Test(string source, string? fixedSource = null)
        {
            TestCode = source;
            // ! FixedCode just ignores null values
            FixedCode = fixedSource!;
            ReferenceAssemblies = Default.ReferenceAssemblies;
        }

        protected override CompilationOptions CreateCompilationOptions() => Default.CompilationOptions;

        protected override ParseOptions CreateParseOptions() => Default.ParseOptions;
    }
}

internal static class CSharpCodeRefactoringVerifier<TCodeRefactoring>
    where TCodeRefactoring : CodeRefactoringProvider, new()
{
    public class Test : CSharpCodeRefactoringTest<TCodeRefactoring, Verifier>
    {
        public Test(string source, string? fixedSource = null)
        {
            TestCode = source;
            // ! FixedCode just ignores null values
            FixedCode = fixedSource!;
            ReferenceAssemblies = Default.ReferenceAssemblies;
        }

        protected override CompilationOptions CreateCompilationOptions() => Default.CompilationOptions;

        protected override ParseOptions CreateParseOptions() => Default.ParseOptions;
    }
}

internal static class Default
{
    public static ParseOptions ParseOptions => new CSharpParseOptions(LanguageVersion.CSharp10, DocumentationMode.Diagnose);

    public static ReferenceAssemblies ReferenceAssemblies => ReferenceAssemblies.NetStandard.NetStandard21;

    public static CompilationOptions CompilationOptions
    {
        get
        {
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true);

            return compilationOptions.WithSpecificDiagnosticOptions(compilationOptions.SpecificDiagnosticOptions.SetItems(NullableWarnings));
        }
    }

    public static TestBehaviors TestBehaviors => TestBehaviors.SkipGeneratedCodeCheck | TestBehaviors.SkipSuppressionCheck;

    /// <summary>
    /// By default, the compiler reports diagnostics for nullable reference types at
    /// <see cref="DiagnosticSeverity.Warning"/>, and the analyzer test framework defaults to only validating
    /// diagnostics at <see cref="DiagnosticSeverity.Error"/>. This map contains all compiler diagnostic IDs
    /// related to nullability mapped to <see cref="ReportDiagnostic.Error"/>, which is then used to enable all
    /// of these warnings for default validation during analyzer and code fix tests.
    /// </summary>
    private static ImmutableDictionary<string, ReportDiagnostic> NullableWarnings { get; } = GetNullableWarningsFromCompiler();

    private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
    {
        string[] args = { "/warnaserror:nullable" };
        var commandLineArguments = CSharpCommandLineParser.Default.Parse(args, baseDirectory: Environment.CurrentDirectory, sdkDirectory: Environment.CurrentDirectory);
        var nullableWarnings = commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;
        return nullableWarnings;
    }
}

[PublicAPI]
internal static class CSharpVerifierExtensionMethods
{
    public static DiagnosticResult AsResult(this DiagnosticDescriptor descriptor) => new(descriptor);

    public static Func<Solution, ProjectId, Solution> AddReferences(params Assembly[] localReferences)
    {
        return (solution, projectId) =>
        {
            var localMetadataReferences = localReferences
                .Distinct()
                .Select(assembly => MetadataReference.CreateFromFile(assembly.Location));

            solution = solution.AddMetadataReferences(projectId, localMetadataReferences);

            return solution;
        };
    }

    public static Func<Solution, ProjectId, Solution> UseLanguageVersion(LanguageVersion languageVersion)
    {
        return (solution, projectId) => solution.WithProjectParseOptions(projectId, new CSharpParseOptions(languageVersion, DocumentationMode.Diagnose));
    }

    public static ReferenceAssemblies AddPackages(this ReferenceAssemblies? referenceAssemblies, params PackageIdentity[] packages)
    {
        return (referenceAssemblies ?? Default.ReferenceAssemblies).AddPackages(packages.ToImmutableArray());
    }
}
