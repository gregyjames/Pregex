namespace PregexTests;

public class GroupTests
{
    [Test]
    public void TestCapturingGroup()
    {
        var regex = RegexBuilder.Create()
            .Group(b => b.Digit().Exactly(3))
            .Literal("-")
            .Group(b => b.Digit().Exactly(4))
            .Build();
        var match = regex.Match("123-4567");
        match.Success.Should().BeTrue();
        match.Groups[1].Value.Should().Be("123");
        match.Groups[2].Value.Should().Be("4567");
    }

    [Test]
    public void TestNonCapturingGroup()
    {
        var regex = RegexBuilder.Create()
            .NonCapturingGroup(b => b.Literal("http").Literal("s").Optional())
            .Literal("://")
            .Build();
        var match = regex.Match("https://");
        match.Success.Should().BeTrue();
        match.Groups.Count.Should().Be(1); // Only the full match, no captured groups
    }

    [Test]
    public void TestNamedGroup()
    {
        var regex = RegexBuilder.Create()
            .NamedGroup("area", b => b.Digit().Exactly(3))
            .Literal("-")
            .NamedGroup("number", b => b.Digit().Exactly(4))
            .Build();
        var match = regex.Match("123-4567");
        match.Success.Should().BeTrue();
        match.Groups["area"].Value.Should().Be("123");
        match.Groups["number"].Value.Should().Be("4567");
    }
}
