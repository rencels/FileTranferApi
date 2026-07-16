using System.Net.Http.Json;
using System.Text.Json;
using ECA.Trams.FileTransferAPI.DTO.ETranslation;
using ECA.Trams.FileTransferAPI.Utils;
using Microsoft.Extensions.Logging;

namespace ECA.Trams.FileTransferAPI.Services;

/// <summary>
/// HTTP client that notifies downstream web services about eTranslation events.
/// </summary>
public class WebHookService : IWebHookService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly HttpClient _httpClient;
    private readonly ISettingsContext _settings;
    private readonly ILogger<WebHookService> _logger;

    public WebHookService(HttpClient httpClient, ISettingsContext settings, ILogger<WebHookService> logger)
    {
        _httpClient = httpClient;
        _settings = settings;
        _logger = logger;
    }

    public Task SignalTranslationDeliveriesEventAsync(ETranslationDeliveriesRequest request, string? savedFilePath, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            eventType = "deliveries",
            request.RequestId,
            request.SourceLanguage,
            request.TargetLanguage,
            request.OutputFormat,
            request.ExternalReference,
            savedFilePath
        };

        return PostEventAsync("deliveries", payload, cancellationToken);
    }

    public Task SignalTranslationSuccessEventAsync(ETranslationSuccessRequest request, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            eventType = "success",
            request.RequestId,
            request.SourceLanguage,
            request.TargetLanguage
        };

        return PostEventAsync("success", payload, cancellationToken);
    }

    public Task SignalTranslationErrorEventAsync(object notification, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            eventType = "error",
            notification
        };

        return PostEventAsync("error", payload, cancellationToken);
    }

    private async Task PostEventAsync(string eventName, object payload, CancellationToken cancellationToken)
    {
        var baseUrl = _settings.WebServicesBaseUrl;
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            _logger.LogWarning("WebServices:BaseUrl is not configured; skipping {EventName} web service call.", eventName);
            return;
        }

        var relativePath = eventName switch
        {
            "deliveries" => _settings.WebServicesDeliveriesPath,
            "success" => _settings.WebServicesSuccessPath,
            "error" => _settings.WebServicesErrorPath,
            _ => $"/api/etranslation/{eventName}"
        };

        var requestUri = CombineUrl(baseUrl, relativePath);

        using var response = await _httpClient.PostAsJsonAsync(requestUri, payload, JsonOptions, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Web service call for {EventName} failed with {StatusCode}. Response: {Body}",
                eventName,
                (int)response.StatusCode,
                body);
            response.EnsureSuccessStatusCode();
        }

        _logger.LogInformation("Web service notified for eTranslation {EventName}.", eventName);
    }

    private static string CombineUrl(string baseUrl, string relativePath)
    {
        return $"{baseUrl.TrimEnd('/')}/{relativePath.TrimStart('/')}";
    }
}
