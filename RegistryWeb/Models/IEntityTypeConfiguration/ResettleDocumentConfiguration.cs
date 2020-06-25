using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class ResettleDocumentConfiguration : IEntityTypeConfiguration<ResettleDocument>
    {
        private string nameDatebase;

        public ResettleDocumentConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<ResettleDocument> builder)
        {
            builder.HasKey(e => e.IdDocument);

            builder.ToTable("resettle_documents", nameDatebase);

            builder.HasIndex(e => e.IdDocumentType)
                .HasName("FK_resettle_documents_id_document_type");

            builder.HasIndex(e => e.IdResettleInfo)
                .HasName("FK_resettle_documents_id_resettle_info");

            builder.Property(e => e.IdDocument)
                .HasColumnName("id_document")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdDocumentType)
                .HasColumnName("id_document_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdResettleInfo)
                .HasColumnName("id_resettle_info")
                .HasColumnType("int(11)");

            builder.Property(e => e.Date)
                .HasColumnName("date")
                .HasColumnType("date");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Number)
                .HasColumnName("number")
                .HasMaxLength(20)
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

            builder.HasOne(d => d.ResettleDocumentTypeNavigation)
                .WithMany(p => p.ResettleDocuments)
                .HasForeignKey(d => d.IdDocumentType)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.ResettleInfoNavigation)
                .WithMany(p => p.ResettleDocuments)
                .HasForeignKey(d => d.IdResettleInfo)
                .OnDelete(DeleteBehavior.Cascade);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
