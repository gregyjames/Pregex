namespace PregexTests;

public class AlternationTests
{
    [Test]
    public void TestAlternation()
    {
        var regex = RegexBuilder.Create()
            .StartOfString()
            .Literal("cat")
            .Or()
            .Literal("dog")
            .EndOfString()
            .Build();
        regex.IsMatch("cat").Should().BeTrue();
        regex.IsMatch("dog").Should().BeTrue();
        regex.IsMatch("bird").Should().BeFalse();
    }

    [Test]
    public void TestAlternationWithGroups()
    {
        var regex = RegexBuilder.Create()
            .StartOfString()
            .Group(b => b.Literal("http").Or().Literal("https"))
            .Literal("://")
            .EndOfString()
            .Build();
        regex.IsMatch("http://").Should().BeTrue();
        regex.IsMatch("https://").Should().BeTrue();
        regex.IsMatch("ftp://").Should().BeFalse();
    }

    [Test]
    public void TestOrPrecedenceWithAnchors()
    {
        var ungrouped = RegexBuilder.Create()
            .StartOfString()
            .Literal("cat")
            .Or()
            .Literal("dog")
            .EndOfString()
            .Build();

        ungrouped.IsMatch("cat123").Should().BeTrue();
        ungrouped.IsMatch("123dog").Should().BeTrue();

        var grouped = RegexBuilder.Create()
            .StartOfString()
            .NonCapturingGroup(b => b.Literal("cat").Or().Literal("dog"))
            .EndOfString()
            .Build();

        grouped.IsMatch("cat123").Should().BeFalse();
        grouped.IsMatch("123dog").Should().BeFalse();
        grouped.IsMatch("cat").Should().BeTrue();
        grouped.IsMatch("dog").Should().BeTrue();
    }

    [Test]
    public void TestOrAtStartEndAndConsecutiveProducesEmptyAlternatives()
    {
        using var leadingOr = RegexBuilder.Create().Or().Literal("a");
        leadingOr.GetPattern().Should().Be("|a");
        leadingOr.Build().IsMatch("").Should().BeTrue();

        using var trailingOr = RegexBuilder.Create().Literal("a").Or();
        trailingOr.GetPattern().Should().Be("a|");
        trailingOr.Build().IsMatch("").Should().BeTrue();

        using var doubleOr = RegexBuilder.Create().Literal("a").Or().Or().Literal("b");
        doubleOr.GetPattern().Should().Be("a||b");
    }

    [Test]
    public void TestOrFollowedByQuantifierThrowsInvalidOperationException()
    {
        using var builder = RegexBuilder.Create().Literal("a").Or();
        builder.Invoking(b => b.ZeroOrMore()).Should().Throw<InvalidOperationException>();
        builder.GetPattern().Should().Be("a|");
    }
}
