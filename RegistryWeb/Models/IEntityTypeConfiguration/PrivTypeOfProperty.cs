using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class PrivTypeOfPropertyConfiguration : IEntityTypeConfiguration<PrivTypeOfProperty>
    {
        private string nameDatebase;

        public PrivTypeOfPropertyConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<PrivTypeOfProperty> builder)
        {
            builder.ToTable("priv_types_of_property", nameDatebase);

            builder.HasKey(e => e.IdTypeOfProperty);

            builder.Property(e => e.IdTypeOfProperty)
                .HasColumnName("id_type_of_property")
                .HasColumnType("int(11)");

            builder.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsUnicode(false);
        }  
    }
}
