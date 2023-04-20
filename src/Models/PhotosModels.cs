namespace PhotoSite.Models;

public class Photos_Index_VueModel {
    public List<Photos_Index_SidebarItem> Sidebar { get; set; } = new List<Photos_Index_SidebarItem>();
}

public class Photos_Index_SidebarItem {
    public string Label { get; set; }
    public string PhotoUrl { get; set; }
}