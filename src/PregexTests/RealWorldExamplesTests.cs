namespace PregexTests;

public class RealWorldExamplesTests
{
    #region Email Tests
    [Test]
    public void TestEmailMatch()
    {
        var emailRegex = RegexBuilder.Create()
            .StartOfString()
            .Word().OneOrMore()
            .Literal("@")
            .Word().OneOrMore()
            .Literal(".")
            .Set("a-zA-Z").Between(2, 6)
            .EndOfString()
            .Build();
        emailRegex.IsMatch("test@example.com").Should().BeTrue();
    }

    [Test]
    public void TestEmailNotMatch()
    {
        var emailRegex = RegexBuilder.Create()
            .StartOfString()
            .Word().OneOrMore()
            .Literal("@")
            .Word().OneOrMore()
            .Literal(".")
            .Set("a-zA-Z").Between(2, 6)
            .EndOfString()
            .Build();
        emailRegex.IsMatch("invalid@").Should().BeFalse();
    }

    [Test]
    public void TestEmailWithNumbers()
    {
        var emailRegex = RegexBuilder.Create()
            .StartOfString()
            .Word().OneOrMore()
            .Literal("@")
            .Word().OneOrMore()
            .Literal(".")
            .Set("a-zA-Z").Between(2, 6)
            .EndOfString()
            .Build();
        emailRegex.IsMatch("user123@test456.com").Should().BeTrue();
    }

    [Test]
    public void TestEmailNoAtSymbol()
    {
        var emailRegex = RegexBuilder.Create()
            .StartOfString()
            .Word().OneOrMore()
            .Literal("@")
            .Word().OneOrMore()
            .Literal(".")
            .Set("a-zA-Z").Between(2, 6)
            .EndOfString()
            .Build();
        emailRegex.IsMatch("testexample.com").Should().BeFalse();
    }

    [Test]
    public void TestEmailInvalidTLD()
    {
        var emailRegex = RegexBuilder.Create()
            .StartOfString()
            .Word().OneOrMore()
            .Literal("@")
            .Word().OneOrMore()
            .Literal(".")
            .Set("a-zA-Z").Between(2, 6)
            .EndOfString()
            .Build();
        emailRegex.IsMatch("test@example.c").Should().BeFalse();
        emailRegex.IsMatch("test@example.toolong").Should().BeFalse();
    }
    #endregion

    #region Phone Tests
    [Test]
    public void TestPhoneMatch()
    {
        var phoneRegex = RegexBuilder.Create()
            .StartOfString()
            .Literal("+").Optional()
            .Group(b => b.Digit().Exactly(3).Literal("-")).Optional()
            .Digit().Exactly(3)
            .Literal("-").Optional()
            .Digit().Exactly(4)
            .EndOfString()
            .Build();
        phoneRegex.IsMatch("123-456-7890").Should().BeTrue();
    }

    [Test]
    public void TestPhoneNotMatch()
    {
        var phoneRegex = RegexBuilder.Create()
            .StartOfString()
            .Literal("+").Optional()
            .Group(b => b.Digit().Exactly(3).Literal("-")).Optional()
            .Digit().Exactly(3)
            .Literal("-").Optional()
            .Digit().Exactly(4)
            .EndOfString()
            .Build();
        phoneRegex.IsMatch("+1-123-456").Should().BeFalse();
    }

    [Test]
    public void TestPhoneWithCountryCode()
    {
        var phoneRegex = RegexBuilder.Create()
            .StartOfString()
            .Literal("+").Optional()
            .Digit().Exactly(1).Literal("-")
            .Group(b => b.Digit().Exactly(3).Literal("-")).Optional()
            .Digit().Exactly(3)
            .Literal("-").Optional()
            .Digit().Exactly(4)
            .EndOfString()
            .Build();
        phoneRegex.IsMatch("+1-123-456-7890").Should().BeTrue();
    }

    [Test]
    public void TestPhoneWithoutDashes()
    {
        var phoneRegex = RegexBuilder.Create()
            .StartOfString()
            .Literal("+").Optional()
            .Digit().AtLeast(10)
            .EndOfString()
            .Build();
        phoneRegex.IsMatch("1234567890").Should().BeTrue();
    }

    [Test]
    public void TestPhoneTooShort()
    {
        var phoneRegex = RegexBuilder.Create()
            .StartOfString()
            .Literal("+").Optional()
            .Group(b => b.Digit().Exactly(3).Literal("-")).Optional()
            .Digit().Exactly(3)
            .Literal("-").Optional()
            .Digit().Exactly(4)
            .EndOfString()
            .Build();
        phoneRegex.IsMatch("123-456").Should().BeFalse();
    }
    #endregion

    [Test]
    public void TestIPAddress()
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
        regex.IsMatch("192.168.0.1").Should().BeTrue();
        regex.IsMatch("10.0.0.1").Should().BeTrue();
        regex.IsMatch("192.168.0").Should().BeFalse();
    }

    [Test]
    public void TestHexColor()
    {
        var regex = RegexBuilder.Create()
            .StartOfString()
            .Literal("#")
            .Set("0-9a-fA-F").Exactly(6)
            .EndOfString()
            .Build();
        regex.IsMatch("#FF5733").Should().BeTrue();
        regex.IsMatch("#000000").Should().BeTrue();
        regex.IsMatch("#FFF").Should().BeFalse();
        regex.IsMatch("FF5733").Should().BeFalse();
    }

    [Test]
    public void TestUsername()
    {
        var regex = RegexBuilder.Create()
            .StartOfString()
            .Set("a-zA-Z0-9_").Between(3, 16)
            .EndOfString()
            .Build();
        regex.IsMatch("user123").Should().BeTrue();
        regex.IsMatch("john_doe").Should().BeTrue();
        regex.IsMatch("ab").Should().BeFalse(); // Too short
        regex.IsMatch("user-name").Should().BeFalse(); // Invalid character
    }

    [Test]
    public void TestDate_MMDDYYYY()
    {
        var regex = RegexBuilder.Create()
            .StartOfString()
            .Digit().Exactly(2)
            .Literal("/")
            .Digit().Exactly(2)
            .Literal("/")
            .Digit().Exactly(4)
            .EndOfString()
            .Build();
        regex.IsMatch("12/25/2023").Should().BeTrue();
        regex.IsMatch("01/01/2000").Should().BeTrue();
        regex.IsMatch("1/1/2023").Should().BeFalse();
        regex.IsMatch("12-25-2023").Should().BeFalse();
    }

    [Test]
    public void TestPhoneWithOptionalCountryCodeMatchesReadmeExample()
    {
        var phoneRegex = RegexBuilder.Create()
            .StartOfString()
            .NonCapturingGroup(b => b.Literal("+").Digit().Between(1, 3).Literal("-")).Optional()
            .Digit().Exactly(3)
            .Literal("-").Optional()
            .Digit().Exactly(3)
            .Literal("-").Optional()
            .Digit().Exactly(4)
            .EndOfString()
            .Build();

        phoneRegex.IsMatch("123-456-7890").Should().BeTrue();
        phoneRegex.IsMatch("+1-123-456-7890").Should().BeTrue();
        phoneRegex.IsMatch("1234567890").Should().BeTrue();
        phoneRegex.IsMatch("+1-123-456").Should().BeFalse();
        phoneRegex.IsMatch("123-456").Should().BeFalse();
    }

    [Test]
    public void TestReadmeEmailUrlAndPasswordExamples()
    {
        var emailRegex = RegexBuilder.Create()
            .StartOfString()
            .Set("a-zA-Z0-9._%-").OneOrMore()
            .Literal("@")
            .Set("a-zA-Z0-9.-").OneOrMore()
            .Literal(".")
            .Set("a-zA-Z").Between(2, 6)
            .EndOfString()
            .Build();

        emailRegex.IsMatch("user.name+1@example.com").Should().BeFalse();
        emailRegex.IsMatch("user_name-1@sub.example.com").Should().BeTrue();
        emailRegex.IsMatch("invalid@").Should().BeFalse();

        var urlRegex = RegexBuilder.Create()
            .NamedGroup("protocol", b => b.Literal("http").Literal("s").Optional())
            .Literal("://")
            .NamedGroup("domain", b => b.NotSet(" /").OneOrMore())
            .NamedGroup("path", b => b.Literal("/").AnyChar().ZeroOrMore()).Optional()
            .Build();

        var urlMatch = urlRegex.Match("https://example.com/path");
        urlMatch.Success.Should().BeTrue();
        urlMatch.Groups["protocol"].Value.Should().Be("https");
        urlMatch.Groups["domain"].Value.Should().Be("example.com");
        urlMatch.Groups["path"].Value.Should().Be("/path");

        var passwordRegex = RegexBuilder.Create()
            .StartOfString()
            .PositiveLookahead(b => b.AnyChar().ZeroOrMore().Digit())
            .PositiveLookahead(b => b.AnyChar().ZeroOrMore().Range('a', 'z'))
            .PositiveLookahead(b => b.AnyChar().ZeroOrMore().Range('A', 'Z'))
            .AnyChar().AtLeast(8)
            .EndOfString()
            .Build();

        passwordRegex.IsMatch("Password1").Should().BeTrue();
        passwordRegex.IsMatch("password").Should().BeFalse();
        passwordRegex.IsMatch("Pass1").Should().BeFalse();
    }

    [Test]
    public void TestReadmeQuickStartPhoneExample()
    {
        var phoneRegex = RegexBuilder.Create()
            .StartOfString()
            .Literal("+").Optional()
            .Digit().Exactly(3)
            .Literal("-").Optional()
            .Digit().Exactly(3)
            .Literal("-").Optional()
            .Digit().Exactly(4)
            .EndOfString()
            .Build();

        phoneRegex.IsMatch("123-456-7890").Should().BeTrue();
        phoneRegex.IsMatch("+1234567890").Should().BeTrue();
        phoneRegex.IsMatch("123-45-6789").Should().BeFalse();
    }
}
