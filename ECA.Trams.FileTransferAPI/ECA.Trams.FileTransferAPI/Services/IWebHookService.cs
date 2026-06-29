namespace ECA.Trams.FileTransferAPI.Services
{
    public interface IWebHookService
    {
        Task SignalTranslationDeliveriesEventAsync();
        
        Task SignaTranslationSuccessEventAsync();
        
        Task SignaTranslationErrorEventAsync();
    }
}

