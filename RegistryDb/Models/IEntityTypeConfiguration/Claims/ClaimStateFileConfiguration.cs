using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities.Claims;

namespace RegistryDb.Models.IEntityTypeConfiguration.Claims
{
    public class ClaimStateFileConfiguration : IEntityTypeConfiguration<ClaimStateFile>
    {
        private string nameDatebase;

        public ClaimStateFileConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<ClaimStateFile> builder)
        {
            builder.HasKey(e => e.IdFile);

            builder.ToTable("claim_state_files", nameDatebase);

            builder.Property(e => e.IdFile)
                .HasColumnName("id_file")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdState)
                .HasColumnName("id_state")
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

            builder.HasOne(d => d.IdClaimStateNavigation)
                .WithMany(p => p.ClaimStateFiles)
                .HasForeignKey(d => d.IdState)
                .HasConstraintName("FK_claim_state_files_id_state");
        }
    }
}
