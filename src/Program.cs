using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using PhotoSite.Authentication;
using PhotoSite.Bundling;
using PhotoSite.Crypto;
using PhotoSite.Library;
using PhotoSite.Models;
using PhotoSite.Users;

var builder = WebApplication.CreateBuilder(args);
var appSettings = new AppSettings();

builder.Configuration.Bind("AppSettings", appSettings);

// Add services to the container.
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

app.Run(async (context) => {
    context.Response.Headers.Add("Referrer-Policy", "strict-origin");
 context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("Content-Security-Policy", "script-src 'self'");
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
});

app.Run();
