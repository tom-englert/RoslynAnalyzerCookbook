using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SolutionAnalyzer.Test
{
    using static CSharpAnalyzerVerifier<SuppressNullForgivingWarningAnalyzer>;

    // begin-snippet:  BasicSuppressionTestSetup
    [TestClass]
    public class BasicSuppressionTestSetup
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

        private static Task VerifyAsync(string source, params DiagnosticResult[] expected)
        {
            return new Test(source)
                .AddSources(IsExternalInit)
                .AddDiagnostics(expected)
                .RunAsync();
        }

        [TestMethod]
        public async Task NullForgivingWarningIsSuppressedForInitOnlyProperties()
        {
            const string source = """
                #nullable enable

                class Test 
                {
                    string InitOnly { get; init; } = default!;
                }
                """;

            await VerifyAsync(source);
        }
    }
    // end-snippet
}
