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
}
