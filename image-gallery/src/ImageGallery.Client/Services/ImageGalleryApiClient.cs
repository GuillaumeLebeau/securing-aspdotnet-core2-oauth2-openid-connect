using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using ImageGallery.Model;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace ImageGallery.Client.Services
{
    public class ImageGalleryApiClient : IImageGalleryApiClient
    {
        private readonly ImageGalleryApiClientSettings _settings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ImageGalleryApiClient(ImageGalleryApiClientSettings settings, IHttpContextAccessor httpContextAccessor)
        {
            _settings = settings;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<Image>> GetImages()
        {
            return await _settings.Url
                    .AppendPathSegment("api/images")
                    .WithOAuthBearerToken(await GetAccessToken().ConfigureAwait(false))
                    .GetJsonAsync<List<Image>>()
                    .ConfigureAwait(false);
        }

        public async Task<Image> GetImage(Guid id)
        {
            return await _settings.Url
                .AppendPathSegment($"api/images/{id}")
                .WithOAuthBearerToken(await GetAccessToken().ConfigureAwait(false))
                .GetJsonAsync<Image>()
                .ConfigureAwait(false);
        }

        public async Task UpdateImage(Guid id, ImageForUpdate imageForUpdate)
        {
            await _settings.Url
                .AppendPathSegment($"api/images/{id}")
                .WithOAuthBearerToken(await GetAccessToken().ConfigureAwait(false))
                .PutJsonAsync(imageForUpdate)
                .ConfigureAwait(false);
        }

        public async Task DeleteImage(Guid id)
        {
            await _settings.Url
                .AppendPathSegment($"api/images/{id}")
                .DeleteAsync()
                .ConfigureAwait(false);
        }

        public async Task AddImage(ImageForCreation imageForCreation)
        {
            await _settings.Url
                .AppendPathSegment($"api/images")
                .WithOAuthBearerToken(await GetAccessToken().ConfigureAwait(false))
                .PostJsonAsync(imageForCreation)
                .ConfigureAwait(false);
        }

        private async Task<string> GetAccessToken()
        {
            var accessToken = string.Empty;

            // get the current HttpContext to access the tokens
            var currentContext = _httpContextAccessor.HttpContext;
            
            // get access token
            accessToken =
                await currentContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            return accessToken;
        }
    }
}