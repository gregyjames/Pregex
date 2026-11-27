using System.Text.RegularExpressions;
using Cysharp.Text;

namespace Pregex;

/// <summary>
/// A fluent, chainable builder for constructing <see cref="Regex"/> instances.
/// </summary>
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
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }

    private static void ThrowIfNullOrEmpty(string value, string paramName)
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }

        if (value.Length == 0)
        {
            throw new ArgumentException("Value cannot be empty.", paramName);
        }
    }

    /// <summary>
    /// Appends an escaped literal string to the pattern.
    /// </summary>
    /// <param name="str">The literal text to match.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder Literal(string str)
    {
        ThrowIfDisposed();
        _pattern.Append(Regex.Escape(str));
        return this;
    }

    /// <summary>
    /// Appends a digit character class (<c>\d</c>) to the pattern.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder Digit()
    {
        ThrowIfDisposed();
        _pattern.Append(@"\d");
        return this;
    }

    /// <summary>
    /// Appends a word character class (<c>\w</c>) to the pattern.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder Word()
    {
        ThrowIfDisposed();
        _pattern.Append(@"\w");
        return this;
    }

    /// <summary>
    /// Appends a whitespace character class (<c>\s</c>) to the pattern.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder Whitespace()
    {
        ThrowIfDisposed();
        _pattern.Append(@"\s");
        return this;
    }

    /// <summary>
    /// Appends a non-digit character class (<c>\D</c>) to the pattern.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder NotDigit()
    {
        ThrowIfDisposed();
        _pattern.Append(@"\D");
        return this;
    }

    /// <summary>
    /// Appends a non-word character class (<c>\W</c>) to the pattern.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder NotWord()
    {
        ThrowIfDisposed();
        _pattern.Append(@"\W");
        return this;
    }

    /// <summary>
    /// Appends a non-whitespace character class (<c>\S</c>) to the pattern.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder NotWhitespace()
    {
        ThrowIfDisposed();
        _pattern.Append(@"\S");
        return this;
    }

    /// <summary>
    /// Appends a wildcard character (<c>.</c>) to the pattern.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder AnyChar()
    {
        ThrowIfDisposed();
        _pattern.Append(".");
        return this;
    }

    // Character sets
    /// <summary>
    /// Appends a positive character class (<c>[...]</c>) matching any character or range in <paramref name="chars"/>.
    /// </summary>
    /// <param name="chars">The characters or ranges to include in the character class.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chars"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="chars"/> is empty.</exception>
    public RegexBuilder Set(string chars)
    {
        ThrowIfDisposed();
        ThrowIfNullOrEmpty(chars, nameof(chars));
        _pattern.Append('[');
        AppendEscapedCharacterClass(chars);
        _pattern.Append(']');
        return this;
    }

    /// <summary>
    /// Appends a negated character class (<c>[^...]</c>) matching any character not in <paramref name="chars"/>.
    /// </summary>
    /// <param name="chars">The characters or ranges to exclude from matching.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chars"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="chars"/> is empty.</exception>
    public RegexBuilder NotSet(string chars)
    {
        ThrowIfDisposed();
        ThrowIfNullOrEmpty(chars, nameof(chars));
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
                AppendCharOrControlEscape(c);
            }
        }
    }

    /// <summary>
    /// Appends a character range (<c>[start-end]</c>) to the pattern.
    /// </summary>
    /// <param name="start">The inclusive lower bound character of the range.</param>
    /// <param name="end">The inclusive upper bound character of the range.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="start"/> is greater than <paramref name="end"/>.</exception>
    public RegexBuilder Range(char start, char end)
    {
        ThrowIfDisposed();
        if (start > end)
        {
            throw new ArgumentOutOfRangeException(nameof(start), $"Start character '{start}' cannot be greater than end character '{end}'.");
        }

        _pattern.Append('[');
        AppendEscapedRangeChar(start);
        _pattern.Append('-');
        AppendEscapedRangeChar(end);
        _pattern.Append(']');
        return this;
    }

    private void AppendEscapedRangeChar(char c)
    {
        if (c == '-')
        {
            _pattern.Append(@"\x2D");
        }
        else if (c is '\\' or ']' or '[' or '^')
        {
            _pattern.Append('\\');
            _pattern.Append(c);
        }
        else
        {
            AppendCharOrControlEscape(c);
        }
    }

    private void AppendCharOrControlEscape(char c)
    {
        switch (c)
        {
            case '\t':
                _pattern.Append(@"\t");
                break;
            case '\n':
                _pattern.Append(@"\n");
                break;
            case '\r':
                _pattern.Append(@"\r");
                break;
            case '\f':
                _pattern.Append(@"\f");
                break;
            case '\v':
                _pattern.Append(@"\v");
                break;
            default:
                _pattern.Append(c);
                break;
        }
    }

    // Quantifiers
    /// <summary>
    /// Appends a one-or-more quantifier (<c>+</c>) to the preceding element.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder OneOrMore()
    {
        ThrowIfDisposed();
        _pattern.Append("+");
        return this;
    }

    /// <summary>
    /// Appends a zero-or-more quantifier (<c>*</c>) to the preceding element.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder ZeroOrMore()
    {
        ThrowIfDisposed();
        _pattern.Append("*");
        return this;
    }

    /// <summary>
    /// Appends an optional quantifier (<c>?</c>) to the preceding element.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder Optional()
    {
        ThrowIfDisposed();
        _pattern.Append("?");
        return this;
    }

    /// <summary>
    /// Appends an exact repetition quantifier (<c>{n}</c>) to the preceding element.
    /// </summary>
    /// <param name="n">The exact number of repetitions.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder Exactly(int n)
    {
        ThrowIfDisposed();
        _pattern.Append($"{{{n}}}");
        return this;
    }

    /// <summary>
    /// Appends a minimum repetition quantifier (<c>{n,}</c>) to the preceding element.
    /// </summary>
    /// <param name="n">The minimum number of repetitions.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder AtLeast(int n)
    {
        ThrowIfDisposed();
        _pattern.Append($"{{{n},}}");
        return this;
    }

    /// <summary>
    /// Appends a bounded repetition quantifier (<c>{min,max}</c>) to the preceding element.
    /// </summary>
    /// <param name="min">The minimum number of repetitions.</param>
    /// <param name="max">The maximum number of repetitions.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder Between(int min, int max)
    {
        ThrowIfDisposed();
        _pattern.Append($"{{{min},{max}}}");
        return this;
    }

    // Anchors
    /// <summary>
    /// Appends a start-of-string or start-of-line anchor (<c>^</c>) to the pattern.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder StartOfString()
    {
        ThrowIfDisposed();
        _pattern.Append("^");
        return this;
    }

    /// <summary>
    /// Appends an end-of-string or end-of-line anchor (<c>$</c>) to the pattern.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder EndOfString()
    {
        ThrowIfDisposed();
        _pattern.Append("$");
        return this;
    }

    /// <summary>
    /// Appends a word boundary anchor (<c>\b</c>) to the pattern.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder WordBoundary()
    {
        ThrowIfDisposed();
        _pattern.Append(@"\b");
        return this;
    }

    /// <summary>
    /// Appends a non-word boundary anchor (<c>\B</c>) to the pattern.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder NotWordBoundary()
    {
        ThrowIfDisposed();
        _pattern.Append(@"\B");
        return this;
    }

    // Groups
    /// <summary>
    /// Appends a capturing group (<c>(...)</c>) built by <paramref name="callback"/>.
    /// </summary>
    /// <param name="callback">An action that configures the pattern inside the group.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder Group(Action<RegexBuilder> callback)
    {
        ThrowIfDisposed();
        _pattern.Append("(");
        callback(this);
        _pattern.Append(")");
        return this;
    }

    /// <summary>
    /// Appends a non-capturing group (<c>(?:...)</c>) built by <paramref name="callback"/>.
    /// </summary>
    /// <param name="callback">An action that configures the pattern inside the group.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder NonCapturingGroup(Action<RegexBuilder> callback)
    {
        ThrowIfDisposed();
        _pattern.Append("(?:");
        callback(this);
        _pattern.Append(")");
        return this;
    }

    /// <summary>
    /// Appends a named capturing group (<c>(?&lt;name&gt;...)</c>) built by <paramref name="callback"/>.
    /// </summary>
    /// <param name="name">The capture group name.</param>
    /// <param name="callback">An action that configures the pattern inside the group.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder NamedGroup(string name, Action<RegexBuilder> callback)
    {
        ThrowIfDisposed();
        _pattern.Append($"(?<{name}>");
        callback(this);
        _pattern.Append(")");
        return this;
    }

    // Alternation
    /// <summary>
    /// Appends an alternation operator (<c>|</c>) to the pattern.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder Or()
    {
        ThrowIfDisposed();
        _pattern.Append("|");
        return this;
    }

    // Lookahead/Lookbehind
    /// <summary>
    /// Appends a positive lookahead assertion (<c>(?=...)</c>) built by <paramref name="callback"/>.
    /// </summary>
    /// <param name="callback">An action that configures the lookahead pattern.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder PositiveLookahead(Action<RegexBuilder> callback)
    {
        ThrowIfDisposed();
        _pattern.Append("(?=");
        callback(this);
        _pattern.Append(")");
        return this;
    }

    /// <summary>
    /// Appends a negative lookahead assertion (<c>(?!...)</c>) built by <paramref name="callback"/>.
    /// </summary>
    /// <param name="callback">An action that configures the lookahead pattern.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder NegativeLookahead(Action<RegexBuilder> callback)
    {
        ThrowIfDisposed();
        _pattern.Append("(?!");
        callback(this);
        _pattern.Append(")");
        return this;
    }

    /// <summary>
    /// Appends a positive lookbehind assertion (<c>(?&lt;=...)</c>) built by <paramref name="callback"/>.
    /// </summary>
    /// <param name="callback">An action that configures the lookbehind pattern.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder PositiveLookbehind(Action<RegexBuilder> callback)
    {
        ThrowIfDisposed();
        _pattern.Append("(?<=");
        callback(this);
        _pattern.Append(")");
        return this;
    }

    /// <summary>
    /// Appends a negative lookbehind assertion (<c>(?&lt;!...)</c>) built by <paramref name="callback"/>.
    /// </summary>
    /// <param name="callback">An action that configures the lookbehind pattern.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder NegativeLookbehind(Action<RegexBuilder> callback)
    {
        ThrowIfDisposed();
        _pattern.Append("(?<!");
        callback(this);
        _pattern.Append(")");
        return this;
    }

    // Regex Options (Flags)
    /// <summary>
    /// Enables <see cref="RegexOptions.IgnoreCase"/> on the built regular expression.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder IgnoreCase()
    {
        ThrowIfDisposed();
        _options |= RegexOptions.IgnoreCase;
        return this;
    }

    /// <summary>
    /// Enables <see cref="RegexOptions.Multiline"/> on the built regular expression.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder Multiline()
    {
        ThrowIfDisposed();
        _options |= RegexOptions.Multiline;
        return this;
    }

    /// <summary>
    /// Enables <see cref="RegexOptions.Singleline"/> on the built regular expression.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder Singleline()
    {
        ThrowIfDisposed();
        _options |= RegexOptions.Singleline;
        return this;
    }

    /// <summary>
    /// Enables <see cref="RegexOptions.ExplicitCapture"/> on the built regular expression.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder ExplicitCapture()
    {
        ThrowIfDisposed();
        _options |= RegexOptions.ExplicitCapture;
        return this;
    }

    /// <summary>
    /// Enables <see cref="RegexOptions.Compiled"/> on the built regular expression.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder Compiled()
    {
        ThrowIfDisposed();
        _options |= RegexOptions.Compiled;
        return this;
    }

    /// <summary>
    /// Enables <see cref="RegexOptions.IgnorePatternWhitespace"/> on the built regular expression.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    public RegexBuilder IgnorePatternWhitespace()
    {
        ThrowIfDisposed();
        _options |= RegexOptions.IgnorePatternWhitespace;
        return this;
    }

    // Build methods
    /// <summary>
    /// Builds and returns a <see cref="Regex"/> from the accumulated pattern and options.
    /// </summary>
    /// <returns>A new <see cref="Regex"/> instance.</returns>
    public Regex Build()
    {
        ThrowIfDisposed();
        return new Regex(_pattern.ToString(), _options);
    }

    /// <summary>
    /// Returns the raw regular expression pattern string.
    /// </summary>
    /// <returns>The built regular expression pattern.</returns>
    public string GetPattern()
    {
        ThrowIfDisposed();
        return _pattern.ToString();
    }

    /// <summary>
    /// Returns the configured <see cref="RegexOptions"/> flags.
    /// </summary>
    /// <returns>The configured <see cref="RegexOptions"/>.</returns>
    public RegexOptions GetOptions()
    {
        ThrowIfDisposed();
        return _options;
    }

    /// <summary>
    /// Returns a formatted string containing the pattern and options.
    /// </summary>
    /// <returns>A string in the form <c>/{pattern}/{options}</c>.</returns>
    public override string ToString()
    {
        ThrowIfDisposed();
        return $"/{_pattern}/{_options}";
    }

    // IDisposable
    /// <summary>
    /// Releases the unmanaged and optionally the managed resources used by this <see cref="RegexBuilder"/>.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to release managed resources; otherwise, <see langword="false"/>.</param>
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

    /// <summary>
    /// Releases the underlying pattern buffer resources used by this <see cref="RegexBuilder"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    // Static factory
    /// <summary>
    /// Creates a new <see cref="RegexBuilder"/> instance.
    /// </summary>
    /// <returns>A new <see cref="RegexBuilder"/>.</returns>
    public static RegexBuilder Create()
    {
        return new RegexBuilder();
    }
}