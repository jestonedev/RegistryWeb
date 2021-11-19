using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class PrivEstateOwnerConfiguration : IEntityTypeConfiguration<PrivEstateOwner>
    {
        private string nameDatebase;

        public PrivEstateOwnerConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<PrivEstateOwner> builder)
        {
            builder.HasKey(e => e.IdOwner);

            builder.ToTable("priv_estate_owners", nameDatebase);

            builder.Property(e => e.IdOwner)
                .HasColumnName("id_owner")
                .HasColumnType("int(11)");

            builder.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
