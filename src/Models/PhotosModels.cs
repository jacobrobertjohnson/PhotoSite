using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace PhotoSite.Models;

public class Photos_Index_VueModel {
    public string FamilyId { get; set; }
    public List<Photos_Index_SidebarItem> Sidebar { get; set; } = new List<Photos_Index_SidebarItem>();
}

public class Photos_Index_SidebarItem {
    public string Label { get; set; }
    public string PhotoUrl { get; set; }
}

public class QueryPhoto {
    public string Id { get; set; }
    public string Extension { get; set; }
    public DateTime DateTaken { get; set; }
}

public class PhotoDates {
    public string Label { get; set; }
    public List<Photo> Photos { get; set; } = new List<Photo>();
}

public class Photo {
    public Photo() { }

    public Photo(string familyId, IUrlHelper url, QueryPhoto dbPhoto) {
        ThumbnailUrl = $"/Photos/{familyId}/Thumbnails/[size]/{dbPhoto.Id}";
        Id = dbPhoto.Id;
    }

    public static List<Photo> MakeList(string familyId, IUrlHelper url, List<QueryPhoto> dbPhotos) {
        return dbPhotos.Select(p => new Photo(familyId, url, p)).ToList();
    }
    
    public string ThumbnailUrl { get; set; }
    public string Id { get; set; }
}