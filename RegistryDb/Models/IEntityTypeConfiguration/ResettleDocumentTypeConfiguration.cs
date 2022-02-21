using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class ResettleDocumentTypeConfiguration : IEntityTypeConfiguration<ResettleDocumentType>
    {
        private string nameDatebase;

        public ResettleDocumentTypeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<ResettleDocumentType> builder)
        {
            builder.HasKey(e => e.IdDocumentType);

            builder.ToTable("resettle_document_types", nameDatebase);

            builder.Property(e => e.IdDocumentType)
                .HasColumnName("id_document_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.DocumentTypeName)
                .IsRequired()
                .HasColumnName("document_type")
                .HasMaxLength(255)
                .IsUnicode(false);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
