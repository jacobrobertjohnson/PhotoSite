namespace PhotoSite.Models;

public class Login_Index_Model {
    public string Message { get; set; } = null;
    public string MessageType { get; set; } = null;
}

public class Login_Authenticate_Request {
    public string Username { get; set; }
    public string Password { get; set; }
}