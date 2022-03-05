using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Privatization;

namespace RegistryDb.Models.IEntityTypeConfiguration.Privatization
{
    public class PrivAdditionalEstateConfiguration : IEntityTypeConfiguration<PrivAdditionalEstate>
    {
        private string nameDatebase;

        public PrivAdditionalEstateConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<PrivAdditionalEstate> builder)
        {
            builder.ToTable("priv_additional_estates", nameDatebase);

            builder.HasKey(e => e.IdEstate);

            builder.Property(e => e.IdEstate)
                .HasColumnName("id_estate")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.IdContract)
                .HasColumnName("id_contract")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.IdStreet)
                .IsRequired()
                .HasColumnName("id_street")
                .HasMaxLength(17)
                .IsUnicode(false);

            builder.Property(e => e.IdBuilding)
                .IsRequired()
                .HasColumnName("id_building")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdPremise)
                .HasColumnName("id_premise")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdSubPremise)
                .HasColumnName("id_sub_premise")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint")
                .HasDefaultValueSql("0")
                .IsRequired();

            builder.HasOne(e => e.PrivContractNavigation)
                .WithMany(p => p.PrivAdditionalEstates)
                .HasForeignKey(e => e.IdContract);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => !e.Deleted);
        }  
    }
}
