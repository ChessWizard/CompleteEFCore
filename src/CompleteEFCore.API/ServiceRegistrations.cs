using Carter;
using CompleteEFCore.API.Domain.Entities.Northwind;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace CompleteEFCore.API;

public static class ServiceRegistrations
{
    public static void AddLibraries(this IServiceCollection services)
    {
        // Swagger
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "CompleteEFCore Service API", Version = "v1" });
        });
            
        services.AddEndpointsApiExplorer();

        // Carter
        services.AddCarter();

        // MediatR
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(Program).Assembly);
        });
    }
    
    public static void AddDbContext(this WebApplicationBuilder builder)
    {
        builder.Services
               .AddDbContext<NorthwindContext>((serviceProvider, options) =>
               {
                   options.UseNpgsql(builder.Configuration.GetSection("DbConnection:Northwind").Value);
               });
    }
    
    public static void ConfigureLibraries(this WebApplication app)
    {
        // Swagger
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.DisplayRequestDuration();
            });
        }

        // Carter
        app.MapCarter();
    }
}