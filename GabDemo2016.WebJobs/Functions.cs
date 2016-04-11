using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using GabDemo2016.WebJobs.ViewModels;
using ImageResizer;
using ImageResizer.ExtensionMethods;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace GabDemo2016.WebJobs
{
    public class Functions
    {
        public void GenerateThumbnail([BlobTrigger("photos/{name}.{ext}")] Stream input,
            [Blob("photo-thumbs/{name}-w200.{ext}", FileAccess.Write)] Stream output)
        {
            var instructions = new Instructions
            {
                Width = 200,
                Mode = FitMode.Carve,
                Scale = ScaleMode.Both
            };
            ImageBuilder.Current.Build(new ImageJob(input, output, instructions));
        }

        public async void FindFaces([BlobTrigger("photos/{photoId}.{ext}")] CloudBlockBlob photoBlob,
            string photoId,
            IBinder binder)
        {
            var faces = await Detect(photoBlob.Uri);
            var bytes = (await photoBlob.OpenReadAsync()).CopyToBytes();

            foreach (var face in faces)
            {
                var croppedFaceRectangle = new double[]
                {
                    face.FaceRectangle.Left, // X1
                    face.FaceRectangle.Top, // Y1
                    face.FaceRectangle.Left + face.FaceRectangle.Width, // X2
                    face.FaceRectangle.Top + face.FaceRectangle.Height, // Y2
                };

                var outputBlob = binder.Bind<CloudBlockBlob>(new BlobAttribute($"faces/{face.FaceId}.jpeg"));
                outputBlob.Properties.ContentType = "image/jpeg";

                using (var output = outputBlob.OpenWrite())
                {
                    CromImage(bytes, output, croppedFaceRectangle);
                }
            }

            NotifyWebAppThroughApi(photoId, faces);
            // NotifyWebAppThroughSockets(photoId, faces);
        }

        private void CromImage(byte[] inputBytes, Stream output, double[] cropRectangle)
        {
            var instructions = new Instructions
            {
                CropRectangle = cropRectangle
            };

            using (var input = new MemoryStream(inputBytes))
            {
                var imageJob = new ImageJob(input, output, instructions) { ResetSourceStream = true };
                ImageBuilder.Current.Build(imageJob);
            }
        }

        #region Notify WebApp
        private HubConnection _hub;
        private IHubProxy _proxy;
        private async void NotifyWebAppThroughSockets(string photoId, IList<FaceViewModel> faces)
        {
            try
            {
                if (_hub == null || _hub.State == ConnectionState.Disconnected)
                {
                    _hub = new HubConnection(CloudConfigurationManager.GetSetting("AppSettings:SocketUrl"));
                    _proxy = _hub.CreateHubProxy("photos");

                    await _hub.Start();
                }

                await _proxy.Invoke("FacesFound", photoId, faces);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async void NotifyWebAppThroughApi(string photoId, IList<FaceViewModel> facesViewModels)
        {
            var client = new HttpClient { BaseAddress = new Uri(CloudConfigurationManager.GetSetting("AppSettings:SocketUrl")) };

            await client.PostAsJsonAsync("api/faces/found", new { photoId, facesViewModels });
        }
        #endregion

        private async Task<IList<FaceViewModel>> Detect(Uri url)
        {
            if (url == null) return null;

            try
            {
                var client = new HttpClient { BaseAddress = new Uri(CloudConfigurationManager.GetSetting("AppSettings:FaceApiUrl")) };

                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", CloudConfigurationManager.GetSetting("AppSettings:FaceApiKey"));

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
