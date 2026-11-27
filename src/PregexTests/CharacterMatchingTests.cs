namespace PregexTests;

public class CharacterMatchingTests
{
    [Test]
    public void TestDigitMatching()
    {
        var regex = RegexBuilder.Create().Digit().Build();
        regex.IsMatch("5").Should().BeTrue();
        regex.IsMatch("a").Should().BeFalse();
    }

    [Test]
    public void TestWordMatching()
    {
        var regex = RegexBuilder.Create().Word().Build();
        regex.IsMatch("a").Should().BeTrue();
        regex.IsMatch("Z").Should().BeTrue();
        regex.IsMatch("5").Should().BeTrue();
        regex.IsMatch("_").Should().BeTrue();
        regex.IsMatch("!").Should().BeFalse();
    }

    [Test]
    public void TestWhitespaceMatching()
    {
        var regex = RegexBuilder.Create().Whitespace().Build();
        regex.IsMatch(" ").Should().BeTrue();
        regex.IsMatch("\t").Should().BeTrue();
        regex.IsMatch("\n").Should().BeTrue();
        regex.IsMatch("a").Should().BeFalse();
    }

    [Test]
    public void TestNotDigitMatching()
    {
        var regex = RegexBuilder.Create().NotDigit().Build();
        regex.IsMatch("a").Should().BeTrue();
        regex.IsMatch("5").Should().BeFalse();
    }

    [Test]
    public void TestAnyCharMatching()
    {
        var regex = RegexBuilder.Create().AnyChar().Build();
        regex.IsMatch("a").Should().BeTrue();
        regex.IsMatch("5").Should().BeTrue();
        regex.IsMatch("!").Should().BeTrue();
    }

    [Test]
    public void TestLiteralEscaping()
    {
        var regex = RegexBuilder.Create().Literal("test.com").Build();
        regex.IsMatch("test.com").Should().BeTrue();
        regex.IsMatch("testacom").Should().BeFalse();
    }

    [Test]
    public void TestNotWordMatching()
    {
        var regex = RegexBuilder.Create().NotWord().Build();
        regex.IsMatch("!").Should().BeTrue();
        regex.IsMatch("@").Should().BeTrue();
        regex.IsMatch(" ").Should().BeTrue();
        regex.IsMatch("a").Should().BeFalse();
        regex.IsMatch("Z").Should().BeFalse();
        regex.IsMatch("5").Should().BeFalse();
        regex.IsMatch("_").Should().BeFalse();
    }

    [Test]
    public void TestNotWhitespaceMatching()
    {
        var regex = RegexBuilder.Create().NotWhitespace().Build();
        regex.IsMatch("a").Should().BeTrue();
        regex.IsMatch("5").Should().BeTrue();
        regex.IsMatch("!").Should().BeTrue();
        regex.IsMatch(" ").Should().BeFalse();
        regex.IsMatch("\t").Should().BeFalse();
        regex.IsMatch("\n").Should().BeFalse();
    }
}
