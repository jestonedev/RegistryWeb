using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class OwnerConfiguration : IEntityTypeConfiguration<Owner>
    {
        private string nameDatebase;

        public OwnerConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<Owner> builder)
        {
            builder.HasKey(e => e.IdOwner);

            builder.ToTable("owners", nameDatebase);

            builder.HasIndex(e => e.IdOwnerType)
                .HasName("FK_owners_id_owner_type");

            builder.HasIndex(e => e.IdProcess)
                .HasName("FK_owners_id_process");

            builder.Property(e => e.IdOwner)
                .HasColumnName("id_owner")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdOwnerType)
                .HasColumnName("id_owner_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)");            

            builder.HasOne(d => d.IdOwnerTypeNavigation)
                .WithMany(p => p.Owners)
                .HasForeignKey(d => d.IdOwnerType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_owners_id_owner_type");

            builder.HasOne(d => d.IdOwnerProcessNavigation)
                .WithMany(p => p.Owners)
                .HasForeignKey(d => d.IdProcess)
                .HasConstraintName("FK_owners_id_process");

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
