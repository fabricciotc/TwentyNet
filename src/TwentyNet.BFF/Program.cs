using TwentyNet.Application;
using TwentyNet.BFF;
using TwentyNet.BFF.GraphQL;
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

builder.Services
    .AddGraphQLServer()
    .AddQueryType<TwentyNet.BFF.GraphQL.Queries.QueryType>()
    .AddType<TwentyNet.BFF.GraphQL.Queries.QueryType>()
    .AddMutationType<TwentyNet.BFF.GraphQL.Mutations.MutationType>()
    .AddType<TwentyNet.BFF.GraphQL.Mutations.MutationType>()
    .AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var frontendPath = System.IO.Path.Combine(app.Environment.ContentRootPath, "Frontend");
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
app.MapGraphQL();
app.MapHub<WorkspaceHub>("/hubs/workspace");

app.Run();
