using System.Text.RegularExpressions;
using Cysharp.Text;

namespace Pregex;

/// <summary>
/// A fluent, chainable builder for constructing <see cref="Regex"/> instances.
/// </summary>
public class RegexBuilder : IDisposable
{
    private enum LastTokenKind
    {
        None,
        Quantifiable,
        MultiCharLiteral,
        QuantifiedGreedy,
        QuantifiedLazy,
    }

    private Utf16ValueStringBuilder _pattern;
    private RegexOptions _options;
    private bool _disposed;
    private int _lastMultiCharLiteralStart = -1;
    private LastTokenKind _lastTokenKind = LastTokenKind.None;

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

    private static bool IsValidGroupName(string name)
    {
        char first = name[0];
        if (!char.IsLetter(first) && first != '_')
        {
            return false;
        }

        for (int i = 1; i < name.Length; i++)
        {
            char c = name[i];
            if (!char.IsLetterOrDigit(c) && c != '_')
            {
                return false;
            }
        }

        return true;
    }

    private void EnsureQuantifiable()
    {
        switch (_lastTokenKind)
        {
            case LastTokenKind.None:
                throw new InvalidOperationException("A quantifier must follow a quantifiable pattern element.");
            case LastTokenKind.QuantifiedGreedy:
            case LastTokenKind.QuantifiedLazy:
                throw new InvalidOperationException(
                    "Cannot apply a quantifier directly to an already quantified element. Wrap the element in a group first, or call Lazy() for non-greedy matching.");
            case LastTokenKind.MultiCharLiteral:
                _pattern.Insert(_lastMultiCharLiteralStart, "(?:");
                _pattern.Append(')');
                _lastMultiCharLiteralStart = -1;
                break;
        }
    }

    private void AppendQuantifierSuffix(char symbol)
    {
        _pattern.Append(symbol);
        _lastTokenKind = LastTokenKind.QuantifiedGreedy;
        _lastMultiCharLiteralStart = -1;
    }

    private RegexBuilder AppendScoped(string prefix, Action<RegexBuilder> callback, LastTokenKind postTokenKind)
    {
        int startLength = _pattern.Length;
        LastTokenKind previousTokenKind = _lastTokenKind;
        int previousLiteralStart = _lastMultiCharLiteralStart;
        RegexOptions previousOptions = _options;

        try
        {
            _pattern.Append(prefix);
            _lastTokenKind = LastTokenKind.None;
            _lastMultiCharLiteralStart = -1;

            callback(this);
            ThrowIfDisposed();

            _pattern.Append(')');
            _lastTokenKind = postTokenKind;
            _lastMultiCharLiteralStart = -1;
            return this;
        }
        catch
        {
            if (!_disposed)
            {
                string previousPattern = _pattern.AsSpan().Slice(0, startLength).ToString();
                _pattern.Clear();
                _pattern.Append(previousPattern);
                _lastTokenKind = previousTokenKind;
                _lastMultiCharLiteralStart = previousLiteralStart;
                _options = previousOptions;
            }

            throw;
        }
    }

    /// <summary>
    /// Appends an escaped literal string to the pattern.
    /// </summary>
    /// <param name="str">The literal text to match.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <remarks>
    /// Special regex metacharacters, whitespace, <c>#</c>, vertical tabs (<c>\v</c>), and null characters (<c>\x00</c>)
    /// are escaped so the input matches literally even when <see cref="RegexOptions.IgnorePatternWhitespace"/> is enabled.
    /// Multi-character literals are automatically wrapped in a non-capturing group (<c>(?:...)</c>) if quantified.
    /// Passing an empty string appends nothing and clears the preceding quantifiable element state.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="str"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder Literal(string str)
    {
        ThrowIfDisposed();
        if (str is null)
        {
            throw new ArgumentNullException(nameof(str));
        }

        if (str.Length == 0)
        {
            _lastTokenKind = LastTokenKind.None;
            _lastMultiCharLiteralStart = -1;
            return this;
        }

        string escaped = Regex.Escape(str);
        if (escaped.IndexOf('\v') >= 0)
        {
            escaped = escaped.Replace("\v", @"\v");
        }

        if (escaped.IndexOf('\0') >= 0)
        {
            escaped = escaped.Replace("\0", @"\x00");
        }

        if (str.Length > 1)
        {
            _lastMultiCharLiteralStart = _pattern.Length;
            _lastTokenKind = LastTokenKind.MultiCharLiteral;
        }
        else
        {
            _lastMultiCharLiteralStart = -1;
            _lastTokenKind = LastTokenKind.Quantifiable;
        }

        _pattern.Append(escaped);
        return this;
    }

    /// <summary>
    /// Appends a digit character class (<c>\d</c>) to the pattern.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder Digit()
    {
        ThrowIfDisposed();
        _lastTokenKind = LastTokenKind.Quantifiable;
        _lastMultiCharLiteralStart = -1;
        _pattern.Append(@"\d");
        return this;
    }

    /// <summary>
    /// Appends a word character class (<c>\w</c>) to the pattern.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder Word()
    {
        ThrowIfDisposed();
        _lastTokenKind = LastTokenKind.Quantifiable;
        _lastMultiCharLiteralStart = -1;
        _pattern.Append(@"\w");
        return this;
    }

    /// <summary>
    /// Appends a whitespace character class (<c>\s</c>) to the pattern.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder Whitespace()
    {
        ThrowIfDisposed();
        _lastTokenKind = LastTokenKind.Quantifiable;
        _lastMultiCharLiteralStart = -1;
        _pattern.Append(@"\s");
        return this;
    }

    /// <summary>
    /// Appends a non-digit character class (<c>\D</c>) to the pattern.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder NotDigit()
    {
        ThrowIfDisposed();
        _lastTokenKind = LastTokenKind.Quantifiable;
        _lastMultiCharLiteralStart = -1;
        _pattern.Append(@"\D");
        return this;
    }

    /// <summary>
    /// Appends a non-word character class (<c>\W</c>) to the pattern.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder NotWord()
    {
        ThrowIfDisposed();
        _lastTokenKind = LastTokenKind.Quantifiable;
        _lastMultiCharLiteralStart = -1;
        _pattern.Append(@"\W");
        return this;
    }

    /// <summary>
    /// Appends a non-whitespace character class (<c>\S</c>) to the pattern.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder NotWhitespace()
    {
        ThrowIfDisposed();
        _lastTokenKind = LastTokenKind.Quantifiable;
        _lastMultiCharLiteralStart = -1;
        _pattern.Append(@"\S");
        return this;
    }

    /// <summary>
    /// Appends a wildcard character (<c>.</c>) to the pattern.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder AnyChar()
    {
        ThrowIfDisposed();
        _lastTokenKind = LastTokenKind.Quantifiable;
        _lastMultiCharLiteralStart = -1;
        _pattern.Append(".");
        return this;
    }

    // Character sets
    /// <summary>
    /// Appends a positive character class (<c>[...]</c>) matching any character or range in <paramref name="chars"/>.
    /// </summary>
    /// <param name="chars">The characters or ranges to include in the character class.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <remarks>
    /// Supports regex character class ranges (such as <c>"a-z0-9"</c>) and escape sequences (such as <c>@"\d"</c>, <c>@"\-"</c>, or <c>@"\c["</c>).
    /// Unescaped <c>[</c>, <c>]</c>, <c>^</c>, trailing <c>\</c>, and control characters (<c>\0</c>, <c>\t</c>, <c>\n</c>, <c>\r</c>, <c>\f</c>, <c>\v</c>)
    /// are automatically escaped, while <c>\c</c> control-escape designators (such as <c>@"\c["</c> or <c>@"\c]"</c>) are preserved.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chars"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="chars"/> is empty or produces an invalid character class.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder Set(string chars)
    {
        return AppendCharacterClass(chars, negated: false);
    }

    /// <summary>
    /// Appends a negated character class (<c>[^...]</c>) matching any character not in <paramref name="chars"/>.
    /// </summary>
    /// <param name="chars">The characters or ranges to exclude from matching.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <remarks>
    /// Supports regex character class ranges (such as <c>"a-z0-9"</c>) and escape sequences (such as <c>@"\d"</c>, <c>@"\-"</c>, or <c>@"\c["</c>).
    /// Unescaped <c>[</c>, <c>]</c>, <c>^</c>, trailing <c>\</c>, and control characters (<c>\0</c>, <c>\t</c>, <c>\n</c>, <c>\r</c>, <c>\f</c>, <c>\v</c>)
    /// are automatically escaped, while <c>\c</c> control-escape designators (such as <c>@"\c["</c> or <c>@"\c]"</c>) are preserved.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chars"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="chars"/> is empty or produces an invalid character class.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder NotSet(string chars)
    {
        return AppendCharacterClass(chars, negated: true);
    }

    private RegexBuilder AppendCharacterClass(string chars, bool negated)
    {
        ThrowIfDisposed();
        ThrowIfNullOrEmpty(chars, nameof(chars));

        string candidate;
        Utf16ValueStringBuilder classBuilder = ZString.CreateStringBuilder();
        try
        {
            classBuilder.Append(negated ? "[^" : "[");
            AppendEscapedCharacterClass(ref classBuilder, chars);
            classBuilder.Append(']');
            candidate = classBuilder.ToString();
        }
        finally
        {
            classBuilder.Dispose();
        }

        try
        {
            _ = new Regex(candidate);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"Invalid character class specification: {ex.Message}", nameof(chars), ex);
        }

        _lastTokenKind = LastTokenKind.Quantifiable;
        _lastMultiCharLiteralStart = -1;
        _pattern.Append(candidate);
        return this;
    }

    private static void AppendEscapedCharacterClass(ref Utf16ValueStringBuilder target, string chars)
    {
        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            if (c == '\\')
            {
                if (i + 1 < chars.Length)
                {
                    char next = chars[++i];
                    if (next == 'c' && i + 1 < chars.Length)
                    {
                        target.Append(@"\c");
                        target.Append(chars[++i]);
                    }
                    else if (next is '\0' or '\t' or '\n' or '\r' or '\f' or '\v')
                    {
                        AppendCharOrControlEscape(ref target, next);
                    }
                    else
                    {
                        target.Append('\\');
                        target.Append(next);
                    }
                }
                else
                {
                    target.Append(@"\\");
                }
            }
            else if (c is ']' or '[' or '^')
            {
                target.Append('\\');
                target.Append(c);
            }
            else
            {
                AppendCharOrControlEscape(ref target, c);
            }
        }
    }

    /// <summary>
    /// Appends a character range (<c>[start-end]</c>) to the pattern.
    /// </summary>
    /// <param name="start">The inclusive lower bound character of the range.</param>
    /// <param name="end">The inclusive upper bound character of the range.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <remarks>
    /// Both <paramref name="start"/> and <paramref name="end"/> are treated as literal characters; special character-class
    /// characters (<c>-</c>, <c>\</c>, <c>[</c>, <c>]</c>, <c>^</c>) and control characters (<c>\0</c>, <c>\t</c>, <c>\n</c>,
    /// <c>\r</c>, <c>\f</c>, <c>\v</c>) are automatically escaped.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="start"/> is greater than <paramref name="end"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder Range(char start, char end)
    {
        ThrowIfDisposed();
        if (start > end)
        {
            throw new ArgumentOutOfRangeException(nameof(start), $"Start character '{start}' cannot be greater than end character '{end}'.");
        }

        _lastTokenKind = LastTokenKind.Quantifiable;
        _lastMultiCharLiteralStart = -1;
        _pattern.Append('[');
        AppendEscapedRangeChar(ref _pattern, start);
        _pattern.Append('-');
        AppendEscapedRangeChar(ref _pattern, end);
        _pattern.Append(']');
        return this;
    }

    private static void AppendEscapedRangeChar(ref Utf16ValueStringBuilder target, char c)
    {
        if (c == '-')
        {
            target.Append(@"\x2D");
        }
        else if (c is '\\' or ']' or '[' or '^')
        {
            target.Append('\\');
            target.Append(c);
        }
        else
        {
            AppendCharOrControlEscape(ref target, c);
        }
    }

    private static void AppendCharOrControlEscape(ref Utf16ValueStringBuilder target, char c)
    {
        switch (c)
        {
            case '\0':
                target.Append(@"\x00");
                break;
            case '\t':
                target.Append(@"\t");
                break;
            case '\n':
                target.Append(@"\n");
                break;
            case '\r':
                target.Append(@"\r");
                break;
            case '\f':
                target.Append(@"\f");
                break;
            case '\v':
                target.Append(@"\v");
                break;
            default:
                target.Append(c);
                break;
        }
    }

    // Quantifiers
    /// <summary>
    /// Appends a one-or-more quantifier (<c>+</c>) to the preceding element.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when there is no preceding quantifiable element or the preceding element is already quantified.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder OneOrMore()
    {
        ThrowIfDisposed();
        EnsureQuantifiable();
        AppendQuantifierSuffix('+');
        return this;
    }

    /// <summary>
    /// Appends a zero-or-more quantifier (<c>*</c>) to the preceding element.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when there is no preceding quantifiable element or the preceding element is already quantified.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder ZeroOrMore()
    {
        ThrowIfDisposed();
        EnsureQuantifiable();
        AppendQuantifierSuffix('*');
        return this;
    }

    /// <summary>
    /// Appends an optional quantifier (<c>?</c>) to the preceding element.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when there is no preceding quantifiable element or the preceding element is already quantified.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder Optional()
    {
        ThrowIfDisposed();
        EnsureQuantifiable();
        AppendQuantifierSuffix('?');
        return this;
    }

    /// <summary>
    /// Appends an exact repetition quantifier (<c>{n}</c>) to the preceding element.
    /// </summary>
    /// <param name="n">The exact number of repetitions.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="n"/> is negative.</exception>
    /// <exception cref="InvalidOperationException">Thrown when there is no preceding quantifiable element or the preceding element is already quantified.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder Exactly(int n)
    {
        ThrowIfDisposed();
        if (n < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), n, "Repetition count cannot be negative.");
        }

        EnsureQuantifiable();
        _pattern.Append('{');
        _pattern.Append(n);
        _pattern.Append('}');
        _lastTokenKind = LastTokenKind.QuantifiedGreedy;
        _lastMultiCharLiteralStart = -1;
        return this;
    }

    /// <summary>
    /// Appends a minimum repetition quantifier (<c>{n,}</c>) to the preceding element.
    /// </summary>
    /// <param name="n">The minimum number of repetitions.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="n"/> is negative.</exception>
    /// <exception cref="InvalidOperationException">Thrown when there is no preceding quantifiable element or the preceding element is already quantified.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder AtLeast(int n)
    {
        ThrowIfDisposed();
        if (n < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), n, "Minimum repetition count cannot be negative.");
        }

        EnsureQuantifiable();
        _pattern.Append('{');
        _pattern.Append(n);
        _pattern.Append(",}");
        _lastTokenKind = LastTokenKind.QuantifiedGreedy;
        _lastMultiCharLiteralStart = -1;
        return this;
    }

    /// <summary>
    /// Appends a bounded repetition quantifier (<c>{min,max}</c>) to the preceding element.
    /// </summary>
    /// <param name="min">The minimum number of repetitions.</param>
    /// <param name="max">The maximum number of repetitions.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="min"/> is negative or <paramref name="max"/> is less than <paramref name="min"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when there is no preceding quantifiable element or the preceding element is already quantified.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder Between(int min, int max)
    {
        ThrowIfDisposed();
        if (min < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(min), min, "Minimum repetition count cannot be negative.");
        }

        if (max < min)
        {
            throw new ArgumentOutOfRangeException(nameof(max), max, $"Maximum repetition count ({max}) cannot be less than minimum ({min}).");
        }

        EnsureQuantifiable();
        _pattern.Append('{');
        _pattern.Append(min);
        _pattern.Append(',');
        _pattern.Append(max);
        _pattern.Append('}');
        _lastTokenKind = LastTokenKind.QuantifiedGreedy;
        _lastMultiCharLiteralStart = -1;
        return this;
    }

    /// <summary>
    /// Modifies the preceding greedy quantifier to be non-greedy (lazy) by appending <c>?</c>.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when not called immediately after a greedy quantifier.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder Lazy()
    {
        ThrowIfDisposed();
        if (_lastTokenKind != LastTokenKind.QuantifiedGreedy)
        {
            throw new InvalidOperationException("Lazy() can only be applied immediately after a greedy quantifier.");
        }

        _pattern.Append('?');
        _lastTokenKind = LastTokenKind.QuantifiedLazy;
        return this;
    }

    // Anchors
    /// <summary>
    /// Appends a start-of-string or start-of-line anchor (<c>^</c>) to the pattern.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder StartOfString()
    {
        ThrowIfDisposed();
        _lastTokenKind = LastTokenKind.None;
        _lastMultiCharLiteralStart = -1;
        _pattern.Append("^");
        return this;
    }

    /// <summary>
    /// Appends an end-of-string or end-of-line anchor (<c>$</c>) to the pattern.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder EndOfString()
    {
        ThrowIfDisposed();
        _lastTokenKind = LastTokenKind.None;
        _lastMultiCharLiteralStart = -1;
        _pattern.Append("$");
        return this;
    }

    /// <summary>
    /// Appends a word boundary anchor (<c>\b</c>) to the pattern.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder WordBoundary()
    {
        ThrowIfDisposed();
        _lastTokenKind = LastTokenKind.None;
        _lastMultiCharLiteralStart = -1;
        _pattern.Append(@"\b");
        return this;
    }

    /// <summary>
    /// Appends a non-word boundary anchor (<c>\B</c>) to the pattern.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder NotWordBoundary()
    {
        ThrowIfDisposed();
        _lastTokenKind = LastTokenKind.None;
        _lastMultiCharLiteralStart = -1;
        _pattern.Append(@"\B");
        return this;
    }

    // Groups
    /// <summary>
    /// Appends a capturing group (<c>(...)</c>) built by <paramref name="callback"/>.
    /// </summary>
    /// <param name="callback">An action that configures the pattern inside the group.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <remarks>
    /// If <paramref name="callback"/> throws an exception (and does not dispose the builder),
    /// any partial pattern, quantifier state, and options modified during the callback are rolled back.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder Group(Action<RegexBuilder> callback)
    {
        ThrowIfDisposed();
        if (callback is null)
        {
            throw new ArgumentNullException(nameof(callback));
        }

        return AppendScoped("(", callback, LastTokenKind.Quantifiable);
    }

    /// <summary>
    /// Appends a non-capturing group (<c>(?:...)</c>) built by <paramref name="callback"/>.
    /// </summary>
    /// <param name="callback">An action that configures the pattern inside the group.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <remarks>
    /// If <paramref name="callback"/> throws an exception (and does not dispose the builder),
    /// any partial pattern, quantifier state, and options modified during the callback are rolled back.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder NonCapturingGroup(Action<RegexBuilder> callback)
    {
        ThrowIfDisposed();
        if (callback is null)
        {
            throw new ArgumentNullException(nameof(callback));
        }

        return AppendScoped("(?:", callback, LastTokenKind.Quantifiable);
    }

    /// <summary>
    /// Appends a named capturing group (<c>(?&lt;name&gt;...)</c>) built by <paramref name="callback"/>.
    /// </summary>
    /// <param name="name">The capture group name.</param>
    /// <param name="callback">An action that configures the pattern inside the group.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <remarks>
    /// Group names must start with a letter or underscore and contain only letters, digits, or underscores.
    /// If <paramref name="callback"/> throws an exception (and does not dispose the builder),
    /// any partial pattern, quantifier state, and options modified during the callback are rolled back.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> or <paramref name="callback"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is empty or not a valid .NET regex group name.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder NamedGroup(string name, Action<RegexBuilder> callback)
    {
        ThrowIfDisposed();
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (name.Length == 0)
        {
            throw new ArgumentException("Group name cannot be empty.", nameof(name));
        }

        if (!IsValidGroupName(name))
        {
            throw new ArgumentException(
                $"Invalid group name '{name}'. Group names must start with a letter or underscore and contain only letters, digits, or underscores.",
                nameof(name));
        }

        if (callback is null)
        {
            throw new ArgumentNullException(nameof(callback));
        }

        return AppendScoped($"(?<{name}>", callback, LastTokenKind.Quantifiable);
    }

    // Alternation
    /// <summary>
    /// Appends an alternation operator (<c>|</c>) to the pattern.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder Or()
    {
        ThrowIfDisposed();
        _lastTokenKind = LastTokenKind.None;
        _lastMultiCharLiteralStart = -1;
        _pattern.Append("|");
        return this;
    }

    // Lookahead/Lookbehind
    /// <summary>
    /// Appends a positive lookahead assertion (<c>(?=...)</c>) built by <paramref name="callback"/>.
    /// </summary>
    /// <param name="callback">An action that configures the lookahead pattern.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <remarks>
    /// Lookarounds are zero-width assertions and cannot be directly quantified.
    /// If <paramref name="callback"/> throws an exception (and does not dispose the builder),
    /// any partial pattern, quantifier state, and options modified during the callback are rolled back.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder PositiveLookahead(Action<RegexBuilder> callback)
    {
        ThrowIfDisposed();
        if (callback is null)
        {
            throw new ArgumentNullException(nameof(callback));
        }

        return AppendScoped("(?=", callback, LastTokenKind.None);
    }

    /// <summary>
    /// Appends a negative lookahead assertion (<c>(?!...)</c>) built by <paramref name="callback"/>.
    /// </summary>
    /// <param name="callback">An action that configures the lookahead pattern.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <remarks>
    /// Lookarounds are zero-width assertions and cannot be directly quantified.
    /// If <paramref name="callback"/> throws an exception (and does not dispose the builder),
    /// any partial pattern, quantifier state, and options modified during the callback are rolled back.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder NegativeLookahead(Action<RegexBuilder> callback)
    {
        ThrowIfDisposed();
        if (callback is null)
        {
            throw new ArgumentNullException(nameof(callback));
        }

        return AppendScoped("(?!", callback, LastTokenKind.None);
    }

    /// <summary>
    /// Appends a positive lookbehind assertion (<c>(?&lt;=...)</c>) built by <paramref name="callback"/>.
    /// </summary>
    /// <param name="callback">An action that configures the lookbehind pattern.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <remarks>
    /// Lookarounds are zero-width assertions and cannot be directly quantified.
    /// If <paramref name="callback"/> throws an exception (and does not dispose the builder),
    /// any partial pattern, quantifier state, and options modified during the callback are rolled back.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder PositiveLookbehind(Action<RegexBuilder> callback)
    {
        ThrowIfDisposed();
        if (callback is null)
        {
            throw new ArgumentNullException(nameof(callback));
        }

        return AppendScoped("(?<=", callback, LastTokenKind.None);
    }

    /// <summary>
    /// Appends a negative lookbehind assertion (<c>(?&lt;!...)</c>) built by <paramref name="callback"/>.
    /// </summary>
    /// <param name="callback">An action that configures the lookbehind pattern.</param>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <remarks>
    /// Lookarounds are zero-width assertions and cannot be directly quantified.
    /// If <paramref name="callback"/> throws an exception (and does not dispose the builder),
    /// any partial pattern, quantifier state, and options modified during the callback are rolled back.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder NegativeLookbehind(Action<RegexBuilder> callback)
    {
        ThrowIfDisposed();
        if (callback is null)
        {
            throw new ArgumentNullException(nameof(callback));
        }

        return AppendScoped("(?<!", callback, LastTokenKind.None);
    }

    // Regex Options (Flags)
    /// <summary>
    /// Enables <see cref="RegexOptions.IgnoreCase"/> on the built regular expression.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
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
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
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
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
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
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
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
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
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
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder IgnorePatternWhitespace()
    {
        ThrowIfDisposed();
        _options |= RegexOptions.IgnorePatternWhitespace;
        return this;
    }

    /// <summary>
    /// Enables <see cref="RegexOptions.CultureInvariant"/> on the built regular expression.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder CultureInvariant()
    {
        ThrowIfDisposed();
        _options |= RegexOptions.CultureInvariant;
        return this;
    }

    /// <summary>
    /// Enables <see cref="RegexOptions.RightToLeft"/> on the built regular expression.
    /// </summary>
    /// <returns>The current <see cref="RegexBuilder"/> instance.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexBuilder RightToLeft()
    {
        ThrowIfDisposed();
        _options |= RegexOptions.RightToLeft;
        return this;
    }

    // Build methods
    /// <summary>
    /// Builds and returns a <see cref="Regex"/> from the accumulated pattern and options.
    /// </summary>
    /// <returns>A new <see cref="Regex"/> instance.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public Regex Build()
    {
        ThrowIfDisposed();
        return new Regex(_pattern.ToString(), _options);
    }

    /// <summary>
    /// Builds and returns a <see cref="Regex"/> from the accumulated pattern, options, and match timeout.
    /// </summary>
    /// <param name="matchTimeout">A time-out interval, or <see cref="Regex.InfiniteMatchTimeout"/> to indicate that the method should not time out.</param>
    /// <returns>A new <see cref="Regex"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="matchTimeout"/> is negative (other than <see cref="Regex.InfiniteMatchTimeout"/>), zero, or exceeds the maximum supported timeout.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public Regex Build(TimeSpan matchTimeout)
    {
        ThrowIfDisposed();
        return new Regex(_pattern.ToString(), _options, matchTimeout);
    }

    /// <summary>
    /// Returns the raw regular expression pattern string.
    /// </summary>
    /// <returns>The built regular expression pattern.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public string GetPattern()
    {
        ThrowIfDisposed();
        return _pattern.ToString();
    }

    /// <summary>
    /// Returns the configured <see cref="RegexOptions"/> flags.
    /// </summary>
    /// <returns>The configured <see cref="RegexOptions"/>.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public RegexOptions GetOptions()
    {
        ThrowIfDisposed();
        return _options;
    }

    /// <summary>
    /// Returns a formatted string containing the pattern and options.
    /// </summary>
    /// <returns>A string in the form <c>/{pattern}/{options}</c>.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the builder has been disposed.</exception>
    public override string ToString()
    {
        ThrowIfDisposed();
        return $"/{_pattern.ToString()}/{_options}";
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