using TwentyNet.Application;
using TwentyNet.BFF;
using TwentyNet.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBffServices(builder.Configuration);
builder.Services.AddPersistence();
builder.Services.AddApplication();

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
