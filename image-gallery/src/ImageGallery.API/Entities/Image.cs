using System;
using System.ComponentModel.DataAnnotations;

namespace ImageGallery.API.Entities
{
    public class Image
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        [Required]
        [MaxLength(200)]
        public string FileName { get; set; }

        [Required]
        [MaxLength(100)]
        public string OwnerId { get; set; }
    }
}