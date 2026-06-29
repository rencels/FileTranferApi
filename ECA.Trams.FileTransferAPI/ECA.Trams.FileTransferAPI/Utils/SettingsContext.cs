using Microsoft.Extensions.Configuration;

namespace ECA.Trams.FileTransferAPI.Utils;

/// <summary>
/// Defines the stongly type configuration properties
/// </summary>
public class SettingsContext : ISettingsContext
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Creates a new instance of <see cref="SettingsContext"/>
    /// </summary>
    /// <param name="configuration"></param>
    public SettingsContext(IConfiguration configuration)
    {
        this._configuration = configuration;
    }

    /// <summary>
    /// Gets full path to the certificate/public key
    /// </summary>
    public string PublicKey { get { return this._configuration.GetValue<string>("LanguageCloudService:PublicKey"); } }

    /// <summary>
    /// Gets the directory where eTranslation result files are written.
    /// </summary>
    public string TempFileLocationOutputPath { get { return this._configuration.GetValue<string>("ETranslationService:OutputPath"); } }

}