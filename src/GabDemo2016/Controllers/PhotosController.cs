using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GabDemo2016.Data;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;

namespace GabDemo2016.Controllers
{
    public class PhotosController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public PhotosController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet, Route("api/photos")]
        public async Task<IActionResult> GetPhotos()
        {
            var results = new
            {
                total = await _dbContext.Photos.CountAsync(),
                items = await _dbContext.Photos.Take(20).ToListAsync()
            };

            return Ok(results);
        }
    }
}
