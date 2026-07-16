using ECA.Trams.FileTransferAPI.DTO.ETranslation;

namespace ECA.Trams.FileTransferAPI.Services;

/// <summary>
/// Calls downstream TRAMS / Language Cloud web services for eTranslation events.
/// Kept separate from file-transfer orchestration.
/// </summary>
public interface IWebHookService
{
    Task SignalTranslationDeliveriesEventAsync(ETranslationDeliveriesRequest request, string? savedFilePath, CancellationToken cancellationToken = default);

    Task SignalTranslationSuccessEventAsync(ETranslationSuccessRequest request, CancellationToken cancellationToken = default);

    Task SignalTranslationErrorEventAsync(object notification, CancellationToken cancellationToken = default);
}
