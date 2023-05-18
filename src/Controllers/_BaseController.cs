using Microsoft.AspNetCore.Mvc;

namespace PhotoSite.Controllers;

public abstract class _BaseController : Controller {
    protected const int ONE_YEAR_IN_SECONDS = 31536000;
    protected IServiceProvider _dependencies;

    public _BaseController(IServiceProvider dependencies)
    {
        _dependencies = dependencies;
    }
}