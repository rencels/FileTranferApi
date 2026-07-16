using Microsoft.Extensions.Configuration;

namespace ECA.Trams.FileTransferAPI.Utils;

/// <summary>
/// Strongly typed configuration properties.
/// </summary>
public class SettingsContext : ISettingsContext
{
    private readonly IConfiguration _configuration;

    public SettingsContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string PublicKey => _configuration.GetValue<string>("LanguageCloudService:PublicKey") ?? string.Empty;

    public string TempFileLocationOutputPath => _configuration.GetValue<string>("ETranslationService:OutputPath") ?? string.Empty;

    public string WebServicesBaseUrl => _configuration.GetValue<string>("WebServices:BaseUrl") ?? string.Empty;

    public string WebServicesDeliveriesPath =>
        _configuration.GetValue<string>("WebServices:DeliveriesPath") ?? "/api/etranslation/deliveries";

    public string WebServicesSuccessPath =>
        _configuration.GetValue<string>("WebServices:SuccessPath") ?? "/api/etranslation/success";

    public string WebServicesErrorPath =>
        _configuration.GetValue<string>("WebServices:ErrorPath") ?? "/api/etranslation/error";
}
