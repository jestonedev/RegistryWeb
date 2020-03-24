using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class StructureTypeOverlapConfiguration : IEntityTypeConfiguration<StructureTypeOverlap>
    {
        private string nameDatebase;

        public StructureTypeOverlapConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<StructureTypeOverlap> builder)
        {
            builder.HasKey(e => e.IdStructureTypeOverlap);

            builder.ToTable("structure_type_overlap", nameDatebase);

            builder.Property(e => e.IdStructureTypeOverlap)
                .HasColumnName("id_structure_type_overlap")
                .HasColumnType("int(11)");

            builder.Property(e => e.StructureTypeOverlapName)
                .IsRequired()
                .HasColumnName("structure_type_overlap")
                .HasMaxLength(255)
                .IsUnicode(false);
        }
    }
}
