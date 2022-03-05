using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.RegistryObjects.Common.Ownerships;

namespace RegistryDb.Models.IEntityTypeConfiguration.RegistryObjects.Common.Ownerships
{
    public class OwnershipPremiseAssocConfiguration : IEntityTypeConfiguration<OwnershipPremiseAssoc>
    {
        private string nameDatebase;

        public OwnershipPremiseAssocConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<OwnershipPremiseAssoc> builder)
        {
            builder.HasKey(e => new { e.IdPremises, e.IdOwnershipRight });

            builder.ToTable("ownership_premises_assoc", nameDatebase);

            builder.HasIndex(e => e.IdOwnershipRight)
                .HasName("FK_ownership_premises_assoc_ownership_rights_id_ownership_right");

            builder.Property(e => e.IdPremises)
                .HasColumnName("id_premises")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdOwnershipRight)
                .HasColumnName("id_ownership_right")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(d => d.OwnershipRightNavigation)
                .WithMany(p => p.OwnershipPremisesAssoc)
                .HasForeignKey(d => d.IdOwnershipRight)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(d => d.PremisesNavigation)
                .WithMany(p => p.OwnershipPremisesAssoc)
                .HasForeignKey(d => d.IdPremises)
                .OnDelete(DeleteBehavior.ClientSetNull);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
