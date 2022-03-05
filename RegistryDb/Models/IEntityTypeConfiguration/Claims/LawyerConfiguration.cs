using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Claims;

namespace RegistryDb.Models.IEntityTypeConfiguration.Claims
{
    public class LawyerConfiguration : IEntityTypeConfiguration<Lawyer>
    {
        private string nameDatebase;

        public LawyerConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<Lawyer> builder)
        {
            builder.HasKey(e => e.IdLawyer);

            builder.ToTable("lawyers", nameDatebase);

            builder.Property(e => e.IdLawyer)
                .HasColumnName("id_lawyer")
                .HasColumnType("int(11)");

            builder.Property(e => e.SNP)
                .HasColumnName("snp")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Post)
                .HasColumnName("post")
                .HasMaxLength(255)
                .IsUnicode(false);
        }
    }
}
