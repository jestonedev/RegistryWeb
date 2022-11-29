using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.RegistryObjects.Common.Resettle;

namespace RegistryDb.Models.IEntityTypeConfiguration.RegistryObjects.Common.Resettle
{
    public class ResettleInfoConfiguration : IEntityTypeConfiguration<ResettleInfo>
    {
        private string nameDatebase;

        public ResettleInfoConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<ResettleInfo> builder)
        {
            builder.HasKey(e => e.IdResettleInfo);

            builder.ToTable("resettle_info", nameDatebase);

            builder.HasIndex(e => e.IdResettleKind)
                .HasName("FK_resettle_premises_info_id_ressetle_kind");

            builder.Property(e => e.IdResettleInfo)
                .HasColumnName("id_resettle_info")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdResettleKind)
                .HasColumnName("id_ressetle_kind")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdResettleKindFact)
                .HasColumnName("id_resettle_kind_fact")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdResettleStage)
                .HasColumnName("id_resettle_stage")
                .HasColumnType("int(11)");

            builder.Property(e => e.ResettleDate)
                .HasColumnName("resettle_date")
                .HasColumnType("date");

            builder.Property(e => e.FinanceSource1)
                .HasColumnName("finance_source_1")
                .HasColumnType("decimal(18,2)");

            builder.Property(e => e.FinanceSource2)
                .HasColumnName("finance_source_2")
                .HasColumnType("decimal(18,2)");

            builder.Property(e => e.FinanceSource3)
                .HasColumnName("finance_source_3")
                .HasColumnType("decimal(18,2)");

            builder.Property(e => e.FinanceSource4)
                .HasColumnName("finance_source_4")
                .HasColumnType("decimal(18,2)");

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(2048);

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");


            builder.HasOne(d => d.ResettleKindNavigation)
                .WithMany(p => p.ResettleInfos)
                .HasForeignKey(d => d.IdResettleKind)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.ResettleKindFactNavigation)
                .WithMany(p => p.ResettleInfosFact)
                .HasForeignKey(d => d.IdResettleKindFact)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.ResettleStageNavigation)
                .WithMany(p => p.ResettleInfos)
                .HasForeignKey(d => d.IdResettleStage)
                .OnDelete(DeleteBehavior.Restrict);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
