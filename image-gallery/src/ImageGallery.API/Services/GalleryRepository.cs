using System;
using System.Collections.Generic;
using System.Linq;
using ImageGallery.API.Entities;

namespace ImageGallery.API.Services
{
    public class GalleryRepository : IGalleryRepository, IDisposable
    {
        private readonly GalleryContext _context;

        public GalleryRepository(GalleryContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        ~GalleryRepository()
        {
            Dispose(false);
        }

        public IEnumerable<Image> GetImages(string ownerId)
        {
            return _context.Images.Where(i => i.OwnerId == ownerId).OrderBy(i => i.Title).ToList();
        }

        public bool IsImageOwner(Guid id, string ownerId)
        {
            return _context.Images.Any(i => i.Id == id && i.OwnerId == ownerId);
        }

        public Image GetImage(Guid id)
        {
            return _context.Images.SingleOrDefault(i => i.Id == id);
        }

        public bool ImageExists(Guid id)
        {
            return _context.Images.Any(i => i.Id == id);
        }

        public void AddImage(Image image)
        {
            _context.Images.Add(image);
        }

        public void UpdateImage(Image image)
        {
            // no code in this implementation
        }

        public void DeleteImage(Image image)
        {
            _context.Images.Remove(image);

            // Note: in a real-life scenario, the image itself should also 
            // be removed from disk.  We don't do this in this demo
            // scenario, as we refill the DB with image URIs (that require
            // the actual files as well) for demo purposes.
        }

        public bool Save()
        {
            return _context.SaveChanges() >= 0;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context?.Dispose();
            }
        }
    }
}