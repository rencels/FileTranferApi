namespace ECA.Trams.FileTransferAPI.Utils;

/// <summary>
/// Strongly typed configuration properties.
/// </summary>
public interface ISettingsContext
{
    /// <summary>
    /// Gets full path to the certificate/public key.
    /// </summary>
    string PublicKey { get; }

    /// <summary>
    /// Gets the directory where eTranslation result files are written.
    /// </summary>
    string TempFileLocationOutputPath { get; }

    /// <summary>
    /// Base URL of the downstream web services API.
    /// </summary>
    string WebServicesBaseUrl { get; }

    /// <summary>
    /// Relative path for deliveries notifications.
    /// </summary>
    string WebServicesDeliveriesPath { get; }

    /// <summary>
    /// Relative path for success notifications.
    /// </summary>
    string WebServicesSuccessPath { get; }

    /// <summary>
    /// Relative path for error notifications.
    /// </summary>
    string WebServicesErrorPath { get; }
}
