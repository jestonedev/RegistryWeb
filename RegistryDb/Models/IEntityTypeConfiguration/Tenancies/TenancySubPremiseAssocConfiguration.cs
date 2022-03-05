using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Tenancies;

namespace RegistryDb.Models.IEntityTypeConfiguration.Tenancies
{
    public class TenancySubPremiseAssocConfiguration : IEntityTypeConfiguration<TenancySubPremiseAssoc>
    {
        private string nameDatebase;

        public TenancySubPremiseAssocConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<TenancySubPremiseAssoc> builder)
        {
            builder.HasKey(e => e.IdAssoc);

            builder.ToTable("tenancy_sub_premises_assoc", nameDatebase);

            builder.HasIndex(e => e.IdProcess)
                .HasName("FK_tenancy_sub_premises_assoc_tenancy_contracts_id_contract");

            builder.HasIndex(e => e.IdSubPremise)
                .HasName("FK_tenancy_sub_premises_assoc_sub_premises_id_sub_premises");

            builder.Property(e => e.IdAssoc)
                .HasColumnName("id_assoc")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdSubPremise)
                .HasColumnName("id_sub_premises")
                .HasColumnType("int(11)");

            builder.Property(e => e.RentTotalArea).HasColumnName("rent_total_area");

            builder.HasOne(d => d.ProcessNavigation)
                .WithMany(p => p.TenancySubPremisesAssoc)
                .HasForeignKey(d => d.IdProcess)
                .HasConstraintName("FK_tenancy_sub_premises_assoc_tenancy_contracts_id_contract");

            builder.HasOne(d => d.SubPremiseNavigation)
                .WithMany(p => p.TenancySubPremisesAssoc)
                .HasForeignKey(d => d.IdSubPremise);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
