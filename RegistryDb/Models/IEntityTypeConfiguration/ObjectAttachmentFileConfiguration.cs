using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class ObjectAttachmentFileConfiguration: IEntityTypeConfiguration<ObjectAttachmentFile>
    {
        private string nameDatebase;

        public ObjectAttachmentFileConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<ObjectAttachmentFile> builder)
        {
            builder.HasKey(e => e.IdAttachment);

            builder.ToTable("object_attachment_files", nameDatebase);

            builder.Property(e => e.IdAttachment)
                .HasColumnName("id_attachment")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(255)
                .IsUnicode(false);

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

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
