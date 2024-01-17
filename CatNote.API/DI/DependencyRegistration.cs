using System.Security.Claims;
using CatNote.API.Configurations;
using CatNote.API.Mappers;
using CatNote.BLL.Mappers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace CatNote.API.DI;

public static class DependencyRegistration
{
    private static string[] openIdArray = { "openid" };

    public static void AddApiServices(this IServiceCollection services, AuthenticationConfiguration authConfig)
    {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:3000", "http://proud-flower-0c97f3910.4.azurestaticapps.net")
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = $"https://{authConfig.Domain}/";
                    options.Audience = authConfig.Audience;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = ClaimTypes.NameIdentifier,

                        ValidateIssuerSigningKey = false,
                        IssuerSigningKey = authConfig.GetSecurityKey()
                    };
                });

            services.AddAutoMapper(typeof(MapperApiProfile).Assembly, typeof(MapperBllProfile).Assembly);

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "API Documentation",
                    Version = "v1.0",
                    Description = ""
                });
                options.ResolveConflictingActions(x => x.First());
                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    BearerFormat = "JWT",
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"https://{authConfig.Domain}/authorize?audience={authConfig.Audience}"),
                            TokenUrl = new Uri($"https://{authConfig.Domain}/oauth/token"),
                            Scopes = new Dictionary<string, string>
                            {
                                { "openid", "OpenId" },

                            }
                        }
                    }
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                        },
                        openIdArray
                    }
                });
            });
    }
}
