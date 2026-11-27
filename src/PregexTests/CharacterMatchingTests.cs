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

    [Test]
    public void TestLiteralThrowsOnNullWithoutCorruptingBuilder()
    {
        using var builder = RegexBuilder.Create().Digit();
        builder.Invoking(b => b.Literal(null!)).Should().Throw<ArgumentNullException>();
        builder.GetPattern().Should().Be(@"\d");
    }

    [Test]
    public void TestLiteralEmptyStringIsNoOpAndResetsMultiCharState()
    {
        using var builder = RegexBuilder.Create().Literal("abc").Literal("");
        builder.GetPattern().Should().Be("abc");
        builder.Build().IsMatch("abc").Should().BeTrue();
    }

    [TestCase(@"\")]
    [TestCase(".")]
    [TestCase("*")]
    [TestCase("+")]
    [TestCase("?")]
    [TestCase("(")]
    [TestCase(")")]
    [TestCase("[")]
    [TestCase("]")]
    [TestCase("{")]
    [TestCase("}")]
    [TestCase("^")]
    [TestCase("$")]
    [TestCase("|")]
    [TestCase("#")]
    [TestCase(" ")]
    public void TestLiteralSingleMetacharacterMatchesLiterally(string ch)
    {
        using var builder = RegexBuilder.Create().StartOfString().Literal(ch).EndOfString();
        var regex = builder.Build();
        regex.IsMatch(ch).Should().BeTrue();
        if (ch != "x")
        {
            regex.IsMatch("x").Should().BeFalse();
        }
    }

    [Test]
    public void TestLiteralMultiCharWithMetacharacters()
    {
        using var builder = RegexBuilder.Create().StartOfString().Literal(@"a+b*c?(d)[e]{1}^$|#\").EndOfString();
        var regex = builder.Build();
        regex.IsMatch(@"a+b*c?(d)[e]{1}^$|#\").Should().BeTrue();
        regex.IsMatch("abcde1").Should().BeFalse();
    }

    [Test]
    public void TestLiteralSurrogatePairEmojiAndCombiningMarks()
    {
        using var emojiBuilder = RegexBuilder.Create().StartOfString().Literal("😀").OneOrMore().EndOfString();
        var emojiRegex = emojiBuilder.Build();
        emojiRegex.IsMatch("😀").Should().BeTrue();
        emojiRegex.IsMatch("😀😀").Should().BeTrue();
        emojiRegex.IsMatch("😃").Should().BeFalse();

        string combining = "e\u0301";
        using var combiningBuilder = RegexBuilder.Create().StartOfString().Literal(combining).EndOfString();
        var combiningRegex = combiningBuilder.Build();
        combiningRegex.IsMatch(combining).Should().BeTrue();
        combiningRegex.IsMatch("é").Should().BeFalse();
    }

    [Test]
    public void TestLiteralControlCharactersAndIgnorePatternWhitespace()
    {
        const string controls = "\0\t\n\r\f\v #";
        using var builder = RegexBuilder.Create()
            .StartOfString()
            .Literal(controls)
            .EndOfString()
            .IgnorePatternWhitespace();

        var regex = builder.Build();
        regex.IsMatch(controls).Should().BeTrue();
        regex.IsMatch("").Should().BeFalse();
    }

    [Test]
    public void TestLargeLiteralWithQuantifier()
    {
        string large = new string('a', 12000);
        using var builder = RegexBuilder.Create().StartOfString().Literal(large).Exactly(2).EndOfString();
        var regex = builder.Build();
        regex.IsMatch(large + large).Should().BeTrue();
        regex.IsMatch(large).Should().BeFalse();
    }

    [Test]
    public void TestLiteralNullCharFollowedByDigitsAndQuantifier()
    {
        using var literalNullZero = RegexBuilder.Create().StartOfString().Literal("\0").Literal("0").EndOfString();
        var nullZeroRegex = literalNullZero.Build();
        nullZeroRegex.IsMatch("\00").Should().BeTrue();
        nullZeroRegex.IsMatch("\0").Should().BeFalse();

        using var quantifiedNull = RegexBuilder.Create().StartOfString().Literal("\0").OneOrMore().EndOfString();
        var quantifiedRegex = quantifiedNull.Build();
        quantifiedRegex.IsMatch("\0").Should().BeTrue();
        quantifiedRegex.IsMatch("\0\0\0").Should().BeTrue();
        quantifiedRegex.IsMatch("a").Should().BeFalse();
    }
}
