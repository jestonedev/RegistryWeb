using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class LitigationTypeConfiguration : IEntityTypeConfiguration<LitigationType>
    {
        private string nameDatebase;

        public LitigationTypeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<LitigationType> builder)
        {
            builder.HasKey(e => e.IdLitigationType);

            builder.ToTable("litigation_types", nameDatebase);

            builder.Property(e => e.IdLitigationType)
                .HasColumnName("id_litigation_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.LitigationTypeName)
                .IsRequired()
                .HasColumnName("litigation_type")
                .HasMaxLength(255)
                .IsUnicode(false);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
