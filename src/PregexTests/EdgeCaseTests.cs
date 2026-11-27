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
}
