using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class OwnerFileConfiguration : IEntityTypeConfiguration<OwnerFile>
    {
        private string nameDatebase;

        public OwnerFileConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<OwnerFile> builder)
        {
            builder.HasKey(e => e.Id);

            builder.ToTable("owner_files", nameDatebase);

            builder.HasIndex(e => e.IdProcess)
                .HasName("FK_owner_files_id_process");

            builder.HasIndex(e => e.IdReasonType)
                .HasName("FK_owner_files_id_reason_type");

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdReasonType)
                .HasColumnName("id_reason_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.ReasonNumber)
                .HasColumnName("reason_number")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.ReasonDate)
                .HasColumnName("reason_date")
                .HasColumnType("date");

            builder.Property(e => e.FileOriginName)
                .HasColumnName("file_origin_name")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.FileDisplayName)
                .HasColumnName("file_display_name")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.FileMimeType)
                .HasColumnName("file_mime_type")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(e => e.OwnerProcess)
                .WithMany(op => op.OwnerFiles)
                .HasForeignKey(e => e.IdProcess)
                .HasConstraintName("FK_owner_files_id_process");

            builder.HasOne(e => e.OwnerReasonType)
                .WithMany(ort => ort.OwnerFiles)
                .HasForeignKey(e => e.IdReasonType)
                .HasConstraintName("FK_owner_files_id_reason_type");

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => !e.Deleted);
        }
    }
}
