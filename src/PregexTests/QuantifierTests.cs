namespace PregexTests;

public class QuantifierTests
{
    [Test]
    public void TestOneOrMore()
    {
        var regex = RegexBuilder.Create()
            .StartOfString()
            .Digit().OneOrMore()
            .EndOfString()
            .Build();
        regex.IsMatch("1").Should().BeTrue();
        regex.IsMatch("123").Should().BeTrue();
        regex.IsMatch("").Should().BeFalse();
        regex.IsMatch("abc").Should().BeFalse();
    }

    [Test]
    public void TestZeroOrMore()
    {
        var regex = RegexBuilder.Create()
            .StartOfString()
            .Digit().ZeroOrMore()
            .EndOfString()
            .Build();
        regex.IsMatch("").Should().BeTrue();
        regex.IsMatch("1").Should().BeTrue();
        regex.IsMatch("123").Should().BeTrue();
        regex.IsMatch("abc").Should().BeFalse();
    }

    [Test]
    public void TestOptional()
    {
        var regex = RegexBuilder.Create()
            .StartOfString()
            .Literal("http")
            .Literal("s").Optional()
            .EndOfString()
            .Build();
        regex.IsMatch("http").Should().BeTrue();
        regex.IsMatch("https").Should().BeTrue();
        regex.IsMatch("httpss").Should().BeFalse();
    }

    [Test]
    public void TestExactly()
    {
        var regex = RegexBuilder.Create()
            .StartOfString()
            .Digit().Exactly(3)
            .EndOfString()
            .Build();
        regex.IsMatch("123").Should().BeTrue();
        regex.IsMatch("12").Should().BeFalse();
        regex.IsMatch("1234").Should().BeFalse();
    }

    [Test]
    public void TestAtLeast()
    {
        var regex = RegexBuilder.Create()
            .StartOfString()
            .Digit().AtLeast(2)
            .EndOfString()
            .Build();
        regex.IsMatch("1").Should().BeFalse();
        regex.IsMatch("12").Should().BeTrue();
        regex.IsMatch("123").Should().BeTrue();
        regex.IsMatch("1234").Should().BeTrue();
    }

    [Test]
    public void TestBetween()
    {
        var regex = RegexBuilder.Create()
            .StartOfString()
            .Digit().Between(2, 4)
            .EndOfString()
            .Build();
        regex.IsMatch("1").Should().BeFalse();
        regex.IsMatch("12").Should().BeTrue();
        regex.IsMatch("123").Should().BeTrue();
        regex.IsMatch("1234").Should().BeTrue();
        regex.IsMatch("12345").Should().BeFalse();
    }

    [Test]
    public void TestMultiCharLiteralWithQuantifiers()
    {
        using var oneOrMore = RegexBuilder.Create().StartOfString().Literal("abc").OneOrMore().EndOfString();
        oneOrMore.GetPattern().Should().Be("^(?:abc)+$");
        var oneOrMoreRegex = oneOrMore.Build();
        oneOrMoreRegex.IsMatch("abc").Should().BeTrue();
        oneOrMoreRegex.IsMatch("abcabc").Should().BeTrue();
        oneOrMoreRegex.IsMatch("abccc").Should().BeFalse();

        using var zeroOrMore = RegexBuilder.Create().StartOfString().Literal("ab").ZeroOrMore().EndOfString();
        zeroOrMore.GetPattern().Should().Be("^(?:ab)*$");
        zeroOrMore.Build().IsMatch("abab").Should().BeTrue();
        zeroOrMore.Build().IsMatch("abb").Should().BeFalse();

        using var optional = RegexBuilder.Create().StartOfString().Literal("https://").Optional().Literal("example.com").EndOfString();
        optional.GetPattern().Should().Be(@"^(?:https://)?example\.com$");
        var optionalRegex = optional.Build();
        optionalRegex.IsMatch("https://example.com").Should().BeTrue();
        optionalRegex.IsMatch("example.com").Should().BeTrue();

        using var exactly = RegexBuilder.Create().Literal("ab").Exactly(2);
        exactly.GetPattern().Should().Be("(?:ab){2}");

        using var atLeast = RegexBuilder.Create().Literal("ab").AtLeast(2);
        atLeast.GetPattern().Should().Be("(?:ab){2,}");

        using var between = RegexBuilder.Create().Literal("ab").Between(1, 3);
        between.GetPattern().Should().Be("(?:ab){1,3}");
    }

    [Test]
    public void TestSingleCharEscapedLiteralAndGroupDoNotDoubleWrap()
    {
        using var singleEscaped = RegexBuilder.Create().Literal(".").OneOrMore();
        singleEscaped.GetPattern().Should().Be(@"\.+");

        using var groupWithLiteral = RegexBuilder.Create().Group(b => b.Literal("abc")).OneOrMore();
        groupWithLiteral.GetPattern().Should().Be("(abc)+");
    }
}
