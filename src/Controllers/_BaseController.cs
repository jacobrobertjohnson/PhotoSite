using Microsoft.AspNetCore.Mvc;

namespace PhotoSite.Controllers;

public abstract class _BaseController : Controller {
    protected IServiceProvider _dependencies;

    public _BaseController(IServiceProvider dependencies)
    {
        _dependencies = dependencies;
    }
}