﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class PrivContractorConfiguration : IEntityTypeConfiguration<PrivContractor>
    {
        private string nameDatebase;

        public PrivContractorConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<PrivContractor> builder)
        {
            builder.ToTable("priv_contractors", nameDatebase);

            builder.HasKey(e => e.IdContractor);

            builder.Property(e => e.IdContractor)
                .HasColumnName("id_contractors")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.IsNonontractor)
                .HasColumnName("is_noncontractor")
                .HasColumnType("tinyint")
                .HasDefaultValueSql("0")
                .IsRequired();

            builder.Property(e => e.IdContract)
                .HasColumnName("id_contract")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.Surname)
                .HasColumnName("surname")
                .HasMaxLength(255)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(e => e.Patronymic)
                .HasColumnName("patronymic")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.IdKinship)
                .HasColumnName("id_kinship")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.DateBirth)
                .HasColumnName("date_birth")
                .HasColumnType("date");

            builder.Property(e => e.Passport)
                .HasColumnName("passport")
                .HasMaxLength(2000)
                .IsUnicode(false);

            builder.Property(e => e.Part)
                .HasColumnName("part")
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(2000)
                .IsUnicode(false);

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint")
                .HasDefaultValueSql("0")
                .IsRequired();

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => !e.Deleted);
        }  
    }
}
