using System.ComponentModel;

using TomsToolbox.Essentials;

namespace ClassLibrary1
{
    public class Class1
    {
        [Description("Test ")]
        [Text("Key", "Value")]
        public string InitProperty { get; init; } = default!;

        public string? AnotherProperty { get; set; }
    }
}
