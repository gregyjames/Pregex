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
}
