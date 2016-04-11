using System.ComponentModel.DataAnnotations;

namespace GabDemo2016.ViewModels
{
    public class PhotoCreateViewModel
    {
        [Required]
        public string Filename { get; set; }
    }
}
