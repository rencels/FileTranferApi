namespace ECA.Trams.FileTransferAPI.Services;

public interface IETranslationResultFileWriter
{
    string WriteResultToFile(long requestId, string languageCode, string base64Result, string outputFormat);

    List<string> GetFilesList(long requestId);
}
