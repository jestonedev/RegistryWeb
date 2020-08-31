using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class JudgeConfiguration : IEntityTypeConfiguration<Judge>
    {
        private string nameDatebase;

        public JudgeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<Judge> builder)
        {
            builder.HasKey(e => e.IdJudge);

            builder.ToTable("judges", nameDatebase);

            builder.Property(e => e.IdJudge)
                .HasColumnName("id_judge")
                .HasColumnType("int(11)");

            builder.Property(e => e.NumDistrict)
                .HasColumnName("num_district")
                .HasColumnType("int(11)");

            builder.Property(e => e.Snp)
                .HasColumnName("snp")
                .HasMaxLength(512)
                .IsUnicode(false);

            builder.Property(e => e.AddrDistrict)
                .HasColumnName("addr_district")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.PhoneDistrict)
                .HasColumnName("phone_district")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.IsInactive)
                .HasColumnName("is_inactive")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
