using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace GabDemo2016.Models
{
    [Table("Photos")]
    public class Photo
    {
        public int Id { get; set; }
        public string Filename { get; set; }

        [JsonIgnore] 
        public List<Face> Faces { get; set; }
    }

    [Table("Faces")]
    public class Face
    {
        public Guid Id { get; set; }

        public Gender? Gender { get; set; }
        public double? Age { get; set; }
        public double? Smile { get; set; }

        public int PhotoId { get; set; }

        [ForeignKey("PhotoId")]
        public Photo Photo { get; set; }
    }
}