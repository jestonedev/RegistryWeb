using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class TenancyFileConfiguration : IEntityTypeConfiguration<TenancyFile>
    {
        private string nameDatebase;

        public TenancyFileConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<TenancyFile> builder)
        {
            builder.HasKey(e => e.IdFile);

            builder.ToTable("tenancy_files", nameDatebase);

            builder.HasIndex(e => e.IdProcess)
                .HasName("FK_tenancy_files_id_process");

            builder.Property(e => e.IdFile)
                .HasColumnName("id_file")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
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

            builder.HasOne(d => d.IdProcessNavigation)
                .WithMany(p => p.TenancyFiles)
                .HasForeignKey(d => d.IdProcess)
                .HasConstraintName("FK_tenancy_files_id_process");
        }
    }
}
