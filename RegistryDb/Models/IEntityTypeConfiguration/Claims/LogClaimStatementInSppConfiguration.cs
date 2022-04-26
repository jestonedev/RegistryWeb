using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities.Claims;

namespace RegistryDb.Models.IEntityTypeConfiguration.Claims
{
    public class LogClaimStatementInSppConfiguration : IEntityTypeConfiguration<LogClaimStatementInSpp>
    {
        private string nameDatebase;

        public LogClaimStatementInSppConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<LogClaimStatementInSpp> builder)
        {
            builder.ToTable("log_claim_statement_in_ssp", nameDatebase);

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdClaim)
                .HasColumnName("id_claim")
                .HasColumnType("int(11)");

            builder.Property(e => e.CreateDate)
                .HasColumnName("create_date");

            builder.Property(e => e.ExecutorLogin)
                .HasColumnName("executor_login")
                .HasMaxLength(255)
                .IsUnicode(false);
        }            
    }
}
