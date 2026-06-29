using System.Text.Json.Serialization;

namespace ECA.Trams.FileTransferAPI.DTO.ETranslation;

public class ETranslationDeliveriesRequest
{
    [JsonPropertyName("requestId")]
    [JsonRequired]
    public long RequestId { get; set; }

    [JsonPropertyName("sourceLanguage")]
    public string SourceLanguage { get; set; }

    [JsonPropertyName("targetLanguage")]
    public string TargetLanguage { get; set; }

    [JsonPropertyName("result")]
    public string Result { get; set; }

    [JsonPropertyName("outputFormat")]
    public string OutputFormat { get; set; }
}
