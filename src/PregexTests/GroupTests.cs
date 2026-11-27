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

        builder.Invoking(b => b.Group(null!)).Should().Throw<ArgumentNullException>();
        builder.Invoking(b => b.NonCapturingGroup(null!)).Should().Throw<ArgumentNullException>();
        builder.Invoking(b => b.NamedGroup("name", null!)).Should().Throw<ArgumentNullException>();

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
        builder.Invoking(b => b.NamedGroup(invalidName, inner => inner.Digit())).Should().Throw<ArgumentException>();
        builder.GetPattern().Should().Be("prefix");
    }

    [Test]
    public void TestNamedGroupNullNameThrowsArgumentNullException()
    {
        using var builder = RegexBuilder.Create().Literal("prefix");
        builder.Invoking(b => b.NamedGroup(null!, inner => inner.Digit())).Should().Throw<ArgumentNullException>();
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
}
