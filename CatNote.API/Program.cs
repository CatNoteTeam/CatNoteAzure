using CatNote.API.Configurations;
using CatNote.API.DI;
using CatNote.API.Middlewares;
using CatNote.BLL.DI;
using CatNote.DAL;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

namespace CatNote.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddFluentValidationAutoValidation();

        var connection = builder.Environment.IsDevelopment()
            ? builder.Configuration.GetConnectionString("DefaultConnection")
            : "Data Source=:memory:";

        if (!builder.Environment.IsDevelopment())
        {
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDataBase");
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });
        }

        builder.Services.AddBusinessServices(connection);

        var authConfig = builder.Configuration.GetSection("Auth0").Get<AuthenticationConfiguration>();

        builder.Services.AddApiServices(authConfig!);

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1.0");
                c.OAuthClientId(builder.Configuration["Auth0:ClientId"]);
                c.OAuthClientSecret(builder.Configuration["Auth0:ClientSecret"]);
                c.OAuthUsePkce();
            });
        }

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
