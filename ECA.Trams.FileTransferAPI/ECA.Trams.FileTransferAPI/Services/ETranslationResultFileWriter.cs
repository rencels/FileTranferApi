using ECA.Trams.FileTransferAPI.Utils;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ECA.Trams.FileTransferAPI.Services;

public partial class ETranslationResultFileWriter : IETranslationResultFileWriter
{
    private readonly ISettingsContext _settings;
    
    public ETranslationResultFileWriter(ISettingsContext settings)
    {
        _settings = settings;
    }

    public string WriteResultToFile(long requestId, string languageCode, string base64Result, string outputFormat)
    {
        if (string.IsNullOrWhiteSpace(base64Result))
        {
            throw new ArgumentException("Result cannot be null or empty.", nameof(base64Result));
        }

        if (string.IsNullOrWhiteSpace(languageCode))
        {
            throw new ArgumentException("Language code cannot be null or empty.", nameof(languageCode));
        }

        if (string.IsNullOrWhiteSpace(outputFormat))
        {
            throw new ArgumentException("Output format cannot be null or empty.", nameof(outputFormat));
        }

        var outputDirectory = _settings.TempFileLocationOutputPath;
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            throw new InvalidOperationException("ETranslationService:OutputPath is not configured.");
        }

        Directory.CreateDirectory(outputDirectory);

        var safeLanguageCode = ValidatePathSegment(languageCode, nameof(languageCode));
        var safeOutputFormat = ValidatePathSegment(outputFormat.TrimStart('.'), nameof(outputFormat));
        var fileName = $"{requestId}-{safeLanguageCode}.{safeOutputFormat}";
        var filePath = GetSafeFilePath(outputDirectory, fileName);

        byte[] content;
        try
        {
            content = Convert.FromBase64String(base64Result);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Result is not valid Base64.", nameof(base64Result), ex);
        }

        File.WriteAllBytes(filePath, content);
        
        return filePath;
    }

    private static string ValidatePathSegment(string value, string paramName)
    {
        if (!SafePathSegmentPattern().IsMatch(value))
        {
            throw new ArgumentException("Value contains invalid path characters.", paramName);
        }

        return value;
    }

    private static string GetSafeFilePath(string outputDirectory, string fileName)
    {
        var fullOutputDirectory = Path.GetFullPath(outputDirectory);
        var fullFilePath = Path.GetFullPath(Path.Combine(fullOutputDirectory, fileName));

        var outputDirectoryPrefix = fullOutputDirectory.EndsWith(Path.DirectorySeparatorChar)
            ? fullOutputDirectory
            : fullOutputDirectory + Path.DirectorySeparatorChar;

        if (!fullFilePath.StartsWith(outputDirectoryPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The resolved file path is outside the configured output directory.");
        }

        return fullFilePath;
    }

    [GeneratedRegex("^[a-zA-Z0-9-]+$")]
    private static partial Regex SafePathSegmentPattern();
}
