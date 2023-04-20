using Newtonsoft.Json;

namespace PhotoSite.Models;

public class ModelBase {
    public ModelBase() { }
    
    public ModelBase(object vueModel) {
        VueModel = JsonConvert.SerializeObject(vueModel);
    }

    public string VueModel { get; set; }
}