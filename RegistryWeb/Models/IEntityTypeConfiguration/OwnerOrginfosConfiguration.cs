using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class OwnerOrginfosConfiguration : IEntityTypeConfiguration<OwnerOrginfos>
    {
        private string nameDatebase;

        public OwnerOrginfosConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<OwnerOrginfos> builder)
        {
            builder.HasKey(e => e.IdOrginfo);

            builder.ToTable("owner_orginfo", nameDatebase);

            builder.HasIndex(e => e.IdProcess)
                .HasName("FK_owner_orginfo_owner_processes_id_process");

            builder.Property(e => e.IdOrginfo)
                .HasColumnName("id_orginfo")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
                .HasColumnType("int(11)");

            builder.Property(e => e.OrgName)
                .IsRequired()
                .HasColumnName("org_name")
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.NumeratorShare)
                .HasColumnName("numerator_share")
                .HasColumnType("int(11)");

            builder.Property(e => e.DenominatorShare)
                .HasColumnName("denominator_share")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(d => d.IdOwnerProcessNavigation)
                .WithMany(p => p.OwnerOrginfos)
                .HasForeignKey(d => d.IdProcess)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_owner_orginfo_owner_processes_id_process");

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
