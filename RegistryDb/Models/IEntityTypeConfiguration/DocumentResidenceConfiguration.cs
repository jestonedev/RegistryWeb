using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class DocumentResidenceConfiguration : IEntityTypeConfiguration<DocumentResidence>
    {
        private string nameDatebase;

        public DocumentResidenceConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<DocumentResidence> builder)
        {
            builder.HasKey(e => e.IdDocumentResidence);

            builder.ToTable("documents_residence", nameDatebase);

            builder.Property(e => e.IdDocumentResidence)
                .HasColumnName("id_document_residence")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.DocumentResidenceName)
                .IsRequired()
                .HasColumnName("document_residence")
                .HasMaxLength(255)
                .IsUnicode(false);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
