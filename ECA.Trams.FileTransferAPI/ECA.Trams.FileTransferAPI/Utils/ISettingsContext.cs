namespace ECA.Trams.FileTransferAPI.Utils;

/// <summary>
/// Defines the stongly type configuration properties
/// </summary>
public interface ISettingsContext
{
    /// <summary>
    /// Gets full path to the certificate/public key
    /// </summary>
    string PublicKey { get; }

    /// <summary>
    /// Gets the directory where eTranslation result files are written.
    /// </summary>
    string TempFileLocationOutputPath { get; }


}

