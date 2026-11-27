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

    [Test]
    public void TestLookaroundNullCallbacksThrowWithoutCorruptingBuilder()
    {
        using var builder = RegexBuilder.Create().Literal("ab");

        builder.Invoking(b => b.PositiveLookahead(null!)).Should().Throw<ArgumentNullException>();
        builder.Invoking(b => b.NegativeLookahead(null!)).Should().Throw<ArgumentNullException>();
        builder.Invoking(b => b.PositiveLookbehind(null!)).Should().Throw<ArgumentNullException>();
        builder.Invoking(b => b.NegativeLookbehind(null!)).Should().Throw<ArgumentNullException>();

        builder.GetPattern().Should().Be("ab");
        builder.OneOrMore().GetPattern().Should().Be("(?:ab)+");
    }

    [Test]
    public void TestLookaroundThrowingCallbackRollsBackPartialPattern()
    {
        using var builder = RegexBuilder.Create().Digit();

        builder.Invoking(b => b.PositiveLookahead(inner =>
        {
            inner.Literal("px");
            throw new InvalidOperationException("boom");
        })).Should().Throw<InvalidOperationException>();

        builder.GetPattern().Should().Be(@"\d");
    }

    [Test]
    public void TestLookaroundEmptyAndQuantifierRejection()
    {
        using var emptyLookahead = RegexBuilder.Create().PositiveLookahead(_ => { });
        emptyLookahead.GetPattern().Should().Be("(?=)");
        emptyLookahead.Build().IsMatch("a").Should().BeTrue();

        using var afterLookbehind = RegexBuilder.Create().PositiveLookbehind(b => b.Literal("$"));
        afterLookbehind.Invoking(b => b.OneOrMore()).Should().Throw<InvalidOperationException>();
    }
}
