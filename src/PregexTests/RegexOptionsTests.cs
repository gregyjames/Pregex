using System.Globalization;

namespace PregexTests;

public class RegexOptionsTests
{
    [Test]
    public void TestIgnoreCase()
    {
        var regex = RegexBuilder.Create()
            .Literal("test")
            .IgnoreCase()
            .Build();
        regex.IsMatch("test").Should().BeTrue();
        regex.IsMatch("TEST").Should().BeTrue();
        regex.IsMatch("TeSt").Should().BeTrue();
    }

    [Test]
    public void TestMultiline()
    {
        var regex = RegexBuilder.Create()
            .StartOfString()
            .Literal("test")
            .Multiline()
            .Build();
        regex.IsMatch("test\nmore").Should().BeTrue();
        regex.IsMatch("first\ntest").Should().BeTrue();
    }

    [Test]
    public void TestSingleline()
    {
        var regex = RegexBuilder.Create()
            .AnyChar().ZeroOrMore()
            .Singleline()
            .Build();
        regex.IsMatch("test\nmore").Should().BeTrue();
    }

    [Test]
    public void TestExplicitCapture()
    {
        var regex = RegexBuilder.Create()
            .Group(b => b.Digit().Exactly(3))           // Unnamed group - won't capture
            .Literal("-")
            .NamedGroup("last", b => b.Digit().Exactly(4))  // Named group - will capture
            .ExplicitCapture()
            .Build();

        var match = regex.Match("123-4567");
        match.Success.Should().BeTrue();
        match.Groups["last"].Value.Should().Be("4567");
        match.Groups.Count.Should().Be(2);
    }

    [Test]
    public void TestCompiled()
    {
        var regex = RegexBuilder.Create()
            .Digit().OneOrMore()
            .Compiled()
            .Build();

        // Test that it works and has the Compiled option
        regex.IsMatch("12345").Should().BeTrue();
        regex.Options.Should().HaveFlag(RegexOptions.Compiled);
    }

    [Test]
    public void TestIgnorePatternWhitespace()
    {
        var builder = RegexBuilder.Create()
            .Digit().Exactly(3)
            .Literal("-")
            .Digit().Exactly(4)
            .IgnorePatternWhitespace();

        var regex = builder.Build();

        // Verify the option is set
        regex.Options.Should().HaveFlag(RegexOptions.IgnorePatternWhitespace);

        // Pattern should still work normally
        regex.IsMatch("123-4567").Should().BeTrue();
    }

    [Test]
    public void TestCultureInvariant()
    {
        var regex = RegexBuilder.Create()
            .Literal("i")
            .IgnoreCase()
            .CultureInvariant()
            .Build();

        regex.Options.Should().HaveFlag(RegexOptions.CultureInvariant);
        regex.IsMatch("I").Should().BeTrue();
    }

    [Test]
    public void TestRightToLeft()
    {
        var regex = RegexBuilder.Create()
            .Digit().OneOrMore()
            .RightToLeft()
            .Build();

        regex.Options.Should().HaveFlag(RegexOptions.RightToLeft);
        regex.Match("a12b34").Value.Should().Be("34");
    }

    [Test]
    public void TestCultureInvariantIgnoresTurkishCaseRules()
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
            using var builder = RegexBuilder.Create()
                .StartOfString()
                .Literal("i")
                .EndOfString()
                .IgnoreCase()
                .CultureInvariant();

            var regex = builder.Build();
            regex.IsMatch("I").Should().BeTrue();
            regex.IsMatch("\u0130").Should().BeFalse();
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Test]
    public void TestRightToLeftWithNamedGroupAndAnchors()
    {
        var regex = RegexBuilder.Create()
            .NamedGroup("num", b => b.Digit().OneOrMore())
            .Literal("-")
            .NamedGroup("word", b => b.Word().OneOrMore())
            .RightToLeft()
            .Build();

        var match = regex.Match("12-ab 34-cd");
        match.Success.Should().BeTrue();
        match.Value.Should().Be("34-cd");
        match.Groups["num"].Value.Should().Be("34");
        match.Groups["word"].Value.Should().Be("cd");
    }

    [Test]
    public void TestExplicitCaptureWithMultiCharLiteralQuantifierAndGroups()
    {
        var regex = RegexBuilder.Create()
            .Literal("ab").OneOrMore()
            .Group(b => b.Digit().Exactly(2))
            .Literal("-")
            .NamedGroup("code", b => b.Word().OneOrMore())
            .ExplicitCapture()
            .Build();

        var match = regex.Match("abab12-xy");
        match.Success.Should().BeTrue();
        match.Groups.Count.Should().Be(2);
        match.Groups["code"].Value.Should().Be("xy");
    }

    [Test]
    public void TestRightToLeftWithLazyMultiCharLiteralAndRange()
    {
        var regex = RegexBuilder.Create()
            .Literal("ab").OneOrMore().Lazy()
            .Range('0', '9').OneOrMore()
            .RightToLeft()
            .Build();

        var match = regex.Match("ab1 abab23");
        match.Success.Should().BeTrue();
        match.Value.Should().Be("ab23");
    }

    [Test]
    public void TestIgnorePatternWhitespaceWithNonAsciiLineSeparators()
    {
        const string separators = "\u0085\u2028\u2029";
        var regex = RegexBuilder.Create()
            .StartOfString()
            .Literal(separators).OneOrMore()
            .EndOfString()
            .IgnorePatternWhitespace()
            .Build();

        regex.IsMatch(separators).Should().BeTrue();
        regex.IsMatch(separators + separators).Should().BeTrue();
        regex.IsMatch("").Should().BeFalse();
    }

    [Test]
    public void TestAllOptionsCombined()
    {
        using var builder = RegexBuilder.Create()
            .StartOfString()
            .Group(b => b.Literal("pre "))
            .NamedGroup("val", b => b.AnyChar().OneOrMore())
            .EndOfString()
            .IgnoreCase()
            .Multiline()
            .Singleline()
            .ExplicitCapture()
            .Compiled()
            .IgnorePatternWhitespace()
            .CultureInvariant()
            .RightToLeft();

        RegexOptions expected = RegexOptions.IgnoreCase
            | RegexOptions.Multiline
            | RegexOptions.Singleline
            | RegexOptions.ExplicitCapture
            | RegexOptions.Compiled
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.CultureInvariant
            | RegexOptions.RightToLeft;

        builder.GetOptions().Should().Be(expected);
        var regex = builder.Build();
        regex.Options.Should().Be(expected);

        var match = regex.Match("first\nPRE a\nb");
        match.Success.Should().BeTrue();
        match.Groups.Count.Should().Be(2);
        match.Groups["val"].Value.Should().Be("a\nb");
    }
}
