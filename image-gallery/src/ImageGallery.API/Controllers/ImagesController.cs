using System;
using System.Collections.Generic;
using System.IO;
using AutoMapper;
using ImageGallery.API.Services;
using ImageGallery.Model;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace ImageGallery.API.Controllers
{
    [Route("api/images")]
    [ApiController]
    [Authorize]
    public class ImagesController : ControllerBase
    {
        private readonly IGalleryRepository _galleryRepository;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;

        public ImagesController(IGalleryRepository galleryRepository, IHostingEnvironment hostingEnvironment, IMapper mapper)
        {
            _galleryRepository = galleryRepository;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
        }
        
        // GET api/images
        [HttpGet]
        public IActionResult GetImages()
        {
            // get from repo
            var imagesFromRepo = _galleryRepository.GetImages();

            // map to model
            var imagesToReturn = _mapper.Map<IEnumerable<Image>>(imagesFromRepo);

            // return
            return Ok(imagesToReturn);
        }
        
        // GET api/images/{id}
        [HttpGet("{id}", Name = "GetImage")]
        public IActionResult GetImage(Guid id)
        {          
            var imageFromRepo = _galleryRepository.GetImage(id);

            if (imageFromRepo == null)
            {
                return NotFound();
            }

            var imageToReturn = _mapper.Map<Image>(imageFromRepo);

            return Ok(imageToReturn);
        }

        // POST api/images
        [HttpPost]
        public IActionResult CreateImage([FromBody] ImageForCreation imageForCreation)
        {
            if (imageForCreation == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                // return 422 - Unprocessable Entity when validation fails
                return new UnprocessableEntityObjectResult(ModelState);
            }

            // Automapper maps only the Title in our configuration
            var imageEntity = _mapper.Map<Entities.Image>(imageForCreation);

            // Create an image from the passed-in bytes (Base64), and 
            // set the filename on the image

            // get this environment's web root path (the path
            // from which static content, like an image, is served)
            var webRootPath = _hostingEnvironment.ContentRootPath;

            // create the filename
            string fileName = Guid.NewGuid() + ".jpg";
            
            // the full file path
            var filePath = Path.Combine(webRootPath, "wwwroot/images", fileName);

            // write bytes and auto-close stream
            System.IO.File.WriteAllBytes(filePath, imageForCreation.Bytes);

            // fill out the filename
            imageEntity.FileName = fileName;

            // ownerId should be set - can't save image in starter solution, will
            // be fixed during the course
            //imageEntity.OwnerId = ...;

            // add and save.  
            _galleryRepository.AddImage(imageEntity);

            if (!_galleryRepository.Save())
            {
                throw new Exception($"Adding an image failed on save.");
            }

            var imageToReturn = _mapper.Map<Image>(imageEntity);

            return CreatedAtRoute("GetImage",
                new { id = imageToReturn.Id },
                imageToReturn);
        }

        // DELETE api/images/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteImage(Guid id)
        {
            var imageFromRepo = _galleryRepository.GetImage(id);

            if (imageFromRepo == null)
            {
                return NotFound();
            }

            _galleryRepository.DeleteImage(imageFromRepo);

            if (!_galleryRepository.Save())
            {
                throw new Exception($"Deleting image with {id} failed on save.");
            }

            return NoContent();
        }

        // PUT api/images/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateImage(Guid id, [FromBody] ImageForUpdate imageForUpdate)
        {
            if (imageForUpdate == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                // return 422 - Unprocessable Entity when validation fails
                return new UnprocessableEntityObjectResult(ModelState);
            }

            var imageFromRepo = _galleryRepository.GetImage(id);
            if (imageFromRepo == null)
            {
                return NotFound();
            }

            _mapper.Map(imageForUpdate, imageFromRepo);

            _galleryRepository.UpdateImage(imageFromRepo);

            if (!_galleryRepository.Save())
            {
                throw new Exception($"Updating image with {id} failed on save.");
            }

            return NoContent();
        }
    }
}
