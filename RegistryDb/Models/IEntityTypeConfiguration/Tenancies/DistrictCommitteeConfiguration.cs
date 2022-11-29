using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Tenancies;

namespace RegistryDb.Models.IEntityTypeConfiguration.Tenancies
{
    public class DistrictCommitteeConfiguration : IEntityTypeConfiguration<DistrictCommittee>
    {
        private string nameDatebase;

        public DistrictCommitteeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<DistrictCommittee> builder)
        {
            builder.HasKey(e => e.IdCommittee);

            builder.ToTable("district_committees", nameDatebase);

            builder.Property(e => e.IdCommittee)
                .HasColumnName("id_committee")
                .HasColumnType("int(11)");

            builder.Property(e => e.NameNominative)
                .IsRequired()
                .HasColumnName("name_nominative")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.NameGenetive)
                .IsRequired()
                .HasColumnName("name_genetive")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.NamePrepositional)
                .IsRequired()
                .HasColumnName("name_prepositional")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.HeadSnpGenetive)
                .IsRequired()
                .HasColumnName("head_snp_genetive")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.HeadPostGenetive)
                .IsRequired()
                .HasColumnName("head_post_genetive")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.HeadSnp)
                .IsRequired()
                .HasColumnName("head_snp")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.HeadPost)
                .IsRequired()
                .HasColumnName("head_post")
                .HasMaxLength(255)
                .IsUnicode(false);
        }
    }
}
