using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageGallery.Client.Models;
using ImageGallery.Client.Services;
using ImageGallery.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace ImageGallery.Client.Controllers
{
    [Authorize]
    public class GalleryController : Controller
    {
        private readonly IImageGalleryApiClient _imageGalleryApiClient;

        public GalleryController(IImageGalleryApiClient imageGalleryApiClient)
        {
            _imageGalleryApiClient = imageGalleryApiClient;
        }

        public async Task<IActionResult> Index()
        {
            await WriteOutIdentityInformation();
            
            var images = await _imageGalleryApiClient.GetImages().ConfigureAwait(false);
            var galleryIndexViewModel = new GalleryIndexViewModel(images);
            return View(galleryIndexViewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> EditImage(Guid id)
        {
            var image = await _imageGalleryApiClient.GetImage(id).ConfigureAwait(false);
            var editImageViewModel = new EditImageViewModel
            {
                Id = image.Id,
                Title = image.Title
            };

            return View(editImageViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditImage(EditImageViewModel editImageViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // create an ImageForUpdate instance
            var imageForUpdate = new ImageForUpdate {Title = editImageViewModel.Title};

            await _imageGalleryApiClient.UpdateImage(editImageViewModel.Id, imageForUpdate)
                .ConfigureAwait(false);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteImage(Guid id)
        {
            await _imageGalleryApiClient.DeleteImage(id).ConfigureAwait(false);
            return RedirectToAction("Index");
        }

        public IActionResult AddImage()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddImage(AddImageViewModel addImageViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // create an ImageForCreation instance
            var imageForCreation = new ImageForCreation {Title = addImageViewModel.Title};

            // take the first (only) file in the Files list
            var imageFile = addImageViewModel.Files.First();

            if (imageFile.Length > 0)
            {
                using (var fileStream = imageFile.OpenReadStream())
                using (var ms = new MemoryStream())
                {
                    fileStream.CopyTo(ms);
                    imageForCreation.Bytes = ms.ToArray();
                }
            }

            await _imageGalleryApiClient.AddImage(imageForCreation).ConfigureAwait(false);
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
                {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }

        private async Task WriteOutIdentityInformation()
        {
            // get the saved identity token
            var identityToken =
                await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.IdToken);

            // write it out
            Debug.WriteLine($"Identity token: {identityToken}");

            // write out the user claims
            foreach (var claim in User.Claims)
            {
                Debug.WriteLine($"Claim type: {claim.Type} - Claim value: {claim.Value}");
            }
        }
    }
}