using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class PremiseConfiguration : IEntityTypeConfiguration<Premise>
    {
        private string nameDatebase;

        public PremiseConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<Premise> builder)
        {
            builder.HasKey(e => e.IdPremises);

            builder.ToTable("premises", nameDatebase);

            builder.HasIndex(e => e.CadastralNum)
                .HasName("cadastral_num");

            builder.HasIndex(e => e.IdBuilding)
                .HasName("FK_premises_buildings_id_building");

            builder.HasIndex(e => e.IdPremisesComment)
                .HasName("FK_premises_id_premises_comment");

            builder.HasIndex(e => e.IdPremisesDoorKeys)
                .HasName("FK_premises_id_premises_door_keys");

            builder.HasIndex(e => e.IdPremisesKind)
                .HasName("FK_premises_premises_kinds_id_premises_kind");

            builder.HasIndex(e => e.IdPremisesType)
                .HasName("FK_premises_premises_types_id_premises_type");

            builder.HasIndex(e => e.IdState)
                .HasName("FK_premises_states_id_state");

            builder.Property(e => e.IdPremises)
                .HasColumnName("id_premises")
                .HasColumnType("int(11)");

            builder.Property(e => e.Account)
                .HasColumnName("account")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.BalanceCost)
                .HasColumnName("balance_cost")
                .HasColumnType("decimal(19,2)")
                .HasDefaultValueSql("0.00");

            builder.Property(e => e.CadastralCost)
                .HasColumnName("cadastral_cost")
                .HasColumnType("decimal(19,2)")
                .HasDefaultValueSql("0.00");

            builder.Property(e => e.CadastralNum)
                .HasColumnName("cadastral_num")
                .HasMaxLength(20)
                .IsUnicode(false);

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .IsUnicode(false);

            builder.Property(e => e.Floor)
                .HasColumnName("floor")
                .HasColumnType("smallint(6)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.Height)
                .HasColumnName("height")
                .HasDefaultValueSql("0");

            builder.Property(e => e.IdBuilding)
                .HasColumnName("id_building")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdPremisesComment)
                .HasColumnName("id_premises_comment")
                .HasColumnType("int(11)")
                .HasDefaultValueSql("1");

            builder.Property(e => e.IdPremisesDoorKeys)
                .HasColumnName("id_premises_door_keys")
                .HasColumnType("int(11)")
                .HasDefaultValueSql("1");

            builder.Property(e => e.IdPremisesKind)
                .HasColumnName("id_premises_kind")
                .HasColumnType("int(11)")
                .HasDefaultValueSql("1");

            builder.Property(e => e.IdPremisesType)
                .HasColumnName("id_premises_type")
                .HasColumnType("int(11)")
                .HasDefaultValueSql("1");

            builder.Property(e => e.IdState)
                .HasColumnName("id_state")
                .HasColumnType("int(11)")
                .HasDefaultValueSql("1");

            builder.Property(e => e.IsMemorial)
                .HasColumnName("is_memorial")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.LivingArea)
                .HasColumnName("living_area")
                .HasDefaultValueSql("0");

            builder.Property(e => e.NumBeds)
                .HasColumnName("num_beds")
                .HasColumnType("smallint(6)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.NumRooms)
                .HasColumnName("num_rooms")
                .HasColumnType("smallint(6)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.PremisesNum)
                .IsRequired()
                .HasColumnName("premises_num")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.RegDate)
                .HasColumnName("reg_date")
                .HasColumnType("date");

            builder.Property(e => e.StateDate).HasColumnName("state_date");

            builder.Property(e => e.TotalArea)
                .HasColumnName("total_area")
                .HasDefaultValueSql("0");

            builder.HasOne(d => d.IdPremisesCommentNavigation)
                .WithMany(p => p.Premises)
                .HasForeignKey(d => d.IdPremisesComment)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_premises_id_premises_comment");

            builder.HasOne(d => d.IdPremisesKindNavigation)
                .WithMany(p => p.Premises)
                .HasForeignKey(d => d.IdPremisesKind)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(d => d.IdPremisesTypeNavigation)
                .WithMany(p => p.Premises)
                .HasForeignKey(d => d.IdPremisesType)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(d => d.IdStateNavigation)
                .WithMany(p => p.Premises)
                .HasForeignKey(d => d.IdState)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_premises_states_id_state");

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
