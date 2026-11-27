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
            inner.Literal("px").Singleline();
            throw new InvalidOperationException("boom");
        })).Should().Throw<InvalidOperationException>();

        builder.GetPattern().Should().Be(@"\d");
        builder.GetOptions().Should().Be(RegexOptions.None);
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

    [Test]
    public void TestLookaroundWithAlternationAndRightToLeft()
    {
        var regex = RegexBuilder.Create()
            .PositiveLookbehind(b => b.Literal("$").Or().Literal("EUR "))
            .Digit().OneOrMore()
            .NegativeLookahead(b => b.Literal(".").Or().Digit())
            .RightToLeft()
            .Build();

        regex.Match("Price: $10, EUR 25, $30.5").Value.Should().Be("25");
    }

    [Test]
    public void TestNestedLookaroundsAndOptionCallAfterLookaroundStillRejectQuantifiers()
    {
        using var builder = RegexBuilder.Create()
            .PositiveLookbehind(lb => lb.Literal("$").PositiveLookahead(la => la.Digit()))
            .Digit().OneOrMore()
            .NegativeLookahead(la => la.Literal(".").Digit())
            .EndOfString();

        var regex = builder.Build();
        regex.Match("cost $42").Value.Should().Be("42");
        regex.IsMatch("cost $42.5").Should().BeFalse();

        using var afterLookaheadOption = RegexBuilder.Create()
            .PositiveLookahead(b => b.Digit())
            .IgnoreCase();
        afterLookaheadOption.Invoking(b => b.OneOrMore()).Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void TestAllLookaroundTypesRollbackOnExceptionAndThrowOnDisposedCallback()
    {
        Action<RegexBuilder, Action<RegexBuilder>>[] lookarounds =
        [
            (b, cb) => b.PositiveLookahead(cb),
            (b, cb) => b.NegativeLookahead(cb),
            (b, cb) => b.PositiveLookbehind(cb),
            (b, cb) => b.NegativeLookbehind(cb),
        ];

        foreach (var invokeLookaround in lookarounds)
        {
            using var rollbackBuilder = RegexBuilder.Create().Literal("ab").IgnoreCase();
            rollbackBuilder.Invoking(b => invokeLookaround(b, inner =>
            {
                inner.Literal("xyz").Singleline();
                throw new InvalidOperationException("boom");
            })).Should().Throw<InvalidOperationException>();

            rollbackBuilder.GetPattern().Should().Be("ab");
            rollbackBuilder.GetOptions().Should().Be(RegexOptions.IgnoreCase);
            rollbackBuilder.OneOrMore().GetPattern().Should().Be("(?:ab)+");

            var disposeBuilder = RegexBuilder.Create().Digit();
            disposeBuilder.Invoking(b => invokeLookaround(b, inner => inner.Dispose()))
                .Should().Throw<ObjectDisposedException>();
        }
    }
}
