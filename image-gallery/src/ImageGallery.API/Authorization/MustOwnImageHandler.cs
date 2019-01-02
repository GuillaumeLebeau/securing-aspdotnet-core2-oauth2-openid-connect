using System;
using System.Security.Claims;
using System.Threading.Tasks;

using ImageGallery.API.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ImageGallery.API.Authorization
{
    public class MustOwnImageHandler : AuthorizationHandler<MustOwnImageRequirement>
    {
        private readonly IGalleryRepository _galleryRepository;

        public MustOwnImageHandler(IGalleryRepository galleryRepository)
        {
            _galleryRepository = galleryRepository;
        }
        
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            MustOwnImageRequirement requirement)
        {
            var filterContext = context.Resource as AuthorizationFilterContext;
            if (filterContext == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var imageId = filterContext.RouteData.Values["id"].ToString();

            if (!Guid.TryParse(imageId, out Guid imageIdAsGuid))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var ownerId = context.User.FindFirstValue("sub");

            if (!_galleryRepository.IsImageOwner(imageIdAsGuid, ownerId))
            {
                context.Fail();
                return Task.CompletedTask;
            }
            
            // all checks out
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}