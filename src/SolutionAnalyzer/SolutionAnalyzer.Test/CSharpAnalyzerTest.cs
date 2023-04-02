using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

using Verifier = Microsoft.CodeAnalysis.Testing.Verifiers.MSTestVerifier;

namespace SolutionAnalyzer.Test;

// begin-snippet: CSharpAnalyzerTest
public class AnalyzerTest<TAnalyzer> : CSharpAnalyzerTest<TAnalyzer, Verifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public AnalyzerTest()
    {
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
