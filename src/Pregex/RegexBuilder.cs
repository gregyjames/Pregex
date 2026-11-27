using System.Text;
using System.Text.RegularExpressions;
using Cysharp.Text;

namespace Pregex;

public class RegexBuilder : IDisposable
{
    private Utf16ValueStringBuilder _pattern;
    private RegexOptions _options;
    private bool _disposed;

    private RegexBuilder()
    {
        _pattern = ZString.CreateStringBuilder();
        _options = RegexOptions.None;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public RegexBuilder Literal(string str)
    {
        ThrowIfDisposed();
        _pattern.Append(Regex.Escape(str));
        return this;
    }

    public RegexBuilder Digit()
    {
        ThrowIfDisposed();
        _pattern.Append(@"\d");
        return this;
    }

    public RegexBuilder Word()
    {
        ThrowIfDisposed();
        _pattern.Append(@"\w");
        return this;
    }

    public RegexBuilder Whitespace()
    {
        ThrowIfDisposed();
        _pattern.Append(@"\s");
        return this;
    }

    public RegexBuilder NotDigit()
    {
        ThrowIfDisposed();
        _pattern.Append(@"\D");
        return this;
    }

    public RegexBuilder NotWord()
    {
        ThrowIfDisposed();
        _pattern.Append(@"\W");
        return this;
    }

    public RegexBuilder NotWhitespace()
    {
        ThrowIfDisposed();
        _pattern.Append(@"\S");
        return this;
    }

    public RegexBuilder AnyChar()
    {
        ThrowIfDisposed();
        _pattern.Append(".");
        return this;
    }

    // Character sets
    public RegexBuilder Set(string chars)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrEmpty(chars);
        _pattern.Append('[');
        AppendEscapedCharacterClass(chars);
        _pattern.Append(']');
        return this;
    }

    public RegexBuilder NotSet(string chars)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrEmpty(chars);
        _pattern.Append("[^");
        AppendEscapedCharacterClass(chars);
        _pattern.Append(']');
        return this;
    }

    private void AppendEscapedCharacterClass(string chars)
    {
        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            if (c == '\\')
            {
                if (i + 1 < chars.Length)
                {
                    _pattern.Append('\\');
                    _pattern.Append(chars[++i]);
                }
                else
                {
                    _pattern.Append(@"\\");
                }
            }
            else if (c is ']' or '[' or '^')
            {
                _pattern.Append('\\');
                _pattern.Append(c);
            }
            else
            {
                _pattern.Append(c);
            }
        }
    }

    public RegexBuilder Range(char start, char end)
    {
        ThrowIfDisposed();
        _pattern.Append($"[{start}-{end}]");
        return this;
    }

    // Quantifiers
    public RegexBuilder OneOrMore()
    {
        ThrowIfDisposed();
        _pattern.Append("+");
        return this;
    }

    public RegexBuilder ZeroOrMore()
    {
        ThrowIfDisposed();
        _pattern.Append("*");
        return this;
    }

    public RegexBuilder Optional()
    {
        ThrowIfDisposed();
        _pattern.Append("?");
        return this;
    }

    public RegexBuilder Exactly(int n)
    {
        ThrowIfDisposed();
        _pattern.Append($"{{{n}}}");
        return this;
    }

    public RegexBuilder AtLeast(int n)
    {
        ThrowIfDisposed();
        _pattern.Append($"{{{n},}}");
        return this;
    }

    public RegexBuilder Between(int min, int max)
    {
        ThrowIfDisposed();
        _pattern.Append($"{{{min},{max}}}");
        return this;
    }

    // Anchors
    public RegexBuilder StartOfString()
    {
        ThrowIfDisposed();
        _pattern.Append("^");
        return this;
    }

    public RegexBuilder EndOfString()
    {
        ThrowIfDisposed();
        _pattern.Append("$");
        return this;
    }

    public RegexBuilder WordBoundary()
    {
        ThrowIfDisposed();
        _pattern.Append(@"\b");
        return this;
    }

    public RegexBuilder NotWordBoundary()
    {
        ThrowIfDisposed();
        _pattern.Append(@"\B");
        return this;
    }

    // Groups
    public RegexBuilder Group(Action<RegexBuilder> callback)
    {
        ThrowIfDisposed();
        _pattern.Append("(");
        callback(this);
        _pattern.Append(")");
        return this;
    }

    public RegexBuilder NonCapturingGroup(Action<RegexBuilder> callback)
    {
        ThrowIfDisposed();
        _pattern.Append("(?:");
        callback(this);
        _pattern.Append(")");
        return this;
    }

    public RegexBuilder NamedGroup(string name, Action<RegexBuilder> callback)
    {
        ThrowIfDisposed();
        _pattern.Append($"(?<{name}>");
        callback(this);
        _pattern.Append(")");
        return this;
    }

    // Alternation
    public RegexBuilder Or()
    {
        ThrowIfDisposed();
        _pattern.Append("|");
        return this;
    }

    // Lookahead/Lookbehind
    public RegexBuilder PositiveLookahead(Action<RegexBuilder> callback)
    {
        ThrowIfDisposed();
        _pattern.Append("(?=");
        callback(this);
        _pattern.Append(")");
        return this;
    }

    public RegexBuilder NegativeLookahead(Action<RegexBuilder> callback)
    {
        ThrowIfDisposed();
        _pattern.Append("(?!");
        callback(this);
        _pattern.Append(")");
        return this;
    }

    public RegexBuilder PositiveLookbehind(Action<RegexBuilder> callback)
    {
        ThrowIfDisposed();
        _pattern.Append("(?<=");
        callback(this);
        _pattern.Append(")");
        return this;
    }

    public RegexBuilder NegativeLookbehind(Action<RegexBuilder> callback)
    {
        ThrowIfDisposed();
        _pattern.Append("(?<!");
        callback(this);
        _pattern.Append(")");
        return this;
    }

    // Regex Options (Flags)
    public RegexBuilder IgnoreCase()
    {
        ThrowIfDisposed();
        _options |= RegexOptions.IgnoreCase;
        return this;
    }

    public RegexBuilder Multiline()
    {
        ThrowIfDisposed();
        _options |= RegexOptions.Multiline;
        return this;
    }

    public RegexBuilder Singleline()
    {
        ThrowIfDisposed();
        _options |= RegexOptions.Singleline;
        return this;
    }

    public RegexBuilder ExplicitCapture()
    {
        ThrowIfDisposed();
        _options |= RegexOptions.ExplicitCapture;
        return this;
    }

    public RegexBuilder Compiled()
    {
        ThrowIfDisposed();
        _options |= RegexOptions.Compiled;
        return this;
    }

    public RegexBuilder IgnorePatternWhitespace()
    {
        ThrowIfDisposed();
        _options |= RegexOptions.IgnorePatternWhitespace;
        return this;
    }

    // Build methods
    public Regex Build()
    {
        ThrowIfDisposed();
        return new Regex(_pattern.ToString(), _options);
    }

    public string GetPattern()
    {
        ThrowIfDisposed();
        return _pattern.ToString();
    }

    public RegexOptions GetOptions()
    {
        ThrowIfDisposed();
        return _options;
    }

    public override string ToString()
    {
        ThrowIfDisposed();
        return $"/{_pattern}/{_options}";
    }

    // IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _pattern.Dispose();
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    // Static factory
    public static RegexBuilder Create()
    {
        return new RegexBuilder();
    }
}