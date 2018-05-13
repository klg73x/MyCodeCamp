using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCodeCamp.Controllers;
using MyCodeCamp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCodeCamp.Models
{
    public class CampUrlResolver : IValueResolver<Camp, CampModel, string>
    {
        private IHttpContextAccessor _httpcontextaccessor;

        public CampUrlResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpcontextaccessor = httpContextAccessor;
        }
        public string Resolve(Camp source, CampModel destination, string destMember, ResolutionContext context)
        {
            var url = (IUrlHelper)_httpcontextaccessor.HttpContext.Items[BaseController.UrlHelper];
            return url.Link("CampGet", new { moniker = source.Moniker });
        }
    }
}
