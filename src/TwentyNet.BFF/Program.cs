using TwentyNet.Application;
using TwentyNet.BFF;
using TwentyNet.BFF.Hubs;
using TwentyNet.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBffServices(builder.Configuration);
builder.Services.AddPersistence();
builder.Services.AddApplication();

builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var frontendPath = Path.Combine(app.Environment.ContentRootPath, "Frontend");
if (Directory.Exists(frontendPath))
{
    var frontendFileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(frontendPath);

    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = frontendFileProvider,
        RequestPath = ""
    });

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = frontendFileProvider,
        RequestPath = ""
    });

    app.MapFallbackToFile("index.html", new StaticFileOptions
    {
        FileProvider = frontendFileProvider
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<WorkspaceHub>("/hubs/workspace");

app.Run();
