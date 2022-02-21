using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class DocumentTypeConfiguration : IEntityTypeConfiguration<DocumentType>
    {
        private string nameDatebase;

        public DocumentTypeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<DocumentType> builder)
        {
            builder.HasKey(e => e.IdDocumentType);

            builder.ToTable("document_types", nameDatebase);

            builder.Property(e => e.IdDocumentType)
                .HasColumnName("id_document_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.DocumentTypeName)
                .IsRequired()
                .HasColumnName("document_type")
                .HasMaxLength(50)
                .IsUnicode(false);
        }
    }
}
