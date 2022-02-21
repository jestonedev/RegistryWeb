using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class OwnerTypeConfiguration : IEntityTypeConfiguration<OwnerType>
    {
        private string nameDatebase;

        public OwnerTypeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<OwnerType> builder)
        {
            builder.HasKey(e => e.IdOwnerType);

            builder.ToTable("owner_type", nameDatebase);

            builder.Property(e => e.IdOwnerType)
                .HasColumnName("id_owner_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.OwnerType1)
                .IsRequired()
                .HasColumnName("owner_type")
                .HasMaxLength(255)
                .IsUnicode(false);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
