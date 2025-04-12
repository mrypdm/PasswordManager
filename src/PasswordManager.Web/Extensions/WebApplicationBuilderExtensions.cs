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
using PasswordManager.Abstractions.Contexts;
using PasswordManager.Abstractions.Counters;
using PasswordManager.Abstractions.Crypto;
using PasswordManager.Abstractions.Factories;
using PasswordManager.Abstractions.Options;
using PasswordManager.Abstractions.Repositories;
using PasswordManager.Abstractions.Services;
using PasswordManager.Abstractions.Storages;
using PasswordManager.Abstractions.Validators;
using PasswordManager.Aes.Crypto;
using PasswordManager.Aes.Factories;
using PasswordManager.Aes.Validators;
using PasswordManager.Core.Counters;
using PasswordManager.Core.Factories;
using PasswordManager.Core.Options;
using PasswordManager.Core.Services;
using PasswordManager.Core.Storages;
using PasswordManager.Data.Contexts;
using PasswordManager.Data.Repositories;
using PasswordManager.External.Factories;
using PasswordManager.Web.Helpers;
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
    public static WebApplicationBuilder ConfigureConnectionOptions(this WebApplicationBuilder builder)
    {
        builder.Services
            .Configure<ConnectionOptions>(builder.Configuration.GetSection(nameof(ConnectionOptions)));
        return builder;
    }

    /// <summary>
    /// Add AES crypto to web application
    /// </summary>
    public static WebApplicationBuilder AddAesCrypto(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddSingleton<IKeyGeneratorFactory, AesKeyGeneratorFactory>()
            .AddSingleton<IKeyValidatorFactory, AesKeyValidatorFactory>()
            .AddSingleton<IKeyValidator, SimpleAesKeyValidator>()
            .AddSingleton<ICrypto, AesCrypto>()
            .AddScoped(services =>
            {
                var userOptions = services.GetRequiredService<IWritableOptions<UserOptions>>();
                var factory = services.GetRequiredService<IKeyGeneratorFactory>();
                return factory.Create(userOptions.Value.SaltBytes, userOptions.Value.Iterations);
            });
        return builder;
    }

    /// <summary>
    /// Add password checkers to web application
    /// </summary>
    public static WebApplicationBuilder AddPasswordCheckers(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddSingleton<IPasswordCheckerFactory>(services =>
            {
                return new CombinedPasswordCheckerFactory([
                    new EntropyPasswordCheckerFactory(),
                    new SeaMonkeyPasswordCheckerFactory(),
                    new ZxcvbnPasswordCheckerFactory(),
                    new PwnedPasswordCheckerFactory()]);
            });
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
            .AddScoped<IDataContext, DataContext>()
            .AddDbContext<SecureDbContext>(opt => opt.UseSqlite(builder.Configuration.GetConnectionString("SecureDb")))
            .AddScoped<ISecureItemsRepository, SecureItemsRepository>()
            .AddScoped<IKeyDataRepository, KeyDataRepository>();
        return builder;
    }

    /// <summary>
    /// Add core services
    /// </summary>
    public static WebApplicationBuilder AddCore(this WebApplicationBuilder builder)
    {
        builder.Services
            .Configure<KeyServiceOptions>(builder.Configuration.GetSection(nameof(KeyServiceOptions)))
            .AddSingleton<ICounter, Counter>()
            .AddSingleton<IKeyStorage, KeyStorage>()
            .AddScoped<IKeyService, KeyService>()
            .AddScoped<IAccountService, AccountService>();
        return builder;
    }

    /// <summary>
    /// Add cookie authentiaction to web application
    /// </summary>
    public static WebApplicationBuilder AddCookieAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddScoped<ICookieAuthorizationHelper, CookieAuthorizationHelper>()
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(opt =>
            {
                opt.LoginPath = "/auth/login";
                opt.LogoutPath = "/auth/logout";
                opt.Events.OnValidatePrincipal = context =>
                {
                    var helper = context.HttpContext.RequestServices
                        .GetRequiredService<ICookieAuthorizationHelper>();

                    if (!helper.ValidatePrincipal(context))
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
    public static WebApplicationBuilder AddControllers(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddAntiforgery(opt =>
            {
                opt.HeaderName = "X-CSRF-TOKEN";
            })
            .ConfigureHttpJsonOptions(opt => opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter()))
            .AddRouting(opt => opt.LowercaseUrls = true);
        builder.Services
            .AddControllersWithViews()
            .AddJsonOptions(opt => opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
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
