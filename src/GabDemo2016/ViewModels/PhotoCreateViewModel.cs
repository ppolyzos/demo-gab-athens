using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GabDemo2016.ViewModels
{
    public class PhotoCreateViewModel
    {
        [Required]
        public string Filename { get; set; }
    }

    public class FacesFoundViewModel
    {
        public string PhotoId { get; set; }
        public IList<FaceViewModel> FacesViewModels { get; set; } 
    }
}
