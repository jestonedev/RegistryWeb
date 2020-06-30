using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class SelectableSignersConfiguration : IEntityTypeConfiguration<SelectableSigner>
    {
        private string nameDatebase;

        public SelectableSignersConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<SelectableSigner> builder)
        {
            builder.HasKey(e => e.IdRecord);

            builder.ToTable("selectable_signers", nameDatebase);

            builder.Property(e => e.IdRecord)
                .HasColumnName("id_record")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdSignerGroup)
                .HasColumnName("id_signer_group")
                .HasColumnType("int(11)");

            builder.Property(e => e.Surname)
                .IsRequired()
                .HasColumnName("surname")
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.Patronymic)
                .HasColumnName("patronymic")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Post)
                .IsRequired()
                .HasColumnName("post")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Phone)
                .HasColumnName("phone")
                .HasMaxLength(255)
                .IsUnicode(false);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
