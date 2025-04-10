using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PasswordManager.Web.Extensions;

var builder = await WebApplication
    .CreateBuilder(args)
    .ConfigureConnectionOptions()
    .AddAesCrypto()
    .AddPasswordCheckers()
    .AddPasswordGeneratorFactory()
    .AddSecureDb()
    .AddCookieAuthentication()
    .AddControllers()
    .AddUserOptionsAsync();

if (builder.Environment.IsDevelopment())
{
    builder.AddSwagger();
}

var application = builder.Build();
application
    .UseHsts()
    .UseHttpsRedirection()
    .UseStaticFiles()
    .UseRouting();

if (builder.Environment.IsDevelopment())
{
    application
        .UseSwagger()
        .UseSwaggerUI();
    application.MapSwagger();
}

application
    .UseAuthentication()
    .UseAuthorization();

application.MapControllers();

await application.MigrateDatabaseAsync(application.Lifetime.ApplicationStopping);

application.Run();
