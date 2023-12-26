using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SolutionAnalyzer.Test
{
    using static ReferenceAssemblies.Net;

    [TestClass]
    public class SuppressNullableOnEntitiesTest
    {
        class Test : CSharpAnalyzerTest<SuppressNullableOnEntitiesAnalyzer, TestVerifier>
        {
            public Test()
            {
                this.WithProjectCompilationOptions(ExtensionMethods.WithCSharpDefaults);
            }

            protected override CompilationWithAnalyzers CreateCompilationWithAnalyzers(Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerOptions options, CancellationToken cancellationToken)
            {
                return compilation.WithAnalyzers(analyzers, new CompilationWithAnalyzersOptions(options, null, true, false, true));
            }
        }

        [TestMethod]
        public async Task ErrorIsSuppressedInEntitiesScope()
        {
            const string source = """
                using System;
                namespace ClassLibrary1.Entities;

                public class User
                {
                    public Guid Id { get; init; }

                    public string {|#0:Name|} { get; set; }
                }
                """;

            var test = new Test
            {
                TestCode = source,
                ReferenceAssemblies = Net60,
                ExpectedDiagnostics =
                {
                    DiagnosticResult.CompilerError("CS8618").WithLocation(0).WithLocation(0).WithArguments("property", "Name").WithIsSuppressed(true)
                }
            };

            await test.RunAsync();
        }

        [TestMethod]
        public async Task ErrorIsNotSuppressedInNonEntitiesScope()
        {
            const string source = """
                using System;
                namespace ClassLibrary1.NoEntities;

                public class User
                {
                    public Guid Id { get; init; }

                    public string {|#0:Name|} { get; set; }
                }
                """;

            var test = new Test
            {
                TestCode = source,
                ReferenceAssemblies = Net60,
                ExpectedDiagnostics =
                {
                    DiagnosticResult.CompilerError("CS8618").WithLocation(0).WithLocation(0).WithArguments("property", "Name").WithIsSuppressed(false)
                }
            };

            await test.RunAsync();
        }
    }
}
