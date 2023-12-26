using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SolutionAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SuppressNullableOnEntitiesAnalyzer : DiagnosticSuppressor
    {
        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } =
            ImmutableArray.Create(Diagnostics.SuppressNullableOnEntities);

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

                if (elementNode.Parent?.Parent is not FileScopedNamespaceDeclarationSyntax { Name: QualifiedNameSyntax name })
                    continue;

                if (name.ToFullString()?.EndsWith(".Entities") == true)
                {
                    context.ReportSuppression(Suppression.Create(Diagnostics.SuppressNullableOnEntities, diagnostic));
                }
            }
        }
    }
}
