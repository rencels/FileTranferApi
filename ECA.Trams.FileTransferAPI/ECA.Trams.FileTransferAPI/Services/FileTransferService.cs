using ECA.Trams.FileTransferAPI.DTO.ETranslation;
using Microsoft.Extensions.Logging;

namespace ECA.Trams.FileTransferAPI.Services;

/// <summary>
/// Orchestrates eTranslation webhook handling: persist results, then notify web services.
/// </summary>
public class FileTransferService : IFileTransferService
{
    private readonly IETranslationResultFileWriter _fileWriter;
    private readonly IWebHookService _webHookService;
    private readonly ILogger<FileTransferService> _logger;

    public FileTransferService(
        IETranslationResultFileWriter fileWriter,
        IWebHookService webHookService,
        ILogger<FileTransferService> logger)
    {
        _fileWriter = fileWriter;
        _webHookService = webHookService;
        _logger = logger;
    }

    public async Task ProcessNotificationAsync(ETranslationDeliveriesRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        string? savedFilePath = null;
        if (!string.IsNullOrWhiteSpace(request.Result)
            && !string.IsNullOrWhiteSpace(request.TargetLanguage)
            && !string.IsNullOrWhiteSpace(request.OutputFormat))
        {
            savedFilePath = _fileWriter.WriteResultToFile(
                request.RequestId,
                request.TargetLanguage,
                request.Result,
                request.OutputFormat);

            _logger.LogInformation(
                "Saved eTranslation delivery result for request {RequestId} to {FilePath}.",
                request.RequestId,
                savedFilePath);
        }

        await _webHookService.SignalTranslationDeliveriesEventAsync(request, savedFilePath);
    }

    public async Task ProcessNotificationAsync(ETranslationSuccessRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        await _webHookService.SignalTranslationSuccessEventAsync(request);
    }

    public async Task ProcessNotificationAsync(object notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        await _webHookService.SignalTranslationErrorEventAsync(notification);
    }
}
