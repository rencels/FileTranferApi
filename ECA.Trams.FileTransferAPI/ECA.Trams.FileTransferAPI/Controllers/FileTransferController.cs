using ECA.Trams.FileTransferAPI.DTO.ETranslation;
using ECA.Trams.FileTransferAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ECA.Trams.FileTransferAPI.Controllers;

/// <summary>
/// Defines the API for eTranslation webhook notifications.
/// </summary>
[Controller]
[Route("webhook/etranslation")]
public class FileTransferController : ControllerBase
{
    //private readonly IEcaLogger _logger;
    private readonly IFileTransferService _webhookService;

    public FileTransferController(IFileTransferService fileTransferService)
    {
        _webhookService = fileTransferService;
    }

    /// <summary>
    /// Receive the messages from the event source
    /// </summary>
    /// <param name="notification">The event notification</param>    
    [HttpPost]
    [Route("v1/test")]
    [Consumes("application/json", "text/json")]
    public async Task<IActionResult> Test([FromBody] object notification)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification), "Parameter cannot be null.");
            }

            var outputDirectory = "/tmp/etranslation";
            Directory.CreateDirectory(outputDirectory);
            var fileName = "sebaTest.txt";
            var filePath = GetSafeFilePath(outputDirectory, fileName);

            System.IO.File.WriteAllText(filePath, notification.ToString());

            await _webhookService.ProcessNotificationAsync(notification);

            return StatusCode((int)System.Net.HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex);
            return StatusCode((int)System.Net.HttpStatusCode.InternalServerError);
        }
    }

    private static string GetSafeFilePath(string outputDirectory, string fileName)
    {
        var fullOutputDirectory = Path.GetFullPath(outputDirectory);
        var fullFilePath = Path.GetFullPath(Path.Combine(fullOutputDirectory, fileName));

        var outputDirectoryPrefix = fullOutputDirectory.EndsWith(Path.DirectorySeparatorChar)
            ? fullOutputDirectory
            : fullOutputDirectory + Path.DirectorySeparatorChar;

        if (!fullFilePath.StartsWith(outputDirectoryPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The resolved file path is outside the configured output directory.");
        }

        return fullFilePath;
    }

    /// <summary>
    /// Receives translation delivery notifications from eTranslation.
    /// </summary>
    [HttpPost]
    [Route("v1/deliveries")]
    [Consumes("application/json", "text/json")]
    public async Task<IActionResult> HandleDeliveriesNotification([FromBody] ETranslationDeliveriesRequest notification)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification), "Parameter cannot be null.");
            }

            await _webhookService.ProcessNotificationAsync(notification);

            return StatusCode((int)System.Net.HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex);
            return StatusCode((int)System.Net.HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Receives translation success notifications from eTranslation.
    /// </summary>
    [HttpPost]
    [Route("v1/success")]
    [Consumes("application/json", "text/json")]
    public async Task<IActionResult> HandleSuccessNotification([FromBody] ETranslationSuccessRequest notification)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification), "Parameter cannot be null.");
            }

            await _webhookService.ProcessNotificationAsync(notification);

            return StatusCode((int)System.Net.HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex);
            return StatusCode((int)System.Net.HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Receives translation error notifications from eTranslation.
    /// </summary>
    [HttpPost]
    [Route("v1/error")]
    [Consumes("application/json", "text/json")]
    public async Task<IActionResult> HandleErrorNotification([FromBody] object notification)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification), "Parameter cannot be null.");
            }

            await _webhookService.ProcessNotificationAsync(notification);

            return StatusCode((int)System.Net.HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex);
            return StatusCode((int)System.Net.HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Returns the list of result files stored for a given eTranslation request.
    /// </summary>
    [HttpGet]
    [Route("v1/documents/{requestId:long}")]
    public IActionResult GetDocumentsList(long requestId)
    {
        try
        {
            var files = _webhookService.GetFilesList(requestId);
            return Ok(files);
        }
        catch (Exception)
        {
            //_logger.LogError(ex);
            return StatusCode((int)System.Net.HttpStatusCode.InternalServerError);
        }
    }
}
