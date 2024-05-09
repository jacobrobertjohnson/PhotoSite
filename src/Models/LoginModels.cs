namespace PhotoSite.Models;

public class Login_Index_Model {
    public string Message { get; set; } = null;
    public string MessageType { get; set; } = null;
}

public class Login_Authenticate_Request {
    public string Username { get; set; }
    public string Password { get; set; }
}

public class AuthenticatedUser {
    public string Username { get; set; }
    public int UserId { get; set; }
    public bool UserAdmin { get; set; }
    public Dictionary<string, UserPermissions> Families { get; set; } = new();
}