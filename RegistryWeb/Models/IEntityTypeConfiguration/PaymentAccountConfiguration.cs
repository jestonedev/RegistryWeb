﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class PaymentAccountConfiguration : IEntityTypeConfiguration<PaymentAccount>
    {
        private string nameDatebase;

        public PaymentAccountConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<PaymentAccount> builder)
        {
            builder.HasKey(e => e.IdAccount);

            builder.ToTable("payments_accounts", nameDatebase);

            builder.Property(e => e.IdAccount)
                .HasColumnName("id_account")
                .HasColumnType("int(11)");

            builder.Property(e => e.Account)
                .HasColumnName("account")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Crn)
                .HasColumnName("crn")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.RawAddress)
                .HasColumnName("raw_address")
                .HasMaxLength(255)
                .IsUnicode(false);
        }
    }
}
