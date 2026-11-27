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

    [Test]
    public void TestStartAndEndOfStringWithEmptyInputAndMultilineLineEndings()
    {
        var emptyOnly = RegexBuilder.Create().StartOfString().EndOfString().Build();
        emptyOnly.IsMatch("").Should().BeTrue();
        emptyOnly.IsMatch("a").Should().BeFalse();
        emptyOnly.IsMatch("\n").Should().BeTrue(); // .NET $ matches before a trailing '\n'
        emptyOnly.IsMatch("\r").Should().BeFalse();
        emptyOnly.IsMatch("\n\n").Should().BeFalse();

        var multilineLine = RegexBuilder.Create()
            .StartOfString()
            .Literal("line2")
            .EndOfString()
            .Multiline()
            .Build();
        multilineLine.IsMatch("line1\nline2\nline3").Should().BeTrue();
        multilineLine.IsMatch("line1\nline2x\nline3").Should().BeFalse();
        multilineLine.IsMatch("line1\r\nline2\r\nline3").Should().BeFalse();
    }

    [Test]
    public void TestWordBoundaryAndNotWordBoundaryWithUnderscoresDigitsPunctuationAndEmptyInput()
    {
        var wholeWord = RegexBuilder.Create()
            .WordBoundary()
            .Literal("v2_0")
            .WordBoundary()
            .Build();
        wholeWord.IsMatch("v2_0").Should().BeTrue();
        wholeWord.IsMatch("release (v2_0)!").Should().BeTrue();
        wholeWord.IsMatch("av2_0").Should().BeFalse();
        wholeWord.IsMatch("v2_0b").Should().BeFalse();
        wholeWord.IsMatch("v2_0_1").Should().BeFalse();
        wholeWord.IsMatch("").Should().BeFalse();

        var interior = RegexBuilder.Create()
            .NotWordBoundary()
            .Literal("_")
            .NotWordBoundary()
            .Build();
        interior.IsMatch("a_b").Should().BeTrue();
        interior.IsMatch("1_2").Should().BeTrue();
        interior.IsMatch("_").Should().BeFalse();
        interior.IsMatch(" _ ").Should().BeFalse();
        interior.IsMatch("").Should().BeFalse();
    }
}
