using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AspNetCoreRateLimit;
using CompanyEmployees.Controllers;
using Contracts;
using Entities;
using Entities.Models;
using LoggerService;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repository;

namespace CompanyEmployees.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services) =>
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                    builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });

        public static void ConfigureIISIntegration(this IServiceCollection services) =>
            services.Configure<IISOptions>(options =>
            {

            });

        public static void ConfigureLoggerService(this IServiceCollection services) =>
            services.AddScoped<ILoggerManager, LoggerManager>();

        public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration) =>
            services.AddDbContext<RepositoryContext>(opts =>
                opts.UseSqlServer(configuration.GetConnectionString("sqlConnection"), b => b.MigrationsAssembly("CompanyEmployees")));

        public static void ConfigureRepositoryManager(this IServiceCollection services) =>
           services.AddScoped<IRepositoryManager, RepositoryManager>();

        public static IMvcBuilder AddCustomCSVFormatter(this IMvcBuilder builder) =>
            builder.AddMvcOptions(config => config.OutputFormatters.Add(new CsvOutputFormatter()));

        public static void AddCustomMediaTypes(this IServiceCollection services)
        {
            services.Configure<MvcOptions>(config =>
            {
                var newtonsoftJsonOutputFormatter =
                    config.OutputFormatters.OfType<NewtonsoftJsonOutputFormatter>()?.FirstOrDefault();

                if (newtonsoftJsonOutputFormatter != null)
                {
                    newtonsoftJsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.codemaze.hateoas+json");
                    newtonsoftJsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.codemaze.apiroot+json");
                }

                var xmlOutputFormatter = config.OutputFormatters.OfType<XmlDataContractSerializerOutputFormatter>()
                    ?.FirstOrDefault();

                if (xmlOutputFormatter != null)
                {
                    xmlOutputFormatter.SupportedMediaTypes.Add("application/vnd.codemaze.hateoas+xml");
                    xmlOutputFormatter.SupportedMediaTypes.Add("application/vnd.codemaze.apiroot+xml");
                }
            });
        }

        public static void ConfigureVersioning(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ApiVersionReader = new HeaderApiVersionReader("api-version");
                options.Conventions.Controller<CompaniesController>().HasApiVersion(new ApiVersion(1, 0));
                options.Conventions.Controller<CompaniesV2Controller>().HasDeprecatedApiVersion(new ApiVersion(2, 0));
            });
        }

        public static void ConfigureResponseCaching(this IServiceCollection serviceCollection) =>
            serviceCollection.AddResponseCaching();

        public static void ConfigureHttpCacheHeaders(this IServiceCollection serviceCollection) =>
            serviceCollection.AddHttpCacheHeaders((options) =>
            {
                options.MaxAge = 65;
                options.CacheLocation = CacheLocation.Private;
            }, options =>
            {
                options.MustRevalidate = true;
            });

        public static void ConfigureRateLimitingOptions(this IServiceCollection serviceCollection)
        {
            var rateLimitRules = new List<RateLimitRule>
            {
                new RateLimitRule
                {
                    Endpoint = "*",
                    Limit = 30,
                    Period = "5m"
                }
            };

            serviceCollection.Configure<IpRateLimitOptions>(options =>
            {
                options.GeneralRules = rateLimitRules;
            });

            serviceCollection.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            serviceCollection.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            serviceCollection.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        }

        public static void ConfigureIdentity(this IServiceCollection serviceCollection)
        {
            var builder = serviceCollection.AddIdentityCore<User>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 10;
                options.User.RequireUniqueEmail = true;
            });

            builder = new IdentityBuilder(builder.UserType, typeof(IdentityRole), builder.Services);
            builder.AddEntityFrameworkStores<RepositoryContext>().AddDefaultTokenProviders();
        }

        public static void ConfigureJWT(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JWTSettings");
            var secretKey = Environment.GetEnvironmentVariable("SECRET");

            serviceCollection.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSettings.GetSection("validIssuer").Value,
                    ValidAudience = jwtSettings.GetSection("validAudience").Value,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });
        }

        public static void ConfigureSwagger(this IServiceCollection services)
        {
            // Creating 2 versions of Swagger Documentation - 2 company controllers
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Jacques Thurling API",
                    Version = "v1",
                    Description = "CompanyEmployees API by Jacques Thurling",
                    TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "John Doe",
                        Email = "John.Doe@gmail.com",
                        Url = new Uri("https://twiiter.com/johndoe")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "CompanyEmployees API LICX",
                        Url = new Uri("https://example.com/license")
                    }
                });
                options.SwaggerDoc("v2", new OpenApiInfo {Title = "Jacques Thurling API", Version = "v2"});

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                // options.IncludeXmlComments(xmlPath);

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Place to add JWT with Bearer",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                //Added Security definition in our configuration
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Name = "Bearer",
                        },
                        new List<string>()
                    }
                });


            });
        }
    }
}
