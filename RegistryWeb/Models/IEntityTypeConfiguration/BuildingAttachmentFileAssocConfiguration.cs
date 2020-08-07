using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class BuildingAttachmentFileAssocConfiguration : IEntityTypeConfiguration<BuildingAttachmentFileAssoc>
    {
        private string nameDatebase;

        public BuildingAttachmentFileAssocConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<BuildingAttachmentFileAssoc> builder)
        {
            builder.HasKey(e => new { e.IdBuilding, e.IdAttachment });

            builder.ToTable("building_attachment_files_assoc", nameDatebase);

            builder.HasIndex(e => e.IdAttachment)
                .HasName("FK_building_attachment_files_assoc_id_attachment");

            builder.Property(e => e.IdBuilding)
                .HasColumnName("id_building")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdAttachment)
                .HasColumnName("id_attachment")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(d => d.ObjectAttachmentFileNavigation)
                .WithMany(p => p.BuildingAttachmentFilesAssoc)
                .HasForeignKey(d => d.IdAttachment)
                .OnDelete(DeleteBehavior.ClientSetNull);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
