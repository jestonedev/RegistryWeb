using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.RegistryObjects.Buildings;

namespace RegistryDb.Models.IEntityTypeConfiguration.RegistryObjects.Buildings
{
    public class ActTypeDocumentConfiguration : IEntityTypeConfiguration<ActTypeDocument>
    {
        private string nameDatebase;

        public ActTypeDocumentConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<ActTypeDocument> builder)
        {
            builder.ToTable("act_type_document", nameDatebase);

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("int(11)");

            builder.Property(e => e.ActFileType)
                .HasMaxLength(50)
                .HasColumnName("act_file_type")
                .IsUnicode(false);

            builder.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name")
                .IsUnicode(false);
        }
    }
}
