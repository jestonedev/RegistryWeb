using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.RegistryObjects.Buildings;

namespace RegistryDb.Models.IEntityTypeConfiguration.RegistryObjects.Buildings
{
    public class BuildingManagmentOrgConfiguration : IEntityTypeConfiguration<BuildingManagmentOrg>
    {
        private string nameDatebase;

        public BuildingManagmentOrgConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<BuildingManagmentOrg> builder)
        {
            builder.HasKey(e => e.IdOrganization);

            builder.ToTable("buildings_managment_orgs", nameDatebase);

            builder.Property(e => e.IdOrganization)
                .HasColumnName("id_organization")
                .HasColumnType("int(11)");

            builder.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name")
                .HasMaxLength(255)
                .IsUnicode(false);
        }
    }
}
