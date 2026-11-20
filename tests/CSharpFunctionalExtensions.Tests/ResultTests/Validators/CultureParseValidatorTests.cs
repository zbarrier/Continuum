using System.Globalization;
using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class CultureParseValidatorTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";
    const string MultiPartPropertyName = MyPropertyName + "." + MyPropertyNameTwo;

    static readonly PropertyPath Path = PropertyPath.Root.Append(MyPropertyName);
    static readonly CultureInfo German = new("de-DE");
    static readonly ValidationError OtherError = new("other", "other_code", "'{0}' other.", "other");

    // Every number parser accepts a leading sign, so unusual sign symbols prove which culture was used, even for integers.
    static readonly CultureInfo UnusualSigns = CreateUnusualSignsCulture();

    static CultureInfo CreateUnusualSignsCulture()
    {
        var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        culture.NumberFormat.PositiveSign = "#";
        culture.NumberFormat.NegativeSign = "~";
        return culture;
    }

    static ValidationError InvalidFormat(string propertyName) =>
        new(propertyName, ExpectedValidationErrorCodes.InvalidFormat, ExpectedValidatorErrorStrings.InvalidFormat, propertyName);

    static ValidationError NotEmpty(string propertyName) =>
        new(propertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, propertyName);

    static void WithCurrentCulture(CultureInfo culture, Action action)
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = culture;
            action();
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    public static TheoryData<string> NumberParsers =>
    [
        "Byte", "SByte", "Int16", "UInt16", "Int32", "UInt32", "Int64", "UInt64",
        "Single", "Double", "Decimal", "BigInteger", "BigIntegerAndNotEmpty"
    ];

    static Result<string> AsText<T>(Result<T> result) =>
        result.IsSuccess
            ? Result.Success(Convert.ToString(result.Value, CultureInfo.InvariantCulture)!)
            : Result.Failure<string>(result.Error);

    static Result<string> ParseDefault(string parser, string value) => parser switch
    {
        "Byte" => AsText(Result.IsByte(value, MyPropertyName)),
        "SByte" => AsText(Result.IsSByte(value, MyPropertyName)),
        "Int16" => AsText(Result.IsInt16(value, MyPropertyName)),
        "UInt16" => AsText(Result.IsUInt16(value, MyPropertyName)),
        "Int32" => AsText(Result.IsInt32(value, MyPropertyName)),
        "UInt32" => AsText(Result.IsUInt32(value, MyPropertyName)),
        "Int64" => AsText(Result.IsInt64(value, MyPropertyName)),
        "UInt64" => AsText(Result.IsUInt64(value, MyPropertyName)),
        "Single" => AsText(Result.IsSingle(value, MyPropertyName)),
        "Double" => AsText(Result.IsDouble(value, MyPropertyName)),
        "Decimal" => AsText(Result.IsDecimal(value, MyPropertyName)),
        "BigInteger" => AsText(Result.IsBigInteger(value, MyPropertyName)),
        "BigIntegerAndNotEmpty" => AsText(Result.IsBigIntegerAndNotEmpty(value, MyPropertyName)),
        _ => throw new ArgumentOutOfRangeException(nameof(parser))
    };

    static Result<string> Parse(string parser, string? value, IFormatProvider? provider) => parser switch
    {
        "Byte" => AsText(Result.IsByte(value, MyPropertyName, provider)),
        "SByte" => AsText(Result.IsSByte(value, MyPropertyName, provider)),
        "Int16" => AsText(Result.IsInt16(value, MyPropertyName, provider)),
        "UInt16" => AsText(Result.IsUInt16(value, MyPropertyName, provider)),
        "Int32" => AsText(Result.IsInt32(value, MyPropertyName, provider)),
        "UInt32" => AsText(Result.IsUInt32(value, MyPropertyName, provider)),
        "Int64" => AsText(Result.IsInt64(value, MyPropertyName, provider)),
        "UInt64" => AsText(Result.IsUInt64(value, MyPropertyName, provider)),
        "Single" => AsText(Result.IsSingle(value, MyPropertyName, provider)),
        "Double" => AsText(Result.IsDouble(value, MyPropertyName, provider)),
        "Decimal" => AsText(Result.IsDecimal(value, MyPropertyName, provider)),
        "BigInteger" => AsText(Result.IsBigInteger(value, MyPropertyName, provider)),
        "BigIntegerAndNotEmpty" => AsText(Result.IsBigIntegerAndNotEmpty(value, MyPropertyName, provider)),
        _ => throw new ArgumentOutOfRangeException(nameof(parser))
    };

    static Result<string> ParseMultiPart(string parser, string? value, IFormatProvider? provider) => parser switch
    {
        "Byte" => AsText(Result.IsByte(value, Path, MyPropertyNameTwo, provider)),
        "SByte" => AsText(Result.IsSByte(value, Path, MyPropertyNameTwo, provider)),
        "Int16" => AsText(Result.IsInt16(value, Path, MyPropertyNameTwo, provider)),
        "UInt16" => AsText(Result.IsUInt16(value, Path, MyPropertyNameTwo, provider)),
        "Int32" => AsText(Result.IsInt32(value, Path, MyPropertyNameTwo, provider)),
        "UInt32" => AsText(Result.IsUInt32(value, Path, MyPropertyNameTwo, provider)),
        "Int64" => AsText(Result.IsInt64(value, Path, MyPropertyNameTwo, provider)),
        "UInt64" => AsText(Result.IsUInt64(value, Path, MyPropertyNameTwo, provider)),
        "Single" => AsText(Result.IsSingle(value, Path, MyPropertyNameTwo, provider)),
        "Double" => AsText(Result.IsDouble(value, Path, MyPropertyNameTwo, provider)),
        "Decimal" => AsText(Result.IsDecimal(value, Path, MyPropertyNameTwo, provider)),
        "BigInteger" => AsText(Result.IsBigInteger(value, Path, MyPropertyNameTwo, provider)),
        "BigIntegerAndNotEmpty" => AsText(Result.IsBigIntegerAndNotEmpty(value, Path, MyPropertyNameTwo, provider)),
        _ => throw new ArgumentOutOfRangeException(nameof(parser))
    };

    static Result<string> ParseChained(string parser, Result<string> result, IFormatProvider? provider) => parser switch
    {
        "Byte" => AsText(result.IsByte(MyPropertyName, provider)),
        "SByte" => AsText(result.IsSByte(MyPropertyName, provider)),
        "Int16" => AsText(result.IsInt16(MyPropertyName, provider)),
        "UInt16" => AsText(result.IsUInt16(MyPropertyName, provider)),
        "Int32" => AsText(result.IsInt32(MyPropertyName, provider)),
        "UInt32" => AsText(result.IsUInt32(MyPropertyName, provider)),
        "Int64" => AsText(result.IsInt64(MyPropertyName, provider)),
        "UInt64" => AsText(result.IsUInt64(MyPropertyName, provider)),
        "Single" => AsText(result.IsSingle(MyPropertyName, provider)),
        "Double" => AsText(result.IsDouble(MyPropertyName, provider)),
        "Decimal" => AsText(result.IsDecimal(MyPropertyName, provider)),
        "BigInteger" => AsText(result.IsBigInteger(MyPropertyName, provider)),
        "BigIntegerAndNotEmpty" => AsText(result.IsBigIntegerAndNotEmpty(MyPropertyName, provider)),
        _ => throw new ArgumentOutOfRangeException(nameof(parser))
    };

    static Result<string> ParseChainedMultiPart(string parser, Result<string> result, IFormatProvider? provider) => parser switch
    {
        "Byte" => AsText(result.IsByte(Path, MyPropertyNameTwo, provider)),
        "SByte" => AsText(result.IsSByte(Path, MyPropertyNameTwo, provider)),
        "Int16" => AsText(result.IsInt16(Path, MyPropertyNameTwo, provider)),
        "UInt16" => AsText(result.IsUInt16(Path, MyPropertyNameTwo, provider)),
        "Int32" => AsText(result.IsInt32(Path, MyPropertyNameTwo, provider)),
        "UInt32" => AsText(result.IsUInt32(Path, MyPropertyNameTwo, provider)),
        "Int64" => AsText(result.IsInt64(Path, MyPropertyNameTwo, provider)),
        "UInt64" => AsText(result.IsUInt64(Path, MyPropertyNameTwo, provider)),
        "Single" => AsText(result.IsSingle(Path, MyPropertyNameTwo, provider)),
        "Double" => AsText(result.IsDouble(Path, MyPropertyNameTwo, provider)),
        "Decimal" => AsText(result.IsDecimal(Path, MyPropertyNameTwo, provider)),
        "BigInteger" => AsText(result.IsBigInteger(Path, MyPropertyNameTwo, provider)),
        "BigIntegerAndNotEmpty" => AsText(result.IsBigIntegerAndNotEmpty(Path, MyPropertyNameTwo, provider)),
        _ => throw new ArgumentOutOfRangeException(nameof(parser))
    };

    // Number parsers

    [Theory]
    [MemberData(nameof(NumberParsers))]
    public void NumberParser_Default_IgnoresCurrentCulture(string parser)
    {
        WithCurrentCulture(UnusualSigns, () =>
        {
            Assert.Equal("5", ParseDefault(parser, "+5").Value);
            Assert.Equal(InvalidFormat(MyPropertyName), ParseDefault(parser, "#5").Error);
        });
    }

    [Theory]
    [MemberData(nameof(NumberParsers))]
    public void NumberParser_Provider_UsesProvider(string parser)
    {
        Assert.Equal("5", Parse(parser, "#5", UnusualSigns).Value);
        Assert.Equal("5", ParseMultiPart(parser, "#5", UnusualSigns).Value);
        Assert.Equal("5", ParseChained(parser, Result.Success("#5"), UnusualSigns).Value);
        Assert.Equal("5", ParseChainedMultiPart(parser, Result.Success("#5"), UnusualSigns).Value);

        Assert.Equal(InvalidFormat(MyPropertyName), Parse(parser, "+5", UnusualSigns).Error);
        Assert.Equal(InvalidFormat(MultiPartPropertyName), ParseMultiPart(parser, "+5", UnusualSigns).Error);
    }

    [Theory]
    [MemberData(nameof(NumberParsers))]
    public void NumberParser_NullProvider_UsesInvariantCulture(string parser)
    {
        WithCurrentCulture(UnusualSigns, () =>
        {
            Assert.Equal("5", Parse(parser, "+5", null).Value);
            Assert.Equal(InvalidFormat(MyPropertyName), Parse(parser, "#5", null).Error);
            Assert.Equal(InvalidFormat(MultiPartPropertyName), ParseMultiPart(parser, "#5", null).Error);
        });
    }

    [Theory]
    [MemberData(nameof(NumberParsers))]
    public void NumberParser_ProviderNullOrEmpty_NotEmptyError(string parser)
    {
        Assert.Equal(NotEmpty(MyPropertyName), Parse(parser, null, UnusualSigns).Error);
        Assert.Equal(NotEmpty(MultiPartPropertyName), ParseMultiPart(parser, "", UnusualSigns).Error);
    }

    [Theory]
    [MemberData(nameof(NumberParsers))]
    public void NumberParser_ProviderFailedResult_ReturnsOriginalError(string parser)
    {
        Assert.Same(OtherError, ParseChained(parser, Result.Failure<string>(OtherError), UnusualSigns).Error);
        Assert.Same(OtherError, ParseChainedMultiPart(parser, Result.Failure<string>(OtherError), UnusualSigns).Error);
    }

    [Fact]
    public void Decimal_Default_IgnoresGermanCurrentCulture()
    {
        WithCurrentCulture(German, () =>
        {
            Assert.Equal(1.5m, Result.IsDecimal("1.5", MyPropertyName).Value);
            Assert.Equal(1234.5m, Result.IsDecimal("1,234.5", MyPropertyName).Value);
            Assert.Equal(1.5d, Result.IsDouble("1.5", MyPropertyName).Value);
            Assert.Equal(1.5f, Result.IsSingle("1.5", MyPropertyName).Value);
        });
    }

    [Fact]
    public void Decimal_GermanProvider_UsesGermanSeparators()
    {
        Assert.Equal(1.5m, Result.IsDecimal("1,5", MyPropertyName, German).Value);
        Assert.Equal(1234.5m, Result.IsDecimal("1.234,5", MyPropertyName, German).Value);
        Assert.Equal(1.5d, Result.IsDouble("1,5", MyPropertyName, German).Value);
        Assert.Equal(1.5f, Result.IsSingle("1,5", MyPropertyName, German).Value);
    }

    [Fact]
    public void Decimal_GroupedValue_MeaningDependsOnCulture()
    {
        // The same text is valid in both cultures but means a different number, so the culture must be chosen deliberately.
        Assert.Equal(1234m, Result.IsDecimal("1,234", MyPropertyName).Value);
        Assert.Equal(1.234m, Result.IsDecimal("1,234", MyPropertyName, German).Value);
    }

    [Fact]
    public void Double_GermanProvider_StillRejectsNonFiniteValues()
    {
        Assert.Equal(InvalidFormat(MyPropertyName), Result.IsDouble("1e400", MyPropertyName, German).Error);
        Assert.Equal(InvalidFormat(MyPropertyName), Result.IsSingle("1e39", MyPropertyName, German).Error);
    }

    [Fact]
    public void BigIntegerAndNotEmpty_Provider_StillRejectsZero()
    {
        Assert.Equal(InvalidFormat(MyPropertyName), Result.IsBigIntegerAndNotEmpty("#0", MyPropertyName, UnusualSigns).Error);
        Assert.Equal(new BigInteger(-5), Result.IsBigIntegerAndNotEmpty("~5", MyPropertyName, UnusualSigns).Value);
    }

    // Date and time parsers

    [Fact]
    public void DateOnly_GermanProvider_ParsesDayFirst()
    {
        var expected = new DateOnly(2026, 1, 2);

        Assert.Equal(expected, Result.IsDateOnly("02.01.2026", MyPropertyName, German).Value);
        Assert.Equal(expected, Result.IsDateOnly("02.01.2026", Path, MyPropertyNameTwo, German).Value);
        Assert.Equal(expected, Result.Success("02.01.2026").IsDateOnly(MyPropertyName, German).Value);
        Assert.Equal(expected, Result.Success("02.01.2026").IsDateOnly(Path, MyPropertyNameTwo, German).Value);
    }

    [Fact]
    public void DateTime_GermanProvider_ParsesDayFirst()
    {
        var expected = new DateTime(2026, 1, 2, 13, 30, 0);

        Assert.Equal(expected, Result.IsDateTime("02.01.2026 13:30", MyPropertyName, German).Value);
        Assert.Equal(expected, Result.IsDateTime("02.01.2026 13:30", Path, MyPropertyNameTwo, German).Value);
        Assert.Equal(expected, Result.Success("02.01.2026 13:30").IsDateTime(MyPropertyName, German).Value);
        Assert.Equal(expected, Result.Success("02.01.2026 13:30").IsDateTime(Path, MyPropertyNameTwo, German).Value);
    }

    [Fact]
    public void DateTimeOffset_GermanProvider_ParsesDayFirst()
    {
        var expected = new DateTimeOffset(2026, 1, 2, 13, 30, 0, TimeSpan.FromHours(1));

        Assert.Equal(expected, Result.IsDateTimeOffset("02.01.2026 13:30 +01:00", MyPropertyName, German).Value);
        Assert.Equal(expected, Result.IsDateTimeOffset("02.01.2026 13:30 +01:00", Path, MyPropertyNameTwo, German).Value);
        Assert.Equal(expected, Result.Success("02.01.2026 13:30 +01:00").IsDateTimeOffset(MyPropertyName, German).Value);
        Assert.Equal(expected, Result.Success("02.01.2026 13:30 +01:00").IsDateTimeOffset(Path, MyPropertyNameTwo, German).Value);
    }

    [Fact]
    public void DateOnly_DefaultAndGermanProvider_ReadSameTextDifferently()
    {
        Assert.Equal(new DateOnly(2026, 1, 2), Result.IsDateOnly("01/02/2026", MyPropertyName).Value);
        Assert.Equal(new DateOnly(2026, 2, 1), Result.IsDateOnly("01/02/2026", MyPropertyName, German).Value);
    }

    [Fact]
    public void TimeOnly_Provider_UsesProviderDesignators()
    {
        var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        culture.DateTimeFormat.PMDesignator = "nachm.";
        var expected = new TimeOnly(14, 30);

        Assert.Equal(expected, Result.IsTimeOnly("2:30 nachm.", MyPropertyName, culture).Value);
        Assert.Equal(expected, Result.IsTimeOnly("2:30 nachm.", Path, MyPropertyNameTwo, culture).Value);
        Assert.Equal(expected, Result.Success("2:30 nachm.").IsTimeOnly(MyPropertyName, culture).Value);
        Assert.Equal(expected, Result.Success("2:30 nachm.").IsTimeOnly(Path, MyPropertyNameTwo, culture).Value);
        Assert.Equal(InvalidFormat(MyPropertyName), Result.IsTimeOnly("2:30 nachm.", MyPropertyName).Error);
    }

    [Fact]
    public void TimeSpan_GermanProvider_UsesGermanDecimalSeparator()
    {
        var expected = new TimeSpan(0, 1, 2, 3, 500);

        Assert.Equal(expected, Result.IsTimeSpan("1:02:03,5", MyPropertyName, German).Value);
        Assert.Equal(expected, Result.IsTimeSpan("1:02:03,5", Path, MyPropertyNameTwo, German).Value);
        Assert.Equal(expected, Result.Success("1:02:03,5").IsTimeSpan(MyPropertyName, German).Value);
        Assert.Equal(expected, Result.Success("1:02:03,5").IsTimeSpan(Path, MyPropertyNameTwo, German).Value);
        Assert.Equal(InvalidFormat(MyPropertyName), Result.IsTimeSpan("1:02:03,5", MyPropertyName).Error);
    }

    [Fact]
    public void DateParser_NullProvider_UsesInvariantCulture()
    {
        WithCurrentCulture(German, () =>
        {
            Assert.Equal(new DateOnly(2026, 1, 2), Result.IsDateOnly("01/02/2026", MyPropertyName, null).Value);
            Assert.Equal(new DateTime(2026, 1, 2), Result.IsDateTime("01/02/2026", MyPropertyName, null).Value);
        });
    }

    [Fact]
    public void DateParser_ProviderFailedResult_ReturnsOriginalError()
    {
        var failed = Result.Failure<string>(OtherError);

        Assert.Same(OtherError, failed.IsDateOnly(MyPropertyName, German).Error);
        Assert.Same(OtherError, failed.IsDateTime(Path, MyPropertyNameTwo, German).Error);
        Assert.Same(OtherError, failed.IsDateTimeOffset(MyPropertyName, German).Error);
        Assert.Same(OtherError, failed.IsTimeOnly(Path, MyPropertyNameTwo, German).Error);
        Assert.Same(OtherError, failed.IsTimeSpan(MyPropertyName, German).Error);
    }
}
