using System.Collections.Generic;
using ImageGallery.Model;

namespace ImageGallery.Client.Models
{
    public class GalleryIndexViewModel
    {
        public IEnumerable<Image> Images { get; private set; } = new List<Image>();

        public GalleryIndexViewModel(List<Image> images)
        {
            Images = images;
        }
    }
}