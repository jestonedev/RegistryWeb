using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class BuildingsConfiguration : IEntityTypeConfiguration<Buildings>
    {
        private string nameDatebase;

        public BuildingsConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<Buildings> builder)
        {
            builder.HasKey(e => e.IdBuilding);

            builder.ToTable("buildings", nameDatebase);

            builder.HasIndex(e => e.IdHeatingType)
                .HasName("FK_buildings_heating_type_id_heating_type");

            builder.HasIndex(e => e.IdStreet);

            builder.HasIndex(e => e.IdState)
                .HasName("FK_buildings_states_id_state");

            builder.HasIndex(e => e.IdStructureType)
                .HasName("FK_buildings_types_of_structure_id_type_of_structure");

            builder.Property(e => e.IdBuilding)
                .HasColumnName("id_building")
                .HasColumnType("int(11)");

            builder.Property(e => e.BalanceCost)
                .HasColumnName("balance_cost")
                .HasColumnType("decimal(12,2)")
                .HasDefaultValueSql("0.00");

            builder.Property(e => e.BtiRooms)
                .HasColumnName("BTI_rooms")
                .HasMaxLength(1512)
                .IsUnicode(false);

            builder.Property(e => e.CadastralCost)
                .HasColumnName("cadastral_cost")
                .HasColumnType("decimal(12,2)")
                .HasDefaultValueSql("0.00");

            builder.Property(e => e.CadastralNum)
                .HasColumnName("cadastral_num")
                .HasMaxLength(20)
                .IsUnicode(false);

            builder.Property(e => e.Canalization)
                .HasColumnName("canalization")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("1");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.DemolishedFactDate).HasColumnName("demolished_fact_date");

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .IsUnicode(false);

            builder.Property(e => e.Electricity)
                .HasColumnName("electricity")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("1");

            builder.Property(e => e.Elevator)
                .HasColumnName("elevator")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.Floors)
                .HasColumnName("floors")
                .HasColumnType("smallint(6)")
                .HasDefaultValueSql("5");

            builder.Property(e => e.HotWaterSupply)
                .HasColumnName("hot_water_supply")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("1");

            builder.Property(e => e.House)
                .IsRequired()
                .HasColumnName("house")
                .HasMaxLength(20)
                .IsUnicode(false);

            builder.Property(e => e.HousingCooperative)
                .HasColumnName("housing_cooperative")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.IdHeatingType)
                .HasColumnName("id_heating_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdState)
                .HasColumnName("id_state")
                .HasColumnType("int(11)")
                .HasDefaultValueSql("1");

            builder.Property(e => e.IdStreet)
                .IsRequired()
                .HasColumnName("id_street")
                .HasMaxLength(17)
                .IsUnicode(false);

            builder.Property(e => e.IdStructureType)
                .HasColumnName("id_structure_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.Improvement)
                .HasColumnName("improvement")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("1");

            builder.Property(e => e.IsMemorial)
                .HasColumnName("is_memorial")
                .HasColumnType("tinyint(4)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.LandArea)
                .HasColumnName("land_area")
                .HasDefaultValueSql("0");

            builder.Property(e => e.LandCadastralDate).HasColumnName("land_cadastral_date");

            builder.Property(e => e.LandCadastralNum)
                .HasColumnName("land_cadastral_num")
                .HasMaxLength(20)
                .IsUnicode(false);

            builder.Property(e => e.LivingArea)
                .HasColumnName("living_area")
                .HasDefaultValueSql("0");

            builder.Property(e => e.NumApartments)
                .HasColumnName("num_apartments")
                .HasColumnType("int(11)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.NumPremises)
                .HasColumnName("num_premises")
                .HasColumnType("int(11)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.NumRooms)
                .HasColumnName("num_rooms")
                .HasColumnType("int(11)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.NumSharedApartments)
                .HasColumnName("num_shared_apartments")
                .HasColumnType("int(11)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.Plumbing)
                .HasColumnName("plumbing")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("1");

            builder.Property(e => e.RadioNetwork)
                .HasColumnName("radio_network")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.RegDate)
                .HasColumnName("reg_date")
                .HasColumnType("date")
                .HasDefaultValueSql("1999-10-29");

            builder.Property(e => e.RentCoefficient)
                .HasColumnName("rent_coefficient")
                .HasColumnType("decimal(19,2)")
                .HasDefaultValueSql("0.00");

            builder.Property(e => e.RubbishChute)
                .HasColumnName("rubbish_chute")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.StartupYear)
                .HasColumnName("startup_year")
                .HasColumnType("int(11)")
                .HasDefaultValueSql("1900");

            builder.Property(e => e.StateDate).HasColumnName("state_date");

            builder.Property(e => e.TotalArea)
                .HasColumnName("total_area")
                .HasDefaultValueSql("0");

            builder.Property(e => e.Wear)
                .HasColumnName("wear")
                .HasDefaultValueSql("0");

            builder.HasOne(d => d.IdHeatingTypeNavigation)
                .WithMany(p => p.Buildings)
                .HasForeignKey(d => d.IdHeatingType);

            builder.HasOne(d => d.IdStreetNavigation)
                .WithMany(p => p.Buildings)
                .HasForeignKey(d => d.IdStreet);

            builder.HasOne(d => d.IdStateNavigation)
                .WithMany(p => p.Buildings)
                .HasForeignKey(d => d.IdState)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_buildings_states_id_state");

            builder.HasOne(d => d.IdStructureTypeNavigation)
                .WithMany(p => p.Buildings)
                .HasForeignKey(d => d.IdStructureType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_buildings_types_of_structure_id_type_of_structure");

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
