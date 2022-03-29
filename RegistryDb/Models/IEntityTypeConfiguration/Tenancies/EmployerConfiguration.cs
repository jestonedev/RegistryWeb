using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities.Tenancies;

namespace RegistryDb.Models.IEntityTypeConfiguration.Tenancies
{
    public class EmployerConfiguration : IEntityTypeConfiguration<Employer>
    {
        private string nameDatebase;

        public EmployerConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<Employer> builder)
        {
            builder.HasKey(e => e.IdEmployer);

            builder.ToTable("employers", nameDatebase);

            builder.Property(e => e.IdEmployer)
                .HasColumnName("id_employer")
                .HasColumnType("int(11)");

            builder.Property(e => e.EmployerName)
                .IsRequired()
                .HasColumnName("employer")
                .HasMaxLength(255)
                .IsUnicode(false);
        }
    }
}
