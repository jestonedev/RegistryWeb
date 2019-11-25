using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
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
                .HasMaxLength(17)
                .IsUnicode(false);

            builder.Property(e => e.Owners)
                .HasColumnName("owners")
                .IsUnicode(false);

            builder.HasOne(d => d.OwnerProcessNavigation)
                .WithOne(p => p.OwnerActiveProcessNavigation)
                .HasForeignKey<OwnerActiveProcess>(d => d.IdProcess);
        }
    }
}
