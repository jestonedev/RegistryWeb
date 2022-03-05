using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Claims;

namespace RegistryDb.Models.IEntityTypeConfiguration.Claims
{
    public class JudgeBuildingAssocConfiguration : IEntityTypeConfiguration<JudgeBuildingAssoc>
    {
        private string nameDatebase;

        public JudgeBuildingAssocConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<JudgeBuildingAssoc> builder)
        {
            builder.HasKey(e => e.IdAssoc);

            builder.ToTable("judges_buildings_assoc", nameDatebase);

            builder.HasIndex(e => e.IdBuilding)
                .HasName("FK_judges_buildings_assoc_id_b");

            builder.HasIndex(e => e.IdJudge)
                .HasName("FK_judges_buildings_assoc_id_j");

            builder.Property(e => e.IdAssoc)
                .HasColumnName("id_assoc")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.IdBuilding)
                .HasColumnName("id_building")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdJudge)
                .HasColumnName("id_judge")
                .HasColumnType("int(11)");

            builder.HasOne(d => d.BuildingNavigation)
                .WithMany(p => p.JudgeBuildingsAssoc)
                .HasForeignKey(d => d.IdBuilding);

            builder.HasOne(d => d.JudgeNavigation)
                .WithMany(p => p.JudgeBuildingsAssoc)
                .HasForeignKey(d => d.IdJudge);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
