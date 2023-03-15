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
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Property);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var property = (IPropertySymbol)context.Symbol;

        var attributes = property.GetAttributes();

        if (!attributes.Any(attr => attr.AttributeClass?.Name == "TextAttribute"))
            return;

        if (attributes.Any(attr => attr.AttributeClass?.Name == "DescriptionAttribute"))
            return;

        context.ReportDiagnostic(Diagnostic.Create(Diagnostics.TextPropertyHasNoDescription, property.Locations.First(), property.Name));
    }
    // end-snippet
}
