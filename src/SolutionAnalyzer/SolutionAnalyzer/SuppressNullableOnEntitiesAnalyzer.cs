using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SolutionAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SuppressNullableOnEntitiesAnalyzer : DiagnosticSuppressor
    {
        private static readonly SuppressionDescriptor SuppressionDescriptor = Diagnostics.SuppressNullableOnEntities;

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } = ImmutableArray.Create(SuppressionDescriptor);

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

                if (elementNode is not PropertyDeclarationSyntax
                    {
                        Parent: ClassDeclarationSyntax
                        {
                            Parent: FileScopedNamespaceDeclarationSyntax
                            {
                                Name: QualifiedNameSyntax name
                            }
                        }
                    })
                    continue;

                if (name.ToString()?.EndsWith(".Entities") == true)
                {
                    context.ReportSuppression(Suppression.Create(SuppressionDescriptor, diagnostic));
                }
            }
        }
    }
}
