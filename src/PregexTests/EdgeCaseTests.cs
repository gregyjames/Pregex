using System.Globalization;

namespace PregexTests;

public class EdgeCaseTests
{
    [Test]
    public void TestEmptyPattern()
    {
        var regex = RegexBuilder.Create().Build();
        regex.IsMatch("").Should().BeTrue();
        regex.IsMatch("anything").Should().BeTrue();
    }

    [Test]
    public void TestSpecialCharacterEscaping()
    {
        var regex = RegexBuilder.Create()
            .Literal("$100.00")
            .Build();
        regex.IsMatch("$100.00").Should().BeTrue();
        regex.IsMatch("$10000").Should().BeFalse();
    }

    [Test]
    public void TestNestedGroups()
    {
        var regex = RegexBuilder.Create()
            .Group(b => b
                .Group(inner => inner.Digit().Exactly(3))
                .Literal("-")
                .Digit().Exactly(4))
            .Build();
        var match = regex.Match("123-4567");
        match.Success.Should().BeTrue();
        match.Groups[1].Value.Should().Be("123-4567");
        match.Groups[2].Value.Should().Be("123");
    }

    [Test]
    public void TestMultipleQuantifiers()
    {
        var regex = RegexBuilder.Create()
            .StartOfString()
            .Digit().Between(1, 3)
            .Literal(".")
            .Digit().Between(1, 3)
            .Literal(".")
            .Digit().Between(1, 3)
            .Literal(".")
            .Digit().Between(1, 3)
            .EndOfString()
            .Build();
        regex.IsMatch("192.168.1.1").Should().BeTrue();
        regex.IsMatch("1.2.3.4").Should().BeTrue();
        regex.IsMatch("999.999.999.999").Should().BeTrue();
    }

    [Test]
    public void TestVeryLongPatternExceedingInitialBuffer()
    {
        using var builder = RegexBuilder.Create().StartOfString();
        for (int i = 0; i < 1000; i++)
        {
            builder.Literal("abcd").Digit();
        }
        builder.EndOfString();

        string pattern = builder.GetPattern();
        pattern.Length.Should().BeGreaterThan(6000);
        builder.Build().IsMatch(string.Concat(Enumerable.Repeat("abcd1", 1000))).Should().BeTrue();
    }

    [TestCase("tr-TR")]
    [TestCase("ar-SA")]
    [TestCase("fa-IR")]
    public void TestQuantifierFormattingIsIndependentOfCurrentCulture(string cultureName)
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(cultureName);
            using var builder = RegexBuilder.Create()
                .Digit().Exactly(10)
                .Word().AtLeast(2)
                .Range('a', 'z').Between(3, 5);

            builder.GetPattern().Should().Be(@"\d{10}\w{2,}[a-z]{3,5}");
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
