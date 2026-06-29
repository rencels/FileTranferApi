using ECA.Trams.FileTransferAPI.Services;
using ECA.Trams.FileTransferAPI.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<ISettingsContext, SettingsContext>();
builder.Services.AddScoped<IETranslationResultFileWriter, ETranslationResultFileWriter>();
builder.Services.AddScoped<IFileTransferService, FileTransferService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
