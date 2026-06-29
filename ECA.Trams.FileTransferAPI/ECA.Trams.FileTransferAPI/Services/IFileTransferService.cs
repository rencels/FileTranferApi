using ECA.Trams.FileTransferAPI.DTO.ETranslation;

namespace ECA.Trams.FileTransferAPI.Services;

public interface IFileTransferService
{
    Task ProcessNotificationAsync(ETranslationDeliveriesRequest request);

    Task ProcessNotificationAsync(ETranslationSuccessRequest request);

    Task ProcessNotificationAsync(object notification);
}

