using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.RegistryObjects.Common.Resettle;

namespace RegistryDb.Models.IEntityTypeConfiguration.RegistryObjects.Common.Resettle
{
    public class ResettleStageConfiguration : IEntityTypeConfiguration<ResettleStage>
    {
        private string nameDatebase;

        public ResettleStageConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<ResettleStage> builder)
        {
            builder.HasKey(e => e.IdResettleStage);

            builder.ToTable("resettle_stages", nameDatebase);

            builder.Property(e => e.IdResettleStage)
                .HasColumnName("id_resettle_stage")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.StageName)
                .IsRequired()
                .HasColumnName("stage_name")
                .HasMaxLength(255)
                .IsUnicode(false);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
