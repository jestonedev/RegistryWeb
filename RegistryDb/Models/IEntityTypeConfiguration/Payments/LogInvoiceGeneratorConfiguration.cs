using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities.Payments;

namespace RegistryDb.Models.IEntityTypeConfiguration.Payments
{
    public class LogInvoiceGeneratorConfiguration: IEntityTypeConfiguration<LogInvoiceGenerator>
    {
        private string nameDatebase;

        public LogInvoiceGeneratorConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<LogInvoiceGenerator> builder)
        {
            builder.ToTable("log_invoice_generator", nameDatebase);

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdAccount)
                .HasColumnName("id_account")
                .HasColumnType("int(11)");

            builder.Property(e => e.CreateDate)
                .HasColumnName("create_date");

            builder.Property(e => e.OnDate)
                .HasColumnName("on_date")
                .HasColumnType("date");

            builder.Property(e => e.Emails)
                .HasColumnName("emails")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.ResultCode)
                .HasColumnName("result_code")
                .HasColumnType("int(11)");
        }            
    }
}
