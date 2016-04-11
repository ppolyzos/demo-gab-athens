using GabDemo2016.Models;
using Microsoft.Data.Entity;

namespace GabDemo2016.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() { }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Face>()
                .HasOne(p => p.Photo)
                .WithMany(b => b.Faces);
        }

        public DbSet<Photo> Photos { get; set; }
        public DbSet<Face> Faces { get; set; }
    }
}
