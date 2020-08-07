using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class FoundationTypeConfiguration : IEntityTypeConfiguration<FoundationType>
    {
        private string nameDatebase;

        public FoundationTypeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<FoundationType> builder)
        {
            builder.HasKey(e => e.IdFoundationType);

            builder.ToTable("foundation_types", nameDatebase);

            builder.Property(e => e.IdFoundationType)
                .HasColumnName("id_foundation_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name")
                .HasMaxLength(255)
                .IsUnicode(false);
        }
    }
}
