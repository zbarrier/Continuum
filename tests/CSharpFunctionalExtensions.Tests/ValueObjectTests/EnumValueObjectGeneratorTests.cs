namespace Continuum.CSharpFunctionalExtensions.Tests.ValueObjectTests;

public partial class EnumValueObjectGeneratorTests
{
    [Fact]
    public void GeneratedMembers_AreInDeclarationOrder_AndExcludeNonMembers()
    {
        Assert.Equal([GeneratedEnum.First, GeneratedEnum.Second, GeneratedEnum.Third], GeneratedEnum.All);
    }

    [Fact]
    public void GeneratedMembers_NestedType_Works()
    {
        Assert.Equal(2, Outer.NestedEnum.All.Count);
        Assert.Equal(Outer.NestedEnum.B, Outer.NestedEnum.FromId(2).Value);
    }

    [Fact]
    public void GeneratedMembers_MultiplePartialDeclarations_Works()
    {
        Assert.Equal([SplitEnum.X, SplitEnum.Y], SplitEnum.All);
    }

    public sealed partial class GeneratedEnum : EnumValueObject<GeneratedEnum>
    {
        public static readonly GeneratedEnum First = new("first");
        public static readonly GeneratedEnum Second = new("second");
        public static readonly GeneratedEnum Third = new("third");

        // Not members: wrong accessibility, instance, or different type.
        private static readonly GeneratedEnum Hidden = new("hidden");
        public static readonly string NotAMember = "x";
        public static GeneratedEnum Property => First;

        private GeneratedEnum(string id) : base(id) { }

        internal static GeneratedEnum GetHidden() => Hidden;
    }

    public static partial class Outer
    {
        public sealed partial class NestedEnum : EnumValueObject<NestedEnum, int>
        {
            public static readonly NestedEnum A = new(1, "a");
            public static readonly NestedEnum B = new(2, "b");

            private NestedEnum(int id, string name) : base(id, name) { }
        }
    }

    public sealed partial class SplitEnum : EnumValueObject<SplitEnum>
    {
        public static readonly SplitEnum X = new("x");

        private SplitEnum(string id) : base(id) { }
    }

    public sealed partial class SplitEnum
    {
        public static readonly SplitEnum Y = new("y");
    }
}
