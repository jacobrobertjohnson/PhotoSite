namespace PhotoSite.Models;

public class UserPrefs_Password_Model {
    public UserPrefs_Password_Model(string result) {
        if (result == "success") {
            Message = "Your password was successfully changed";
            MessageType = "succeeded";
        } else if (result == "currentPasswordRequired") {
            Message = "The Current Password field is required";
            MessageType = "failed";
        } else if (result == "newPasswordRequired") {
            Message = "The New Password field is required";
            MessageType = "failed";
        } else if (result == "confirmPasswordRequired") {
            Message = "The Confirm New Password field is required";
            MessageType = "failed";
        } else if (result == "mismatch") {
            Message = "The New Password and Confirm New Password fields must match";
            MessageType = "failed";
        } else if (result == "currentPasswordWrong") {
            Message = "An incorrect Current Password was provided";
            MessageType = "failed";
        }
    }

    public string Message { get; set; } = null;
    public string MessageType { get; set; } = null;
}

public class UserPrefs_ChangePassword_Request {
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmNewPassword { get; set; }
}