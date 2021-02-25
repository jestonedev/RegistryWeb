using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class PrivContractConfiguration : IEntityTypeConfiguration<PrivContract>
    {
        private string nameDatebase;

        public PrivContractConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<PrivContract> builder)
        {
            builder.ToTable("priv_contracts", nameDatebase);

            builder.HasKey(e => e.IdContract);

            builder.Property(e => e.IdContract)
                .HasColumnName("id_contract")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.RegNumber)
                .HasColumnName("reg_number")
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(e => e.IsRefusenik)
                .HasColumnName("is_refusenik")
                .HasColumnType("tinyint")
                .HasDefaultValueSql("0")
                .IsRequired();

            builder.Property(e => e.IsRasprivatization)
                .HasColumnName("is_rasprivatization")
                .HasColumnType("tinyint")
                .HasDefaultValueSql("0")
                .IsRequired();

            builder.Property(e => e.IsRelocation)
                .HasColumnName("is_relocation")
                .HasColumnType("tinyint")
                .HasDefaultValueSql("0")
                .IsRequired();
        }  
    }
}
