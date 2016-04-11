using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using GabDemo2016.Data;
using GabDemo2016.Models;
using GabDemo2016.Services;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Data.Entity;

namespace GabDemo2016.Hubs
{
    [HubName("photos")]
    public class PhotosHub : Hub
    {
        private readonly Random _random = new Random();

        private readonly ApplicationDbContext _dbContext;
        private readonly IStorageService _storageService;

        public PhotosHub(ApplicationDbContext dbContext, IStorageService storageService)
        {
            _dbContext = dbContext;
            _storageService = storageService;
        }

        public async Task<IEnumerable<Photo>> GetPhotos()
        {
            var photos = await _dbContext.Photos.ToListAsync();

            foreach (var photo in photos)
            {
                Clients.Caller.addPhoto(photo);
            }

            var faces = await _dbContext.Faces.ToListAsync();
            if (faces.Any())
            {
                Clients.Caller.addFaces(faces);
            }

            return photos;
        }

        public void Notify(string name)
        {
            Clients.All.notify(name);
        }

        public void GenerateMessage()
        {
            Clients.All.notify($"{DateTime.Now.ToString("HH:mm:ss")}: Random Message id: {_random.Next(1, 100)}");
        }

        public async void Reset()
        {
            await _dbContext.Database.ExecuteSqlCommandAsync("DELETE FROM Photos; DELETE from Faces;");
            _storageService.Clear(StorageContainer.Faces);
            _storageService.Clear(StorageContainer.Photos);
        }
    }
}