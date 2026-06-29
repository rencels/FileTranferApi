using ECA.Trams.FileTransferAPI.DTO.ETranslation;

namespace ECA.Trams.FileTransferAPI.Services;

public class FileTransferService : IFileTransferService
{
    public Task ProcessNotificationAsync(ETranslationDeliveriesRequest request)
    {
        throw new NotImplementedException();
    }

    public Task ProcessNotificationAsync(ETranslationSuccessRequest request)
    {
        throw new NotImplementedException();
    }

    public Task ProcessNotificationAsync(object notification)
    {
        throw new NotImplementedException();
    }
}

