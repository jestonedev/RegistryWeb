using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class PremisesTypeConfiguration : IEntityTypeConfiguration<PremisesType>
    {
        private string nameDatebase;

        public PremisesTypeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<PremisesType> builder)
        {
            builder.HasKey(e => e.IdPremisesType);

            builder.ToTable("premises_types", nameDatebase);

            builder.Property(e => e.IdPremisesType)
                .HasColumnName("id_premises_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.PremisesTypeName)
                .IsRequired()
                .HasColumnName("premises_type")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.PremisesTypeAsNum)
                .HasColumnName("premises_type_as_num")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.PremisesTypeShort)
                .HasColumnName("premises_type_short")
                .HasMaxLength(10)
                .IsUnicode(false);
        }
    }
}
