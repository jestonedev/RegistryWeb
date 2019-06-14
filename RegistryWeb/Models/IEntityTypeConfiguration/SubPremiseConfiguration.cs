using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class SubPremiseConfiguration : IEntityTypeConfiguration<SubPremise>
    {
        private string nameDatebase;

        public SubPremiseConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<SubPremise> builder)
        {
            builder.HasKey(e => e.IdSubPremises);

            builder.ToTable("sub_premises", nameDatebase);

            builder.HasIndex(e => e.IdPremises)
                .HasName("FK_sub_premises_premises_id_premises");

            builder.HasIndex(e => e.IdState)
                .HasName("FK_sub_premises_states_id_state");

            builder.Property(e => e.IdSubPremises)
                .HasColumnName("id_sub_premises")
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

            builder.Property(e => e.IdPremises)
                .HasColumnName("id_premises")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdState)
                .HasColumnName("id_state")
                .HasColumnType("int(11)")
                .HasDefaultValueSql("1");

            builder.Property(e => e.LivingArea)
                .HasColumnName("living_area")
                .HasDefaultValueSql("0");

            builder.Property(e => e.StateDate).HasColumnName("state_date");

            builder.Property(e => e.SubPremisesNum)
                .HasColumnName("sub_premises_num")
                .HasMaxLength(20)
                .IsUnicode(false);

            builder.Property(e => e.TotalArea)
                .HasColumnName("total_area")
                .HasDefaultValueSql("0");

            builder.HasOne(d => d.IdPremisesNavigation)
                .WithMany(p => p.SubPremises)
                .HasForeignKey(d => d.IdPremises);

            builder.HasOne(d => d.IdStateNavigation)
                .WithMany(p => p.SubPremises)
                .HasForeignKey(d => d.IdState)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_sub_premises_states_id_state");

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
