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
        return new Test(source).AddDiagnostics(expected)
            .AddPackages(PackageReference.TomsToolbox_Essentials)
            .RunAsync();
    }

    private static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor) => new(descriptor);

    // end-snippet

    [TestMethod]
    public async Task ErrorWhenTextPropertyHasNoDescription()
    {
        // begin-snippet:  EnforceDescriptionAnalyzerTest_Source
        const string source = """
            using System.ComponentModel;
            using TomsToolbox.Essentials;
            
            namespace MyApp;
            
            class TypeName
            {   
                [Text("Key", "Value")]
                int {|#0:BadProperty|} { get; set; }

                [Description("Some description")]
                [Text("Key", "Value")]
                int {|#1:GoodProperty|} { get; set; }

                int AnotherProperty { get; set; }
            }
            """;
        // end-snippet

        await VerifyAsync(source);
    }
}
