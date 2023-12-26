using Microsoft.CodeAnalysis;

namespace SolutionAnalyzer;

// begin-snippet:  Diagnostics
public static class Diagnostics
{
    private const string Category = "Custom";

    public static readonly DiagnosticDescriptor TextPropertyHasNoDescription = new("CUS001",
        "Property with Text attribute has no description",
        "Property {0} has a Text attribute but no Description attribute",
        Category,
        DiagnosticSeverity.Error, isEnabledByDefault: true);
    // end-snippet

    // begin-snippet:  Diagnostics_Suppressor
    public static readonly SuppressionDescriptor SuppressNullForgivingWarning = new("CUS002",
        "NX0004",
        "Null forgiving is a standard pattern for init only properties");
    // end-snippet

    public static readonly SuppressionDescriptor SuppressNullableOnEntities = new("CUS003",
        "CS8618",
        "Pattern by EF"
    );
}
