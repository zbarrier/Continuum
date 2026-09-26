namespace Continuum.CSharpFunctionalExtensions;

public static class ScalePrecisionHelpers
{
    public static void ValidateAndCalculateScaleAndPrecision(decimal value, int scale, int precision, bool ignoreTrailingZeroes,
        out int valueScale, out int valuePrecision, out int actualIntegerDigits, out int expectedIntegerDigits)
    {
        if (scale < 0)
            throw new ArgumentOutOfRangeException(nameof(scale), $"Scale must be a positive integer. [value:{scale}].");
        if (precision < 0)
            throw new ArgumentOutOfRangeException(nameof(precision), $"Precision must be a positive integer. [value:{precision}].");
        if (precision < scale)
            throw new ArgumentOutOfRangeException(nameof(scale), $"Scale must be less than precision. [scale:{scale}, precision:{precision}].");

        valueScale = GetScale(value, ignoreTrailingZeroes);
        valuePrecision = GetPrecision(value, ignoreTrailingZeroes);

        actualIntegerDigits = valuePrecision - valueScale;
        expectedIntegerDigits = precision - scale;
    }

    private static UInt32[] GetBits(decimal value)
    {
        // We want the integer parts as uint
        // C# doesn't permit int[] to uint[] conversion, but .NET does. This is somewhat evil...
        return (uint[])(object)decimal.GetBits(value);
    }

    private static decimal GetMantissa(decimal value)
    {
        var bits = GetBits(value);
        return (bits[2] * 4294967296m * 4294967296m) + (bits[1] * 4294967296m) + bits[0];
    }

    private static uint GetUnsignedScale(decimal value)
    {
        var bits = GetBits(value);
        uint scale = (bits[3] >> 16) & 31;
        return scale;
    }

    private static int GetScale(decimal value, bool ignoreTrailingZeros)
    {
        uint scale = GetUnsignedScale(value);
        if (ignoreTrailingZeros)
        {
            return (int)(scale - NumTrailingZeros(value));
        }

        return (int)scale;
    }

    private static uint NumTrailingZeros(decimal value)
    {
        uint trailingZeros = 0;
        uint scale = GetUnsignedScale(value);
        for (decimal tmp = GetMantissa(value); tmp % 10m == 0 && trailingZeros < scale; tmp /= 10)
        {
            trailingZeros++;
        }

        return trailingZeros;
    }

    private static int GetPrecision(decimal value, bool ignoreTrailingZeros)
    {
        // Precision: number of times we can divide by 10 before we get to 0
        uint precision = 0;
        for (decimal tmp = GetMantissa(value); tmp >= 1; tmp /= 10)
        {
            precision++;
        }

        if (ignoreTrailingZeros)
        {
            return (int)(precision - NumTrailingZeros(value));
        }

        return (int)precision;
    }
}
