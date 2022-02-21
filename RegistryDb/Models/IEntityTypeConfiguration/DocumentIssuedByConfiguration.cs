using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class DocumentIssuedByConfiguration : IEntityTypeConfiguration<DocumentIssuedBy>
    {
        private string nameDatebase;

        public DocumentIssuedByConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<DocumentIssuedBy> builder)
        {
            builder.HasKey(e => e.IdDocumentIssuedBy);

            builder.ToTable("documents_issued_by", nameDatebase);

            builder.Property(e => e.IdDocumentIssuedBy)
                .HasColumnName("id_document_issued_by")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.DocumentIssuedByName)
                .IsRequired()
                .HasColumnName("document_issued_by")
                .HasMaxLength(255)
                .IsUnicode(false);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
