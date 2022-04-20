using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class UinForClaimStatementInSspConfiguration : IEntityTypeConfiguration<UinForClaimStatementInSsp>
    {
        private string nameDatebase;

        public UinForClaimStatementInSspConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<UinForClaimStatementInSsp> builder)
        {
            builder.ToTable("uin_for_claim_statement_in_ssp", nameDatebase);
            
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("int(11)");
            
            builder.Property(e => e.IdClaim)
                .HasColumnName("id_claim")
                .HasColumnType("int(11)");

            builder.Property(e => e.Uin)
                .HasColumnName("uin")
                .HasMaxLength(25)
                .IsUnicode(false);
        }
    }
}
