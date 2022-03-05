using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.SqlViews;

namespace RegistryDb.Models.IEntityTypeConfiguration.Owners
{
    public class OwnerActiveProcessConfiguration : IEntityTypeConfiguration<OwnerActiveProcess>
    {
        private string nameDatebase;

        public OwnerActiveProcessConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<OwnerActiveProcess> builder)
        {
            builder.HasKey(e => e.IdProcess);

            builder.ToTable("v_owner_active_processes", nameDatebase);

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
                .HasColumnType("int(11)");

            builder.Property(e => e.Owners)
                .HasColumnName("owners")
                .IsUnicode(false);

            builder.Property(e => e.IdBuilding)
                .HasColumnName("id_building")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdPremise)
                .HasColumnName("id_premises")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdSubPremise)
                .HasColumnName("id_sub_premises")
                .HasColumnType("int(11)");

            builder.Property(e => e.CountOwners)
                .HasColumnName("count_owners")
                .HasColumnType("int(11)");

            builder.HasOne(d => d.OwnerProcessNavigation)
                .WithOne(p => p.OwnerActiveProcessNavigation)
                .HasForeignKey<OwnerActiveProcess>(d => d.IdProcess);
        }
    }
}
