using ECA.Trams.FileTransferAPI.Services; 
using ECA.Trams.FileTransferAPI.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ISettingsContext, SettingsContext>();
builder.Services.AddScoped<IETranslationResultFileWriter, ETranslationResultFileWriter>();
builder.Services.AddScoped<IFileTransferService, FileTransferService>();
builder.Services.AddHttpClient<IWebHookService, WebHookService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Azure terminates TLS at the front door; do not force HTTPS redirect inside the container.
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
