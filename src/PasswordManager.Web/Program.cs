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
    .AddUserOptionsAsync();

var application = builder.Build();
application
    .UseExceptionHandler("/error")
    .UseHsts()
    .UseHttpsRedirection()
    .UseStaticFiles()
    .UseRouting()
    .UseAuthentication()
    .UseAuthorization();
application.MapRazorPages();
application.MapControllers();

await application.MigrateDatabaseAsync(application.Lifetime.ApplicationStopping);

application.Run();
