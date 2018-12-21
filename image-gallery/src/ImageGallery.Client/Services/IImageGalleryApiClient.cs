using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImageGallery.Model;

namespace ImageGallery.Client.Services
{
    public interface IImageGalleryApiClient
    {
        Task<List<Image>> GetImages();
        Task<Image> GetImage(Guid id);
        Task UpdateImage(Guid id, ImageForUpdate imageForUpdate);
        Task DeleteImage(Guid id);
        Task AddImage(ImageForCreation imageForCreation);
    }
}