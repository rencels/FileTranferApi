using System.Text.Json.Serialization;

namespace ECA.Trams.FileTransferAPI.DTO.ETranslation;

public class ETranslationSuccessRequest
{
    [JsonPropertyName("requestId")]
    [JsonRequired]
    public long RequestId { get; set; }

    [JsonPropertyName("sourceLanguage")]
    public string SourceLanguage { get; set; }

    [JsonPropertyName("targetLanguage")]
    public string TargetLanguage { get; set; }
}
