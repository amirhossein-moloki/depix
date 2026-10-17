using System.Text;

namespace Depix.Cli;

public class ParsedCommand
{
    public string RawInput { get; }
    public string CommandName { get; }
    public List<string> Arguments { get; }

    public ParsedCommand(string rawInput, string commandName, List<string> arguments)
    {
        RawInput = rawInput;
        CommandName = commandName;
        Arguments = arguments;
    }

    public string? GetOptionValue(string optionName)
    {
        for (int i = 0; i < Arguments.Count; i++)
        {
            if (string.Equals(Arguments[i], optionName, StringComparison.OrdinalIgnoreCase) && i + 1 < Arguments.Count)
            {
                return Arguments[i + 1];
            }
        }
        return null;
    }

    public bool HasOption(string optionName)
    {
        return Arguments.Any(a => string.Equals(a, optionName, StringComparison.OrdinalIgnoreCase));
    }
}

public static class CommandParser
{
    public static ParsedCommand Parse(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return new ParsedCommand(string.Empty, string.Empty, new List<string>());
        }

        var tokens = Tokenize(line.Trim());
        if (tokens.Count == 0)
        {
            return new ParsedCommand(line, string.Empty, new List<string>());
        }

        var commandName = tokens[0].ToLowerInvariant();
        var arguments = tokens.Skip(1).ToList();

        return new ParsedCommand(line, commandName, arguments);
    }

    public static List<string> Tokenize(string input)
    {
        var tokens = new List<string>();
        if (string.IsNullOrWhiteSpace(input)) return tokens;

        var sb = new StringBuilder();
        bool inQuotes = false;
        char quoteChar = '\0';
        bool escaped = false;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (escaped)
            {
                sb.Append(c);
                escaped = false;
                continue;
            }

            if (c == '\\')
            {
                escaped = true;
                continue;
            }

            if (inQuotes)
            {
                if (c == quoteChar)
                {
                    inQuotes = false;
                }
                else
                {
                    sb.Append(c);
                }
            }
            else
            {
                if (c == '"' || c == '\'')
                {
                    inQuotes = true;
                    quoteChar = c;
                }
                else if (char.IsWhiteSpace(c))
                {
                    if (sb.Length > 0)
                    {
                        tokens.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
        }

        if (sb.Length > 0)
        {
            tokens.Add(sb.ToString());
        }

        return tokens;
    }
}
