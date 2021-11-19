using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class PrivContractConfiguration : IEntityTypeConfiguration<PrivContract>
    {
        private string nameDatebase;

        public PrivContractConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<PrivContract> builder)
        {
            builder.ToTable("priv_contracts", nameDatebase);

            builder.HasKey(e => e.IdContract);

            builder.Property(e => e.IdContract)
                .HasColumnName("id_contract")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.RegNumber)
                .HasColumnName("reg_number")
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(e => e.PrivAddress)
                .HasColumnName("priv_address")
                .HasMaxLength(255)
                .IsUnicode(false);
            builder.Property(e => e.PrivFloor)
                .HasColumnName("priv_floor")
                .HasMaxLength(255)
                .IsUnicode(false);
            builder.Property(e => e.PrivRooms)
                .HasColumnName("priv_rooms")
                .HasColumnType("int(11)");
            builder.Property(e => e.PrivTotalSpace)
                .HasColumnName("priv_total_space")
                .HasColumnType("decimal(10, 2)");
            builder.Property(e => e.PrivLivingSpace)
                .HasColumnName("priv_living_space")
                .HasColumnType("decimal(10, 2)");
            builder.Property(e => e.PrivApartmentSpace)
                .HasColumnName("priv_apartment_space")
                .HasColumnType("decimal(10, 2)");
            builder.Property(e => e.PrivLoggiaSpace)
                .HasColumnName("priv_loggia_space")
                .HasColumnType("decimal(10, 2)");
            builder.Property(e => e.PrivAncillarySpace)
                .HasColumnName("priv_ancillary_space")
                .HasColumnType("decimal(10, 2)");
            builder.Property(e => e.PrivCeilingHeight)
                .HasColumnName("priv_ceiling_height")
                .HasColumnType("decimal(10, 2)");
            builder.Property(e => e.PrivCadastreNumber)
                .HasColumnName("priv_cadastre_number")
                .HasMaxLength(100)
                .IsUnicode(false);

            builder.Property(e => e.IdStreet)
                .HasColumnName("id_street")
                .HasMaxLength(255)
                .IsUnicode(false);
            builder.HasOne(e => e.StreetNavigation)
                .WithMany(p => p.PrivContracts)
                .HasForeignKey(e => e.IdStreet);

            builder.Property(e => e.IdBuilding)
                .HasColumnName("id_building")
                .HasColumnType("int(11)");
            builder.HasOne(e => e.BuildingNavigation)
                .WithMany(p => p.PrivContracts)
                .HasForeignKey(e => e.IdBuilding);

            builder.Property(e => e.IdPremise)
                .HasColumnName("id_premise")
                .HasColumnType("int(11)");
            builder.HasOne(e => e.PremiseNavigation)
                .WithMany(p => p.PrivContracts)
                .HasForeignKey(e => e.IdPremise);

            builder.Property(e => e.IdSubPremise)
                .HasColumnName("id_sub_premise")
                .HasColumnType("int(11)");
            builder.HasOne(e => e.SubPremiseNavigation)
                .WithMany(p => p.PrivContracts)
                .HasForeignKey(e => e.IdSubPremise);

            builder.Property(e => e.IdExecutor)
                .HasColumnName("id_executor")
                .HasColumnType("int(11)")
                .IsRequired();
            builder.HasOne(e => e.ExecutorNavigation)
                .WithMany(p => p.PrivContracts)
                .HasForeignKey(e => e.IdExecutor);

            builder.Property(e => e.ApplicationDate)
                .HasColumnName("application_date")
                .HasColumnType("datetime");
            builder.Property(e => e.DateIssue)
                .HasColumnName("date_issue")
                .HasColumnType("date");
            builder.Property(e => e.RegistrationDate)
                .HasColumnName("registration_date")
                .HasColumnType("datetime");
            builder.Property(e => e.DateIssueCivil)
                .HasColumnName("date_issue_civil")
                .HasColumnType("date");

            builder.Property(e => e.SocrentRegNumber)
                .HasColumnName("socrent_reg_number")
                .HasMaxLength(50)
                .IsUnicode(false);
            builder.Property(e => e.SocrentDate)
                .HasColumnName("socrent_date")
                .HasColumnType("datetime");

            builder.Property(e => e.IdTypeProperty)
                .HasColumnName("id_type_property")
                .HasColumnType("int(11)");
            builder.HasOne(e => e.TypeOfProperty)
                .WithMany(e => e.PrivContracts)
                .HasForeignKey(e => e.IdTypeProperty);

            builder.Property(e => e.IsRefusenik)
               .HasColumnName("is_refusenik")
               .HasColumnType("tinyint(1)")
               .HasDefaultValueSql("0")
               .IsRequired();
            builder.Property(e => e.IsRasprivatization)
                .HasColumnName("is_rasprivatization")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0")
                .IsRequired();
            builder.Property(e => e.IsRelocation)
                .HasColumnName("is_relocation")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0")
                .IsRequired();
            builder.Property(e => e.IsRefuse)
                .HasColumnName("is_refuse")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0")
                .IsRequired();

            builder.Property(e => e.AdditionalInfo)
                .HasColumnName("additional_info")
                .HasMaxLength(65535)
                .IsUnicode(false);
            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(767)
                .IsUnicode(false);

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0")
                .IsRequired();

            builder.HasQueryFilter(e => !e.Deleted);
        }
    }
}
