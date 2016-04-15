using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GabDemo2016.Data;
using GabDemo2016.Hubs;
using GabDemo2016.Models;
using GabDemo2016.ViewModels;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.Data.Entity;

namespace GabDemo2016.Controllers
{
    public class FacesController : Controller
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

        private readonly ApplicationDbContext _dbContext;
        public FacesController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet, Route("api/faces/{photoId}")]
        public async Task<IActionResult> GetFaces(int photoId)
        {
            var results = await _dbContext.Faces
                .Where(f => f.PhotoId == photoId)
                .ToListAsync();

            return Ok(results);
        }

        [HttpPost, Route("api/faces/found")]
        public async Task<IActionResult> FacesFound([FromBody]FacesFoundViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            var photo = _dbContext.Photos.FirstOrDefault(c => c.Filename == model.PhotoId);
            if (photo == null) return new HttpNotFoundResult();

            var faces = model.FacesViewModels.Select(c => new Face
            {
                Id = c.FaceId,
                PhotoId = photo.Id,
                Gender = c.FaceAttributes?.Gender,
                Age = c.FaceAttributes?.Age,
                Smile = c.FaceAttributes?.Smile
            }).ToList();


            _photosHub.Clients.All.addFaces(faces);

            _dbContext.Faces.AddRange(faces);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}