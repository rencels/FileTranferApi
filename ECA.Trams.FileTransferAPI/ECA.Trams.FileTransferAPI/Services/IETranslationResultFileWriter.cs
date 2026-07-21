namespace ECA.Trams.FileTransferAPI.Services;

public interface IETranslationResultFileWriter
{
    string WriteResultToFile(long requestId, string languageCode, string base64Result, string outputFormat);

    string WriteResultToFile(string payloadContent);

    List<string> GetFilesList(long requestId);
}
