using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.Web.Extensions;

var builder = await WebApplication
    .CreateBuilder(args)
    .AddConnectionOptions()
    .AddAesCrypto()
    .AddPasswordCheckers()
    .AddPasswordGeneratorFactory()
    .AddSecureDb()
    .AddCookieAuthentication()
    .AddRazorPages()
    .AddSwagger()
    .AddUserOptionsAsync();

var application = builder.Build();
application
    .UseHsts()
    .UseHttpsRedirection()
    .UseStaticFiles()
    .UseRouting()
    .UseSwagger()
    .UseSwaggerUI()
    .UseAuthentication()
    .UseAuthorization();

application.MapControllers();
application.MapSwagger();

await application.MigrateDatabaseAsync(application.Lifetime.ApplicationStopping);

application.Run();
