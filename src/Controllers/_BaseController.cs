using Microsoft.AspNetCore.Mvc;
using PhotoSite.Models;
using Newtonsoft.Json;
using PhotoSite.Authentication;

namespace PhotoSite.Controllers;

public abstract class _BaseController : Controller {
    protected const int ONE_YEAR_IN_SECONDS = 31536000;

    protected readonly Dictionary<int, string> _monthNames = new() {
        { 1, "January" },
        { 2, "February" },
        { 3, "March" },
        { 4, "April" },
        { 5, "May" },
        { 6, "June" },
        { 7, "July" },
        { 8, "August" },
        { 9, "September" },
        { 10, "October" },
        { 11, "November" },
        { 12, "December" },
    };

    protected IServiceProvider _dependencies;
    protected IAuthenticator _authenticator;

    protected readonly Dictionary<string, UserPermissions> _families;
    protected readonly bool _userAdmin;
    protected readonly int _userId;

    public _BaseController(IServiceProvider dependencies)
    {
        _dependencies = dependencies;
        _authenticator = dependencies.GetService<IAuthenticator>();

        _families = JsonConvert.DeserializeObject<Dictionary<string, UserPermissions>>(
            _authenticator.GetClaimValue("families") ?? "{}");

        _userAdmin = bool.Parse(_authenticator.GetClaimValue("userAdmin") ?? "false");
        _userId = int.Parse(_authenticator.GetClaimValue("userId") ?? "-1");
    }
}