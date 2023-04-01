using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

using Verifier = Microsoft.CodeAnalysis.Testing.Verifiers.MSTestVerifier;

namespace SolutionAnalyzer.Test;

#pragma warning disable NX0001 // Find general usages of the NullForgiving operator

// begin-snippet: CSharpAnalyzerTest
public class AnalyzerTest<TAnalyzer> : CSharpAnalyzerTest<TAnalyzer, Verifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public AnalyzerTest(string? source = null)
    {
        TestCode = source!;
        ReferenceAssemblies = ReferenceAssemblies.Net.Net60;
    }

    protected override CompilationOptions CreateCompilationOptions() => base.CreateCompilationOptions().WithCSharpDefaults();
}
// end-snippet

// begin-snippet: CSharpSuppressorTest
public class SuppressorTest<TAnalyzer, TSuppressor> : AnalyzerTest<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TSuppressor : DiagnosticAnalyzer, new()
{
    public SuppressorTest(string? source = null)
        : base(source)
    {
    }

    protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
    {
        return base.GetDiagnosticAnalyzers().Append(new TSuppressor());
    }

    protected override CompilationWithAnalyzers CreateCompilationWithAnalyzers(Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerOptions options, CancellationToken cancellationToken)
    {
        // Workaround https://github.com/dotnet/roslyn-sdk/issues/1078
        TestBehaviors |= TestBehaviors.SkipSuppressionCheck;
        return compilation.WithAnalyzers(analyzers, new CompilationWithAnalyzersOptions(options, null, true, false, true));
    }
}
// end-snippet

public class CodeFixTest<TAnalyzer, TCodeFix> : CSharpCodeFixTest<TAnalyzer, TCodeFix, Verifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    public CodeFixTest(string source, string? fixedSource = null)
    {
        TestCode = source;
        FixedCode = fixedSource!;
    }

    protected override CompilationOptions CreateCompilationOptions() => base.CreateCompilationOptions().WithCSharpDefaults();
}

public class RefactoringTest<TCodeRefactoring> : CSharpCodeRefactoringTest<TCodeRefactoring, Verifier>
    where TCodeRefactoring : CodeRefactoringProvider, new()
{
    public RefactoringTest(string source, string? fixedSource = null)
    {
        TestCode = source;
        FixedCode = fixedSource!;
    }

    protected override CompilationOptions CreateCompilationOptions() => base.CreateCompilationOptions().WithCSharpDefaults();
}
