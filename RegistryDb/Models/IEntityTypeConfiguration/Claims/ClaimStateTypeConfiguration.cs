using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Claims;

namespace RegistryDb.Models.IEntityTypeConfiguration.Claims
{
    public class ClaimStateTypeConfiguration : IEntityTypeConfiguration<ClaimStateType>
    {
        private string nameDatebase;

        public ClaimStateTypeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<ClaimStateType> builder)
        {
            builder.HasKey(e => e.IdStateType);

            builder.ToTable("claim_state_types", nameDatebase);

            builder.Property(e => e.IdStateType)
                .HasColumnName("id_state_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.StateType)
                .HasColumnName("state_type")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.IsStartStateType)
                .HasColumnName("is_start_state_type")
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
