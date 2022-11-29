using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.RegistryObjects.Common.Funds;

namespace RegistryDb.Models.IEntityTypeConfiguration.RegistryObjects.Common.Funds
{
    public class FundPremiseAssocConfiguration : IEntityTypeConfiguration<FundPremiseAssoc>
    {
        private string nameDatebase;

        public FundPremiseAssocConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<FundPremiseAssoc> builder)
        {
            builder.HasKey(e => new { e.IdPremises, e.IdFund });

            builder.ToTable("funds_premises_assoc", nameDatebase);

            builder.HasIndex(e => e.IdFund)
                .HasName("FK_ownership_premises_assoc_funds_history_id_fund");

            builder.Property(e => e.IdPremises)
                .HasColumnName("id_premises")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdFund)
                .HasColumnName("id_fund")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(d => d.IdFundNavigation)
                .WithMany(p => p.FundsPremisesAssoc)
                .HasForeignKey(d => d.IdFund)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ownership_premises_assoc_funds_history_id_fund");

            builder.HasOne(d => d.IdPremisesNavigation)
                .WithMany(p => p.FundsPremisesAssoc)
                .HasForeignKey(d => d.IdPremises);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
