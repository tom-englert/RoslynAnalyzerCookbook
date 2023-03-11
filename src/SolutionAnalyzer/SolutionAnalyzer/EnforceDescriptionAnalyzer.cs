using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SolutionAnalyzer;

// begin-snippet:  EnforceDescriptionAnalyzer_Declaration

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EnforceDescriptionAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Diagnostics.TextPropertyHasNoDescription);

    // end-snippet

    // begin-snippet:  EnforceDescriptionAnalyzer_Implementation

    public override void Initialize(AnalysisContext context)
    {
    }


    // end-snippet
}
