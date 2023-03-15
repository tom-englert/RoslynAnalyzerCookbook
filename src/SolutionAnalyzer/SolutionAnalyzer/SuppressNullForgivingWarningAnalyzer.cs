using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SolutionAnalyzer
{
    // begin-snippet:  SuppressNullForgivingAnalyzer_Declaration
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SuppressNullForgivingWarningAnalyzer : DiagnosticSuppressor
    {
        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } =
            ImmutableArray.Create(Diagnostics.SuppressNullForgivingWarning);
        // end-snippet

        // begin-snippet:  SuppressNullForgivingAnalyzer_Implementation
        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
        }
        // end-snippet
    }
}
