using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.RegistryObjects.Common.Funds;

namespace RegistryDb.Models.IEntityTypeConfiguration.RegistryObjects.Common.Funds
{
    public class FundTypeConfiguration : IEntityTypeConfiguration<FundType>
    {
        private string nameDatebase;

        public FundTypeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<FundType> builder)
        {
            builder.HasKey(e => e.IdFundType);

            builder.ToTable("fund_types", nameDatebase);

            builder.Property(e => e.IdFundType)
                .HasColumnName("id_fund_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.FundTypeName)
                .IsRequired()
                .HasColumnName("fund_type")
                .HasMaxLength(255)
                .IsUnicode(false);
        }
    }
}
