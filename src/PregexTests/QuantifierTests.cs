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

    [Test]
    public void TestZeroRepetitionBounds()
    {
        using var exactlyZero = RegexBuilder.Create().StartOfString().Digit().Exactly(0).EndOfString();
        exactlyZero.GetPattern().Should().Be(@"^\d{0}$");
        exactlyZero.Build().IsMatch("").Should().BeTrue();
        exactlyZero.Build().IsMatch("1").Should().BeFalse();

        using var atLeastZero = RegexBuilder.Create().StartOfString().Digit().AtLeast(0).EndOfString();
        atLeastZero.GetPattern().Should().Be(@"^\d{0,}$");
        atLeastZero.Build().IsMatch("").Should().BeTrue();
        atLeastZero.Build().IsMatch("123").Should().BeTrue();

        using var betweenZeroZero = RegexBuilder.Create().StartOfString().Digit().Between(0, 0).EndOfString();
        betweenZeroZero.GetPattern().Should().Be(@"^\d{0,0}$");
        betweenZeroZero.Build().IsMatch("").Should().BeTrue();
        betweenZeroZero.Build().IsMatch("1").Should().BeFalse();

        using var betweenZeroThree = RegexBuilder.Create().StartOfString().Digit().Between(0, 3).EndOfString();
        betweenZeroThree.GetPattern().Should().Be(@"^\d{0,3}$");
        betweenZeroThree.Build().IsMatch("").Should().BeTrue();
        betweenZeroThree.Build().IsMatch("123").Should().BeTrue();
        betweenZeroThree.Build().IsMatch("1234").Should().BeFalse();
    }

    [Test]
    public void TestNegativeAndInvertedQuantifierBoundsThrowWithoutCorruptingBuilder()
    {
        using var builder = RegexBuilder.Create().Literal("abc");

        builder.Invoking(b => b.Exactly(-1)).Should().Throw<ArgumentOutOfRangeException>();
        builder.Invoking(b => b.Exactly(int.MinValue)).Should().Throw<ArgumentOutOfRangeException>();
        builder.Invoking(b => b.AtLeast(-1)).Should().Throw<ArgumentOutOfRangeException>();
        builder.Invoking(b => b.Between(-1, 2)).Should().Throw<ArgumentOutOfRangeException>();
        builder.Invoking(b => b.Between(5, 2)).Should().Throw<ArgumentOutOfRangeException>();

        builder.GetPattern().Should().Be("abc");
        builder.OneOrMore().GetPattern().Should().Be("(?:abc)+");
    }

    [Test]
    public void TestQuantifiersInInvalidPositionsThrowInvalidOperationExceptionWithoutCorruptingBuilder()
    {
        using var empty = RegexBuilder.Create();
        empty.Invoking(b => b.OneOrMore()).Should().Throw<InvalidOperationException>();
        empty.Invoking(b => b.ZeroOrMore()).Should().Throw<InvalidOperationException>();
        empty.Invoking(b => b.Optional()).Should().Throw<InvalidOperationException>();
        empty.Invoking(b => b.Exactly(2)).Should().Throw<InvalidOperationException>();
        empty.Invoking(b => b.AtLeast(2)).Should().Throw<InvalidOperationException>();
        empty.Invoking(b => b.Between(1, 2)).Should().Throw<InvalidOperationException>();
        empty.GetPattern().Should().Be("");

        using var afterEmptyLiteral = RegexBuilder.Create().Literal("a").Literal("");
        afterEmptyLiteral.Invoking(b => b.OneOrMore()).Should().Throw<InvalidOperationException>();
        afterEmptyLiteral.GetPattern().Should().Be("a");

        using var afterOr = RegexBuilder.Create().Literal("a").Or();
        afterOr.Invoking(b => b.OneOrMore()).Should().Throw<InvalidOperationException>();
        afterOr.GetPattern().Should().Be("a|");

        using var afterAnchor = RegexBuilder.Create().StartOfString();
        afterAnchor.Invoking(b => b.OneOrMore()).Should().Throw<InvalidOperationException>();
        afterAnchor.GetPattern().Should().Be("^");

        using var afterLookahead = RegexBuilder.Create().PositiveLookahead(b => b.Digit());
        afterLookahead.Invoking(b => b.Optional()).Should().Throw<InvalidOperationException>();
        afterLookahead.GetPattern().Should().Be(@"(?=\d)");

        using var consecutive = RegexBuilder.Create().Digit().OneOrMore();
        consecutive.Invoking(b => b.Optional()).Should().Throw<InvalidOperationException>();
        consecutive.Invoking(b => b.OneOrMore()).Should().Throw<InvalidOperationException>();
        consecutive.GetPattern().Should().Be(@"\d+");

        using var afterLazy = RegexBuilder.Create().Digit().OneOrMore().Lazy();
        afterLazy.Invoking(b => b.Optional()).Should().Throw<InvalidOperationException>();
        afterLazy.GetPattern().Should().Be(@"\d+?");
    }

    [Test]
    public void TestLazyModifierWithAllGreedyQuantifiersAndMatchingBehavior()
    {
        RegexBuilder.Create().Digit().OneOrMore().Lazy().GetPattern().Should().Be(@"\d+?");
        RegexBuilder.Create().Digit().ZeroOrMore().Lazy().GetPattern().Should().Be(@"\d*?");
        RegexBuilder.Create().Digit().Optional().Lazy().GetPattern().Should().Be(@"\d??");
        RegexBuilder.Create().Digit().Exactly(2).Lazy().GetPattern().Should().Be(@"\d{2}?");
        RegexBuilder.Create().Digit().AtLeast(2).Lazy().GetPattern().Should().Be(@"\d{2,}?");
        RegexBuilder.Create().Digit().Between(1, 3).Lazy().GetPattern().Should().Be(@"\d{1,3}?");
        RegexBuilder.Create().Literal("ab").OneOrMore().Lazy().GetPattern().Should().Be("(?:ab)+?");

        var lazyTag = RegexBuilder.Create()
            .Literal("<")
            .AnyChar().OneOrMore().Lazy()
            .Literal(">")
            .Build();
        lazyTag.Match("<a><b>").Value.Should().Be("<a>");
    }

    [Test]
    public void TestLazyInInvalidPositionsThrowsInvalidOperationException()
    {
        using var empty = RegexBuilder.Create();
        empty.Invoking(b => b.Lazy()).Should().Throw<InvalidOperationException>();

        using var afterDigit = RegexBuilder.Create().Digit();
        afterDigit.Invoking(b => b.Lazy()).Should().Throw<InvalidOperationException>();
        afterDigit.GetPattern().Should().Be(@"\d");

        using var doubleLazy = RegexBuilder.Create().Digit().OneOrMore().Lazy();
        doubleLazy.Invoking(b => b.Lazy()).Should().Throw<InvalidOperationException>();
        doubleLazy.GetPattern().Should().Be(@"\d+?");
    }

    [Test]
    public void TestOptionCallBetweenMultiCharLiteralAndQuantifierPreservesWrapping()
    {
        using var builder = RegexBuilder.Create()
            .StartOfString()
            .Literal("abc")
            .IgnoreCase()
            .OneOrMore()
            .EndOfString();

        builder.GetPattern().Should().Be("^(?:abc)+$");
        builder.Build().IsMatch("ABCabc").Should().BeTrue();
    }

    [Test]
    public void TestIntMaxValueQuantifierBoundsProduceValidPatterns()
    {
        using var exactlyMax = RegexBuilder.Create().Digit().Exactly(int.MaxValue);
        exactlyMax.GetPattern().Should().Be($@"\d{{{int.MaxValue}}}");
        exactlyMax.Build().IsMatch("1").Should().BeFalse();
        exactlyMax.Compiled().Build().IsMatch("1").Should().BeFalse();

        using var atLeastMax = RegexBuilder.Create().Digit().AtLeast(int.MaxValue);
        atLeastMax.GetPattern().Should().Be($@"\d{{{int.MaxValue},}}");
        atLeastMax.Build().IsMatch("1").Should().BeFalse();

        using var betweenMax = RegexBuilder.Create().Digit().Between(0, int.MaxValue);
        betweenMax.GetPattern().Should().Be($@"\d{{0,{int.MaxValue}}}");
        betweenMax.Build().IsMatch("1").Should().BeTrue();
    }

    [Test]
    public void TestMultiCharLiteralQuantifierDoesNotCreateCaptureGroup()
    {
        var regex = RegexBuilder.Create()
            .Literal("ab").OneOrMore()
            .Group(b => b.Digit().OneOrMore())
            .Build();

        var match = regex.Match("abab42");
        match.Success.Should().BeTrue();
        match.Groups.Count.Should().Be(2);
        match.Groups[1].Value.Should().Be("42");
    }

    [TestCase(255)]
    [TestCase(512)]
    [TestCase(4096)]
    public void TestMultiCharLiteralQuantifierWrappingAcrossBufferBoundaries(int prefixLength)
    {
        string prefix = new string('x', prefixLength);
        using var builder = RegexBuilder.Create()
            .StartOfString()
            .Literal(prefix)
            .Literal("ab").OneOrMore()
            .EndOfString();

        var regex = builder.Build();
        regex.IsMatch(prefix + "abab").Should().BeTrue();
        regex.IsMatch(prefix + "abb").Should().BeFalse();
    }

    [Test]
    public void TestOptionCallBetweenQuantifierAndLazyPreservesLazyModifier()
    {
        using var builder = RegexBuilder.Create()
            .Literal("<")
            .AnyChar().OneOrMore()
            .IgnoreCase()
            .Lazy()
            .Literal(">");

        builder.GetPattern().Should().Be("<.+?>");
        builder.Build().Match("<a><b>").Value.Should().Be("<a>");
    }

    [Test]
    public void TestZeroRepetitionBoundsWithLazyAndCompiled()
    {
        using var exactlyZeroLazy = RegexBuilder.Create().StartOfString().Digit().Exactly(0).Lazy().EndOfString().Compiled();
        exactlyZeroLazy.GetPattern().Should().Be(@"^\d{0}?$");
        exactlyZeroLazy.Build().IsMatch("").Should().BeTrue();
        exactlyZeroLazy.Build().IsMatch("1").Should().BeFalse();

        using var atLeastZeroLazy = RegexBuilder.Create().Digit().AtLeast(0).Lazy().Compiled();
        atLeastZeroLazy.GetPattern().Should().Be(@"\d{0,}?");
        atLeastZeroLazy.Build().Match("123").Value.Should().Be("");

        using var betweenZeroZeroLazy = RegexBuilder.Create().StartOfString().Digit().Between(0, 0).Lazy().EndOfString().Compiled();
        betweenZeroZeroLazy.GetPattern().Should().Be(@"^\d{0,0}?$");
        betweenZeroZeroLazy.Build().IsMatch("").Should().BeTrue();

        using var multiCharZero = RegexBuilder.Create().StartOfString().Literal("ab").Exactly(0).EndOfString();
        multiCharZero.GetPattern().Should().Be(@"^(?:ab){0}$");
        multiCharZero.Build().IsMatch("").Should().BeTrue();
        multiCharZero.Build().IsMatch("ab").Should().BeFalse();
    }

    [Test]
    public void TestInspectionAndBuildMethodsPreserveQuantifierAndMultiCharLiteralState()
    {
        using var multiCharBuilder = RegexBuilder.Create().Literal("ab");
        multiCharBuilder.GetPattern().Should().Be("ab");
        multiCharBuilder.GetOptions().Should().Be(RegexOptions.None);
        multiCharBuilder.ToString().Should().Be("/ab/None");
        multiCharBuilder.Build().IsMatch("ab").Should().BeTrue();
        multiCharBuilder.Build(TimeSpan.FromSeconds(1)).IsMatch("ab").Should().BeTrue();

        multiCharBuilder.OneOrMore();
        multiCharBuilder.GetPattern().Should().Be("(?:ab)+");
        multiCharBuilder.Build().IsMatch("abab").Should().BeTrue();

        using var lazyBuilder = RegexBuilder.Create().Digit().OneOrMore();
        lazyBuilder.GetPattern().Should().Be(@"\d+");
        lazyBuilder.Build().Match("123").Value.Should().Be("123");

        lazyBuilder.Lazy();
        lazyBuilder.GetPattern().Should().Be(@"\d+?");
        lazyBuilder.Build().Match("123").Value.Should().Be("1");
    }

    [Test]
    public void TestQuantifierAndLazyInsideScopedCallbackDoNotLeakOuterQuantifiableState()
    {
        using var quantifierBuilder = RegexBuilder.Create().Digit();
        quantifierBuilder.Invoking(b => b.Group(inner => inner.OneOrMore()))
            .Should().Throw<InvalidOperationException>();
        quantifierBuilder.GetPattern().Should().Be(@"\d");
        quantifierBuilder.OneOrMore().GetPattern().Should().Be(@"\d+");

        using var lazyBuilder = RegexBuilder.Create().Digit().OneOrMore();
        lazyBuilder.Invoking(b => b.Group(inner => inner.Lazy()))
            .Should().Throw<InvalidOperationException>();
        lazyBuilder.GetPattern().Should().Be(@"\d+");
        lazyBuilder.Lazy().GetPattern().Should().Be(@"\d+?");
    }
}
