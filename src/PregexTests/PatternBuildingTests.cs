namespace PregexTests;

public class PatternBuildingTests
{
    [Test]
    public void TestGetOptions()
    {
        var builder = RegexBuilder.Create()
            .Digit().OneOrMore()
            .IgnoreCase()
            .Multiline();

        var options = builder.GetOptions();
        options.Should().HaveFlag(RegexOptions.IgnoreCase);
        options.Should().HaveFlag(RegexOptions.Multiline);
    }

    [Test]
    public void TestGetOptionsNoFlags()
    {
        var builder = RegexBuilder.Create()
            .Digit().OneOrMore();

        var options = builder.GetOptions();
        options.Should().Be(RegexOptions.None);
    }

    [Test]
    public void TestGetOptionsMultipleFlags()
    {
        var builder = RegexBuilder.Create()
            .Word().OneOrMore()
            .IgnoreCase()
            .Multiline()
            .Compiled()
            .Singleline();

        var options = builder.GetOptions();
        options.Should().HaveFlag(RegexOptions.IgnoreCase);
        options.Should().HaveFlag(RegexOptions.Multiline);
        options.Should().HaveFlag(RegexOptions.Compiled);
        options.Should().HaveFlag(RegexOptions.Singleline);
    }

    [Test]
    public void TestToString()
    {
        var builder = RegexBuilder.Create()
            .StartOfString()
            .Digit().OneOrMore()
            .EndOfString();

        var result = builder.ToString();
        result.Should().StartWith("/");
        result.Should().Contain(@"^\d+$");
        result.Should().EndWith("/None");
    }

    [Test]
    public void TestToStringWithOptions()
    {
        var builder = RegexBuilder.Create()
            .Word().OneOrMore()
            .IgnoreCase()
            .Multiline();

        var result = builder.ToString();
        result.Should().StartWith("/");
        result.Should().Contain(@"\w+");
        result.Should().Contain("IgnoreCase");
        result.Should().Contain("Multiline");
    }

    [Test]
    public void TestGetPattern()
    {
        var builder = RegexBuilder.Create()
            .StartOfString()
            .Digit().OneOrMore()
            .EndOfString();
        builder.GetPattern().Should().Be(@"^\d+$");
    }

    [Test]
    public void TestComplexPattern()
    {
        var builder = RegexBuilder.Create()
            .Literal("http")
            .Literal("s").Optional()
            .Literal("://")
            .Word().OneOrMore();
        builder.GetPattern().Should().Be(@"https?://\w+");
    }

    [Test]
    public void TestBuildWithMatchTimeout()
    {
        using var builder = RegexBuilder.Create().Digit().OneOrMore();

        var timed = builder.Build(TimeSpan.FromSeconds(2));
        timed.MatchTimeout.Should().Be(TimeSpan.FromSeconds(2));
        timed.IsMatch("123").Should().BeTrue();

        var infinite = builder.Build(Regex.InfiniteMatchTimeout);
        infinite.MatchTimeout.Should().Be(Regex.InfiniteMatchTimeout);

        builder.Invoking(b => b.Build(TimeSpan.Zero)).Should().Throw<ArgumentOutOfRangeException>();
        builder.Invoking(b => b.Build(TimeSpan.FromMilliseconds(-2))).Should().Throw<ArgumentOutOfRangeException>();
        builder.Invoking(b => b.Build(TimeSpan.MinValue)).Should().Throw<ArgumentOutOfRangeException>();
        builder.Invoking(b => b.Build(TimeSpan.MaxValue)).Should().Throw<ArgumentOutOfRangeException>();

        var smallestPositive = builder.Build(TimeSpan.FromTicks(1));
        smallestPositive.MatchTimeout.Should().Be(TimeSpan.FromTicks(1));

        var maxSupported = builder.Build(TimeSpan.FromMilliseconds(int.MaxValue - 1));
        maxSupported.MatchTimeout.Should().Be(TimeSpan.FromMilliseconds(int.MaxValue - 1));

        builder.Invoking(b => b.Build(TimeSpan.FromMilliseconds(int.MaxValue))).Should().Throw<ArgumentOutOfRangeException>();
        builder.Invoking(b => b.Build(TimeSpan.FromTicks(-1))).Should().Throw<ArgumentOutOfRangeException>();

        var equivalentInfinite = builder.Build(TimeSpan.FromMilliseconds(-1));
        equivalentInfinite.MatchTimeout.Should().Be(Regex.InfiniteMatchTimeout);
    }

    [Test]
    public void TestEmptyBuilderMetadata()
    {
        using var builder = RegexBuilder.Create();
        builder.GetPattern().Should().Be("");
        builder.GetOptions().Should().Be(RegexOptions.None);
        builder.ToString().Should().Be("//None");
    }

    [Test]
    public void TestMultipleBuildsAndChainingAfterBuild()
    {
        using var builder = RegexBuilder.Create().Digit().OneOrMore();

        var first = builder.Build();
        first.IsMatch("123").Should().BeTrue();
        first.IsMatch("123abc").Should().BeTrue();

        builder.Literal("abc").EndOfString();
        var second = builder.Build();
        second.IsMatch("123abc").Should().BeTrue();
        second.IsMatch("123").Should().BeFalse();

        builder.GetPattern().Should().Be(@"\d+abc$");
        builder.ToString().Should().Be(@"/\d+abc$/None");
    }

    [Test]
    public void TestFailedBuildWithInvalidTimeoutPreservesBuilderAndMultiCharState()
    {
        using var builder = RegexBuilder.Create().Literal("ab").IgnoreCase();

        builder.Invoking(b => b.Build(TimeSpan.Zero)).Should().Throw<ArgumentOutOfRangeException>();
        builder.Invoking(b => b.Build(TimeSpan.FromMilliseconds(-2))).Should().Throw<ArgumentOutOfRangeException>();
        builder.Invoking(b => b.Build(TimeSpan.MaxValue)).Should().Throw<ArgumentOutOfRangeException>();

        builder.GetPattern().Should().Be("ab");
        builder.GetOptions().Should().Be(RegexOptions.IgnoreCase);
        builder.OneOrMore().GetPattern().Should().Be("(?:ab)+");
        builder.Build(TimeSpan.FromSeconds(1)).IsMatch("ABab").Should().BeTrue();
    }

    [Test]
    public void TestBuildWithShortTimeoutThrowsRegexMatchTimeoutExceptionOnBacktrackingInput()
    {
        using var builder = RegexBuilder.Create()
            .StartOfString()
            .Group(b => b.Literal("a").OneOrMore()).OneOrMore()
            .Literal("b")
            .EndOfString();

        var regex = builder.Build(TimeSpan.FromMilliseconds(10));
        string pathological = new string('a', 30) + "c";

        Action act = () => _ = regex.IsMatch(pathological);
        act.Should().Throw<RegexMatchTimeoutException>();
    }

    [Test]
    public void TestReadmeUtilityMethodsSnippetMatchesDocumentedOutputs()
    {
        using var builder = RegexBuilder.Create()
            .StartOfString()
            .Digit().OneOrMore()
            .EndOfString()
            .IgnoreCase()
            .Multiline();

        builder.GetPattern().Should().Be(@"^\d+$");
        builder.GetOptions().Should().Be(RegexOptions.IgnoreCase | RegexOptions.Multiline);
        builder.ToString().Should().Be(@"/^\d+$/IgnoreCase, Multiline");
        builder.Build().IsMatch("123").Should().BeTrue();
        builder.Build(TimeSpan.FromSeconds(2)).MatchTimeout.Should().Be(TimeSpan.FromSeconds(2));
    }
}
