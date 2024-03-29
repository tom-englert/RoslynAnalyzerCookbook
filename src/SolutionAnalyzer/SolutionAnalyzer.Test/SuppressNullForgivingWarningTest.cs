﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Nullable.Extended.Analyzer;

namespace SolutionAnalyzer.Test
{
    // begin-snippet:  BasicSuppressionTestSetup
    using Test = CSharpDiagnosticSuppressorTest<NullForgivingDetectionAnalyzer, SuppressNullForgivingWarningAnalyzer, TestVerifier>;
    using static ReferenceAssemblies.Net;

    [TestClass]
    public class SuppressNullForgivingWarningTest
    {
        [TestMethod]
        public async Task CompilationDoesNotGenerateErrors()
        {
            const string source = """
                class Test 
                {
                    string InitOnly { get; init; } = default;
                }
                """;

            await new Test
            {
                TestCode = source,
                ReferenceAssemblies = Net60,
                SolutionTransforms = { WithProjectCompilationOptions(o => o.WithNullableContextOptions(NullableContextOptions.Disable)) }
            }
            .RunAsync();
        }
        // end-snippet

        // begin-snippet:  SuppressNullForgivingWarningTest
        private static readonly NullForgivingDetectionAnalyzer NullForgivingDetectionAnalyzer = new();
        private static readonly DiagnosticDescriptor Nx0002 = NullForgivingDetectionAnalyzer.SupportedDiagnostics.Single(item => item.Id == "NX0002");
        private static readonly DiagnosticDescriptor Nx0004 = NullForgivingDetectionAnalyzer.SupportedDiagnostics.Single(item => item.Id == "NX0004");

        [TestMethod]
        public async Task NullForgivingWarningIsSuppressedForInitOnlyProperties()
        {
            const string source = """
                class Test
                {
                    string? InitOnly { get; init; } = default{|#0:!|};
                    string Normal { get; set; } = default{|#1:!|};
                }
                """;

            await new Test
            {
                TestCode = source,
                ReferenceAssemblies = Net60.AddPackages(PackageReference.TomsToolbox_Essentials),
                ExpectedDiagnostics =
                {
                    Nx0004.AsResult().WithLocation(0).WithArguments("InitOnly").WithIsSuppressed(true),
                    Nx0002.AsResult().WithLocation(1).WithArguments("Normal").WithIsSuppressed(false)
                }
            }
            .RunAsync();
        }
        // end-snippet
    }
}
