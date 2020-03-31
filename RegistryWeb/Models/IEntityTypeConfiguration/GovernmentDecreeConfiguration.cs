using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class GovernmentDecreeConfiguration : IEntityTypeConfiguration<GovernmentDecree>
    {
        private string nameDatebase;

        public GovernmentDecreeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<GovernmentDecree> builder)
        {
            builder.HasKey(e => e.IdDecree);

            builder.ToTable("government_decree", nameDatebase);

            builder.Property(e => e.IdDecree)
                .HasColumnName("id_decree")
                .HasColumnType("int(11)");

            builder.Property(e => e.Number)
                .IsRequired()
                .HasColumnName("number")
                .HasMaxLength(255)
                .IsUnicode(false);
        }
    }
}
