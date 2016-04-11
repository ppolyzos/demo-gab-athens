using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GabDemo2016.Data;
using GabDemo2016.Hubs;
using GabDemo2016.Models;
using GabDemo2016.Services;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace GabDemo2016.Controllers
{
    public class FileUploadController : Controller
    {
        private IConnectionManager _connectionManager;
        private IHubContext _photosHub;

        [FromServices]
        public IConnectionManager ConnectionManager
        {
            get { return _connectionManager; }
            set
            {
                _connectionManager = value;
                _photosHub = _connectionManager.GetHubContext<PhotosHub>();
            }
        }

        private readonly IStorageService _storageService;
        private readonly IImageService _imageService;
        private readonly ApplicationDbContext _dbContext;

        public FileUploadController(IStorageService storageService,
            IImageService imageService,
            ApplicationDbContext dbContext)
        {
            _storageService = storageService;
            _imageService = imageService;
            _dbContext = dbContext;
        }

        [HttpPost, Route("api/upload")]
        public async Task<IActionResult> Upload(ICollection<IFormFile> files)
        {
            var photos = new List<Photo>();
            foreach (var file in files)
            {
                if (file.Length <= 0) return HttpBadRequest("Error");

                var photo = await SavePhoto(file);
                if (photo == null) continue;

                _photosHub.Clients.All.addPhoto(photo);

                photos.Add(photo);
                await _dbContext.SaveChangesAsync();
            }

            return Ok(photos);
        }

        private async Task<Photo> SavePhoto(IFormFile file)
        {
            var resizedImageBytes = _imageService.ResizeImage(file.OpenReadStream(), 800, true);

            var filename = Guid.NewGuid().ToString();
            var blob = await _storageService.UploadStream(resizedImageBytes, StorageContainer.Photos, $"{filename}.jpeg");

            var photo = new Photo
            {
                Filename = filename
            };

            _dbContext.Photos.Add(photo);
            await _dbContext.SaveChangesAsync();

            return photo;
        }
    }
}
