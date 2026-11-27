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
}
