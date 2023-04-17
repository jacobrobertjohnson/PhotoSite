using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using PhotoSite.Authentication;

namespace PhotoSite.Models;

public class Home_Index_Model {
    IServiceProvider _dependencies;
    AppSettings _config;
    IAuthenticator _authenticator;
    string[] _families;

    public Home_Index_Model(IServiceProvider dependencies)
    {
        _dependencies = dependencies;
        _config = dependencies.GetService<AppSettings>();
        _authenticator = dependencies.GetService<IAuthenticator>();
        _families = _authenticator.GetClaimValue("families").Split(',');
    }

    public List<Home_Index_Family> Families {
        get => _config.Families
            .Where(fam => _families.Contains(fam.Id))
            .Select(fam => new Home_Index_Family(_dependencies, fam))
            .Append(Home_Index_Family.MakeGlobalSection(_dependencies))
            .ToList();
    }
}

public class Home_Index_Family
{
    string[] _photosFamilies;
    IAuthenticator _authenticator;

    public Home_Index_Family(IServiceProvider dependencies) {
        _authenticator = dependencies.GetService<IAuthenticator>();
        _photosFamilies = _authenticator.GetClaimValue("families").Split(',');
    }

    public Home_Index_Family(IServiceProvider dependencies, Family fam) : this(dependencies) {
        var urlHelper = dependencies.GetService<IUrlHelper>();

        Name = fam.Name;

        if (_photosFamilies.Contains(fam.Id))
            Apps.Add(new Home_Index_App() {
                Name = "Photos",
                Url = urlHelper.Action("Index", "Photos")
            });
    }

    public string Name { get; set; }

    public List<Home_Index_App> Apps { get; set; } = new List<Home_Index_App>();

    internal static Home_Index_Family MakeGlobalSection(IServiceProvider dependencies)
    {
        var urlHelper = dependencies.GetService<IUrlHelper>();

        return new Home_Index_Family(dependencies)
        {
            Name = "Global",
            Apps = new List<Home_Index_App>() {
                new Home_Index_App() {
                    Name = "Log Out",
                    Url = urlHelper.Action("Logout", "Login")
                }
            }
        };
    }
}

public class Home_Index_App {
    public string Name { get; set; }
    public string Url { get; set; }
}