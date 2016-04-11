using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GabDemo2016.Models;
using GabDemo2016.Properties;
using GabDemo2016.ViewModels;
using Microsoft.Extensions.OptionsModel;
using Newtonsoft.Json;

namespace GabDemo2016.Services
{
    public interface IFaceDetectionService
    {
        Task<IList<Face>> FindFaces(Photo photo);
    }

    public class FaceDetectionService : IFaceDetectionService
    {
        private readonly AppSettings _options;
        private readonly IImageService _imageService;
        private readonly IStorageService _storageService;

        public FaceDetectionService(IOptions<AppSettings> options,
            IImageService imageService,
            IStorageService storageService)
        {
            _options = options.Value;
            _imageService = imageService;
            _storageService = storageService;
        }

        public async Task<IList<Face>> FindFaces(Photo photo)
        {
            var url = new Uri($"{_options.AssetsUrl}/photos/{photo.Filename}.jpeg");
            var faces = await Detect(url);

            var bytes = await _storageService.GetItem(url);
            if (bytes == null) return null;

            foreach (var face in faces)
            {
                var croppedFaceRectangle = new double[]
                {
                    face.FaceRectangle.Left, // X1
                    face.FaceRectangle.Top, // Y1
                    face.FaceRectangle.Left + face.FaceRectangle.Width, // X2
                    face.FaceRectangle.Top + face.FaceRectangle.Height, // Y2
                };

                var croppedFaceBytes = _imageService.CromImage(bytes, croppedFaceRectangle);
                await UploadFace(croppedFaceBytes, face);
            }

            return faces.Select(c => new Face
            {
                Id = c.FaceId,
                PhotoId = photo.Id,
                Gender = c.FaceAttributes?.Gender,
                Age = c.FaceAttributes?.Age,
                Smile = c.FaceAttributes?.Smile
            }).ToList();
        }

        private async Task UploadFace(byte[] faceBytes, FaceViewModel face)
        {
            var blob = await _storageService.UploadStream(faceBytes, StorageContainer.Faces, $"{face.FaceId}.jpeg");
            face.Url = blob.Uri;
        }

        private async Task<IList<FaceViewModel>> Detect(Uri url)
        {
            if (url == null) return null;

            try
            {
                var client = new HttpClient { BaseAddress = new Uri(_options.FaceApiUrl) };

                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _options.FaceApiKey);

                var response = await client.PostAsJsonAsync("detect?returnFaceAttributes=age,gender,smile", new { url });
                var content = response.Content;
                var contents = await content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<FaceViewModel>>(contents);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
