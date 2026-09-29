// Portions of this file are adapted from NewId (https://github.com/phatboyg/NewId).
// Copyright 2007-2019 Chris Patterson.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Modified: adapted for Continuum (namespace, .NET 10, nullable annotations, XML documentation).

namespace Continuum.Tests.NewIdUsageTests;

public class Using_a_new_id
{
    [Fact]
    public void Should_format_just_like_a_default_guid_formatter()
    {
        var newId = new NewId();

        Assert.Equal("00000000-0000-0000-0000-000000000000", newId.ToString());
    }

    [Fact]
    public void Should_format_just_like_a_fancy_guid_formatter()
    {
        var newId = new NewId();

        Assert.Equal("{00000000-0000-0000-0000-000000000000}", newId.ToString("B"));
    }

    [Fact]
    public void Should_format_just_like_a_narrow_guid_formatter()
    {
        var newId = new NewId();

        Assert.Equal("00000000000000000000000000000000", newId.ToString("N"));
    }

    [Fact]
    public void Should_format_just_like_a_parenthesis_guid_formatter()
    {
        var newId = new NewId();

        Assert.Equal("(00000000-0000-0000-0000-000000000000)", newId.ToString("P"));
    }

    [Fact]
    public void Should_work_from_guid_to_newid_to_guid()
    {
        var g = Guid.NewGuid();

        var n = new NewId(g.ToByteArray());

        var gs = g.ToString("d");
        var ns = n.ToString("d");

        Assert.Equal(ns, gs);
    }
}
