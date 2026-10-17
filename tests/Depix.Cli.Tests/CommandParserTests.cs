using Depix.Cli;
using Xunit;

namespace Depix.Cli.Tests;

public class CommandParserTests
{
    [Fact]
    public void Parse_EmptyLine_ReturnsEmptyCommand()
    {
        var result = CommandParser.Parse("   ");
        Assert.Equal(string.Empty, result.CommandName);
        Assert.Empty(result.Arguments);
    }

    [Fact]
    public void Parse_SingleWord_ReturnsCommandNameInLowerCase()
    {
        var result = CommandParser.Parse("HELP");
        Assert.Equal("help", result.CommandName);
        Assert.Empty(result.Arguments);
    }

    [Fact]
    public void Parse_CommandWithArguments_ParsesArgumentsCorrectly()
    {
        var result = CommandParser.Parse("users find user@example.com");
        Assert.Equal("users", result.CommandName);
        Assert.Equal(2, result.Arguments.Count);
        Assert.Equal("find", result.Arguments[0]);
        Assert.Equal("user@example.com", result.Arguments[1]);
    }

    [Fact]
    public void Parse_QuotedStrings_PreservesSpacesInsideQuotes()
    {
        var result = CommandParser.Parse("eval \"var name = \\\"Depix\\\";\"");
        Assert.Equal("eval", result.CommandName);
        Assert.Single(result.Arguments);
        Assert.Equal("var name = \"Depix\";", result.Arguments[0]);
    }

    [Fact]
    public void GetOptionValue_ReturnsValueFollowingOption()
    {
        var result = CommandParser.Parse("logs errors --last 1h");
        Assert.Equal("1h", result.GetOptionValue("--last"));
    }
}
