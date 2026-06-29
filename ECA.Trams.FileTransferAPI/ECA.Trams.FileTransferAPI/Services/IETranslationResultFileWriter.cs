using ECA.Trams.FileTransferAPI.DTO.ETranslation;
using System.Threading.Tasks;

namespace ECA.Trams.FileTransferAPI.Services;

public interface IETranslationResultFileWriter
{
    string WriteResultToFile(long requestId, string languageCode, string base64Result, string outputFormat);
}
