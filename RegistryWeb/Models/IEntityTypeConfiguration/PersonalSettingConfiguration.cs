using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class PersonalSettingConfiguration : IEntityTypeConfiguration<PersonalSetting>
    {
        private string nameDatebase;

        public PersonalSettingConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<PersonalSetting> builder)
        {
            builder.HasKey(e => e.IdUser);

            builder.ToTable("personal_settings", nameDatebase);

            builder.Property(e => e.IdUser)
                .HasColumnName("id_user")
                .HasColumnType("int(11)");

            builder.Property(e => e.SqlDriver)
                .HasColumnName("sql_driver")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.HasOne(d => d.IdUserNavigation)
                .WithOne(p => p.PersonalSetting)
                .HasForeignKey<PersonalSetting>(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_personal_settings_id_user");
        }
    }
}
