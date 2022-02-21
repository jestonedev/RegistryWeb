using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class PrivRealtorConfiguration : IEntityTypeConfiguration<PrivRealtor>
    {
        private string nameDatebase;

        public PrivRealtorConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<PrivRealtor> builder)
        {
            builder.HasKey(e => e.IdRealtor);

            builder.ToTable("priv_realtors", nameDatebase);

            builder.Property(e => e.IdRealtor)
                .HasColumnName("id_realtor")
                .HasColumnType("int(11)");

            builder.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Passport)
                .IsRequired()
                .HasColumnName("passport")
                .HasMaxLength(2000)
                .IsUnicode(false);

            builder.Property(e => e.DateBirth)
                .HasColumnName("date_birth")
                .HasColumnType("date");

            builder.Property(e => e.PlaceOfRegistration)
                .HasColumnName("place_of_registration")
                .HasMaxLength(2000)
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
