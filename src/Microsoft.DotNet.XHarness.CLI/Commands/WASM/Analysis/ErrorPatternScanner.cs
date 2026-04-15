using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.DotNet.XHarness.Common.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.DotNet.XHarness.CLI.Commands.Wasm;

public class ErrorPatternScanner
{
    private const int MaxRegexPatternLength = 100_000;
    private const int MaxCompiledRegexPatterns = 500;

    private readonly ILogger _logger;
    private readonly List<string> _errorPatternStrings = new();
    private readonly List<Regex> _errorPatternRegexes = new();
    private readonly bool _empty;

    public ErrorPatternScanner(string patternsFile, ILogger logger)
    {
        _logger = logger;
        if (string.IsNullOrEmpty(patternsFile))
            throw new ArgumentNullException(nameof(patternsFile));

        HostPathSecurity.ThrowIfUnsafeHostPath(patternsFile, nameof(patternsFile));
        if (!File.Exists(patternsFile))
            throw new FileNotFoundException(patternsFile);

        FilePayloadSecurity.ThrowIfFileExceedsMaxBytes(patternsFile, nameof(patternsFile));

        foreach (string line in File.ReadAllLines(patternsFile))
        {
            if (line.Trim().Length <= 1)
                continue;

            char type = line[0];
            string pattern = line[1..];

            switch (type)
            {
                case '#':
                    // comment
                    break;

                case '@':
                    _errorPatternStrings.Add(pattern);
                    break;

                case '%':
                    {
                        if (pattern.Length > MaxRegexPatternLength)
                        {
                            _logger.LogWarning($"ErrorPatternScanner: Skipping regex pattern longer than {MaxRegexPatternLength} characters.");
                            break;
                        }
                        if (_errorPatternRegexes.Count >= MaxCompiledRegexPatterns)
                        {
                            _logger.LogWarning($"ErrorPatternScanner: Maximum number of compiled regex patterns ({MaxCompiledRegexPatterns}) reached; ignoring the rest.");
                            break;
                        }
                        try
                        {
                            _errorPatternRegexes.Add(RegexSecurity.Create(pattern, RegexOptions.IgnoreCase));
                        }
                        catch (Exception ex) when (ex is ArgumentException || ex is ArgumentNullException || ex is ArgumentOutOfRangeException)
                        {
                            _logger.LogWarning($"ErrorPatternScanner: Failed to compile regex error pattern '{pattern}': {ex.Message}");
                        }
                    }
                    break;

                default:
                    _logger.LogWarning($"ErrorPatternScanner: Unknown type prefix '{type}' on line '{line}'. Ignoring.");
                    break;
            }
        }

        _empty = _errorPatternRegexes.Count == 0 && _errorPatternStrings.Count == 0;
    }

    public bool IsError(string line, out string? matchedPattern)
    {
        matchedPattern = null;
        if (_empty)
            return false;

        string? patternString = _errorPatternStrings.FirstOrDefault(pattern => line.Contains(pattern, StringComparison.InvariantCultureIgnoreCase));
        if (patternString != null)
        {
            matchedPattern = patternString;
            return true;
        }

        Regex? matchedRegex = _errorPatternRegexes.FirstOrDefault(regex => regex.IsMatch(line));
        if (matchedRegex != null)
        {
            matchedPattern = matchedRegex.ToString();
            return true;
        }

        return false;
    }
}
