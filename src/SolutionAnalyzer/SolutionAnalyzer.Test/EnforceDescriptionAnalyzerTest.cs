﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SolutionAnalyzer.Test;

// begin-snippet:  BasicTestSetup

using Test = CSharpAnalyzerTest<EnforceDescriptionAnalyzer, TestVerifier>;
using static ReferenceAssemblies.Net;

[TestClass]
public class EnforceDescriptionAnalyzerTest
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

        await new Test { TestCode = source }.RunAsync();
    }
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
                string? {|#1:GoodProperty|} { get; set; }

                int AnotherProperty { get; set; }
            }
            """;
        // end-snippet

        // begin-snippet:  EnforceDescriptionAnalyzerTest_Verification
        await new Test
        {
            TestCode = source,
            ReferenceAssemblies = Net60.AddPackages(PackageReference.TomsToolbox_Essentials),
            ExpectedDiagnostics =
            {
                Diagnostics.TextPropertyHasNoDescription.AsResult().WithArguments("BadProperty").WithLocation(0)
            },
        }.RunAsync();
        // end-snippet
    }

    [TestMethod]
    public async Task DemonstrateFeatureUsage()
    {
        const string source = """
            using System.ComponentModel;
            using TomsToolbox.Essentials;
            
            namespace MyApp 
            {
                class TypeName
                {   
                    [Text("Key", "Value")]
                    int {|#0:BadProperty|} { get; set; }

                    [Description("Some description")]
                    [Text("Key", "Value")]
                    string {|#1:GoodProperty|} { get; set; }

                    int AnotherProperty { get; set; }
                }
            }
            """;

        // Using object initializer notation
        await new Test
        {
            TestCode = source,
            ReferenceAssemblies = Net60.AddPackages(PackageReference.TomsToolbox_Essentials),
            SolutionTransforms =
            {
                WithLanguageVersion(LanguageVersion.CSharp7_2),
                WithProjectCompilationOptions(c => c.WithNullableContextOptions(NullableContextOptions.Disable)),
                AddAssemblyReferences()
            },
            ExpectedDiagnostics =
            {
                Diagnostics.TextPropertyHasNoDescription.AsResult().WithArguments("BadProperty").WithLocation(0)
            },
        }.RunAsync();

        // Using fluent notation
        await new Test()
            .AddSources(source)
            .WithReferenceAssemblies(Net60)
            .WithLanguageVersion(LanguageVersion.CSharp7_2)
            .WithProjectCompilationOptions(c => c.WithNullableContextOptions(NullableContextOptions.Disable))
            .AddPackages(PackageReference.TomsToolbox_Essentials)
            .AddReferences()
            .AddExpectedDiagnostics(Diagnostics.TextPropertyHasNoDescription.AsResult().WithArguments("BadProperty").WithLocation(0))
            .RunAsync();
    }
}
