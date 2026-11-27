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
        builder.Invoking(b => b.GetPattern()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.GetOptions()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.ToString()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.Literal("a")).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.Digit()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.OneOrMore()).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.Group(inner => inner.Word())).Should().Throw<ObjectDisposedException>();
        builder.Invoking(b => b.IgnoreCase()).Should().Throw<ObjectDisposedException>();
    }
}
