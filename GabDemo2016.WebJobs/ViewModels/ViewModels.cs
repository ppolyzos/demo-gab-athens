using System;

namespace GabDemo2016.WebJobs.ViewModels
{
    public enum Gender
    {
        Male,
        Female
    }

    public class FaceViewModel
    {
        public Guid FaceId { get; set; }
        public int PhotoId { get; set; }
        public Uri Url { get; set; }
        public FaceRectangleViewModel FaceRectangle { get; set; }
        public FaceAttributesViewModel FaceAttributes { get; set; }
    }

    public class FaceAttributesViewModel
    {
        public double Age { get; set; }
        public Gender Gender { get; set; }
        public double Smile { get; set; }

    }

    public class FaceRectangleViewModel
    {
        public int Top { get; set; }
        public int Left { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
