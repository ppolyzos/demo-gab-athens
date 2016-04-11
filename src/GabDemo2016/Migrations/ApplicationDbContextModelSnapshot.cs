using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using GabDemo2016.Data;

namespace GabDemo2016.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("GabDemo2016.Models.Face", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<double?>("Age");

                    b.Property<int?>("Gender");

                    b.Property<int?>("PhotoId");

                    b.Property<double?>("Smile");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "Faces");
                });

            modelBuilder.Entity("GabDemo2016.Models.Photo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Filename");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "Photos");
                });

            modelBuilder.Entity("GabDemo2016.Models.Face", b =>
                {
                    b.HasOne("GabDemo2016.Models.Photo")
                        .WithMany()
                        .HasForeignKey("PhotoId");
                });
        }
    }
}
