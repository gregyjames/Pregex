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

    [Test]
    public void TestGroupNullCallbackThrowsWithoutCorruptingBuilder()
    {
        using var builder = RegexBuilder.Create().Literal("abc");

        builder.Invoking(b => b.Group(null!)).Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("callback");
        builder.Invoking(b => b.NonCapturingGroup(null!)).Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("callback");
        builder.Invoking(b => b.NamedGroup("name", null!)).Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("callback");

        builder.GetPattern().Should().Be("abc");
        builder.OneOrMore().GetPattern().Should().Be("(?:abc)+");
    }

    [Test]
    public void TestGroupThrowingCallbackRollsBackPartialPatternAndRestoresTokenState()
    {
        using var builder = RegexBuilder.Create().Literal("ab").IgnoreCase();

        builder.Invoking(b => b.Group(inner =>
        {
            inner.Digit().OneOrMore().Multiline();
            throw new InvalidOperationException("boom");
        })).Should().Throw<InvalidOperationException>();

        builder.GetPattern().Should().Be("ab");
        builder.GetOptions().Should().Be(RegexOptions.IgnoreCase);
        builder.OneOrMore().GetPattern().Should().Be("(?:ab)+");
    }

    [Test]
    public void TestGroupCallbackDisposingBuilderThrowsObjectDisposedException()
    {
        var builder = RegexBuilder.Create().Digit();
        builder.Invoking(b => b.Group(inner => inner.Dispose())).Should().Throw<ObjectDisposedException>();
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase("0")]
    [TestCase("123")]
    [TestCase("1abc")]
    [TestCase("a-b")]
    [TestCase("a>b")]
    [TestCase("a b")]
    [TestCase("a.b")]
    [TestCase("a\u0301")]
    public void TestNamedGroupInvalidNamesThrowArgumentExceptionWithoutCorruptingBuilder(string invalidName)
    {
        using var builder = RegexBuilder.Create().Literal("prefix");
        builder.Invoking(b => b.NamedGroup(invalidName, inner => inner.Digit())).Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("name");
        builder.GetPattern().Should().Be("prefix");
    }

    [Test]
    public void TestNamedGroupNullNameThrowsArgumentNullException()
    {
        using var builder = RegexBuilder.Create().Literal("prefix");
        builder.Invoking(b => b.NamedGroup(null!, inner => inner.Digit())).Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("name");
        builder.GetPattern().Should().Be("prefix");
    }

    [TestCase("_")]
    [TestCase("_a")]
    [TestCase("a1")]
    [TestCase("café")]
    public void TestNamedGroupValidNamesMatch(string validName)
    {
        var regex = RegexBuilder.Create().NamedGroup(validName, b => b.Digit().OneOrMore()).Build();
        var match = regex.Match("42");
        match.Success.Should().BeTrue();
        match.Groups[validName].Value.Should().Be("42");
    }

    [Test]
    public void TestDuplicateNamedGroupsEmptyGroupsAndDeeplyNestedGroups()
    {
        var duplicate = RegexBuilder.Create()
            .NamedGroup("item", b => b.Literal("a"))
            .Or()
            .NamedGroup("item", b => b.Literal("b"))
            .Build();
        duplicate.Match("b").Groups["item"].Value.Should().Be("b");

        var emptyGroup = RegexBuilder.Create().Group(_ => { }).Build();
        emptyGroup.IsMatch("").Should().BeTrue();

        var nested = RegexBuilder.Create();
        void AddDepth(RegexBuilder b, int depth)
        {
            if (depth == 0)
            {
                b.Digit();
                return;
            }

            b.NonCapturingGroup(inner => AddDepth(inner, depth - 1));
        }

        AddDepth(nested, 25);
        nested.Build().IsMatch("7").Should().BeTrue();
        nested.Dispose();
    }

    [Test]
    public void TestNestedGroupExceptionCaughtByOuterCallbackRestoresInnerStateAndOptions()
    {
        using var builder = RegexBuilder.Create().IgnoreCase();

        builder.Group(outer =>
        {
            outer.Literal("xy");
            try
            {
                outer.NonCapturingGroup(inner =>
                {
                    inner.Digit().Multiline();
                    throw new InvalidOperationException("inner boom");
                });
            }
            catch (InvalidOperationException)
            {
            }

            outer.OneOrMore();
        });

        builder.GetPattern().Should().Be("((?:xy)+)");
        builder.GetOptions().Should().Be(RegexOptions.IgnoreCase);
        builder.Build().IsMatch("XYxy").Should().BeTrue();
    }

    [Test]
    public void TestNestedNamedGroupsAndOuterDisposeCaughtByOuterCallback()
    {
        var nested = RegexBuilder.Create()
            .NamedGroup("full", b => b
                .NamedGroup("area", inner => inner.Digit().Exactly(3))
                .Literal("-")
                .NamedGroup("line", inner => inner.Digit().Exactly(4)))
            .Build();

        var match = nested.Match("555-1234");
        match.Success.Should().BeTrue();
        match.Groups["full"].Value.Should().Be("555-1234");
        match.Groups["area"].Value.Should().Be("555");
        match.Groups["line"].Value.Should().Be("1234");

        var disposedInNested = RegexBuilder.Create().Literal("ab");
        disposedInNested.Invoking(b => b.Group(outer =>
        {
            try
            {
                outer.Group(inner => inner.Dispose());
            }
            catch (ObjectDisposedException)
            {
            }
        })).Should().Throw<ObjectDisposedException>();
    }

    [Test]
    public void TestLargePatternRollbackRestoresMultiCharLiteralState()
    {
        string prefix = new string('p', 4096);
        using var builder = RegexBuilder.Create().Literal(prefix).Literal("xy");

        builder.Invoking(b => b.Group(inner =>
        {
            inner.Literal(new string('z', 1024));
            throw new InvalidOperationException("boom");
        })).Should().Throw<InvalidOperationException>();

        builder.OneOrMore().GetPattern().Should().Be(prefix + "(?:xy)+");
    }

    [Test]
    public void TestNamedGroupValidationMatchesDotNetRegexAcrossAllBmpCharacters()
    {
        for (int code = 0; code <= char.MaxValue; code++)
        {
            char c = (char)code;

            if (char.IsLetter(c) || c == '_')
            {
                using var singleCharBuilder = RegexBuilder.Create().NamedGroup(c.ToString(), b => b.Literal("x"));
                var regex = singleCharBuilder.Build();
                regex.Match("x").Groups[c.ToString()].Value.Should().Be("x", $"U+{code:X4} should be a valid group start char");
            }
            else
            {
                using var invalidStartBuilder = RegexBuilder.Create();
                invalidStartBuilder.Invoking(b => b.NamedGroup(c.ToString(), inner => inner.Literal("x")))
                    .Should().Throw<ArgumentException>($"U+{code:X4} should be rejected as group start char");
            }

            if (char.IsLetterOrDigit(c) || c == '_')
            {
                string name = "a" + c;
                using var subsequentBuilder = RegexBuilder.Create().NamedGroup(name, b => b.Literal("x"));
                var regex = subsequentBuilder.Build();
                regex.Match("x").Groups[name].Value.Should().Be("x", $"U+{code:X4} should be a valid subsequent group char");
            }
            else
            {
                string name = "a" + c;
                using var invalidSubsequentBuilder = RegexBuilder.Create();
                invalidSubsequentBuilder.Invoking(b => b.NamedGroup(name, inner => inner.Literal("x")))
                    .Should().Throw<ArgumentException>($"U+{code:X4} should be rejected as subsequent group char");
            }
        }
    }

    [Test]
    public void TestEmptyBuilderRollbackAfterLargeBufferGrowthRestoresEmptyState()
    {
        using var builder = RegexBuilder.Create();

        builder.Invoking(b => b.Group(inner =>
        {
            inner.Literal(new string('a', 5000)).IgnoreCase().Multiline();
            throw new InvalidOperationException("boom");
        })).Should().Throw<InvalidOperationException>();

        builder.GetPattern().Should().Be("");
        builder.GetOptions().Should().Be(RegexOptions.None);
        builder.Invoking(b => b.OneOrMore()).Should().Throw<InvalidOperationException>();

        builder.Literal("xy").OneOrMore().GetPattern().Should().Be("(?:xy)+");
        builder.Build().IsMatch("xyxy").Should().BeTrue();
    }

    [Test]
    public void TestGroupCallbackOptionsPersistOnSuccess()
    {
        using var builder = RegexBuilder.Create()
            .StartOfString()
            .Group(b => b.Literal("abc").IgnoreCase())
            .EndOfString();

        builder.GetOptions().Should().Be(RegexOptions.IgnoreCase);
        builder.Build().IsMatch("ABC").Should().BeTrue();
    }

    [Test]
    public void TestAllGroupTypesRollbackStateAndThrowOnDisposedCallback()
    {
        using var nonCap = RegexBuilder.Create().Literal("ab").IgnoreCase();
        nonCap.Invoking(b => b.NonCapturingGroup(inner =>
        {
            inner.Digit().Singleline();
            throw new InvalidOperationException("boom");
        })).Should().Throw<InvalidOperationException>();
        nonCap.GetPattern().Should().Be("ab");
        nonCap.GetOptions().Should().Be(RegexOptions.IgnoreCase);
        nonCap.OneOrMore().GetPattern().Should().Be("(?:ab)+");

        using var named = RegexBuilder.Create().Literal("cd").Multiline();
        named.Invoking(b => b.NamedGroup("grp", inner =>
        {
            inner.Word().Compiled();
            throw new InvalidOperationException("boom");
        })).Should().Throw<InvalidOperationException>();
        named.GetPattern().Should().Be("cd");
        named.GetOptions().Should().Be(RegexOptions.Multiline);
        named.OneOrMore().GetPattern().Should().Be("(?:cd)+");

        var disposedNonCap = RegexBuilder.Create().Digit();
        disposedNonCap.Invoking(b => b.NonCapturingGroup(inner => inner.Dispose()))
            .Should().Throw<ObjectDisposedException>();

        var disposedNamed = RegexBuilder.Create().Digit();
        disposedNamed.Invoking(b => b.NamedGroup("g", inner => inner.Dispose()))
            .Should().Throw<ObjectDisposedException>();
    }

    [Test]
    public void TestNamedGroupRejectsSurrogatePairsWithoutCorruptingBuilder()
    {
        using var builder = RegexBuilder.Create().Literal("ab");
        builder.Invoking(b => b.NamedGroup("😀", inner => inner.Digit())).Should().Throw<ArgumentException>();
        builder.Invoking(b => b.NamedGroup("a😀", inner => inner.Digit())).Should().Throw<ArgumentException>();
        builder.Invoking(b => b.NamedGroup("\U00010400", inner => inner.Digit())).Should().Throw<ArgumentException>();
        builder.Invoking(b => b.NamedGroup("a\U0001D7CE", inner => inner.Digit())).Should().Throw<ArgumentException>();
        builder.GetPattern().Should().Be("ab");
        builder.OneOrMore().GetPattern().Should().Be("(?:ab)+");
    }

    [Test]
    public void TestScopedCallbackInspectionSucceedsAndUnclosedBuildRollsBackCleanly()
    {
        using var builder = RegexBuilder.Create().Literal("ab").IgnoreCase();
        string? seenPattern = null;
        RegexOptions? seenOptions = null;
        string? seenToString = null;

        builder.Group(inner =>
        {
            inner.Digit();
            seenPattern = inner.GetPattern();
            seenOptions = inner.GetOptions();
            seenToString = inner.ToString();
        });

        seenPattern.Should().Be(@"ab(\d");
        seenOptions.Should().Be(RegexOptions.IgnoreCase);
        seenToString.Should().Be(@"/ab(\d/IgnoreCase");
        builder.GetPattern().Should().Be(@"ab(\d)");

        builder.Invoking(b => b.Group(inner =>
        {
            inner.Word().Multiline();
            inner.Build();
        })).Should().Throw<ArgumentException>();

        builder.GetPattern().Should().Be(@"ab(\d)");
        builder.GetOptions().Should().Be(RegexOptions.IgnoreCase);
        builder.OneOrMore().GetPattern().Should().Be(@"ab(\d)+");
    }
}
