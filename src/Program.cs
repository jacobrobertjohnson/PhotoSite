using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using PhotoSite.Authentication;
using PhotoSite.Bundling;
using PhotoSite.Crypto;
using PhotoSite.Filters;
using PhotoSite.Library;
using PhotoSite.Models;
using PhotoSite.Users;

var builder = WebApplication.CreateBuilder(args);
var appSettings = new AppSettings();

builder.Configuration.Bind("AppSettings", appSettings);

// Add services to the container.
builder.Services.AddMvc(options =>
{
    options.Filters.Add(typeof(ExceptionLogFilter));
});
builder.Services.AddControllersWithViews();
builder.Services.AddResponseCaching();

builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

builder.Services.AddAuthentication(options => {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options => {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Login";
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.HttpOnly = true;
    });

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<BundleFile>();
builder.Services.AddSingleton<AppSettings>(appSettings);
builder.Services.AddSingleton<IAuthenticator, Authenticator>();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddSingleton<ISqliteContext, SqliteContext>();
builder.Services.AddSingleton<ILibraryProvider, SqliteLibraryProvider>();
builder.Services.AddSingleton<ICryptoProvider, CryptoProvider>();
builder.Services.AddSingleton<IUserProvider, SqliteUserProvider>();

builder.Services.AddScoped<IUrlHelper>(factory =>
{
    var actionContext = factory.GetService<IActionContextAccessor>().ActionContext;
    return factory.GetService<IUrlHelperFactory>().GetUrlHelper(actionContext);
});

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(appSettings.DataProtectionKeyPath))
    .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration()
    {
        EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
        ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Use(async (context, next) => {
    context.Response.Headers["Referrer-Policy"] = "strict-origin";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Content-Security-Policy"] = "default-src https 'self' 'unsafe-inline'";
    context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Permissions-Policy"] = "fullscreen=(self), microphone=(), camera=(), geolocation=()";

    await next(context);
});

app.Run();
