using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MyCodeCamp.Controllers
{
    [Produces("application/json")]
    [Route("api/Base")]
    public abstract class BaseController : Controller
    {
        public const string UrlHelper = "UrlHelper";
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            context.HttpContext.Items[UrlHelper] = this.Url;
        }
    }
}