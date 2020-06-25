using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class ResettleInfoToConfiguration : IEntityTypeConfiguration<ResettleInfoTo>
    {
        private string nameDatebase;

        public ResettleInfoToConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<ResettleInfoTo> builder)
        {
            builder.HasKey(e => e.IdKey);
            
            builder.ToTable("resettle_info_to", nameDatebase);

            builder.HasIndex(e => e.IdResettleInfo)
                .HasName("FK_resettle_info_sub_premises_to_id_resettle_info");

            builder.Property(e => e.IdKey)
                .HasColumnName("id_key")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdResettleInfo)
                .HasColumnName("id_resettle_info")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdObject)
                .HasColumnName("id_object")
                .HasColumnType("int(11)");

            builder.Property(e => e.ObjectType)
                .HasColumnName("object_type")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(d => d.ResettleInfoNavigation)
                .WithMany(p => p.ResettleInfoTo)
                .HasForeignKey(d => d.IdResettleInfo)
                .OnDelete(DeleteBehavior.Cascade);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
