﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class ObjectStateConfiguration : IEntityTypeConfiguration<ObjectState>
    {
        private string nameDatebase;

        public ObjectStateConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<ObjectState> builder)
        {
            builder.HasKey(e => e.IdState);

            builder.ToTable("object_states", nameDatebase);

            builder.Property(e => e.IdState)
                .HasColumnName("id_state")
                .HasColumnType("int(11)");

            builder.Property(e => e.StateFemale)
                .IsRequired()
                .HasColumnName("state_female")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.StateNeutral)
                .IsRequired()
                .HasColumnName("state_neutral")
                .HasMaxLength(255)
                .IsUnicode(false);
        }
    }
}
