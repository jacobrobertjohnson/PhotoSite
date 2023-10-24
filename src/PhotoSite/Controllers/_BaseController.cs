using Microsoft.AspNetCore.Mvc;

namespace PhotoSite.Controllers;

public abstract class _BaseController : Controller {
    protected const int ONE_YEAR_IN_SECONDS = 31536000;

    protected readonly Dictionary<int, string> _monthNames = new() {
        { 1, "January" },
        { 2, "February" },
        { 3, "March" },
        { 4, "April" },
        { 5, "May" },
        { 6, "June" },
        { 7, "July" },
        { 8, "August" },
        { 9, "September" },
        { 10, "October" },
        { 11, "November" },
        { 12, "December" },
    };

    protected IServiceProvider _dependencies;

    public _BaseController(IServiceProvider dependencies)
    {
        _dependencies = dependencies;
    }
}