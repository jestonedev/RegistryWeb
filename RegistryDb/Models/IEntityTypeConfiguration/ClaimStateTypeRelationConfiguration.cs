using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class ClaimStateTypeRelationConfiguration : IEntityTypeConfiguration<ClaimStateTypeRelation>
    {
        private string nameDatebase;

        public ClaimStateTypeRelationConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<ClaimStateTypeRelation> builder)
        {
            builder.HasKey(e => e.IdRelation);

            builder.ToTable("claim_state_types_relations", nameDatebase);

            builder.Property(e => e.IdRelation)
                .HasColumnName("id_relation")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdStateFrom)
                .HasColumnName("id_state_from")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdStateTo)
                .HasColumnName("id_state_to")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
