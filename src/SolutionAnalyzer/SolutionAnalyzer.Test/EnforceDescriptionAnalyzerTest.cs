using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SolutionAnalyzer.Test;

// begin-snippet:  BasicTestSetup
using static CSharpAnalyzerVerifier<EnforceDescriptionAnalyzer>;

[TestClass]
public class BasicTestSetup
{
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

        await new Test(source).RunAsync();
    }
}
// end-snippet

[TestClass]
public class EnforceDescriptionAnalyzerTest
{
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

        // begin-snippet:  EnforceDescriptionAnalyzerTest_Verification
        await new Test(source)
        {
            AdditionalPackages = { PackageReference.TomsToolbox_Essentials },
            ExpectedDiagnostics = { Diagnostics.TextPropertyHasNoDescription.AsResult().WithArguments("BadProperty").WithLocation(0) },
        }.RunAsync();
        // end-snippet
    }
}
