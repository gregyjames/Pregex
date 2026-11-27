namespace PregexTests;

public class CharacterSetTests
{
    [Test]
    public void TestCharacterSet()
    {
        var regex = RegexBuilder.Create().Set("abc").Build();
        regex.IsMatch("a").Should().BeTrue();
        regex.IsMatch("b").Should().BeTrue();
        regex.IsMatch("c").Should().BeTrue();
        regex.IsMatch("d").Should().BeFalse();
    }

    [Test]
    public void TestNotCharacterSet()
    {
        var regex = RegexBuilder.Create().NotSet("abc").Build();
        regex.IsMatch("a").Should().BeFalse();
        regex.IsMatch("b").Should().BeFalse();
        regex.IsMatch("c").Should().BeFalse();
        regex.IsMatch("d").Should().BeTrue();
    }

    [Test]
    public void TestRange()
    {
        var regex = RegexBuilder.Create().Range('a', 'z').Build();
        regex.IsMatch("a").Should().BeTrue();
        regex.IsMatch("m").Should().BeTrue();
        regex.IsMatch("z").Should().BeTrue();
        regex.IsMatch("A").Should().BeFalse();
        regex.IsMatch("5").Should().BeFalse();
    }

    [Test]
    public void TestSetDoesNotOverEscapeCharacterClassLiterals()
    {
        using var builder = RegexBuilder.Create().Set(".*+?()$ ");
        builder.GetPattern().Should().Be("[.*+?()$ ]");

        var regex = builder.Build();
        regex.IsMatch(".").Should().BeTrue();
        regex.IsMatch(" ").Should().BeTrue();
        regex.IsMatch("$").Should().BeTrue();
        regex.IsMatch("a").Should().BeFalse();
    }

    [Test]
    public void TestSetPreservesEscapedLiteralHyphen()
    {
        using var builder = RegexBuilder.Create().Set(@"a\-z");
        builder.GetPattern().Should().Be(@"[a\-z]");

        var regex = builder.Build();
        regex.IsMatch("a").Should().BeTrue();
        regex.IsMatch("-").Should().BeTrue();
        regex.IsMatch("z").Should().BeTrue();
        regex.IsMatch("m").Should().BeFalse();
    }

    [Test]
    public void TestSetSupportsRanges()
    {
        using var builder = RegexBuilder.Create().Set("a-z0-9");
        builder.GetPattern().Should().Be("[a-z0-9]");

        var regex = builder.Build();
        regex.IsMatch("m").Should().BeTrue();
        regex.IsMatch("5").Should().BeTrue();
        regex.IsMatch("-").Should().BeFalse();
    }

    [Test]
    public void TestSetEscapesBracketsCaretAndTrailingBackslash()
    {
        using var caretBuilder = RegexBuilder.Create().Set("^abc");
        caretBuilder.GetPattern().Should().Be(@"[\^abc]");
        var caretRegex = caretBuilder.Build();
        caretRegex.IsMatch("^").Should().BeTrue();
        caretRegex.IsMatch("a").Should().BeTrue();
        caretRegex.IsMatch("d").Should().BeFalse();

        using var bracketBuilder = RegexBuilder.Create().Set("a]b[c");
        bracketBuilder.GetPattern().Should().Be(@"[a\]b\[c]");
        var bracketRegex = bracketBuilder.Build();
        bracketRegex.IsMatch("]").Should().BeTrue();
        bracketRegex.IsMatch("[").Should().BeTrue();

        using var backslashBuilder = RegexBuilder.Create().Set(@"\");
        backslashBuilder.GetPattern().Should().Be(@"[\\]");
        backslashBuilder.Build().IsMatch(@"\").Should().BeTrue();
    }

    [Test]
    public void TestNotSetEscapesSpecialCharactersWithoutOverEscaping()
    {
        using var builder = RegexBuilder.Create().NotSet(" /]^");
        builder.GetPattern().Should().Be(@"[^ /\]\^]");

        var regex = builder.Build();
        regex.IsMatch("a").Should().BeTrue();
        regex.IsMatch(" ").Should().BeFalse();
        regex.IsMatch("/").Should().BeFalse();
        regex.IsMatch("]").Should().BeFalse();
        regex.IsMatch("^").Should().BeFalse();
    }

    [Test]
    public void TestSetAndNotSetThrowOnNullOrEmpty()
    {
        using var builder = RegexBuilder.Create();

        builder.Invoking(b => b.Set("")).Should().Throw<ArgumentException>();
        builder.Invoking(b => b.NotSet("")).Should().Throw<ArgumentException>();
        builder.Invoking(b => b.Set(null!)).Should().Throw<ArgumentNullException>();
        builder.Invoking(b => b.NotSet(null!)).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void TestNotSetPreservesEscapedLiteralHyphen()
    {
        using var builder = RegexBuilder.Create().NotSet(@"a\-z");
        builder.GetPattern().Should().Be(@"[^a\-z]");

        var regex = builder.Build();
        regex.IsMatch("m").Should().BeTrue();
        regex.IsMatch("a").Should().BeFalse();
        regex.IsMatch("-").Should().BeFalse();
        regex.IsMatch("z").Should().BeFalse();
    }

    [Test]
    public void TestSetEscapesControlCharacters()
    {
        using var builder = RegexBuilder.Create().Set("\t\n\r\f\v");
        builder.GetPattern().Should().Be(@"[\t\n\r\f\v]");

        var regex = builder.Build();
        regex.IsMatch("\t").Should().BeTrue();
        regex.IsMatch("\n").Should().BeTrue();
        regex.IsMatch("\r").Should().BeTrue();
        regex.IsMatch("\f").Should().BeTrue();
        regex.IsMatch("\v").Should().BeTrue();
        regex.IsMatch(" ").Should().BeFalse();
    }

    [Test]
    public void TestSetHandlesEscapedBackslashesAndShorthands()
    {
        using var escapedBackslash = RegexBuilder.Create().Set(@"\\");
        escapedBackslash.GetPattern().Should().Be(@"[\\]");
        escapedBackslash.Build().IsMatch(@"\").Should().BeTrue();

        using var oddBackslashes = RegexBuilder.Create().Set(@"\\\");
        oddBackslashes.GetPattern().Should().Be(@"[\\\\]");
        oddBackslashes.Build().IsMatch(@"\").Should().BeTrue();

        using var shorthand = RegexBuilder.Create().Set(@"\d_");
        shorthand.GetPattern().Should().Be(@"[\d_]");
        var shorthandRegex = shorthand.Build();
        shorthandRegex.IsMatch("5").Should().BeTrue();
        shorthandRegex.IsMatch("_").Should().BeTrue();
        shorthandRegex.IsMatch("a").Should().BeFalse();
    }

    [Test]
    public void TestRangeEscapesSpecialBoundaryCharacters()
    {
        using var caretRange = RegexBuilder.Create().Range('^', 'z');
        caretRange.GetPattern().Should().Be(@"[\^-z]");
        var caretRegex = caretRange.Build();
        caretRegex.IsMatch("^").Should().BeTrue();
        caretRegex.IsMatch("a").Should().BeTrue();
        caretRegex.IsMatch("z").Should().BeTrue();
        caretRegex.IsMatch("A").Should().BeFalse();

        using var backslashRange = RegexBuilder.Create().Range('\\', 'z');
        backslashRange.GetPattern().Should().Be(@"[\\-z]");
        var backslashRegex = backslashRange.Build();
        backslashRegex.IsMatch(@"\").Should().BeTrue();
        backslashRegex.IsMatch("a").Should().BeTrue();
        backslashRegex.IsMatch("-").Should().BeFalse();

        using var bracketRange = RegexBuilder.Create().Range('[', ']');
        bracketRange.GetPattern().Should().Be(@"[\[-\]]");
        var bracketRegex = bracketRange.Build();
        bracketRegex.IsMatch("[").Should().BeTrue();
        bracketRegex.IsMatch(@"\").Should().BeTrue();
        bracketRegex.IsMatch("]").Should().BeTrue();
        bracketRegex.IsMatch("a").Should().BeFalse();

        using var hyphenStartRange = RegexBuilder.Create().Range('-', '0');
        hyphenStartRange.GetPattern().Should().Be(@"[\x2D-0]");
        var hyphenStartRegex = hyphenStartRange.Build();
        hyphenStartRegex.IsMatch("-").Should().BeTrue();
        hyphenStartRegex.IsMatch(".").Should().BeTrue();
        hyphenStartRegex.IsMatch("0").Should().BeTrue();
        hyphenStartRegex.IsMatch("1").Should().BeFalse();

        using var hyphenEndRange = RegexBuilder.Create().Range('+', '-');
        hyphenEndRange.GetPattern().Should().Be(@"[+-\x2D]");
        var hyphenEndRegex = hyphenEndRange.Build();
        hyphenEndRegex.IsMatch("+").Should().BeTrue();
        hyphenEndRegex.IsMatch(",").Should().BeTrue();
        hyphenEndRegex.IsMatch("-").Should().BeTrue();
        hyphenEndRegex.IsMatch(".").Should().BeFalse();
    }

    [Test]
    public void TestRangeThrowsWhenStartGreaterThanEnd()
    {
        using var builder = RegexBuilder.Create();
        builder.Invoking(b => b.Range('z', 'a')).Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void TestSetAndNotSetEscapeNullCharFollowedByDigits()
    {
        using var setBuilder = RegexBuilder.Create().Set("\00");
        setBuilder.GetPattern().Should().Be(@"[\x000]");
        var setRegex = setBuilder.Build();
        setRegex.IsMatch("\0").Should().BeTrue();
        setRegex.IsMatch("0").Should().BeTrue();
        setRegex.IsMatch("1").Should().BeFalse();

        using var notSetBuilder = RegexBuilder.Create().NotSet("\00");
        notSetBuilder.GetPattern().Should().Be(@"[^\x000]");
        var notSetRegex = notSetBuilder.Build();
        notSetRegex.IsMatch("\0").Should().BeFalse();
        notSetRegex.IsMatch("0").Should().BeFalse();
        notSetRegex.IsMatch("a").Should().BeTrue();
    }

    [TestCase("z-a")]
    [TestCase("a-[")]
    [TestCase("a-]")]
    [TestCase("a-^")]
    [TestCase(@"a-\d")]
    [TestCase(@"\q")]
    [TestCase(@"\x")]
    [TestCase(@"\u12")]
    [TestCase(@"\p")]
    [TestCase(@"\c")]
    public void TestSetAndNotSetInvalidSpecsThrowArgumentExceptionWithoutCorruptingBuilder(string invalidChars)
    {
        using var builder = RegexBuilder.Create().Literal("prefix");
        builder.Invoking(b => b.Set(invalidChars)).Should().Throw<ArgumentException>();
        builder.GetPattern().Should().Be("prefix");

        builder.Invoking(b => b.NotSet(invalidChars)).Should().Throw<ArgumentException>();
        builder.GetPattern().Should().Be("prefix");

        builder.Digit().Build().IsMatch("prefix5").Should().BeTrue();
    }

    [Test]
    public void TestSetHandlesLeadingTrailingSingleAndTripleHyphens()
    {
        using var leading = RegexBuilder.Create().Set("-abc");
        leading.Build().IsMatch("-").Should().BeTrue();
        leading.Build().IsMatch("a").Should().BeTrue();

        using var trailing = RegexBuilder.Create().Set("abc-");
        trailing.Build().IsMatch("-").Should().BeTrue();
        trailing.Build().IsMatch("c").Should().BeTrue();

        using var single = RegexBuilder.Create().Set("-");
        single.Build().IsMatch("-").Should().BeTrue();
        single.Build().IsMatch("a").Should().BeFalse();

        using var triple = RegexBuilder.Create().Set("---");
        triple.Build().IsMatch("-").Should().BeTrue();
    }

    [Test]
    public void TestRangeBoundaryCases()
    {
        using var sameChar = RegexBuilder.Create().Range('a', 'a');
        sameChar.GetPattern().Should().Be("[a-a]");
        sameChar.Build().IsMatch("a").Should().BeTrue();
        sameChar.Build().IsMatch("b").Should().BeFalse();

        using var hyphenOnly = RegexBuilder.Create().Range('-', '-');
        hyphenOnly.GetPattern().Should().Be(@"[\x2D-\x2D]");
        hyphenOnly.Build().IsMatch("-").Should().BeTrue();
        hyphenOnly.Build().IsMatch("a").Should().BeFalse();

        using var controlRange = RegexBuilder.Create().Range('\0', '\x1F');
        controlRange.GetPattern().Should().Be(@"[\x00-" + "\x1F]");
        controlRange.Build().IsMatch("\0").Should().BeTrue();
        controlRange.Build().IsMatch("\t").Should().BeTrue();
        controlRange.Build().IsMatch(" ").Should().BeFalse();

        using var fullRange = RegexBuilder.Create().Range('\0', '\uFFFF');
        fullRange.Build().IsMatch("\0").Should().BeTrue();
        fullRange.Build().IsMatch("\uFFFF").Should().BeTrue();
    }

    [Test]
    public void TestSetNotSetAndRangeWithIgnorePatternWhitespace()
    {
        using var setRegex = RegexBuilder.Create().Set("a #").IgnorePatternWhitespace();
        setRegex.Build().IsMatch(" ").Should().BeTrue();
        setRegex.Build().IsMatch("#").Should().BeTrue();
        setRegex.Build().IsMatch("a").Should().BeTrue();

        using var notSetRegex = RegexBuilder.Create().NotSet(" #").IgnorePatternWhitespace();
        notSetRegex.Build().IsMatch(" ").Should().BeFalse();
        notSetRegex.Build().IsMatch("#").Should().BeFalse();
        notSetRegex.Build().IsMatch("a").Should().BeTrue();

        using var rangeRegex = RegexBuilder.Create().Range('\t', '\r').IgnorePatternWhitespace();
        rangeRegex.Build().IsMatch("\n").Should().BeTrue();
        rangeRegex.Build().IsMatch("\v").Should().BeTrue();
        rangeRegex.Build().IsMatch("a").Should().BeFalse();
    }

    [Test]
    public void TestSetAndNotSetEscapeNullCharFollowedByHexDigits()
    {
        using var setBuilder = RegexBuilder.Create().Set("\0fA");
        setBuilder.GetPattern().Should().Be(@"[\x00fA]");
        var setRegex = setBuilder.Build();
        setRegex.IsMatch("\0").Should().BeTrue();
        setRegex.IsMatch("f").Should().BeTrue();
        setRegex.IsMatch("A").Should().BeTrue();
        setRegex.IsMatch("a").Should().BeFalse();

        using var notSetBuilder = RegexBuilder.Create().NotSet("\0fA");
        notSetBuilder.GetPattern().Should().Be(@"[^\x00fA]");
        var notSetRegex = notSetBuilder.Build();
        notSetRegex.IsMatch("\0").Should().BeFalse();
        notSetRegex.IsMatch("f").Should().BeFalse();
        notSetRegex.IsMatch("A").Should().BeFalse();
        notSetRegex.IsMatch("b").Should().BeTrue();
    }

    [Test]
    public void TestRangeNullBoundaryAndInvertedHyphenThrows()
    {
        using var nullOnly = RegexBuilder.Create().Range('\0', '\0');
        nullOnly.GetPattern().Should().Be(@"[\x00-\x00]");
        nullOnly.Build().IsMatch("\0").Should().BeTrue();
        nullOnly.Build().IsMatch("a").Should().BeFalse();

        nullOnly.Invoking(b => b.Range('a', '-')).Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void TestSetAndNotSetEscapeBackslashFollowedByControlCharacters()
    {
        using var setBuilder = RegexBuilder.Create().Set("\\\0\\\t\\\n\\\r\\\f\\\v");
        setBuilder.GetPattern().Should().Be(@"[\x00\t\n\r\f\v]");
        var setRegex = setBuilder.Build();
        setRegex.IsMatch("\0").Should().BeTrue();
        setRegex.IsMatch("\t").Should().BeTrue();
        setRegex.IsMatch("\n").Should().BeTrue();
        setRegex.IsMatch("\r").Should().BeTrue();
        setRegex.IsMatch("\f").Should().BeTrue();
        setRegex.IsMatch("\v").Should().BeTrue();
        setRegex.IsMatch(@"\").Should().BeFalse();

        using var notSetBuilder = RegexBuilder.Create().NotSet("\\\0\\\n");
        notSetBuilder.GetPattern().Should().Be(@"[^\x00\n]");
        var notSetRegex = notSetBuilder.Build();
        notSetRegex.IsMatch("\0").Should().BeFalse();
        notSetRegex.IsMatch("\n").Should().BeFalse();
        notSetRegex.IsMatch(@"\").Should().BeTrue();
    }

    [Test]
    public void TestSetSupportsUnicodePropertiesHexEscapesAndMultiRanges()
    {
        var unicodeProp = RegexBuilder.Create().Set(@"\p{Lu}").Build();
        unicodeProp.IsMatch("A").Should().BeTrue();
        unicodeProp.IsMatch("a").Should().BeFalse();

        var hexRange = RegexBuilder.Create().Set(@"\x41-\x5A").Build();
        hexRange.IsMatch("M").Should().BeTrue();
        hexRange.IsMatch("m").Should().BeFalse();

        var multiRange = RegexBuilder.Create().Set("a-b-c").Build();
        multiRange.IsMatch("a").Should().BeTrue();
        multiRange.IsMatch("-").Should().BeTrue();
        multiRange.IsMatch("c").Should().BeTrue();
        multiRange.IsMatch("d").Should().BeFalse();
    }

    [Test]
    public void TestRangeSurrogateBoundsWithCompiled()
    {
        var regex = RegexBuilder.Create()
            .Range('\uD800', '\uDFFF')
            .Compiled()
            .Build();

        regex.IsMatch("\uD800").Should().BeTrue();
        regex.IsMatch("\uDFFF").Should().BeTrue();
        regex.IsMatch("a").Should().BeFalse();
    }

    [TestCase(@"\c[", @"[\c[]", "\x1B", "[")]
    [TestCase(@"\c\", @"[\c\]", "\x1C", @"\")]
    [TestCase(@"\c]", @"[\c]]", "\x1D", "]")]
    [TestCase(@"\c^", @"[\c^]", "\x1E", "^")]
    public void TestSetAndNotSetPreserveControlEscapesWithMetacharacterDesignators(
        string spec,
        string expectedInnerPattern,
        string expectedMatch,
        string literalChar)
    {
        using var setBuilder = RegexBuilder.Create().StartOfString().Set(spec).EndOfString();
        setBuilder.GetPattern().Should().Be("^" + expectedInnerPattern + "$");
        var setRegex = setBuilder.Build();
        setRegex.IsMatch(expectedMatch).Should().BeTrue();
        setRegex.IsMatch(literalChar).Should().BeFalse();
        setRegex.IsMatch("\x1C").Should().Be(expectedMatch == "\x1C");

        using var notSetBuilder = RegexBuilder.Create().StartOfString().NotSet(spec).EndOfString();
        notSetBuilder.GetPattern().Should().Be("^[^" + expectedInnerPattern[1..] + "$");
        var notSetRegex = notSetBuilder.Build();
        notSetRegex.IsMatch(expectedMatch).Should().BeFalse();
        notSetRegex.IsMatch(literalChar).Should().BeTrue();
    }

    [Test]
    public void TestSetHandlesControlEscapesCombinedWithLiteralsBackslashesAndRanges()
    {
        using var gsAndBracket = RegexBuilder.Create().Set(@"\c]]");
        gsAndBracket.GetPattern().Should().Be(@"[\c]\]]");
        var gsAndBracketRegex = gsAndBracket.Build();
        gsAndBracketRegex.IsMatch("\x1D").Should().BeTrue();
        gsAndBracketRegex.IsMatch("]").Should().BeTrue();
        gsAndBracketRegex.IsMatch("a").Should().BeFalse();

        using var fsAndBackslash = RegexBuilder.Create().Set(@"\c\\");
        fsAndBackslash.GetPattern().Should().Be(@"[\c\\\]");
        var fsAndBackslashRegex = fsAndBackslash.Build();
        fsAndBackslashRegex.IsMatch("\x1C").Should().BeTrue();
        fsAndBackslashRegex.IsMatch(@"\").Should().BeTrue();
        fsAndBackslashRegex.IsMatch("a").Should().BeFalse();

        using var escapedBackslashBeforeC = RegexBuilder.Create().Set(@"\\c[");
        escapedBackslashBeforeC.GetPattern().Should().Be(@"[\\c\[]");
        var escapedBackslashRegex = escapedBackslashBeforeC.Build();
        escapedBackslashRegex.IsMatch(@"\").Should().BeTrue();
        escapedBackslashRegex.IsMatch("c").Should().BeTrue();
        escapedBackslashRegex.IsMatch("[").Should().BeTrue();
        escapedBackslashRegex.IsMatch("\x1B").Should().BeFalse();

        using var escapedBackslashThenControl = RegexBuilder.Create().Set(@"\\\c[");
        escapedBackslashThenControl.GetPattern().Should().Be(@"[\\\c[]");
        var escapedThenControlRegex = escapedBackslashThenControl.Build();
        escapedThenControlRegex.IsMatch(@"\").Should().BeTrue();
        escapedThenControlRegex.IsMatch("\x1B").Should().BeTrue();
        escapedThenControlRegex.IsMatch("[").Should().BeFalse();

        using var controlRange = RegexBuilder.Create().Set(@"\c[- ");
        controlRange.GetPattern().Should().Be(@"[\c[- ]");
        var controlRangeRegex = controlRange.Build();
        controlRangeRegex.IsMatch("\x1B").Should().BeTrue();
        controlRangeRegex.IsMatch("\x1F").Should().BeTrue();
        controlRangeRegex.IsMatch(" ").Should().BeTrue();
        controlRangeRegex.IsMatch("!").Should().BeFalse();

        using var invalidControl = RegexBuilder.Create().Literal("keep");
        invalidControl.Invoking(b => b.Set(@"\c1")).Should().Throw<ArgumentException>();
        invalidControl.Invoking(b => b.Set("\\c\0")).Should().Throw<ArgumentException>();
        invalidControl.Invoking(b => b.NotSet(@"\c-")).Should().Throw<ArgumentException>();
        invalidControl.GetPattern().Should().Be("keep");
    }

    [Test]
    public void TestRangeHandlesAllSpecialCharacterBoundaryCombinations()
    {
        char[] specialChars =
        [
            '\0', '\t', '\n', '\v', '\f', '\r', ' ', '#', '-', '[', '\\', ']', '^'
        ];

        for (int i = 0; i < specialChars.Length; i++)
        {
            for (int j = i; j < specialChars.Length; j++)
            {
                char start = specialChars[i];
                char end = specialChars[j];
                using var builder = RegexBuilder.Create().StartOfString().Range(start, end).EndOfString();
                var normal = builder.Build();
                normal.IsMatch(start.ToString()).Should().BeTrue();
                normal.IsMatch(end.ToString()).Should().BeTrue();

                var ignoreWs = builder.IgnorePatternWhitespace().Build();
                ignoreWs.IsMatch(start.ToString()).Should().BeTrue();
                ignoreWs.IsMatch(end.ToString()).Should().BeTrue();
            }
        }
    }
}
