using System.IO;
using ImageResizer;

namespace GabDemo2016.Services
{
    public interface IImageService
    {
        byte[] ResizeImage(Stream input, int width, bool autoRotate = false);
        byte[] CromImage(byte[] inputArray, double[] cropRectangle);
    }

    public class ImageService : IImageService
    {
        public byte[] ResizeImage(Stream input, int width, bool autoRotate = false)
        {
            var instructions = new Instructions
            {
                Width = width,
                Mode = FitMode.Stretch,
                Scale = ScaleMode.Both,
                AutoRotate = autoRotate
            };

            using (var output = new MemoryStream())
            {
                ImageBuilder.Current.Build(new ImageJob(input, output, instructions));
                input.Dispose();

                output.Seek(0, SeekOrigin.Begin);
                return output.ToArray();
            }
        }

        public byte[] CromImage(byte[] inputArray, double[] cropRectangle)
        {
            var instructions = new Instructions
            {
                CropRectangle = cropRectangle
            };

            using (var source = new MemoryStream())
            using (var targetStream = new MemoryStream())
            {
                source.Write(inputArray, 0, inputArray.Length);
                source.Seek(0, SeekOrigin.Begin);
                ;
                ImageBuilder.Current.Build(new ImageJob(source, targetStream, instructions));

                targetStream.Seek(0, SeekOrigin.Begin);
                return targetStream.ToArray();
            }
        }
    }
}
