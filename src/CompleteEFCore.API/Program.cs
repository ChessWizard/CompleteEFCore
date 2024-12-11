using CompleteEFCore.API;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddLibraries();

builder.AddDbContext();

var app = builder.Build();

app.ConfigureLibraries();

app.Run();
