using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class ResettleInfoSubPremiseFromConfiguration : IEntityTypeConfiguration<ResettleInfoSubPremiseFrom>
    {
        private string nameDatebase;

        public ResettleInfoSubPremiseFromConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<ResettleInfoSubPremiseFrom> builder)
        {
            builder.HasKey(e => e.IdKey);
            
            builder.ToTable("resettle_info_sub_premises_from", nameDatebase);

            builder.HasIndex(e => e.IdSubPremise)
                .HasName("FK_resettle_info_sub_premises_from_id_sub_premises");

            builder.HasIndex(e => e.IdResettleInfo)
                .HasName("FK_resettle_info_sub_premises_from_id_resettle_info");

            builder.Property(e => e.IdKey)
                .HasColumnName("id_key")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdSubPremise)
                .HasColumnName("id_sub_premises")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdResettleInfo)
                .HasColumnName("id_resettle_info")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(d => d.SubPremiseNavigation)
                .WithMany(p => p.ResettleInfoSubPremisesFrom)
                .HasForeignKey(d => d.IdSubPremise)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.ResettleInfoNavigation)
                .WithMany(p => p.ResettleInfoSubPremisesFrom)
                .HasForeignKey(d => d.IdResettleInfo)
                .OnDelete(DeleteBehavior.Cascade);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
