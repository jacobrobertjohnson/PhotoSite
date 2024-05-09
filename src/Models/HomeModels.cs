using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using PhotoSite.Authentication;

namespace PhotoSite.Models;

public class Home_Index_Model {
    IServiceProvider _dependencies;
    Dictionary<string, UserPermissions> _families;
    bool _userAdmin;

    public Home_Index_Model(IServiceProvider dependencies, Dictionary<string, UserPermissions> families, bool userAdmin)
    {
        _dependencies = dependencies;
        _families = families;
        _userAdmin = userAdmin;
    }

    public List<Home_Index_Family> Families {
        get => _families
            .Select(fam => new Home_Index_Family(_dependencies, fam.Value, fam.Key))
            .Append(Home_Index_Family.MakeGlobalSection(_dependencies, _userAdmin))
            .ToList();
    }
}

public class Home_Index_Family {
    public Home_Index_Family() { }

    public Home_Index_Family(IServiceProvider dependencies, UserPermissions fam, string name) {
        var urlHelper = dependencies.GetService<IUrlHelper>();

        Name = dependencies.GetService<AppSettings>()
            .Families
            .First(f => f.Id == name)
            .Name;

        if (fam.Photos.Enabled)
            Apps.Add(new Home_Index_App() {
                Name = "Photos",
                Icon = "🖼️",
                Url = urlHelper.Action("Index", "Photos", new { familyId = name })
            });
    }

    public string Name { get; set; }

    public List<Home_Index_App> Apps { get; set; } = new List<Home_Index_App>();

    internal static Home_Index_Family MakeGlobalSection(IServiceProvider dependencies, bool userAdmin)
    {
        var urlHelper = dependencies.GetService<IUrlHelper>();
        var group = new Home_Index_Family()
        {
            Name = "Global",
            Apps = new(),
        };

        if (userAdmin)
            group.Apps.Add(new Home_Index_App() {
                Name = "Users",
                Icon = "🙎‍♂️",
                Url = urlHelper.Action("Index", "Users")
            });

        group.Apps.Add(new Home_Index_App() {
            Name = "Change Password",
            Icon = "🔑",
            Url = urlHelper.Action("Password", "UserPrefs")
        });

        group.Apps.Add(new Home_Index_App() {
            Name = "Log Out",
            Icon = "🚪",
            Url = urlHelper.Action("Logout", "Login")
        });

        return group;
    }
}

public class Home_Index_App {
    public string Name { get; set; }
    public string Url { get; set; }
    public string Icon { get; set; }
}