using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class HeatingTypeConfiguration : IEntityTypeConfiguration<HeatingType>
    {
        private string nameDatebase;

        public HeatingTypeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<HeatingType> builder)
        {
            builder.HasKey(e => e.IdHeatingType);

            builder.ToTable("heating_type", nameDatebase);

            builder.Property(e => e.IdHeatingType)
                .HasColumnName("id_heating_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.HeatingType1)
                .HasColumnName("heating_type")
                .HasMaxLength(255)
                .IsUnicode(false);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
