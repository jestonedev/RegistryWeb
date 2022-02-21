using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class PrivContractorWarrantTemplateConfiguration : IEntityTypeConfiguration<PrivContractorWarrantTemplate>
    {
        private string nameDatebase;

        public PrivContractorWarrantTemplateConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<PrivContractorWarrantTemplate> builder)
        {
            builder.HasKey(e => e.IdTemplate);

            builder.ToTable("priv_contractor_warrant_templates", nameDatebase);

            builder.Property(e => e.IdTemplate)
                .IsRequired()
                .HasColumnName("id_template")
                .HasColumnType("int(11)");

            builder.Property(e => e.WarrantText)
                .IsRequired()
                .HasColumnName("warrant_text")
                .HasMaxLength(2000)
                .IsUnicode(false);

            builder.Property(e => e.IdCategory)
                .HasColumnName("id_category")
                .HasColumnType("int(11)");
        }
    }
}
