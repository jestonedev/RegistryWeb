using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Tenancies;

namespace RegistryDb.Models.IEntityTypeConfiguration.Tenancies
{
    public class TenancyPremiseAssocConfiguration : IEntityTypeConfiguration<TenancyPremiseAssoc>
    {
        private string nameDatebase;

        public TenancyPremiseAssocConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<TenancyPremiseAssoc> builder)
        {
            builder.HasKey(e => e.IdAssoc);

            builder.ToTable("tenancy_premises_assoc", nameDatebase);

            builder.HasIndex(e => e.IdPremise)
                .HasName("FK_tenancy_premises_assoc_premises_id_premises");

            builder.HasIndex(e => e.IdProcess)
                .HasName("FK_tenancy_premises_assoc_tenancy_contracts_id_contract");

            builder.Property(e => e.IdAssoc)
                .HasColumnName("id_assoc")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.IdPremise)
                .HasColumnName("id_premises")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
                .HasColumnType("int(11)");

            builder.Property(e => e.RentLivingArea).HasColumnName("rent_living_area");

            builder.Property(e => e.RentTotalArea).HasColumnName("rent_total_area");

            builder.HasOne(d => d.PremiseNavigation)
                .WithMany(p => p.TenancyPremisesAssoc)
                .HasForeignKey(d => d.IdPremise);

            builder.HasOne(d => d.ProcessNavigation)
                .WithMany(p => p.TenancyPremisesAssoc)
                .HasForeignKey(d => d.IdProcess)
                .HasConstraintName("FK_tenancy_premises_assoc_tenancy_contracts_id_contract");

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
