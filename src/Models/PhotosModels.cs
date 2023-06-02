using Microsoft.AspNetCore.Mvc;

namespace PhotoSite.Models;

public class Photos_Index_AspModel : ModelBase {
    public Photos_Index_AspModel(object vueModel) : base(vueModel) { }
    public List<Photos_Index_SidebarItem> Sidebar { get; set; } = new List<Photos_Index_SidebarItem>();
    public List<Photo> Thumbnails { get; set; } = new List<Photo>();
    public string FamilyId { get; set; }
}

public class Photos_Index_VueModel {
    public string FamilyId { get; set; }
}

public class Photos_Index_SidebarItem {
    public string Label { get; set; }
    public string Url { get; set; }
    public List<Photos_Index_SidebarItem> Children { get; set; } = null;
}

public class QueryPhoto {
    public string Id { get; set; }
    public string Extension { get; set; }
    public DateTime DateTaken { get; set; }
    public string OriginalFilename { get; internal set; }
}

public class PhotoDates {
    public string Label { get; set; }
    public List<Photo> Photos { get; set; } = new List<Photo>();
}

public class Photo {
    public Photo() { }

    public Photo(string familyId, IUrlHelper url, QueryPhoto dbPhoto) {
        ThumbnailUrl = $"/Photos/{familyId}/Thumbnails/[size]/{dbPhoto.Id}{dbPhoto.Extension}";
        ViewerUrl = $"/Photos/{familyId}/Viewer/{dbPhoto.Id}{dbPhoto.Extension}";
        Id = dbPhoto.Id;
    }

    public static List<Photo> MakeList(string familyId, IUrlHelper url, List<QueryPhoto> dbPhotos) {
        return dbPhotos.Select(p => new Photo(familyId, url, p)).ToList();
    }
    
    public string ThumbnailUrl { get; set; }
    public string ViewerUrl { get; set; }
    public string Id { get; set; }
}

public class Photos_Viewer_AspModel : ModelBase {
    public Photos_Viewer_AspModel(object vueModel) : base(vueModel) { }
    public string PhotoUrl { get; set; }
    public string PrevPhotoUrl { get; set; }
    public string NextPhotoUrl { get; set; }
    public string Filename { get; set; }
    public string FamilyId { get; internal set; }
}

public class DeletePhoto_Request {
    public string[] fileIds { get; set; } = new string[0];
}