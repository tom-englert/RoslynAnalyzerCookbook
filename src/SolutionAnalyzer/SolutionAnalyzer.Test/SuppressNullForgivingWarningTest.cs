using Microsoft.CodeAnalysis;
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

            await new Test
            {
                TestState = { Sources = { source, IsExternalInit } }
            }
            .RunAsync();
        }
        // end-snippet

        // begin-snippet:  SuppressNullForgivingWarningTest
        private static readonly NullForgivingDetectionAnalyzer NullForgivingDetectionAnalyzer = new();
        private static readonly DiagnosticDescriptor Nx0002 = NullForgivingDetectionAnalyzer.SupportedDiagnostics.Single(item => item.Id == "NX0002");

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

            await new Test
            {
                TestState = { Sources = { source, IsExternalInit } },
                AdditionalAnalyzers = { NullForgivingDetectionAnalyzer },
                ExpectedDiagnostics =
                {
                    Nx0002.AsResult().WithLocation(0).WithArguments("InitOnly").WithIsSuppressed(true),
                    Nx0002.AsResult().WithLocation(1).WithArguments("Normal").WithIsSuppressed(false)
                }
            }
            .RunAsync();
        }
        // end-snippet
    }
}
