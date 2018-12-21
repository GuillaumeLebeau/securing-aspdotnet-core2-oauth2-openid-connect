using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using ImageGallery.Model;

namespace ImageGallery.Client.Services
{
    public class ImageGalleryApiClient : IImageGalleryApiClient
    {
        private readonly string _apiUrl;

        public ImageGalleryApiClient(string apiUrl)
        {
            _apiUrl = apiUrl;
        }

        public async Task<List<Image>> GetImages()
        {
            return await _apiUrl
                    .AppendPathSegment("api/images")
                    .GetJsonAsync<List<Image>>()
                    .ConfigureAwait(false);
        }

        public async Task<Image> GetImage(Guid id)
        {
            return await _apiUrl
                .AppendPathSegment($"api/images/{id}")
                .GetJsonAsync<Image>()
                .ConfigureAwait(false);
        }

        public async Task UpdateImage(Guid id, ImageForUpdate imageForUpdate)
        {
            await _apiUrl
                .AppendPathSegment($"api/images/{id}")
                .PutJsonAsync(imageForUpdate)
                .ConfigureAwait(false);
        }

        public async Task DeleteImage(Guid id)
        {
            await _apiUrl
                .AppendPathSegment($"api/images/{id}")
                .DeleteAsync()
                .ConfigureAwait(false);
        }

        public async Task AddImage(ImageForCreation imageForCreation)
        {
            await _apiUrl
                .AppendPathSegment($"api/images")
                .PostJsonAsync(imageForCreation)
                .ConfigureAwait(false);
        }
    }
}