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
    public List<string> Families { get; set; } = new List<string>();
    public List<string> PhotoFamilies { get; set; } = new List<string>();
    public List<string> PhotoDeleteFamilies { get; set; } = new List<string>();
    public List<string> PhotoPermanentDeleteFamilies { get; set; } = new List<string>();
}