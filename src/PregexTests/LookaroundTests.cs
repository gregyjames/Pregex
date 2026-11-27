namespace PregexTests;

public class LookaroundTests
{
    [Test]
    public void TestPositiveLookahead()
    {
        var regex = RegexBuilder.Create()
            .Digit().OneOrMore()
            .PositiveLookahead(b => b.Literal("%"))
            .Build();
        regex.IsMatch("50%").Should().BeTrue();
        regex.IsMatch("50").Should().BeFalse();
        var match = regex.Match("50%");
        match.Value.Should().Be("50"); // Lookahead doesn't consume
    }

    [Test]
    public void TestNegativeLookahead()
    {
        // Test with literal strings to avoid digit matching complexity
        var regex = RegexBuilder.Create()
            .Literal("test")
            .NegativeLookahead(b => b.Literal("123"))
            .Build();
        regex.IsMatch("test").Should().BeTrue();
        regex.IsMatch("test456").Should().BeTrue();
        regex.IsMatch("test123").Should().BeFalse();
    }

    [Test]
    public void TestPositiveLookbehind()
    {
        var regex = RegexBuilder.Create()
            .PositiveLookbehind(b => b.Literal("$"))
            .Digit().OneOrMore()
            .WordBoundary()
            .Build();
        var matchWithDollar = regex.Match("Price: $50 total");
        matchWithDollar.Success.Should().BeTrue();
        matchWithDollar.Value.Should().Be("50"); // Lookbehind doesn't consume

        var matchWithoutDollar = regex.Match("Quantity: 50 total");
        matchWithoutDollar.Success.Should().BeFalse();
    }

    [Test]
    public void TestNegativeLookbehind()
    {
        // Match "test" only when NOT preceded by "abc"
        var regex = RegexBuilder.Create()
            .NegativeLookbehind(b => b.Literal("abc"))
            .Literal("test")
            .Build();
        regex.IsMatch("test").Should().BeTrue();
        regex.IsMatch("xyztest").Should().BeTrue();
        regex.IsMatch("abctest").Should().BeFalse();
    }
}
