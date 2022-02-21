using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class StructureTypeConfiguration : IEntityTypeConfiguration<StructureType>
    {
        private string nameDatebase;

        public StructureTypeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<StructureType> builder)
        {
            builder.HasKey(e => e.IdStructureType);

            builder.ToTable("structure_types", nameDatebase);

            builder.Property(e => e.IdStructureType)
                .HasColumnName("id_structure_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.StructureTypeName)
                .IsRequired()
                .HasColumnName("structure_type")
                .HasMaxLength(255)
                .IsUnicode(false);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
