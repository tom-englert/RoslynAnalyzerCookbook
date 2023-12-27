using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SolutionAnalyzer
{
    // begin-snippet:  SuppressNullForgivingAnalyzer_Declaration
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SuppressNullForgivingWarningAnalyzer : DiagnosticSuppressor
    {
        private static readonly SuppressionDescriptor SuppressionDescriptor = Diagnostics.SuppressNullForgivingWarning;

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } =
            ImmutableArray.Create(SuppressionDescriptor);
        // end-snippet

        // begin-snippet:  SuppressNullForgivingAnalyzer_Implementation
        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            var cancellationToken = context.CancellationToken;

            foreach (var diagnostic in context.ReportedDiagnostics)
            {
                var location = diagnostic.Location;
                var sourceTree = location.SourceTree;
                if (sourceTree == null)
                    continue;

                var root = sourceTree.GetRoot(cancellationToken);

                var sourceSpan = location.SourceSpan;
                var elementNode = root.FindNode(sourceSpan);

                if (elementNode.Parent is not EqualsValueClauseSyntax { Parent: PropertyDeclarationSyntax propertyDeclaration })
                    continue;

                if (propertyDeclaration.AccessorList?.Accessors.Any(item => item.IsKind(SyntaxKind.InitAccessorDeclaration)) == true)
                {
                    context.ReportSuppression(Suppression.Create(SuppressionDescriptor, diagnostic));
                }
            }
        }
        // end-snippet
    }
}
