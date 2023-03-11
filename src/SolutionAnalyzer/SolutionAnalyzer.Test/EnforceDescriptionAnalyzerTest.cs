using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SolutionAnalyzer.Test;

// begin-snippet:  BasicTestSetup

using static CSharpAnalyzerVerifier<EnforceDescriptionAnalyzer>;

[TestClass]
public class BasicTestSetup
{
    private static Task VerifyAsync(string source, params DiagnosticResult[] expected)
    {
        return new Test(source).AddDiagnostics(expected).RunAsync();
    }

    [TestMethod]
    public async Task CompilationDoesNotGenerateErrors()
    {
        const string source = """
            namespace MyApp;
            
            class TypeName
            {   
                int SomeProperty { get; set; }
            }
            """;

        await VerifyAsync(source);
    }
}

// end-snippet

[TestClass]
public class EnforceDescriptionAnalyzerTest
{
    // begin-snippet:  EnforceDescriptionAnalyzerTest_VerifyDeclaration
    private static Task VerifyAsync(string source, params DiagnosticResult[] expected)
    {
        return new Test(source).AddDiagnostics(expected).RunAsync();
    }

    private static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor) => new(descriptor);

    // end-snippet

    [TestMethod]
    public async Task ErrorWhenTextPropertyHasNoDescription()
    {
        const string source = """
            namespace MyApp
            {
                class TypeName
                {   
                    int {|#0:BadProperty|} { get; set; }

                    int {|#1:GoodProperty|} { get; set; }
                }
            }
            """;

        await VerifyAsync(source);
    }
}
