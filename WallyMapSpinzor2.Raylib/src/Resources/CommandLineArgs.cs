using System.Collections.Generic;
using Raylib_cs;

namespace WallyMapSpinzor2.Raylib;

public class CommandLineArgs
{
    private readonly Dictionary<string, string> argDict = [];

    public void ParseArg(string arg)
    {
        string[] parts = arg.Split('=', 2);
        if (parts.Length != 2)
        {
            Rl.TraceLog(TraceLogLevel.Warning, $"Argument with invalid format: {arg}");
            return;
        }
        argDict[parts[0]] = parts[1];
    }

    public void ParseArgs(string[] args)
    {
        foreach (string arg in args)
            ParseArg(arg);
    }

    public static CommandLineArgs CreateFromArgs(string[] args)
    {
        CommandLineArgs result = new();
        result.ParseArgs(args);
        return result;
    }

    public bool TryGetArg(string key, out string? value) => argDict.TryGetValue(key, out value);
}