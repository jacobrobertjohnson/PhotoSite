namespace PhotoSite.Models;

public class UserPermissions {
    public PhotoPermissions Photos { get; set; } = new();
    public bool ShowOnHome {
        get => Photos.Enabled;
    }
}

public class PhotoPermissions {
    public bool Enabled { get; set; }
    public bool Delete { get; set; }
    public bool DeletePermanently { get; set; }
}

public class Users_Index_Model {
    public Users_Index_User[] Users { get; set; } = new Users_Index_User[0];
}

public class Users_Index_User {
    public string Username { get; set; }
    public int UserId { get; set; }
}

public class Users_User_Model {
    public int UserId { get; set; }
    public string Username { get; set; }
    public string NewPassword { get; set; }
    public bool Enabled { get; set; }
    public bool UserAdmin { get; set; }
    public Dictionary<string, UserPermissions> Permissions { get; set; } = new();
}