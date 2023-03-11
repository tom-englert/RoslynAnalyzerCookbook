# Roslyn Analyzer Cookbook

This repository is a demo project showing how create custom Roslyn analyzers. 

It focuses on how to add custom analyzers to a solution, which enforce custom rules and design patterns that apply only to a specific solution, without the need to create a package or extension.

However most of the topics also apply when generating an analyzer package or extension, and will be also useful in this scenarios.

> Most of the chapters correspond to a commit of the related code, so one can follow this tutorial step by step.

## Points of interest

- Improved test framework: Sanitized and simplified test verifiers, with up to date defaults, using extension methods to customize for individual tests using fluent notation.
- Shows how to reference NuGet packages in the test code, automated by MSBuild.
- Integration of the analyzers into the solution without the need to create a package or install a Visual Studio extension.

## Use case #1

Enforce that every property that has a `[Text]` attribute also has a `[Description]` attribute, by showing a warning,
so e.g. a basic user documentation can be generated automatically for dedicated properties using reflection.

> In real life the will be probably a more specific attribute than a simple `[Text]`, this is just used to make this sample more universal.

## Add the scaffold for the analyzer to the solution

In the first step the scaffold for the analyzers and the corresponding tests will be added to the solution.

> Using the "Analyzer with Code Fix" template adds too much unused stuff, with problematic defaults, so it's better to start from scratch with sanitized test verifiers.

### Add an empty project "SolutionAnalyzer":
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.5.0" />
  </ItemGroup>
  <Import Project="Packaging.targets" Condition="'$(IsPackable)'=='True'" />
</Project>
```
<sup><a href='/src/SolutionAnalyzer/SolutionAnalyzer/SolutionAnalyzer.csproj' title='Snippet source file'>snippet source</a></sup>

### Add an empty project "SolutionAnalyzer.Test"
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.5.0" />
    <PackageReference Include="MSTest.TestAdapter" Version="3.0.2" />
    <PackageReference Include="MSTest.TestFramework" Version="3.0.2" />
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp.Analyzer.Testing.MSTest" Version="1.1.1" />
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp.CodeFix.Testing.MSTest" Version="1.1.1" />
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp.CodeRefactoring.Testing.MSTest" Version="1.1.1" />
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp.Workspaces" Version="4.5.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\SolutionAnalyzer\SolutionAnalyzer.csproj" />
  </ItemGroup>
</Project>
```
<sup><a href='/src/SolutionAnalyzer/SolutionAnalyzer.Test/SolutionAnalyzer.Test.csproj' title='Snippet source file'>snippet source</a></sup>

### Add the sanitized test verifiers to the test project

<!-- snippet: CSharpAnalyzerVerifier -->
<a id='snippet-csharpanalyzerverifier'></a>
```cs
internal static class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public class Test : CSharpAnalyzerTest<TAnalyzer, Verifier>
    {
        public Test(string source)
        {
            TestCode = source;
            ReferenceAssemblies = Default.ReferenceAssemblies;
            TestBehaviors = Default.TestBehaviors;
        }

        protected override CompilationOptions CreateCompilationOptions() => Default.CompilationOptions;

        protected override ParseOptions CreateParseOptions() => Default.ParseOptions;
    }
}
```
<sup><a href='/src/SolutionAnalyzer/SolutionAnalyzer.Test/Verifiers.cs#L18-L38' title='Snippet source file'>snippet source</a> | <a href='#snippet-csharpanalyzerverifier' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
> see source for the full version including defaults and extension methods

## Add the analyzer and the corresponding unit test

### Define the diagnostic descriptor
> It's a good practice to keep the definition of all descriptors in one place, so you don't 
> loose track of the id's when having more than one analyzer in the project.
> Also it's easier to reference the descriptors in the tests.
<!-- snippet: Diagnostics -->
<a id='snippet-diagnostics'></a>
```cs
public static class Diagnostics
{
    private const string Category = "Custom";

    public static readonly DiagnosticDescriptor TextPropertyHasNoDescription = new("CUS001",
        "Property with Text attribute has no description",
        "Property {0} has a Text attribute but no Description attribute",
        Category,
        DiagnosticSeverity.Error, isEnabledByDefault: true);
}
```
<sup><a href='/src/SolutionAnalyzer/SolutionAnalyzer/Diagnostics.cs#L5-L16' title='Snippet source file'>snippet source</a> | <a href='#snippet-diagnostics' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

### Add an empty analyzer class to the analyzer project

<!-- snippet: EnforceDescriptionAnalyzer_Declaration -->
<a id='snippet-enforcedescriptionanalyzer_declaration'></a>
```cs
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EnforceDescriptionAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Diagnostics.TextPropertyHasNoDescription);
```
<sup><a href='/src/SolutionAnalyzer/SolutionAnalyzer/EnforceDescriptionAnalyzer.cs#L8-L15' title='Snippet source file'>snippet source</a> | <a href='#snippet-enforcedescriptionanalyzer_declaration' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

### Add a basic test with a minimal class as source code.

<!-- snippet: BasicTestSetup -->
<a id='snippet-basictestsetup'></a>
```cs
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
```
<sup><a href='/src/SolutionAnalyzer/SolutionAnalyzer.Test/EnforceDescriptionAnalyzerTest.cs#L7-L35' title='Snippet source file'>snippet source</a> | <a href='#snippet-basictestsetup' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
