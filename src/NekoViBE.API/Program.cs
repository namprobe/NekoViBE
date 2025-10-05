using NekoViBE.API.Configurations;
using NekoViBE.API.Extensions;

var builder = WebApplication.CreateBuilder(args)
    .ConfigureServices();

builder.Environment.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

var app = builder.Build()
    .ConfigurePipeline();

// Configure application with proper order: migrations → hangfire → jobs
await app.ConfigureApplicationAsync();

app.Run();
