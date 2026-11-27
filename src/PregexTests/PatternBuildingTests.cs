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
}
