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
        public Test(string source)
        {
            TestCode = source;
            ReferenceAssemblies = Default.ReferenceAssemblies;
            TestBehaviors = Default.TestBehaviors;
        }

        protected override CompilationOptions CreateCompilationOptions() => Default.CompilationOptions;

        protected override ParseOptions CreateParseOptions() => Default.ParseOptions;
    }
}

// end-snippet

internal static class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    public class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, Verifier>
    {
        public Test(string source, string? fixedSource = null)
        {
            TestCode = source;
            FixedCode = fixedSource!;
            ReferenceAssemblies = Default.ReferenceAssemblies;
            TestBehaviors = Default.TestBehaviors;
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
            FixedCode = fixedSource!;
            ReferenceAssemblies = Default.ReferenceAssemblies;
            TestBehaviors = Default.TestBehaviors;
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
    public static TTest AddPackages<TTest>(this TTest test, params PackageIdentity[] packages)
        where TTest : AnalyzerTest<Verifier>
    {
        test.ReferenceAssemblies = test.ReferenceAssemblies.WithPackages(packages.ToImmutableArray());
        return test;
    }

    public static TTest AddDiagnostics<TTest>(this TTest test, params DiagnosticResult[] expected)
        where TTest : AnalyzerTest<Verifier>
    {
        test.ExpectedDiagnostics.AddRange(expected);
        return test;
    }

    public static TTest AddSolutionTransform<TTest>(this TTest test, Func<Solution, Project, Solution> transform)
        where TTest : AnalyzerTest<Verifier>
    {
        test.SolutionTransforms.Add((solution, projectId) =>
        {
            var project = solution.GetProject(projectId);
            return project == null ? solution : transform(solution, project);
        });

        return test;
    }

    public static TTest AddSources<TTest>(this TTest test, params string[] sources)
        where TTest : AnalyzerTest<Verifier>
    {
        foreach (var source in sources)
        {
            test.TestState.Sources.Add(source);
        }

        return test;
    }

    public static TTest AddReferences<TTest>(this TTest test, params Assembly[] localReferences)
        where TTest : AnalyzerTest<Verifier>
    {
        test.SolutionTransforms.Add((solution, projectId) =>
        {
            var localMetadataReferences = localReferences
                .Distinct()
                .Select(assembly => MetadataReference.CreateFromFile(assembly.Location));

            solution = solution.AddMetadataReferences(projectId, localMetadataReferences);

            return solution;
        });

        return test;
    }

    public static TTest WithReferenceAssemblies<TTest>(this TTest test, ReferenceAssemblies referenceAssemblies)
        where TTest : AnalyzerTest<Verifier>
    {
        if (test.ReferenceAssemblies.Packages.Any())
            throw new InvalidOperationException("You must call add packages after specifying reference assemblies");

        test.ReferenceAssemblies = referenceAssemblies;
        return test;
    }

    public static TTest WithLangVersion<TTest>(this TTest test, LanguageVersion languageVersion)
        where TTest : AnalyzerTest<Verifier>
    {
        test.SolutionTransforms.Add((solution, projectId) => solution.WithProjectParseOptions(projectId, new CSharpParseOptions(languageVersion, DocumentationMode.Diagnose)));
        return test;
    }

    public static TTest WithProjectCompilationOptions<TTest>(this TTest test, Func<CompilationOptions, CompilationOptions> callback)
        where TTest : AnalyzerTest<Verifier>
    {
        test.AddSolutionTransform((solution, project) => solution.WithProjectCompilationOptions(project.Id, callback(project.CompilationOptions ?? Default.CompilationOptions)));

        return test;
    }
}
