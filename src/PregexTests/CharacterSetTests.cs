namespace PregexTests;

public class CharacterSetTests
{
    [Test]
    public void TestCharacterSet()
    {
        var regex = RegexBuilder.Create().Set("abc").Build();
        regex.IsMatch("a").Should().BeTrue();
        regex.IsMatch("b").Should().BeTrue();
        regex.IsMatch("c").Should().BeTrue();
        regex.IsMatch("d").Should().BeFalse();
    }

    [Test]
    public void TestNotCharacterSet()
    {
        var regex = RegexBuilder.Create().NotSet("abc").Build();
        regex.IsMatch("a").Should().BeFalse();
        regex.IsMatch("b").Should().BeFalse();
        regex.IsMatch("c").Should().BeFalse();
        regex.IsMatch("d").Should().BeTrue();
    }

    [Test]
    public void TestRange()
    {
        var regex = RegexBuilder.Create().Range('a', 'z').Build();
        regex.IsMatch("a").Should().BeTrue();
        regex.IsMatch("m").Should().BeTrue();
        regex.IsMatch("z").Should().BeTrue();
        regex.IsMatch("A").Should().BeFalse();
        regex.IsMatch("5").Should().BeFalse();
    }

    [Test]
    public void TestSetDoesNotOverEscapeCharacterClassLiterals()
    {
        using var builder = RegexBuilder.Create().Set(".*+?()$ ");
        builder.GetPattern().Should().Be("[.*+?()$ ]");

        var regex = builder.Build();
        regex.IsMatch(".").Should().BeTrue();
        regex.IsMatch(" ").Should().BeTrue();
        regex.IsMatch("$").Should().BeTrue();
        regex.IsMatch("a").Should().BeFalse();
    }

    [Test]
    public void TestSetPreservesEscapedLiteralHyphen()
    {
        using var builder = RegexBuilder.Create().Set(@"a\-z");
        builder.GetPattern().Should().Be(@"[a\-z]");

        var regex = builder.Build();
        regex.IsMatch("a").Should().BeTrue();
        regex.IsMatch("-").Should().BeTrue();
        regex.IsMatch("z").Should().BeTrue();
        regex.IsMatch("m").Should().BeFalse();
    }

    [Test]
    public void TestSetSupportsRanges()
    {
        using var builder = RegexBuilder.Create().Set("a-z0-9");
        builder.GetPattern().Should().Be("[a-z0-9]");

        var regex = builder.Build();
        regex.IsMatch("m").Should().BeTrue();
        regex.IsMatch("5").Should().BeTrue();
        regex.IsMatch("-").Should().BeFalse();
    }

    [Test]
    public void TestSetEscapesBracketsCaretAndTrailingBackslash()
    {
        using var caretBuilder = RegexBuilder.Create().Set("^abc");
        caretBuilder.GetPattern().Should().Be(@"[\^abc]");
        var caretRegex = caretBuilder.Build();
        caretRegex.IsMatch("^").Should().BeTrue();
        caretRegex.IsMatch("a").Should().BeTrue();
        caretRegex.IsMatch("d").Should().BeFalse();

        using var bracketBuilder = RegexBuilder.Create().Set("a]b[c");
        bracketBuilder.GetPattern().Should().Be(@"[a\]b\[c]");
        var bracketRegex = bracketBuilder.Build();
        bracketRegex.IsMatch("]").Should().BeTrue();
        bracketRegex.IsMatch("[").Should().BeTrue();

        using var backslashBuilder = RegexBuilder.Create().Set(@"\");
        backslashBuilder.GetPattern().Should().Be(@"[\\]");
        backslashBuilder.Build().IsMatch(@"\").Should().BeTrue();
    }

    [Test]
    public void TestNotSetEscapesSpecialCharactersWithoutOverEscaping()
    {
        using var builder = RegexBuilder.Create().NotSet(" /]^");
        builder.GetPattern().Should().Be(@"[^ /\]\^]");

        var regex = builder.Build();
        regex.IsMatch("a").Should().BeTrue();
        regex.IsMatch(" ").Should().BeFalse();
        regex.IsMatch("/").Should().BeFalse();
        regex.IsMatch("]").Should().BeFalse();
        regex.IsMatch("^").Should().BeFalse();
    }

    [Test]
    public void TestSetAndNotSetThrowOnNullOrEmpty()
    {
        using var builder = RegexBuilder.Create();

        builder.Invoking(b => b.Set("")).Should().Throw<ArgumentException>();
        builder.Invoking(b => b.NotSet("")).Should().Throw<ArgumentException>();
        builder.Invoking(b => b.Set(null!)).Should().Throw<ArgumentNullException>();
        builder.Invoking(b => b.NotSet(null!)).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void TestNotSetPreservesEscapedLiteralHyphen()
    {
        using var builder = RegexBuilder.Create().NotSet(@"a\-z");
        builder.GetPattern().Should().Be(@"[^a\-z]");

        var regex = builder.Build();
        regex.IsMatch("m").Should().BeTrue();
        regex.IsMatch("a").Should().BeFalse();
        regex.IsMatch("-").Should().BeFalse();
        regex.IsMatch("z").Should().BeFalse();
    }

    [Test]
    public void TestSetEscapesControlCharacters()
    {
        using var builder = RegexBuilder.Create().Set("\t\n\r\f\v");
        builder.GetPattern().Should().Be(@"[\t\n\r\f\v]");

        var regex = builder.Build();
        regex.IsMatch("\t").Should().BeTrue();
        regex.IsMatch("\n").Should().BeTrue();
        regex.IsMatch("\r").Should().BeTrue();
        regex.IsMatch("\f").Should().BeTrue();
        regex.IsMatch("\v").Should().BeTrue();
        regex.IsMatch(" ").Should().BeFalse();
    }

    [Test]
    public void TestSetHandlesEscapedBackslashesAndShorthands()
    {
        using var escapedBackslash = RegexBuilder.Create().Set(@"\\");
        escapedBackslash.GetPattern().Should().Be(@"[\\]");
        escapedBackslash.Build().IsMatch(@"\").Should().BeTrue();

        using var oddBackslashes = RegexBuilder.Create().Set(@"\\\");
        oddBackslashes.GetPattern().Should().Be(@"[\\\\]");
        oddBackslashes.Build().IsMatch(@"\").Should().BeTrue();

        using var shorthand = RegexBuilder.Create().Set(@"\d_");
        shorthand.GetPattern().Should().Be(@"[\d_]");
        var shorthandRegex = shorthand.Build();
        shorthandRegex.IsMatch("5").Should().BeTrue();
        shorthandRegex.IsMatch("_").Should().BeTrue();
        shorthandRegex.IsMatch("a").Should().BeFalse();
    }

    [Test]
    public void TestRangeEscapesSpecialBoundaryCharacters()
    {
        using var caretRange = RegexBuilder.Create().Range('^', 'z');
        caretRange.GetPattern().Should().Be(@"[\^-z]");
        var caretRegex = caretRange.Build();
        caretRegex.IsMatch("^").Should().BeTrue();
        caretRegex.IsMatch("a").Should().BeTrue();
        caretRegex.IsMatch("z").Should().BeTrue();
        caretRegex.IsMatch("A").Should().BeFalse();

        using var backslashRange = RegexBuilder.Create().Range('\\', 'z');
        backslashRange.GetPattern().Should().Be(@"[\\-z]");
        var backslashRegex = backslashRange.Build();
        backslashRegex.IsMatch(@"\").Should().BeTrue();
        backslashRegex.IsMatch("a").Should().BeTrue();
        backslashRegex.IsMatch("-").Should().BeFalse();

        using var bracketRange = RegexBuilder.Create().Range('[', ']');
        bracketRange.GetPattern().Should().Be(@"[\[-\]]");
        var bracketRegex = bracketRange.Build();
        bracketRegex.IsMatch("[").Should().BeTrue();
        bracketRegex.IsMatch(@"\").Should().BeTrue();
        bracketRegex.IsMatch("]").Should().BeTrue();
        bracketRegex.IsMatch("a").Should().BeFalse();

        using var hyphenStartRange = RegexBuilder.Create().Range('-', '0');
        hyphenStartRange.GetPattern().Should().Be(@"[\x2D-0]");
        var hyphenStartRegex = hyphenStartRange.Build();
        hyphenStartRegex.IsMatch("-").Should().BeTrue();
        hyphenStartRegex.IsMatch(".").Should().BeTrue();
        hyphenStartRegex.IsMatch("0").Should().BeTrue();
        hyphenStartRegex.IsMatch("1").Should().BeFalse();

        using var hyphenEndRange = RegexBuilder.Create().Range('+', '-');
        hyphenEndRange.GetPattern().Should().Be(@"[+-\x2D]");
        var hyphenEndRegex = hyphenEndRange.Build();
        hyphenEndRegex.IsMatch("+").Should().BeTrue();
        hyphenEndRegex.IsMatch(",").Should().BeTrue();
        hyphenEndRegex.IsMatch("-").Should().BeTrue();
        hyphenEndRegex.IsMatch(".").Should().BeFalse();
    }

    [Test]
    public void TestRangeThrowsWhenStartGreaterThanEnd()
    {
        using var builder = RegexBuilder.Create();
        builder.Invoking(b => b.Range('z', 'a')).Should().Throw<ArgumentOutOfRangeException>();
    }
}
