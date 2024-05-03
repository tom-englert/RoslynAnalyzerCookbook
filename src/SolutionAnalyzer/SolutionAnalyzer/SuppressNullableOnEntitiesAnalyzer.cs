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
                // diagnostic.Location is the constructor, if the class has one, while .AdditionalLocations[0] always points to the property.

                var location = diagnostic.AdditionalLocations[0];

                if (location is not
                    {
                        SourceTree: { } sourceTree,
                        SourceSpan: var sourceSpan
                    })
                    continue;

                var root = sourceTree.GetRoot(cancellationToken);

                var elementNode = root.FindNode(sourceSpan);

                // Just for demo, two times the same check:

                // #1 check by syntax tree

                if (elementNode is not PropertyDeclarationSyntax
                    {
                        Parent: ClassDeclarationSyntax
                        {
                            Parent: BaseNamespaceDeclarationSyntax
                            {
                                Name: QualifiedNameSyntax nameSyntax
                            }
                        }
                    })
                    continue;

                if (nameSyntax.ToString()?.EndsWith(".Entities") != true)
                    continue;

                // #2 same check in semantic model

                var declaredSymbol = context
                    .GetSemanticModel(sourceTree)
                    .GetDeclaredSymbol(elementNode);

                if (declaredSymbol is not IPropertySymbol { ContainingNamespace.Name: "Entities" })
                    continue;

                // Check successful, suppress diagnostic:

                context.ReportSuppression(Suppression.Create(SuppressionDescriptor, diagnostic));
            }
        }
    }
}
