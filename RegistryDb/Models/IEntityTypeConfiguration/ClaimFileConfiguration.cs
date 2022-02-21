using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class ClaimFileConfiguration : IEntityTypeConfiguration<ClaimFile>
    {
        private string nameDatebase;

        public ClaimFileConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<ClaimFile> builder)
        {
            builder.HasKey(e => e.IdFile);

            builder.ToTable("claim_files", nameDatebase);

            builder.HasIndex(e => e.IdClaim)
                .HasName("FK_claim_files_claims_id_claim");

            builder.Property(e => e.IdFile)
                .HasColumnName("id_file")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdClaim)
                .HasColumnName("id_claim")
                .HasColumnType("int(11)");

            builder.Property(e => e.FileName)
                .HasColumnName("file_name")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.DisplayName)
                .HasColumnName("display_name")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.MimeType)
                .HasColumnName("mime_type")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.HasOne(d => d.IdClaimNavigation)
                .WithMany(p => p.ClaimFiles)
                .HasForeignKey(d => d.IdClaim)
                .HasConstraintName("FK_claim_files_claims_id_claim");
        }
    }
}
