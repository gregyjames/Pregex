namespace PregexTests;

public class AnchorTests
{
    [Test]
    public void TestStartOfString()
    {
        var regex = RegexBuilder.Create().StartOfString().Literal("test").Build();
        regex.IsMatch("test").Should().BeTrue();
        regex.IsMatch("test123").Should().BeTrue();
        regex.IsMatch("123test").Should().BeFalse();
    }

    [Test]
    public void TestEndOfString()
    {
        var regex = RegexBuilder.Create().Literal("test").EndOfString().Build();
        regex.IsMatch("test").Should().BeTrue();
        regex.IsMatch("123test").Should().BeTrue();
        regex.IsMatch("test123").Should().BeFalse();
    }

    [Test]
    public void TestWordBoundary()
    {
        var regex = RegexBuilder.Create()
            .WordBoundary()
            .Literal("test")
            .WordBoundary()
            .Build();
        regex.IsMatch("test").Should().BeTrue();
        regex.IsMatch("a test b").Should().BeTrue();
        regex.IsMatch("testing").Should().BeFalse();
        regex.IsMatch("atestb").Should().BeFalse();
    }

    [Test]
    public void TestNotWordBoundary()
    {
        var regex = RegexBuilder.Create()
            .Literal("test")
            .NotWordBoundary()
            .Build();

        regex.IsMatch("testing").Should().BeTrue();
        regex.IsMatch("test123").Should().BeTrue();

        regex.IsMatch("test ").Should().BeFalse();
        regex.IsMatch("test.").Should().BeFalse();

        var regex2 = RegexBuilder.Create()
            .NotWordBoundary()
            .Literal("ing")
            .Build();

        // Should match "ing" when it's in the middle of a word
        regex2.IsMatch("testing").Should().BeTrue();
        regex2.IsMatch("running").Should().BeTrue();

        // Should NOT match "ing" at the start
        regex2.IsMatch("ing").Should().BeFalse();
    }
}
