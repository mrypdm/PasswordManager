using System;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.Abstractions.Checkers;
using PasswordManager.Abstractions.Crypto;
using PasswordManager.Abstractions.Factories;
using PasswordManager.Abstractions.Validators;
using PasswordManager.Aes;
using PasswordManager.Core.Checkers;
using PasswordManager.Core.Factories;
using PasswordManager.External.Checkers;
using PasswordManager.Options;
using PasswordManager.SecureData.Contexts;
using PasswordManager.SecureData.KeyStorage;
using PasswordManager.SecureData.Repositories;
using PasswordManager.SecureData.Services;
using PasswordManager.UserSettings;
using PasswordManager.Web.Extensions;
using PasswordManager.Web.Options;

namespace PasswordManager.Web.Extensions;

/// <summary>
/// Extensions for <see cref="WebApplicationBuilder"/>
/// </summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Add user options to web application
    /// </summary>
    public static async Task<WebApplicationBuilder> AddUserOptionsAsync(this WebApplicationBuilder builder,
        int secondsTimeout = 10)
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(secondsTimeout));
        var uesrOptionsPath = builder.Configuration.GetValue<string>("UserOptionsPath");
        var userSettings = await JsonWriteableOptions.CreateAsync<UserOptions>(uesrOptionsPath, cancellation.Token);
        builder.Services.AddSingleton<IWritableOptions<UserOptions>>(userSettings);
        return builder;
    }

    /// <summary>
    /// Add connection options to web application
    /// </summary>
    public static WebApplicationBuilder AddConnectionOptions(this WebApplicationBuilder builder)
    {
        builder.Services
            .Configure<ConnectionOptions>(builder.Configuration.GetSection("ConnectionOptions"));
        return builder;
    }

    /// <summary>
    /// Add dev options to web application
    /// </summary>
    public static WebApplicationBuilder AddDevOptions(this WebApplicationBuilder builder)
    {
        builder.Services
            .Configure<DevOptions>(builder.Configuration.GetSection("DevOptions"));
        return builder;
    }

    /// <summary>
    /// Add AES crypto to web application
    /// </summary>
    public static WebApplicationBuilder AddAesCrypto(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddSingleton<IKeyGeneratorFactory, AesKeyGeneratorFactory>()
            .AddScoped(services =>
            {
                var userOptions = services.GetRequiredService<IWritableOptions<UserOptions>>();
                var factory = services.GetRequiredService<IKeyGeneratorFactory>();
                return factory.Create(userOptions.Value.MasterKeySaltBytes, userOptions.Value.MasterKeyIterations);
            })
            .AddScoped<IKeyValidator, AesKeyValidator>()
            .AddScoped<ICrypto, AesCrypto>();
        return builder;
    }

    /// <summary>
    /// Add password checkers to web application
    /// </summary>
    public static WebApplicationBuilder AddPasswordCheckers(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddScoped<IPasswordCheckerFactory, EntropyPasswordCheckerFactory>()
            .AddScoped<IPasswordChecker, SeaMonkeyPasswordChecker>()
            .AddScoped<IPasswordChecker, ZxcvbnPasswordChecker>()
            .AddScoped<IPasswordChecker, PwnedPasswordChecker>();
        return builder;
    }

    /// <summary>
    /// Add password generator factory to web application
    /// </summary>
    public static WebApplicationBuilder AddPasswordGeneratorFactory(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddScoped<IPasswordGeneratorFactory, PasswordGeneratorFactory>();
        return builder;
    }

    /// <summary>
    /// Add secure db to web application
    /// </summary>
    public static WebApplicationBuilder AddSecureDb(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddDbContext<SecureDbContext>(opt => opt.UseSqlite(builder.Configuration.GetConnectionString("SecureDb")))
            .AddSingleton<IMasterKeyStorage, MasterKeyStorage>()
            .AddScoped<ISecureItemsRepository, SecureItemsRepository>()
            .AddScoped<IMasterKeyDataRepository, SecureItemsRepository>()
            .AddScoped<IMasterKeyService, MasterKeyService>();
        return builder;
    }

    /// <summary>
    /// Add cookie authentiaction to web application
    /// </summary>
    public static WebApplicationBuilder AddCookieAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(opt =>
            {
                opt.LoginPath = "/login";
                opt.LogoutPath = opt.LoginPath;
                opt.AccessDeniedPath = "/error";
                opt.Events.OnValidatePrincipal = context =>
                {
                    if (!context.ValidatePrincipal())
                    {
                        context.RejectPrincipal();
                    }
                    return Task.CompletedTask;
                };
            });
        builder.Services.AddAuthorization(opt => opt.FallbackPolicy = opt.DefaultPolicy);
        return builder;
    }

    /// <summary>
    /// Add routing to web application
    /// </summary>
    public static WebApplicationBuilder AddRazorPages(this WebApplicationBuilder builder)
    {
        builder.Services
            .ConfigureHttpJsonOptions(opt => opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter()))
            .AddRouting(opt => opt.LowercaseUrls = true);
        builder.Services
            .AddControllers()
            .AddJsonOptions(opt => opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        builder.Services
            .AddRazorPages();
        return builder;
    }

    /// <summary>
    /// Add connection options to web application
    /// </summary>
    public static WebApplicationBuilder AddSwagger(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(opt =>
            {
                opt.IncludeXmlComments(
                    Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
            });
        return builder;
    }
}
