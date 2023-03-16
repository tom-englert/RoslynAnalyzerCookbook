using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Nullable.Extended.Analyzer;

namespace SolutionAnalyzer.Test
{
    using static CSharpAnalyzerVerifier<SuppressNullForgivingWarningAnalyzer>;

    // begin-snippet:  BasicSuppressionTestSetup
    [TestClass]
    public class SuppressNullForgivingWarningTest
    {
        // Required for init-only support.
        private const string IsExternalInit = """
            namespace System.Runtime.CompilerServices 
            {
                internal abstract class IsExternalInit 
                {
                }
            }
            """;

        [TestMethod]
        public async Task BasicTestSetup()
        {
            const string source = """
                #nullable enable

                class Test 
                {
                    string InitOnly { get; init; } = default!;
                }
                """;

            await new Test(source)
                .AddSources(IsExternalInit)
                .RunAsync();
        }
        // end-snippet

        // begin-snippet:  SuppressNullForgivingWarningTest
        private static readonly NullForgivingDetectionAnalyzer NullForgivingDetectionAnalyzer = new();
        private static readonly DiagnosticDescriptor Nx0002 = NullForgivingDetectionAnalyzer.SupportedDiagnostics.Single(item => item.Id == "NX0002");

        private static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor) => new(descriptor);

        [TestMethod]
        public async Task NullForgivingWarningIsSuppressedForInitOnlyProperties()
        {
            const string source = """
                #nullable enable

                class Test
                {
                    string InitOnly { get; init; } = default{|#0:!|};
                    string Normal { get; set; } = default{|#1:!|};
                }
                """;

            var expected = new[]
            {
                Diagnostic(Nx0002).WithLocation(0).WithArguments("InitOnly").WithIsSuppressed(true),
                Diagnostic(Nx0002).WithLocation(1).WithArguments("Normal").WithIsSuppressed(false)
            };

            await new Test(source)
                .AddSources(IsExternalInit)
                .AddDiagnostics(expected)
                .AddAnalyzer(NullForgivingDetectionAnalyzer)
                .RunAsync();
        }
        // end-snippet
    }
}
