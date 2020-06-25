using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class PremisesJurisdictionActFilesConfiguration: IEntityTypeConfiguration<PremisesJurisdictionActFiles>
    {
        private string nameDatebase;

        public PremisesJurisdictionActFilesConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<PremisesJurisdictionActFiles> builder)
        {
            builder.HasKey(e => e.IdJurisdiction);

            builder.ToTable("premises_jurisdiction_act_files", "registry_test");

            builder.Property(e => e.IdJurisdiction)
                .HasColumnName("id")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdPremises)
                .HasColumnName("id_premises")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdActFile)
                .HasColumnName("id_act_file")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdActFileTypeDocument)
                .HasColumnName("id_act_file_type_document")
                .HasColumnType("int(11)");

            builder.Property(e => e.Number)
                .HasColumnName("number")
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.Date)
                .HasColumnName("date");

            builder.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(4)")
                .HasDefaultValueSql("0");

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

            builder.HasOne(d => d.IdActFileTypeDocumentNavigation)
                .WithMany(p => p.PremisesJurisdictionActFiles)
                .HasForeignKey(d => d.IdActFileTypeDocument)
                .OnDelete(DeleteBehavior.ClientSetNull)//;
                .HasConstraintName("FK_premises_jurisdiction_act_files_id_act_file_type_document");

            builder.HasOne(d => d.PremiseNavigation)
                .WithMany(p => p.PremisesJurisdictionActFiles)
                .HasForeignKey(d => d.IdPremises)
                .OnDelete(DeleteBehavior.ClientSetNull);

        }
    }
}
