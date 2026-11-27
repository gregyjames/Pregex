namespace PregexTests;

public class DisposableTests
{
    [Test]
    public void TestUsingBlockWorks()
    {
        Regex regex;
        using (var builder = RegexBuilder.Create().StartOfString().Digit().OneOrMore().EndOfString())
        {
            regex = builder.Build();
        }

        regex.IsMatch("12345").Should().BeTrue();
        regex.IsMatch("abc").Should().BeFalse();
    }

    [Test]
    public void TestDisposeIsIdempotent()
    {
        var builder = RegexBuilder.Create().Literal("test");
        builder.Dispose();

        builder.Invoking(b => b.Dispose()).Should().NotThrow();
    }

    [Test]
    public void TestMethodsAfterDisposeThrowObjectDisposedException()
    {
        var builder = RegexBuilder.Create().Digit();
        builder.Dispose();

        builder.Invoking(b => b.Build()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.Build(TimeSpan.FromSeconds(1))).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.GetPattern()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.GetOptions()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.ToString()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.Literal("a")).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.Digit()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.Word()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.Whitespace()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.NotDigit()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.NotWord()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.NotWhitespace()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.AnyChar()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.Set("abc")).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.NotSet("abc")).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.Range('a', 'z')).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.OneOrMore()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.ZeroOrMore()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.Optional()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.Exactly(2)).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.AtLeast(2)).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.Between(1, 2)).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.Lazy()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.StartOfString()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.EndOfString()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.WordBoundary()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.NotWordBoundary()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.Group(inner => inner.Word())).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.NonCapturingGroup(inner => inner.Word())).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.NamedGroup("g", inner => inner.Word())).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.Or()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.PositiveLookahead(inner => inner.Word())).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.NegativeLookahead(inner => inner.Word())).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.PositiveLookbehind(inner => inner.Word())).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.NegativeLookbehind(inner => inner.Word())).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.IgnoreCase()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.Multiline()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.Singleline()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.ExplicitCapture()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.Compiled()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.IgnorePatternWhitespace()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.CultureInvariant()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.RightToLeft()).Should().Throw<ObjectDisposedException>();
    }
}
