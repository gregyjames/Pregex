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

    [Test]
    public void TestSetAndNotSetControlEscapesWithTrailingCharsAndCompiled()
    {
        using var nullControl = RegexBuilder.Create().Set(@"\c@");
        nullControl.GetPattern().Should().Be(@"[\c@]");
        nullControl.Build().IsMatch("\0").Should().BeTrue();
        nullControl.Compiled().Build().IsMatch("a").Should().BeFalse();

        using var usControl = RegexBuilder.Create().Set(@"\c_");
        usControl.GetPattern().Should().Be(@"[\c_]");
        usControl.Compiled().Build().IsMatch("\x1F").Should().BeTrue();

        using var gsAndEscapedBracket = RegexBuilder.Create().Set(@"\c\]").Compiled().IgnorePatternWhitespace();
        gsAndEscapedBracket.GetPattern().Should().Be(@"[\c\\]]");
        var gsRegex = gsAndEscapedBracket.Build();
        gsRegex.IsMatch("\x1C").Should().BeTrue();
        gsRegex.IsMatch("]").Should().BeTrue();
        gsRegex.IsMatch("a").Should().BeFalse();

        using var gsFollowedByLetter = RegexBuilder.Create().Set(@"\c]a").Compiled();
        gsFollowedByLetter.GetPattern().Should().Be(@"[\c]a]");
        var gsLetterRegex = gsFollowedByLetter.Build();
        gsLetterRegex.IsMatch("\x1D").Should().BeTrue();
        gsLetterRegex.IsMatch("a").Should().BeTrue();
        gsLetterRegex.IsMatch("]").Should().BeFalse();

        using var notSetSubtractionEscaped = RegexBuilder.Create().NotSet("a-z-[b]");
        notSetSubtractionEscaped.GetPattern().Should().Be(@"[^a-z-\[b\]]");
        var notSetSubRegex = notSetSubtractionEscaped.Build();
        notSetSubRegex.IsMatch("b").Should().BeFalse();
        notSetSubRegex.IsMatch("[").Should().BeFalse();
        notSetSubRegex.IsMatch("]").Should().BeFalse();
        notSetSubRegex.IsMatch("A").Should().BeTrue();
    }

    [Test]
    public void TestInvalidSetNotSetAndRangePreservePrecedingMultiCharLiteralState()
    {
        using var builder = RegexBuilder.Create().Literal("ab");

        builder.Invoking(b => b.Set("z-a")).Should().Throw<ArgumentException>();
        builder.Invoking(b => b.NotSet(@"\c")).Should().Throw<ArgumentException>();
        builder.Invoking(b => b.Range('z', 'a')).Should().Throw<ArgumentOutOfRangeException>();

        builder.GetPattern().Should().Be("ab");
        builder.OneOrMore().GetPattern().Should().Be("(?:ab)+");
        builder.Build().IsMatch("abab").Should().BeTrue();
    }

    [Test]
    public void TestSetControlEscapeBackslashFollowedByControlEscapesAndRanges()
    {
        using var fsAndNull = RegexBuilder.Create().StartOfString().Set("\\c\\\0").EndOfString().Compiled();
        fsAndNull.GetPattern().Should().Be(@"^[\c\\x00]$");
        fsAndNull.Build().IsMatch("\x1C").Should().BeTrue();
        fsAndNull.Build().IsMatch("\0").Should().BeTrue();
        fsAndNull.Build().IsMatch(@"\").Should().BeFalse();

        using var fsAndHyphen = RegexBuilder.Create().StartOfString().Set(@"\c\-").EndOfString();
        fsAndHyphen.Build().IsMatch("\x1C").Should().BeTrue();
        fsAndHyphen.Build().IsMatch("-").Should().BeTrue();
        fsAndHyphen.Build().IsMatch("a").Should().BeFalse();

        using var fsToZRange = RegexBuilder.Create().StartOfString().Set(@"\c\-z").EndOfString();
        fsToZRange.Build().IsMatch("\x1C").Should().BeTrue();
        fsToZRange.Build().IsMatch(" ").Should().BeTrue();
        fsToZRange.Build().IsMatch("z").Should().BeTrue();
        fsToZRange.Build().IsMatch("{").Should().BeFalse();

        using var fsAndBackslashAndLetter = RegexBuilder.Create().StartOfString().Set(@"\c\\\a").EndOfString();
        fsAndBackslashAndLetter.Build().IsMatch("\x1C").Should().BeTrue();
        fsAndBackslashAndLetter.Build().IsMatch(@"\").Should().BeTrue();
        fsAndBackslashAndLetter.Build().IsMatch("a").Should().BeTrue();
        fsAndBackslashAndLetter.Build().IsMatch("b").Should().BeFalse();
    }

    [Test]
    public void TestRangeNonAsciiSeparatorsWithIgnorePatternWhitespaceAndCompiled()
    {
        foreach (char sep in new[] { '\u0085', '\u2028', '\u2029' })
        {
            using var builder = RegexBuilder.Create()
                .StartOfString()
                .Range(sep, sep)
                .EndOfString()
                .IgnorePatternWhitespace()
                .Compiled();

            var regex = builder.Build();
            regex.IsMatch(sep.ToString()).Should().BeTrue();
            regex.IsMatch(" ").Should().BeFalse();
        }
    }

    [TestCase(@"\c[", "\x1B", "[")]
    [TestCase(@"\c\", "\x1C", @"\")]
    [TestCase(@"\c]", "\x1D", "]")]
    [TestCase(@"\c^", "\x1E", "^")]
    public void TestSetAndNotSetControlEscapesQuantifiedWithCompiledAndIgnorePatternWhitespace(
        string spec,
        string expectedMatch,
        string nonMatch)
    {
        using var setBuilder = RegexBuilder.Create()
            .StartOfString()
            .Set(spec).OneOrMore()
            .EndOfString()
            .IgnorePatternWhitespace()
            .Compiled();
        var setRegex = setBuilder.Build();
        setRegex.IsMatch(expectedMatch + expectedMatch).Should().BeTrue();
        setRegex.IsMatch(nonMatch).Should().BeFalse();

        using var notSetBuilder = RegexBuilder.Create()
            .StartOfString()
            .NotSet(spec).OneOrMore()
            .EndOfString()
            .IgnorePatternWhitespace()
            .Compiled();
        var notSetRegex = notSetBuilder.Build();
        notSetRegex.IsMatch(nonMatch + "a").Should().BeTrue();
        notSetRegex.IsMatch(expectedMatch).Should().BeFalse();
    }

    [Test]
    public void TestSetAndNotSetBoundaryBracketCaretHyphenCombinations()
    {
        using var leadingHyphenBracket = RegexBuilder.Create().StartOfString().Set("-[").EndOfString();
        leadingHyphenBracket.GetPattern().Should().Be(@"^[-\[]$");
        leadingHyphenBracket.Build().IsMatch("-").Should().BeTrue();
        leadingHyphenBracket.Build().IsMatch("[").Should().BeTrue();
        leadingHyphenBracket.Build().IsMatch("a").Should().BeFalse();

        using var notLeadingHyphenBracket = RegexBuilder.Create().StartOfString().NotSet("-[").EndOfString();
        notLeadingHyphenBracket.GetPattern().Should().Be(@"^[^-\[]$");
        notLeadingHyphenBracket.Build().IsMatch("-").Should().BeFalse();
        notLeadingHyphenBracket.Build().IsMatch("[").Should().BeFalse();
        notLeadingHyphenBracket.Build().IsMatch("a").Should().BeTrue();

        using var bracketRange = RegexBuilder.Create().StartOfString().Set("[-]").EndOfString();
        bracketRange.GetPattern().Should().Be(@"^[\[-\]]$");
        bracketRange.Build().IsMatch(@"\").Should().BeTrue();
        bracketRange.Build().IsMatch("a").Should().BeFalse();

        using var caretToLetter = RegexBuilder.Create().StartOfString().Set("^-a").EndOfString();
        caretToLetter.GetPattern().Should().Be(@"^[\^-a]$");
        caretToLetter.Build().IsMatch("_").Should().BeTrue();
        caretToLetter.Build().IsMatch("b").Should().BeFalse();

        using var controlToRightBracket = RegexBuilder.Create().StartOfString().Set(@"\c]-]").EndOfString();
        controlToRightBracket.GetPattern().Should().Be(@"^[\c]-\]]$");
        controlToRightBracket.Build().IsMatch("\x1D").Should().BeTrue();
        controlToRightBracket.Build().IsMatch("A").Should().BeTrue();
        controlToRightBracket.Build().IsMatch("^").Should().BeFalse();
    }

    [Test]
    public void TestRangeSurrogateBoundsWithIgnoreCaseAndCompiled()
    {
        var regex = RegexBuilder.Create()
            .StartOfString()
            .Range('\uD800', '\uDFFF')
            .EndOfString()
            .IgnoreCase()
            .Compiled()
            .Build();

        regex.IsMatch("\uD800").Should().BeTrue();
        regex.IsMatch("\uDFFF").Should().BeTrue();
        regex.IsMatch("a").Should().BeFalse();
    }

    [Test]
    public void TestSetAndNotSetAllBmpSingleCharactersRoundTrip()
    {
        for (int code = 0; code <= char.MaxValue; code++)
        {
            char c = (char)code;
            string spec = c == '\\' ? @"\" : c.ToString();
            string target = c.ToString();
            string other = code == 'x' ? "y" : "x";

            using var setBuilder = RegexBuilder.Create()
                .StartOfString()
                .Set(spec)
                .EndOfString()
                .IgnorePatternWhitespace();
            var setRegex = setBuilder.Build();
            setRegex.IsMatch(target).Should().BeTrue($"Set should match BMP U+{code:X4}");
            setRegex.IsMatch(other).Should().BeFalse($"Set for U+{code:X4} should not match '{other}'");

            using var notSetBuilder = RegexBuilder.Create()
                .StartOfString()
                .NotSet(spec)
                .EndOfString()
                .IgnorePatternWhitespace();
            var notSetRegex = notSetBuilder.Build();
            notSetRegex.IsMatch(target).Should().BeFalse($"NotSet should exclude BMP U+{code:X4}");
            notSetRegex.IsMatch(other).Should().BeTrue($"NotSet for U+{code:X4} should match '{other}'");
        }
    }

    [Test]
    public void TestRangeAllBmpSingleCharacterBoundsUnderIgnorePatternWhitespace()
    {
        for (int code = 0; code <= char.MaxValue; code++)
        {
            char c = (char)code;
            using var builder = RegexBuilder.Create()
                .StartOfString()
                .Range(c, c)
                .EndOfString()
                .IgnorePatternWhitespace();

            var regex = builder.Build();
            regex.IsMatch(c.ToString()).Should().BeTrue($"Range(U+{code:X4}, U+{code:X4}) should match itself");
            string other = code == 'x' ? "y" : "x";
            regex.IsMatch(other).Should().BeFalse($"Range(U+{code:X4}, U+{code:X4}) should not match '{other}'");
        }
    }

    [Test]
    public void TestSetAndNotSetAllAsciiControlEscapeDesignators()
    {
        for (int code = 0; code <= 127; code++)
        {
            char designator = (char)code;
            string spec = @"\c" + designator;
            bool isValid = (designator >= '@' && designator <= '_') || (designator >= 'a' && designator <= 'z');

            using var builder = RegexBuilder.Create().Literal("ab");
            if (isValid)
            {
                string expectedControl = ((char)(code & 0x1F)).ToString();
                var setRegex = builder.Set(spec).Build();
                setRegex.IsMatch("ab" + expectedControl).Should().BeTrue($"\\c{designator} should match U+{code & 0x1F:X4}");

                using var notSetBuilder = RegexBuilder.Create().StartOfString().NotSet(spec).EndOfString();
                var notSetRegex = notSetBuilder.Build();
                notSetRegex.IsMatch(expectedControl).Should().BeFalse($"NotSet(\\c{designator}) should exclude U+{code & 0x1F:X4}");
                notSetRegex.IsMatch("~").Should().BeTrue();
            }
            else
            {
                builder.Invoking(b => b.Set(spec)).Should().Throw<ArgumentException>($"\\c U+{code:X4} should be rejected");
                builder.Invoking(b => b.NotSet(spec)).Should().Throw<ArgumentException>($"NotSet \\c U+{code:X4} should be rejected");
                builder.GetPattern().Should().Be("ab");
                builder.OneOrMore().GetPattern().Should().Be("(?:ab)+");
            }
        }
    }

    [Test]
    public void TestSetAndNotSetControlEscapeRanges()
    {
        using var fullControlRange = RegexBuilder.Create()
            .StartOfString()
            .Set(@"\c@-\c_")
            .EndOfString()
            .Compiled();

        var fullRegex = fullControlRange.Build();
        for (int code = 0; code <= 0x1F; code++)
        {
            fullRegex.IsMatch(((char)code).ToString()).Should().BeTrue();
        }
        fullRegex.IsMatch(" ").Should().BeFalse();

        using var notControlRange = RegexBuilder.Create()
            .StartOfString()
            .NotSet(@"\c@-\c_")
            .EndOfString();
        var notRegex = notControlRange.Build();
        notRegex.IsMatch("\0").Should().BeFalse();
        notRegex.IsMatch("\x1F").Should().BeFalse();
        notRegex.IsMatch(" ").Should().BeTrue();
    }

    [TestCase(@"\u123")]
    [TestCase(@"\x1")]
    [TestCase(@"\k<a>")]
    [TestCase(@"\p{NotARealUnicodeCategory}")]
    [TestCase("a--")]
    [TestCase(@"a-\]")]
    public void TestSetAndNotSetRejectMalformedEscapesAndRangesWithoutCorruptingBuilder(string invalidChars)
    {
        using var builder = RegexBuilder.Create().Literal("ab");

        builder.Invoking(b => b.Set(invalidChars)).Should().Throw<ArgumentException>();
        builder.Invoking(b => b.NotSet(invalidChars)).Should().Throw<ArgumentException>();

        builder.GetPattern().Should().Be("ab");
        builder.OneOrMore().GetPattern().Should().Be("(?:ab)+");
    }

    [Test]
    public void TestSetNotSetAndRangeWithUnicodeCategoriesAndShorthandNegation()
    {
        var nonDigit = RegexBuilder.Create().StartOfString().NotSet(@"\d").EndOfString().Build();
        nonDigit.IsMatch("a").Should().BeTrue();
        nonDigit.IsMatch("5").Should().BeFalse();

        var nonLetter = RegexBuilder.Create().StartOfString().Set(@"\P{L}").EndOfString().Build();
        nonLetter.IsMatch("5").Should().BeTrue();
        nonLetter.IsMatch("A").Should().BeFalse();
    }

    [Test]
    public void TestSetAndNotSetHandleControlWordEscapesAndRejectInvalidClassEscapes()
    {
        using var escapes = RegexBuilder.Create().StartOfString().Set(@"\a\b\e").EndOfString();
        escapes.GetPattern().Should().Be(@"^[\a\b\e]$");
        var escapesRegex = escapes.Build();
        escapesRegex.IsMatch("\a").Should().BeTrue();
        escapesRegex.IsMatch("\b").Should().BeTrue();
        escapesRegex.IsMatch("\x1B").Should().BeTrue();
        escapesRegex.IsMatch("a").Should().BeFalse();

        using var rangeAndEscapedHyphen = RegexBuilder.Create().StartOfString().Set(@"a-z\-0-9").EndOfString();
        var combinedRegex = rangeAndEscapedHyphen.Build();
        combinedRegex.IsMatch("m").Should().BeTrue();
        combinedRegex.IsMatch("-").Should().BeTrue();
        combinedRegex.IsMatch("5").Should().BeTrue();
        combinedRegex.IsMatch("A").Should().BeFalse();

        using var escapedSpaceAndHash = RegexBuilder.Create().StartOfString().Set(@"\ \#").EndOfString();
        escapedSpaceAndHash.GetPattern().Should().Be(@"^[\ \#]$");
        escapedSpaceAndHash.Build().IsMatch(" ").Should().BeTrue();
        escapedSpaceAndHash.Build().IsMatch("#").Should().BeTrue();
        escapedSpaceAndHash.Build().IsMatch(@"\").Should().BeFalse();

        using var invalidEscapeBuilder = RegexBuilder.Create().Literal("ab");
        invalidEscapeBuilder.Invoking(b => b.Set(@"\B")).Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("chars");
        invalidEscapeBuilder.Invoking(b => b.Set(@"\q")).Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("chars");
        invalidEscapeBuilder.Invoking(b => b.NotSet(@"\k")).Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("chars");
        invalidEscapeBuilder.GetPattern().Should().Be("ab");
        invalidEscapeBuilder.OneOrMore().GetPattern().Should().Be("(?:ab)+");
    }

    [Test]
    public void TestRangeOutOfOrderIncludesActualValueAndEscapedClassRangesMatch()
    {
        using var rangeBuilder = RegexBuilder.Create().Literal("ab").IgnoreCase();
        var ex = rangeBuilder.Invoking(b => b.Range('z', 'a')).Should().Throw<ArgumentOutOfRangeException>().Which;
        ex.ParamName.Should().Be("start");
        ex.ActualValue.Should().Be('z');
        rangeBuilder.GetOptions().Should().Be(RegexOptions.IgnoreCase);
        rangeBuilder.OneOrMore().GetPattern().Should().Be("(?:ab)+");

        var hexHyphenRange = RegexBuilder.Create().StartOfString().Set(@"\x2D-9").EndOfString().Build();
        hexHyphenRange.IsMatch("-").Should().BeTrue();
        hexHyphenRange.IsMatch("0").Should().BeTrue();
        hexHyphenRange.IsMatch("9").Should().BeTrue();
        hexHyphenRange.IsMatch("a").Should().BeFalse();

        var escapedHyphenLiteralAndNine = RegexBuilder.Create().StartOfString().Set(@"\--9").EndOfString().Build();
        escapedHyphenLiteralAndNine.IsMatch("-").Should().BeTrue();
        escapedHyphenLiteralAndNine.IsMatch("9").Should().BeTrue();
        escapedHyphenLiteralAndNine.IsMatch("0").Should().BeFalse();

        var escapedBracketRange = RegexBuilder.Create().StartOfString().Set(@"\[-\]").EndOfString().Build();
        escapedBracketRange.IsMatch("[").Should().BeTrue();
        escapedBracketRange.IsMatch(@"\").Should().BeTrue();
        escapedBracketRange.IsMatch("]").Should().BeTrue();
        escapedBracketRange.IsMatch("a").Should().BeFalse();

        var caretIdentityRange = RegexBuilder.Create().StartOfString().NotSet(@"\^-\^").EndOfString().Build();
        caretIdentityRange.IsMatch("^").Should().BeFalse();
        caretIdentityRange.IsMatch("a").Should().BeTrue();
    }
}
