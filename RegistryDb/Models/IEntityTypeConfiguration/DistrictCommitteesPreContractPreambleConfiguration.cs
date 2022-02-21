using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class DistrictCommitteesPreContractPreambleConfiguration : IEntityTypeConfiguration<DistrictCommitteesPreContractPreamble>
    {
        private string nameDatebase;

        public DistrictCommitteesPreContractPreambleConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<DistrictCommitteesPreContractPreamble> builder)
        {
            builder.HasKey(e => e.IdPreamble);

            builder.ToTable("district_committees_pre_conctract_preambles", nameDatebase);

            builder.Property(e => e.IdPreamble)
                .HasColumnName("id_preamble")
                .HasColumnType("int(11)");

            builder.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Preamble)
                .IsRequired()
                .HasColumnName("preamble")
                .HasMaxLength(4096)
                .IsUnicode(false);
        }
    }
}
