// Portions of this file are adapted from NewId (https://github.com/phatboyg/NewId).
// Copyright 2007-2019 Chris Patterson.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Modified: adapted for Continuum (namespace, .NET 10, nullable annotations, XML documentation).

namespace Continuum.Tests.NewIdFormatterTests;

public class Using_the_newid_formatters
{
    private readonly Dictionary<string, string[]> _testValues;
    public Using_the_newid_formatters()
    {
        var directory = AppDomain.CurrentDomain.BaseDirectory;
        var textsFileName = Path.Combine(directory, "texts.txt");
        var fileText = File.ReadAllText(textsFileName);


        _testValues = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string[]>>(fileText)!;
    }

    // Base32
    [Fact]
    public void Should_compare_known_conversions_Base32Lower() => CompareKnownEncoding("Base32Lower", new Base32Formatter());
    [Fact]
    public void Should_compare_known_conversions_Base32Upper() => CompareKnownEncoding("Base32Upper", new Base32Formatter(true));
    [Fact]
    public void Should_compare_known_conversions_CustomBase32() => CompareKnownEncoding("CustomBase32", new Base32Formatter("0123456789ABCDEFGHIJKLMNOPQRSTUV"));

    // ZBase32
    [Fact]
    public void Should_compare_known_conversions_ZBase32Lower() => CompareKnownEncoding("ZBase32Lower", new ZBase32Formatter());
    [Fact]
    public void Should_compare_known_conversions_ZBase32Upper() => CompareKnownEncoding("ZBase32Upper", new ZBase32Formatter(true));

    // Hex
    [Fact]
    public void Should_compare_known_conversions_HexBase16Lower() => CompareKnownEncoding("HexBase16Lower", new HexFormatter());
    [Fact]
    public void Should_compare_known_conversions_HexBase16Upper() => CompareKnownEncoding("HexBase16Upper", new HexFormatter(true));

    // DashedHex
    [Fact]
    public void Should_compare_known_conversions_DashedHexBase16Lower() => CompareKnownEncoding("DashedHexBase16Lower", new DashedHexFormatter());
    [Fact]
    public void Should_compare_known_conversions_DashedHexBase16Upper() => CompareKnownEncoding("DashedHexBase16Upper", new DashedHexFormatter(upperCase: true));
    [Fact]
    public void Should_compare_known_conversions_DashedHexBase16BracketsLower() => CompareKnownEncoding("DashedHexBase16BracketsLower", new DashedHexFormatter('{', '}'));
    [Fact]
    public void Should_compare_known_conversions_DashedHexBase16BracketsUpper() => CompareKnownEncoding("DashedHexBase16BracketsUpper", new DashedHexFormatter('{', '}', upperCase: true));


    void CompareKnownEncoding(string name, INewIdFormatter formatter)
    {
        var guids = _testValues["Guids"];
        var expectedValues = _testValues[name];
        Assert.Equal(expectedValues.Length, guids.Length);

        for (var i = 0; i < guids.Length; i++)
        {
            var newId = new NewId(guids[i]);
            var text = newId.ToString(formatter);
            Assert.Equal(text, expectedValues[i]);
        }
        Console.WriteLine("Compared {0} equal conversions", guids.Length);
    }

    [Fact]
    public void Should_convert_back_using_parser()
    {
        var n = new NewId("F6B27C7C-8AB8-4498-AC97-3A6107A21320");

        var formatter = new ZBase32Formatter(true);

        var ns = n.ToString(formatter);

        var parser = new ZBase32Parser();
        var newId = parser.Parse(ns);


        Assert.Equal(newId, n);
    }

    [Fact]
    public void Should_convert_back_using_standard_parser()
    {
        var n = new NewId("F6B27C7C-8AB8-4498-AC97-3A6107A21320");

        var formatter = new Base32Formatter(true);

        var ns = n.ToString(formatter);

        var parser = new Base32Parser();
        var newId = parser.Parse(ns);


        Assert.Equal(newId, n);
    }

    [Fact]
    public void Should_convert_using_custom_base32_formatting_characters()
    {
        var n = new NewId("F6B27C7C-8AB8-4498-AC97-3A6107A21320");

        var formatter = new Base32Formatter("0123456789ABCDEFGHIJKLMNOPQRSTUV");

        var ns = n.ToString(formatter);

        Assert.Equal("UQP7OV4AN129HB4N79GGF8GJ10", ns);
    }

    [Fact]
    public void Should_convert_using_standard_base32_formatting_characters()
    {
        var n = new NewId("F6B27C7C-8AB8-4498-AC97-3A6107A21320");

        var formatter = new Base32Formatter(true);

        var ns = n.ToString(formatter);

        Assert.Equal("62ZHY7EKXBCJRLEXHJQQPIQTBA", ns);
    }

    [Fact]
    public void Should_convert_using_the_optimized_human_readable_formatter()
    {
        var n = new NewId("F6B27C7C-8AB8-4498-AC97-3A6107A21320");

        var formatter = new ZBase32Formatter(true);

        var ns = n.ToString(formatter);

        Assert.Equal("6438A9RKZBNJTMRZ8JOOXEOUBY", ns);
    }

    [Fact]
    public void Should_translate_often_transposed_characters_to_proper_values()
    {
        var n = new NewId("F6B27C7C-8AB8-4498-AC97-3A6107A21320");

        var ns = "6438A9RK2BNJTMRZ8J0OXE0UBY";

        var parser = new ZBase32Parser(true);
        var newId = parser.Parse(ns);


        Assert.Equal(newId, n);
    }
}
