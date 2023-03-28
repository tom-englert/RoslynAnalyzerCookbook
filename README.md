# Roslyn Analyzer Cookbook

This repository is a demo project showing how create custom Roslyn analyzers. 

It focuses on how to add custom analyzers to a solution, which enforce custom rules and design patterns that apply only to a specific solution, without the need to create a package or extension.

However most of the topics also apply when generating an analyzer package or extension, and will be also useful in this scenarios.

> Most of the chapters correspond to a commit of the related code, so one can follow this tutorial step by step.

## Points of interest

- Improved test framework: Sanitized and simplified test verifiers, with up to date defaults, using extension methods to customize for individual tests using fluent notation.
- Shows how to reference NuGet packages in the test code, automated by MSBuild.
- Integration of the analyzers into the solution without the need to create a package or install a Visual Studio extension.

## Use cases
- [Diagnostic analyzer to conditionally enforce coding rules](#use-case-1)
- [Supression analyzers to suppress warnings depending on the context](#use-case-2)


## Use case #1

Enforce that every property that has a `[Text]` attribute also has a `[Description]` attribute, by showing a warning,
so e.g. a basic user documentation can be generated automatically for dedicated properties using reflection.

> In real life the will be probably a more specific attribute than a simple `[Text]`, this is just used to make this sample more universal.

### Add the scaffold for the analyzer to the solution

In the first step the scaffold for the analyzers and the corresponding tests will be added to the solution.

> Using the "Analyzer with Code Fix" template adds too much unused stuff, with problematic defaults, so it's better to start from scratch with sanitized test verifiers.

#### Add an empty project "SolutionAnalyzer":
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

#### Add an empty project "SolutionAnalyzer.Test"
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

#### Add the sanitized test verifiers to the test project

<!-- snippet: CSharpAnalyzerVerifier -->
<a id='snippet-csharpanalyzerverifier'></a>
```cs
internal static class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public class Test : CSharpAnalyzerTest<TAnalyzer, Verifier>
    {
        public Test(string? source = null)
        {
            // ! TestCode nullability annotation is wrong
            TestCode = source!;
            ReferenceAssemblies = Default.ReferenceAssemblies;
        }

        public List<PackageIdentity> AdditionalPackages { get; } = new();

        public List<DiagnosticAnalyzer> AdditionalAnalyzers { get; } = new();

        protected override CompilationOptions CreateCompilationOptions() => Default.CompilationOptions;

        protected override ParseOptions CreateParseOptions() => Default.ParseOptions;
```
<sup><a href='/src/SolutionAnalyzer/SolutionAnalyzer.Test/Verifiers.cs#L18-L39' title='Snippet source file'>snippet source</a> | <a href='#snippet-csharpanalyzerverifier' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
> see source for the full version including defaults and extension methods

### Add the analyzer and the corresponding unit test

#### Define the diagnostic descriptor
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
```
<sup><a href='/src/SolutionAnalyzer/SolutionAnalyzer/Diagnostics.cs#L5-L15' title='Snippet source file'>snippet source</a> | <a href='#snippet-diagnostics' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

#### Add an empty analyzer class to the analyzer project

<!-- snippet: EnforceDescriptionAnalyzer_Declaration -->
<a id='snippet-enforcedescriptionanalyzer_declaration'></a>
```cs
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EnforceDescriptionAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Diagnostics.TextPropertyHasNoDescription);
```
<sup><a href='/src/SolutionAnalyzer/SolutionAnalyzer/EnforceDescriptionAnalyzer.cs#L8-L13' title='Snippet source file'>snippet source</a> | <a href='#snippet-enforcedescriptionanalyzer_declaration' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

#### Add a basic test with a minimal class as source code.

<!-- snippet: BasicTestSetup -->
<a id='snippet-basictestsetup'></a>
```cs
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
```
<sup><a href='/src/SolutionAnalyzer/SolutionAnalyzer.Test/EnforceDescriptionAnalyzerTest.cs#L5-L26' title='Snippet source file'>snippet source</a> | <a href='#snippet-basictestsetup' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

### Update the unit test to reflect the use case

Now add the `Text` attribute to the properties of the test source:
<!-- snippet: EnforceDescriptionAnalyzerTest_Source -->
<a id='snippet-enforcedescriptionanalyzertest_source'></a>
```cs
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
```
<sup><a href='/src/SolutionAnalyzer/SolutionAnalyzer.Test/EnforceDescriptionAnalyzerTest.cs#L34-L53' title='Snippet source file'>snippet source</a> | <a href='#snippet-enforcedescriptionanalyzertest_source' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The problem here is that the `Text` property is defined in a NuGet package, and the test now fails, reporting compiler errors for the test source.

To fix this a reference to this missing package must be added in the test compilation. 
Of course the reference should point to the same package with the same version as referenced by the project; 
now the `PackageIdentity ` type needed to specify a package only accepts a string for package name and version, 
and specifying these hard coded in the source code is duplication and not a good practice.

A solution is to auto-generate a code snippet with all referenced package of the project, so it will be synchronized with the project automatically. 

It will be done by adding this build target to the test project:
<!-- snippet: GeneratePackageReferences -->
<a id='snippet-generatepackagereferences'></a>
```csproj
<Target Name="_GeneratePackageReferences" BeforeTargets="Build">
  <ItemGroup>
    <_GPRLine Include="// ReSharper disable All" />
    <_GPRLine Include="using Microsoft.CodeAnalysis.Testing%3B%0D%0A" />
    <_GPRLine Include="[System.CodeDom.Compiler.GeneratedCode(&quot;MSBuild&quot;, null)]" />
    <_GPRLine Include="internal static class PackageReference" />
    <_GPRLine Include="{" />
    <_GPRLine Include="%20%20%20%20public static readonly PackageIdentity $([System.String]::Copy(&quot;%(PackageReference.Identity)&quot;).Replace(&quot;.&quot;, &quot;_&quot;)) = new(&quot;%(PackageReference.Identity)&quot;, &quot;%(PackageReference.Version)&quot;)%3B"
              Condition="!$([System.String]::Copy(&quot;%(PackageReference.Identity)&quot;).StartsWith(&quot;Microsoft.&quot;)) 
                     AND !$([System.String]::Copy(&quot;%(PackageReference.Identity)&quot;).StartsWith(&quot;MSTest.Test&quot;))
                     AND '%(PackageReference.PrivateAssets)'!='All'
                     "/>
    <_GPRLine Include="}" />
  </ItemGroup>
  <WriteLinesToFile File="PackageReference.cs" Lines="@(_GPRLine)" Overwrite="True" />
</Target>
```
<sup><a href='/src/SolutionAnalyzer/SolutionAnalyzer.Test/SolutionAnalyzer.Test.csproj#L26-L43' title='Snippet source file'>snippet source</a> | <a href='#snippet-generatepackagereferences' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

This will translate all `PackageReference` items in the project to a corresponding entry in the `PackageRefrence` class, so after the next build the file `PackageRefrence.cs` will look like this:
<!-- snippet: PackageReference.cs -->
<a id='snippet-PackageReference.cs'></a>
```cs
// ReSharper disable All
using Microsoft.CodeAnalysis.Testing;

[System.CodeDom.Compiler.GeneratedCode("MSBuild", null)]
internal static class PackageReference
{
    public static readonly PackageIdentity TomsToolbox_Essentials = new("TomsToolbox.Essentials", "2.8.5");
}
```
<sup><a href='/src/SolutionAnalyzer/SolutionAnalyzer.Test/PackageReference.cs#L1-L8' title='Snippet source file'>snippet source</a> | <a href='#snippet-PackageReference.cs' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
So now the package reference can be added to the test compilation:
```cs
ReferenceAssemblies = Default.ReferenceAssemblies.AddPackages(PackageReference.TomsToolbox_Essentials);
```

Now the test succeeds again, so the test framework is set up properly.

Next step is to reflect the requirements of the use case, so the tests fails because the analyzer has no implementation yet.

### Update the test to reflect the use case
There should be an error reported for the `BadProperty`, since it has a `Text` but no `Description` attribute, 
so this behavior will be enforced in the test:
<!-- snippet: EnforceDescriptionAnalyzerTest_Verification -->
<a id='snippet-enforcedescriptionanalyzertest_verification'></a>
```cs
await new Test(source)
{
    AdditionalPackages = { PackageReference.TomsToolbox_Essentials },
    ExpectedDiagnostics = { Diagnostics.TextPropertyHasNoDescription.AsResult().WithArguments("BadProperty").WithLocation(0) },
}.RunAsync();
```
<sup><a href='/src/SolutionAnalyzer/SolutionAnalyzer.Test/EnforceDescriptionAnalyzerTest.cs#L55-L61' title='Snippet source file'>snippet source</a> | <a href='#snippet-enforcedescriptionanalyzertest_verification' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Now the test fails, because the analyzer is still empty, and does not generate the desired warnings yet, 
so finally the analyzer can be implemented using TDD.

### Implement the analyzer

Since the use case is not too complex, the analyzer implementation is lightweight, too:

<!-- snippet: EnforceDescriptionAnalyzer_Implementation -->
<a id='snippet-enforcedescriptionanalyzer_implementation'></a>
```cs
public override void Initialize(AnalysisContext context)
{
    context.EnableConcurrentExecution();
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

    context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Property);
}

private static void AnalyzeSymbol(SymbolAnalysisContext context)
{
    var property = (IPropertySymbol)context.Symbol;

    var attributes = property.GetAttributes();

    if (!attributes.Any(attr => attr.AttributeClass?.Name == "TextAttribute"))
        return;

    if (attributes.Any(attr => attr.AttributeClass?.Name == "DescriptionAttribute"))
        return;

    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.TextPropertyHasNoDescription, property.Locations.First(), property.Name));
}
```
<sup><a href='/src/SolutionAnalyzer/SolutionAnalyzer/EnforceDescriptionAnalyzer.cs#L15-L38' title='Snippet source file'>snippet source</a> | <a href='#snippet-enforcedescriptionanalyzer_implementation' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

It registers a symbol action to analyze all properties, and checks if the attributes are set according to the requirement.

Now the test succeeds, so the analyzer is working correctly.

Last step is to integrate the analyzer in the solution, so it is active in every project.

### Integrate the analyzer in the solution

To use the analyzer in any project, a reference to the analyzer project needs to be added, and the project output needs to be declared as `Analyzer`.
Since the analyzer should be referenced by any project of the solution, it's a good idea to add the reference in the `Directory.Build.Props` file, and exclude the analyzer project by a condition to avoid circular references.

<!-- snippet: AnalyzerIntegration -->
<a id='snippet-analyzerintegration'></a>
```props
<ItemGroup Condition='!$(MSBuildProjectName.ToUpperInvariant().EndsWith("ANALYZER")) AND !$(MSBuildProjectName.ToUpperInvariant().EndsWith(".TEST"))'>
  <ProjectReference Include="$(MSBuildThisFileDirectory)SolutionAnalyzer\SolutionAnalyzer\SolutionAnalyzer.csproj" ReferenceOutputAssembly="false" OutputItemType="Analyzer" />
</ItemGroup>
```
<sup><a href='/src/Directory.Build.props#L9-L13' title='Snippet source file'>snippet source</a> | <a href='#snippet-analyzerintegration' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Use case #2

In the project the [Nullable.Extended.Analyzer](https://github.com/tom-englert/Nullable.Extended) is used to force documentation of the 
usage of the null forgiving symbol. 

Since it's a standard pattern to initialize an init-only property with `default!`, no extra documentation is needed and the warning should be suppressed.

### Add the scaffold for the suppression analyzer

As a first step the scaffold is added to the solution:

#### Define the diagnostic descriptor
<!-- snippet: Diagnostics_Suppressor -->
<a id='snippet-diagnostics_suppressor'></a>
```cs
public static readonly SuppressionDescriptor SuppressNullForgivingWarning = new("CUS002",
    "NX0002",
    "Null forgiving is a standard pattern for init only properties");
```
<sup><a href='/src/SolutionAnalyzer/SolutionAnalyzer/Diagnostics.cs#L17-L21' title='Snippet source file'>snippet source</a> | <a href='#snippet-diagnostics_suppressor' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

#### Add an empty analyzer
<!-- snippet: SuppressNullForgivingAnalyzer_Declaration -->
<a id='snippet-suppressnullforgivinganalyzer_declaration'></a>
```cs
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SuppressNullForgivingWarningAnalyzer : DiagnosticSuppressor
{
    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } =
        ImmutableArray.Create(Diagnostics.SuppressNullForgivingWarning);
```
<sup><a href='/src/SolutionAnalyzer/SolutionAnalyzer/SuppressNullForgivingWarningAnalyzer.cs#L10-L16' title='Snippet source file'>snippet source</a> | <a href='#snippet-suppressnullforgivinganalyzer_declaration' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

#### Add the test scaffold
<!-- snippet: BasicSuppressionTestSetup -->
<a id='snippet-basicsuppressiontestsetup'></a>
```cs
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
```
<sup><a href='/src/SolutionAnalyzer/SolutionAnalyzer.Test/SuppressNullForgivingWarningTest.cs#L10-L42' title='Snippet source file'>snippet source</a> | <a href='#snippet-basicsuppressiontestsetup' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

### Prepare the test to include the analyzer with the diagnostic to suppress

At next the test needs to know about the diagnostic that should be suppressed. 
Additionally to the package reference the assembly of the analyzer needs to be referenced, too, so the analyzer is available at the test's runtime:

<!-- snippet: ReferenceNullableExtendedAnalyzer -->
<a id='snippet-referencenullableextendedanalyzer'></a>
```csproj
<ItemGroup>
  <PackageReference Include="Nullable.Extended.Analyzer" Version="1.10.4539" PrivateAssets="all" GeneratePathProperty="true"/>
  <Reference Include="$(PkgNullable_Extended_Analyzer)\analyzers\dotnet\cs\Nullable.Extended.Analyzer.dll" />
</ItemGroup>
```
<sup><a href='/src/SolutionAnalyzer/SolutionAnalyzer.Test/SolutionAnalyzer.Test.csproj#L19-L24' title='Snippet source file'>snippet source</a> | <a href='#snippet-referencenullableextendedanalyzer' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Since there is now more than one analyzer involved, the test framework needs to be extended to allow for adding the additional analyzer that the tests should run against.

Also reporting suppressed diagnostics needs to be enabled, so the suppressed diagnostic is not missing, but explicitly reported as suppressed, so the test can verify this correctly.
<!-- snippet: CSharpAnalyzerVerifier_Suppressor -->
<a id='snippet-csharpanalyzerverifier_suppressor'></a>
```cs
private bool _reportSuppressedDiagnostics;

protected override Task RunImplAsync(CancellationToken cancellationToken)
{
    // Workaround https://github.com/dotnet/roslyn-sdk/issues/1078
    _reportSuppressedDiagnostics = GetDiagnosticAnalyzers().Any(analyzer => analyzer is DiagnosticSuppressor);
    if (_reportSuppressedDiagnostics)
    {
        TestBehaviors |= TestBehaviors.SkipSuppressionCheck;
    }

    ReferenceAssemblies = ReferenceAssemblies.AddPackages(AdditionalPackages.ToImmutableArray());

    return base.RunImplAsync(cancellationToken);
}

protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
{
    return base.GetDiagnosticAnalyzers().Concat(AdditionalAnalyzers);
}

protected override CompilationWithAnalyzers CreateCompilationWithAnalyzers(Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerOptions options, CancellationToken cancellationToken)
{
    return compilation.WithAnalyzers(analyzers, new CompilationWithAnalyzersOptions(options, null, true, false, _reportSuppressedDiagnostics));
}
```
<sup><a href='/src/SolutionAnalyzer/SolutionAnalyzer.Test/Verifiers.cs#L41-L68' title='Snippet source file'>snippet source</a> | <a href='#snippet-csharpanalyzerverifier_suppressor' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

With this preconditions the suppressor test can be implemented:

<!-- snippet: SuppressNullForgivingWarningTest -->
<a id='snippet-suppressnullforgivingwarningtest'></a>
```cs
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
```
<sup><a href='/src/SolutionAnalyzer/SolutionAnalyzer.Test/SuppressNullForgivingWarningTest.cs#L44-L73' title='Snippet source file'>snippet source</a> | <a href='#snippet-suppressnullforgivingwarningtest' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

And based on the test the suppression analyzer can be implemented:

<!-- snippet: SuppressNullForgivingAnalyzer_Implementation -->
<a id='snippet-suppressnullforgivinganalyzer_implementation'></a>
```cs
public override void ReportSuppressions(SuppressionAnalysisContext context)
{
    var cancellationToken = context.CancellationToken;

    foreach (var diagnostic in context.ReportedDiagnostics)
    {
        var location = diagnostic.Location;
        var sourceTree = location.SourceTree;
        if (sourceTree == null)
            continue;

        var root = sourceTree.GetRoot(cancellationToken);

        var sourceSpan = location.SourceSpan;
        var elementNode = root.FindNode(sourceSpan);

        if (elementNode.Parent is not EqualsValueClauseSyntax { Parent: PropertyDeclarationSyntax propertyDeclaration })
            continue;

        if (propertyDeclaration.AccessorList?.Accessors.Any(item => item.IsKind(SyntaxKind.InitAccessorDeclaration)) == true)
        {
            context.ReportSuppression(Suppression.Create(Diagnostics.SuppressNullForgivingWarning, diagnostic));
        }
    }
}
```
<sup><a href='/src/SolutionAnalyzer/SolutionAnalyzer/SuppressNullForgivingWarningAnalyzer.cs#L18-L44' title='Snippet source file'>snippet source</a> | <a href='#snippet-suppressnullforgivinganalyzer_implementation' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
