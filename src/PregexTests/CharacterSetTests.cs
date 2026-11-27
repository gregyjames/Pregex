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
}
