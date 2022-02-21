using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RegistryDb.Models.Entities
{
    public class BuildingDemolitionActFileConfiguration : IEntityTypeConfiguration<BuildingDemolitionActFile>
    {
        private string nameDatebase;

        public BuildingDemolitionActFileConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<BuildingDemolitionActFile> builder)
        {
            builder.ToTable("building_demolition_act_files", nameDatebase);

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdBuilding)
                .HasColumnName("id_building")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdActFile)
                .HasColumnName("id_act_file")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdActTypeDocument)
                .HasColumnName("id_act_type_document")
                .HasColumnType("int(11)");

            builder.Property(e => e.Number)
                .HasMaxLength(50)
                .HasColumnName("number")
                .IsUnicode(false);

            builder.Property(e => e.Date)
                .HasColumnName("date")
                .HasColumnType("date");

            builder.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name")
                .IsUnicode(false);

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(d => d.ActFile)
                .WithOne(p => p.BuildingDemolitionActFile)
                .HasForeignKey<BuildingDemolitionActFile>(d => d.IdActFile);

            builder.HasOne(d => d.ActTypeDocument)
                .WithMany(p => p.BuildingDemolitionActFiles)
                .HasForeignKey(d => d.IdActTypeDocument);

            builder.HasOne(d => d.Building)
                .WithMany(p => p.BuildingDemolitionActFiles)
                .HasForeignKey(d => d.IdBuilding);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}