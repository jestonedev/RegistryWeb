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
            builder.Property(e => e.CadastreNumber)
                .HasColumnName("cadastre_number")
                .HasMaxLength(100)
                .IsUnicode(false);
            builder.Property(e => e.Floor)
                .HasColumnName("floor")
                .HasMaxLength(255)
                .IsUnicode(false);
            builder.Property(e => e.Rooms)
                .HasColumnName("rooms")
                .HasColumnType("int(11)");
            builder.Property(e => e.LivingSpace)
                .HasColumnName("living_space")
                .HasColumnType("decimal(10, 2)");
            builder.Property(e => e.LoggiaSpace)
                .HasColumnName("loggia_space")
                .HasColumnType("decimal(10, 2)");
            builder.Property(e => e.AncillarySpace)
                .HasColumnName("ancillary_space")
                .HasColumnType("decimal(10, 2)");
            builder.Property(e => e.ApartmentSpace)
                .HasColumnName("apartment_space")
                .HasColumnType("decimal(10, 2)");
            builder.Property(e => e.TotalSpace)
                .HasColumnName("total_space")
                .HasColumnType("decimal(10, 2)");
            builder.Property(e => e.CeilingHeight)
                .HasColumnName("ceiling_height")
                .HasColumnType("decimal(10, 2)");
            builder.Property(e => e.ApplicationDate)
                .HasColumnName("application_date")
                .HasColumnType("datetime");
            builder.Property(e => e.DateIssue)
                .HasColumnName("date_issue")
                .HasColumnType("date");
            builder.Property(e => e.RegistrationDate)
                .HasColumnName("registration_date")
                .HasColumnType("datetime");
            builder.Property(e => e.IdTypeProperty)
                .HasColumnName("id_type_property")
                .HasColumnType("int(11)");
            builder.Property(e => e.IdRroadType)
                .HasColumnName("id_road_type")
                .HasColumnType("int(11)");
            builder.Property(e => e.InsertDate)
                .HasColumnName("insert_date")
                .HasColumnType("datetime");
            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(767)
                .IsUnicode(false);
            builder.Property(e => e.IdType)
                .HasColumnName("id_type")
                .HasColumnType("int(11)");
            builder.Property(e => e.RegisterMfcNumber)
                .HasColumnName("register_mfc_number")
                .HasMaxLength(255)
                .IsUnicode(false);
            builder.Property(e => e.DateIssueCivil)
                .HasColumnName("date_issue_civil")
                .HasColumnType("date");
            builder.Property(e => e.AdditionalInfo)
                .HasColumnName("additional_info")
                .HasMaxLength(65535)
                .IsUnicode(false);
            builder.Property(e => e.IsRefusenik)
                .HasColumnName("is_refusenik")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0")
                .IsRequired();
            builder.Property(e => e.IsShares)
                .HasColumnName("is_shares")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0")
                .IsRequired();
            builder.Property(e => e.IsRasprivatization)
                .HasColumnName("is_rasprivatization")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0")
                .IsRequired();
            builder.Property(e => e.IsHostel)
                .HasColumnName("is_hostel")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");
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
            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0")
                .IsRequired();

            builder.Property(e => e.User)
                .HasColumnName("user")
                .HasMaxLength(255)
                .IsUnicode(false);
            builder.Property(e => e.IdSpecialist)
                .HasColumnName("id_specialist")
                .HasColumnType("int(11)");
            builder.Property(e => e.IdExecutor)
                .HasColumnName("id_executor")
                .HasColumnType("int(11)")
                .IsRequired();
            builder.HasOne(e => e.ExecutorNavigation)
                .WithMany(p => p.PrivContracts)
                .HasForeignKey(e => e.IdExecutor);

            builder.Property(e => e.AddressPrivatization)
                .HasColumnName("address_privatization")
                .HasMaxLength(255)
                .IsUnicode(false);
            builder.Property(e => e.IdStreet)
                .HasColumnName("id_street_")
                .HasMaxLength(255)
                .IsUnicode(false);
            builder.HasOne(e => e.StreetNavigation)
                .WithMany(p => p.PrivContracts)
                .HasForeignKey(e => e.IdStreet);
            builder.Property(e => e.IdBuilding)
                .HasColumnName("id_building_")
                .HasColumnType("int(11)");
            builder.HasOne(e => e.BuildingNavigation)
                .WithMany(p => p.PrivContracts)
                .HasForeignKey(e => e.IdBuilding);
            builder.Property(e => e.IdPremise)
                .HasColumnName("id_premise_")
                .HasColumnType("int(11)");
            builder.HasOne(e => e.PremiseNavigation)
                .WithMany(p => p.PrivContracts)
                .HasForeignKey(e => e.IdPremise);
            builder.Property(e => e.IdSubPremise)
                .HasColumnName("id_sub_premise_")
                .HasColumnType("int(11)");
            builder.HasOne(e => e.SubPremiseNavigation)
                .WithMany(p => p.PrivContracts)
                .HasForeignKey(e => e.IdSubPremise);

            builder.HasQueryFilter(e => !e.Deleted);
        }
    }
}
