using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class PersonalSettingsConfiguration : IEntityTypeConfiguration<PersonalSetting>
    {
        private string nameDatebase;

        public PersonalSettingsConfiguration(string nameDatebase)
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

            builder.Property(e => e.PaymentAccauntTableJson)
                .HasColumnName("payment_accaunt_table_json")
                .IsUnicode(false);

            builder.HasOne(ps => ps.AclUser)
                .WithOne(au => au.PersonalSetting)
                .HasForeignKey<PersonalSetting>(ps => ps.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_personal_settings_id_user");
        }
    }
}
